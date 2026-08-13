#!/usr/bin/env python3
"""
Static audit of SphereII Learn By Doing.

Reads every Config/**/*.xml, resolves the tuning cvars, and writes:

  TESTPLAN.md   one row per XP award path - the trigger to fire, what gates it,
                what it pays, what throttles it - plus the expected actions and
                cooldown-floor time for every level up to the vanilla max.
  FINDINGS.md   award paths that can never pay out, throttles that do not exist,
                and progressions the runtime log cannot see.
  lbd_model.json  the parsed model, consumed by lbd_logparse.py.

Usage:  python lbd_audit.py [--game "<path to 7 Days To Die>"]
"""
import os, re, sys, json, math, argparse
import xml.etree.ElementTree as ET
from collections import defaultdict

HERE = os.path.dirname(os.path.abspath(__file__))
MOD = os.path.normpath(os.path.join(HERE, "..", "Config"))
DEFAULT_GAME = r"C:\Program Files (x86)\Steam\steamapps\common\7 Days To Die"

ap = argparse.ArgumentParser()
ap.add_argument("--game", default=DEFAULT_GAME)
ap.add_argument("--out", default=HERE)
args = ap.parse_args()

VANILLA = os.path.join(args.game, "Data", "Config", "progression.xml")

XP_CVAR = re.compile(r"^\$([a-z0-9_]+)_lbd_xp$")
TONEXT = re.compile(r"^\$([a-z0-9_]+)_lbd_xptonext$")

# --------------------------------------------------------------- load the mod
docs, parse_errors = {}, []
for root, _, names in os.walk(MOD):
    for n in names:
        if n.lower().endswith(".xml"):
            p = os.path.join(root, n)
            try:
                docs[p] = ET.parse(p).getroot()
            except ET.ParseError as e:
                parse_errors.append((p, str(e)))

def rel(f):
    return os.path.relpath(f, MOD).replace("\\", "/")

# ------------------------------------------------------- tuning cvar constants
consts, const_src = {}, {}

def harvest(container, src):
    for te in container.iter("triggered_effect"):
        if te.get("action") == "ModifyCVar" and te.get("operation") == "set":
            consts[te.get("cvar")] = te.get("value")
            const_src[te.get("cvar")] = src

for f, r in docs.items():
    for a in r.iter("append"):
        if "ProgressionLearnByDoing_Init" in a.get("xpath", ""):
            harvest(a, rel(f))
    for b in r.iter("buff"):
        if b.get("name") == "ProgressionLearnByDoing_Init":
            harvest(b, rel(f))

# Every cvar the mod writes at runtime, at any point, with any operation. A value
# expression naming one of these is computed live (e.g. $temp_quest_xp_bonus, set
# from $completedQuestTier then multiplied) and is NOT a dead reference, even
# though it has no constant to resolve to.
runtime_cvars = set()
for f, r in docs.items():
    for te in r.iter("triggered_effect"):
        if te.get("action") == "ModifyCVar" and te.get("cvar"):
            runtime_cvars.add(te.get("cvar"))

def resolve(expr, depth=0):
    """Resolve @$cvar chains to a number. None = cannot resolve (reads as 0 in game)."""
    if expr is None or depth > 8:
        return None
    e = str(expr).strip().lstrip("@")
    if e.startswith("$") or e.startswith("."):
        return resolve(consts[e], depth + 1) if e in consts else None
    try:
        return float(e)
    except ValueError:
        return None

def is_runtime(expr):
    """True if the expression names a cvar the mod computes at runtime."""
    if expr is None:
        return False
    return str(expr).strip().lstrip("@") in runtime_cvars

# ------------------------------------------------------------- buff durations
durations = {}
for f, r in docs.items():
    for b in r.iter("buff"):
        n = b.get("name")
        d = b.find("duration")
        if n and d is not None:
            durations[n] = float(d.get("value"))

# ---------------------------------------------------------------- award paths
def reqs_of(el):
    out = []
    for rq in el.findall("requirement"):
        bits = [f"{k}={v}" for k, v in rq.attrib.items() if k != "name"]
        out.append(rq.get("name", "") + ("(" + ", ".join(bits) + ")" if bits else ""))
    return out

# The mod logs through SCore's LogMessageCVars, which expands @cvar tokens in the
# message; vanilla LogMessage does not. Treat both as "this effect writes a log
# line" so the audit keeps working across the switch.
def is_log(te):
    return (te.get("action") or "").split(",")[0].strip() in ("LogMessage", "LogMessageCVars")

def req_sig(te):
    """Requirement set of an effect, ignoring the debug gate, for award/log pairing."""
    out = []
    for rq in te.findall("requirement"):
        if rq.get("cvar") == "$lbd_debug":
            continue
        out.append(tuple(sorted(rq.attrib.items())))
    for wrap in te.findall("requirements"):
        for rq in wrap.findall("requirement"):
            out.append(tuple(sorted(rq.attrib.items())))
    return frozenset(out)

awards = defaultdict(list)
tonext_init, levelup_buff, real_name = {}, {}, {}
group_of = {}                      # key -> (manager buff, effect_group name)
key_to_lubuff = {}                 # key -> the _LevelUpCheck buff it hands off to
has_award_log = defaultdict(int)
has_levelcheck_log = defaultdict(int)
labels = defaultdict(set)   # progkey -> display names used in its LBD DEBUG lines
levelup_labels = defaultdict(set)  # progression-lower -> labels from its level-up buff

def group_key(eg):
    for te in eg.iter("triggered_effect"):
        cv = te.get("cvar") or ""
        m = XP_CVAR.match(cv) or TONEXT.match(cv)
        if m:
            return m.group(1)
    return None

for f, r in docs.items():
    for apnd in r.iter("append"):
        m = re.search(r"buff\[@name='([^']+)'\]", apnd.get("xpath", ""))
        mgr = m.group(1) if m else None
        for eg in apnd.iter("effect_group"):
            k = group_key(eg)
            for te in eg.findall("triggered_effect"):
                cv = te.get("cvar") or ""
                mx = XP_CVAR.match(cv)
                if te.get("action") == "ModifyCVar" and mx and te.get("operation") == "add":
                    cd = next((rq.get("buff") for rq in te.findall("requirement")
                               if rq.get("name") == "NotHasBuff"), None)
                    awards[mx.group(1)].append({
                        "file": rel(f), "manager": mgr, "group": eg.get("name", ""),
                        "trigger": te.get("trigger"),
                        "value_expr": te.get("value"), "value": resolve(te.get("value")),
                        "runtime": is_runtime(te.get("value")),
                        "cooldown_buff": cd,
                        "cooldown_s": durations.get(cd) if cd else None,
                        "cooldown_defined": (cd in durations) if cd else None,
                        "reqs": [x for x in reqs_of(te) if not x.startswith("HasBuff(buff=god")],
                    })
                    if mgr and k:
                        group_of[k] = (mgr, eg.get("name", ""))
                # the level-up buff this group hands off to, used to join the
                # XP cvar name (mod-private) to the real progression it grants
                if te.get("action") == "AddBuff" and k and \
                   (te.get("buff") or "").endswith("_LevelUpCheck"):
                    key_to_lubuff[k] = te.get("buff")
                mt = TONEXT.match(cv)
                if te.get("action") == "ModifyCVar" and mt and te.get("operation") == "set":
                    tonext_init[mt.group(1)] = resolve(te.get("value"))
                if is_log(te) and k:
                    msg = te.get("message") or ""
                    if "Level Up Check" in msg:
                        has_levelcheck_log[k] += 1
                    # the human label this progression uses in its own log lines.
                    # It is not derivable from the progression name - attFortitude
                    # writes "Fortitude Attribute", perkFlurryOfStrength still
                    # writes "Flurry of Blows" - so the log parser needs the real
                    # strings to map a line back to a progression.
                    lm = re.match(r"LBD DEBUG:\s*(.+?)\s+-\s+", msg)
                    if lm:
                        labels[k].add(lm.group(1))

            # Award-log pairing, done structurally rather than by message text.
            # The mod uses 55+ phrasings for its XP lines ("Attempting 'Craft' XP",
            # "General Use Synergy XP", "COMBO HIT! Applying Bonus XP"), so matching
            # on wording is worthless. A log line shadows an award when it shares the
            # trigger and its requirements are a SUBSET of the award's - the mod
            # deliberately leaves the cooldown check off the log line, which is why
            # those messages say "Attempting" rather than "granted".
            if k:
                logs, awds = [], []
                for te in eg.findall("triggered_effect"):
                    if is_log(te):
                        logs.append((te.get("trigger"), req_sig(te)))
                    elif te.get("action") == "ModifyCVar" \
                            and XP_CVAR.match(te.get("cvar") or "") \
                            and te.get("operation") == "add":
                        awds.append((te.get("trigger"), req_sig(te)))
                pool = list(logs)
                for trig, s in awds:
                    hit = next((i for i, (lt, ls) in enumerate(pool)
                                if lt == trig and ls <= s), None)
                    if hit is not None:
                        pool.pop(hit)
                        has_award_log[k] += 1

# ---------------------------------------------- level-up buffs & success paths
levelup_groups = {}      # progkey -> list of 1-based effect_group indexes that grant
success_logged, fail_logged = set(), set()
gates = defaultdict(list)
curve_of = {}          # progression-lower -> its own xptonext multiplier

for f, r in docs.items():
    for b in r.iter("buff"):
        name = b.get("name") or ""
        if not name.endswith("_LevelUpCheck"):
            continue
        target = None
        for te in b.iter("triggered_effect"):
            if te.get("action") == "AddProgressionLevel":
                target = te.get("progression_name")
                break
        if not target:
            continue
        key = target.lower()
        real_name[key] = target
        levelup_buff[key] = name
        # SUCCESS / FAILED labels live here, not in the manager group
        for te in b.iter("triggered_effect"):
            if is_log(te):
                lm = re.match(r"LBD DEBUG:\s*(.+?)\s+-\s+", te.get("message") or "")
                if lm:
                    levelup_labels[key].add(lm.group(1))
        # the curve is not global any more: crafting skills multiply by
        # $lbd_xp_crafting_curve_multiplier, everything else by the 1.2 default
        for te in b.iter("triggered_effect"):
            cv = te.get("cvar") or ""
            if te.get("action") == "ModifyCVar" and cv.endswith("_lbd_xptonext")                     and te.get("operation") == "multiply":
                c = resolve(te.get("value"))
                if c:
                    curve_of[key] = c
                break
        idxs = []
        for i, eg in enumerate(b.findall("effect_group"), start=1):
            tes = eg.findall("triggered_effect")
            grants = any(t.get("action") == "AddProgressionLevel"
                         and t.get("progression_name") == target for t in tes)
            logs = any(is_log(t) for t in tes)
            if grants:
                idxs.append(i)
                if logs:
                    success_logged.add(key)
                for rq in eg.findall("requirement"):
                    if rq.get("name") == "ProgressionLevel" and \
                       (rq.get("progression_name") or "").lower() != key:
                        gates[key].append(f"{rq.get('progression_name')} "
                                          f"{rq.get('operation')} {rq.get('value')}")
            elif logs:
                fail_logged.add(key)
        levelup_groups[key] = (name, idxs)

# ------------------------------------------------------------ vanilla maxlevel
maxlev = {}
maxlev_ci = {}          # lowercase -> the real vanilla casing
if os.path.exists(VANILLA):
    def walk(el, inherited):
        ml = el.get("max_level")
        here = int(ml) if ml is not None else inherited
        if el.get("name") and el.tag in ("attribute", "perk", "skill", "crafting_skill"):
            maxlev[el.get("name").lower()] = here
            maxlev_ci[el.get("name").lower()] = el.get("name")
        for c in el:
            walk(c, here)
    walk(ET.parse(VANILLA).getroot(), 1)
else:
    print(f"!! vanilla progression.xml not found at {VANILLA}\n"
          f"   pass --game <install path>; max levels will be unknown", file=sys.stderr)

# ------------------------- debug lines that can never print (ordering)
# Effects fire in document order. A log line gated on NotHasBuff <cooldown> that
# sits AFTER the AddBuff applying that cooldown is evaluated with the buff already
# on, so it never prints. The award still pays, because awards are ordered ahead of
# the cooldown - the progression works while looking dead in the log. Salvage
# Operations produced nothing across 19 wrench swings for exactly this reason.
suppressed_logs = []
for f, r in docs.items():
    for eg in r.iter("effect_group"):
        tes = eg.findall("triggered_effect")
        cd_at = {}
        for i, te in enumerate(tes):
            if te.get("action") == "AddBuff" and "CoolDown" in (te.get("buff") or ""):
                cd_at.setdefault((te.get("trigger"), te.get("buff")), i)
        for i, te in enumerate(tes):
            if not is_log(te):
                continue
            for q in te.findall("requirement"):
                if q.get("name") != "NotHasBuff":
                    continue
                buf = q.get("buff") or ""
                if "CoolDown" not in buf:
                    continue
                j = cd_at.get((te.get("trigger"), buf))
                if j is not None and j < i:
                    suppressed_logs.append((rel(f), eg.get("name", "?"),
                                            te.get("trigger"), buf))


# ------------------------------- SCore requirement / action names vs its classes
# XML names a SCore requirement by its EXACT class name ("RequirementIsTargetBlock,
# SCore"), but a SCore action by its class name minus the MinEventAction prefix
# ("ShowPerkLevelUp, SCore" -> MinEventActionShowPerkLevelUp). Get either wrong and
# the game does not complain: the requirement is dropped, so a gated effect fires
# unconditionally. That is how a repeat-craft check sat open through a whole play
# session while looking perfectly correct in the config.
SCORE_DIR = os.path.normpath(os.path.join(HERE, "..", "..", "0-SCore"))
score_classes = set()
if os.path.isdir(SCORE_DIR):
    _cls = re.compile(r"^\s*(?:public\s+)?(?:sealed\s+|abstract\s+)?class\s+([A-Za-z0-9_]+)", re.M)
    for _root, _, _names in os.walk(SCORE_DIR):
        if "obj" in _root or "bin" in _root:
            continue
        for _n in _names:
            if _n.endswith(".cs"):
                try:
                    score_classes.update(_cls.findall(
                        open(os.path.join(_root, _n), encoding="utf-8", errors="replace").read()))
                except OSError:
                    pass

bad_score_refs = defaultdict(list)
if score_classes:
    for f, r in docs.items():
        for el in r.iter():
            nm = el.get("name") if el.tag == "requirement" else None
            act = el.get("action") if el.tag == "triggered_effect" else None
            for raw, kind in ((nm, "requirement"), (act, "action")):
                if not raw or ", SCore" not in raw:
                    continue
                base = raw.split(",")[0].strip().lstrip("!")
                want = base if kind == "requirement" else "MinEventAction" + base
                if want not in score_classes:
                    bad_score_refs[(kind, raw.strip(), want)].append(rel(f))


# ------------------------------------------- vanilla blocks, with Extends resolved
# BlocksFromXml.CreateProperties inherits a parent block's properties down the
# Extends chain, so a block with no Tags of its own still carries its parent's.
# Reading the raw <property name="Tags"> is therefore wrong and invents gaps:
# shotgunTurret looks untagged but inherits trapsSkill from autoTurret. Harvest
# drops inherit the same way, which is what RequirementBlockHasHarvestTags reads.
VBLOCKS = os.path.join(args.game, "Data", "Config", "blocks.xml")
block_own, block_ext, block_drops_own = {}, {}, {}
if os.path.exists(VBLOCKS):
    for b in ET.parse(VBLOCKS).getroot().iter("block"):
        n = b.get("name")
        if not n:
            continue
        props = {}
        for pr in b.findall("property"):
            props[pr.get("name")] = pr.get("value")
            if pr.get("name") == "Extends":
                # param1 names the properties explicitly NOT inherited
                block_ext[n] = (pr.get("value"),
                                {x.strip() for x in (pr.get("param1") or "").split(",") if x.strip()})
        block_own[n] = props
        drops = defaultdict(list)
        for d in b.findall("drop"):
            drops[d.get("event")].append(d.get("tag") or "")
        block_drops_own[n] = dict(drops)

_tag_cache, _drop_cache = {}, {}

def block_tags(name, _seen=None):
    """Effective Tags of a block after following Extends."""
    if name in _tag_cache:
        return _tag_cache[name]
    _seen = _seen or set()
    if name in _seen or name not in block_own:
        return set()
    _seen.add(name)
    raw = block_own[name].get("Tags")
    if raw is None and name in block_ext:
        parent, excluded = block_ext[name]
        raw = ",".join(block_tags(parent, _seen)) if "Tags" not in excluded else None
    out = {t.strip() for t in (raw or "").split(",") if t.strip()}
    _tag_cache[name] = out
    return out

def block_harvest_tags(name, _seen=None):
    """Effective Harvest drop tags, which inherit unless the child redefines them."""
    if name in _drop_cache:
        return _drop_cache[name]
    _seen = _seen or set()
    if name in _seen or name not in block_own:
        return set()
    _seen.add(name)
    own = block_drops_own.get(name, {})
    if "Harvest" in own:
        vals = own["Harvest"]
    elif name in block_ext:
        parent, excluded = block_ext[name]
        vals = [] if "drop" in excluded else list(block_harvest_tags(parent, _seen))
    else:
        vals = []
    out = {t.strip() for v in vals for t in str(v).split(",") if t.strip()}
    _drop_cache[name] = out
    return out

# every block tag / harvest tag / block name the mod's requirements rely on
block_reqs = defaultdict(list)     # (kind, value) -> [files]
if block_own:
    for f, r in docs.items():
        for rq in r.iter("requirement"):
            nm = (rq.get("name") or "").split(",")[0].strip().lstrip("!")
            if nm in ("RequirementIsTargetBlock", "RequirementCraftedBlockHasTags") and rq.get("tags"):
                block_reqs[("tag", rq.get("tags"))].append(rel(f))
            elif nm == "RequirementBlockHasHarvestTags" and rq.get("tags"):
                block_reqs[("harvest", rq.get("tags"))].append(rel(f))
            elif nm == "BlockHasName" and rq.get("block_name"):
                block_reqs[("name", rq.get("block_name"))].append(rel(f))

def count_matches(kind, value):
    wanted = {v.strip() for v in value.split(",") if v.strip()}
    if kind == "tag":
        return sum(1 for n in block_own if block_tags(n) & wanted)
    if kind == "harvest":
        return sum(1 for n in block_own if block_harvest_tags(n) & wanted)
    pats = [re.compile("^" + re.escape(w).replace(r"\*", ".*") + "$", re.I) for w in wanted]
    return sum(1 for n in block_own if any(p.match(n) for p in pats))

dead_block_reqs = []
for (kind, value), files in sorted(block_reqs.items()):
    n = count_matches(kind, value)
    if n == 0:
        dead_block_reqs.append((kind, value, sorted(set(files)), len(files)))

CURVE = resolve("$lbd_xp_curve_multiplier") or 1.2

# -------------------------------------------------------------------- modeling
def fmt_time(s):
    if s is None:
        return "-"
    if s < 90:
        return f"{s:.0f}s"
    if s < 5400:
        return f"{s/60:.1f}m"
    if s < 172800:
        return f"{s/3600:.1f}h"
    return f"{s/86400:.0f}d"

def fmt_n(n):
    return f"{n:,}" if n is not None else "-"

def is_dead(a):
    """Award grants nothing: no constant to resolve AND not computed at runtime."""
    return a["value"] in (None, 0.0) and not a.get("runtime")

# XP cvar names are mod-private and need not match the progression they feed
# (e.g. $perkflurryofblows_lbd_xp now grants perkFlurryOfStrength). Resolve the
# real name through the level-up buff before looking anything up in vanilla.
buff_to_target = {b: real_name[k] for k, (b, _) in levelup_groups.items() if b}
buff_to_groups = {b: idxs for (b, idxs) in levelup_groups.values() if b}

model, rows = {}, []
for key in sorted(awards):
    aw = awards[key]
    prog = buff_to_target.get(key_to_lubuff.get(key), real_name.get(key, key))
    mx = maxlev.get(prog.lower())
    start = tonext_init.get(key)
    live = [a for a in aw if a["value"] not in (None, 0.0)]

    best = None
    if live:
        def rate(a):
            cd = a["cooldown_s"] if a["cooldown_s"] and a["cooldown_s"] > 0 else 1.0
            return a["value"] / cd
        best = max(live, key=rate)

    curve = curve_of.get(prog.lower(), CURVE)
    levels = []
    if best and mx and start:
        cd = best["cooldown_s"] if best["cooldown_s"] and best["cooldown_s"] > 0 else None
        thr = start
        for lvl in range(1, mx + 1):
            acts = math.ceil(thr / best["value"])
            levels.append({"level": lvl, "threshold": round(thr, 1), "actions": acts,
                           "floor_s": acts * cd if cd else None})
            thr *= curve

    model[key] = {
        "progression": prog, "max_level": mx, "xptonext_start": start, "curve": curve,
        # the hand-off AddBuff is not always in the same effect_group as the XP
        # adds, so fall back to the buff that grants this progression
        "awards": aw, "levelup_buff": key_to_lubuff.get(key) or levelup_buff.get(prog.lower()),
        "levelup_groups": buff_to_groups.get(
            key_to_lubuff.get(key) or levelup_buff.get(prog.lower()), []),
        "gates": sorted(set(gates.get(prog.lower(), []))),
        "levels": levels,
        # success/fail logging is recorded against the granted progression, which is
        # not always the XP cvar's name (perkflurryofblows feeds perkFlurryOfStrength)
        "obs": {"award_logs": has_award_log[key], "award_paths": len(aw),
                "levelcheck_log": has_levelcheck_log[key] > 0,
                "success_log": prog.lower() in success_logged,
                "fail_log": prog.lower() in fail_logged},
        "group": group_of.get(key),
        "labels": sorted(labels.get(key, set()) | levelup_labels.get(prog.lower(), set())),
    }
    rows.append(key)

json.dump({"constants": consts, "const_source": const_src, "curve": CURVE,
           "durations": durations, "progressions": model},
          open(os.path.join(args.out, "lbd_model.json"), "w", encoding="utf-8"), indent=1)

# ------------------------------------- every buff reference vs buff definitions
# Buff names are case sensitive. AddBuff against a name that does not resolve is
# a silent no-op; NotHasBuff against one is always true. Both fail quietly.
defined_buffs = set()
buff_refs = defaultdict(list)     # referenced name -> [(file, how)]

# vanilla buffs are legitimate targets (buffStatusHungry03, buffEncumberedInv, ...)
VBUFFS = os.path.join(args.game, "Data", "Config", "buffs.xml")
vanilla_buffs = set()
if os.path.exists(VBUFFS):
    for b in ET.parse(VBUFFS).getroot().iter("buff"):
        if b.get("name"):
            vanilla_buffs.add(b.get("name"))

for f, r in docs.items():
    for b in r.iter("buff"):
        if b.get("name"):
            defined_buffs.add(b.get("name"))
    for te in r.iter("triggered_effect"):
        if te.get("action") == "AddBuff" and te.get("buff"):
            buff_refs[te.get("buff")].append((rel(f), "AddBuff"))
    for rq in r.iter("requirement"):
        if rq.get("name") in ("HasBuff", "NotHasBuff") and rq.get("buff"):
            buff_refs[rq.get("buff")].append((rel(f), rq.get("name")))

known_buffs = defined_buffs | vanilla_buffs
buffs_ci = {b.lower(): b for b in known_buffs}
bad_buffs = {}                    # name -> (verdict, sites)
for name, sites in buff_refs.items():
    if name in known_buffs or name == "god":
        continue
    hit = buffs_ci.get(name.lower())
    bad_buffs[name] = (f"case mismatch -> {hit}" if hit else "not defined anywhere",
                       sites)

# ------------------------------- every progression_name reference vs vanilla
# Covers level-up, decay and requirement sites alike. A name the game does not
# know is silently inert: AddProgressionLevel does nothing, ProgressionLevel
# reads 0.
badrefs = defaultdict(list)     # unknown name -> [(file, site)]
caserefs = defaultdict(list)    # name whose casing differs from vanilla
if maxlev:
    for f, r in docs.items():
        for el in r.iter():
            pn = el.get("progression_name")
            if not pn:
                continue
            site = f"{el.tag}/{el.get('action') or el.get('name') or ''}"
            if pn.lower() not in maxlev:
                badrefs[pn].append((rel(f), site))
            elif maxlev_ci[pn.lower()] != pn:
                caserefs[f"{pn} -> vanilla is {maxlev_ci[pn.lower()]}"].append((rel(f), site))

# ------------------------------------------------------------------ FINDINGS
dead = [(k, a) for k in rows for a in model[k]["awards"] if is_dead(a)]
nothrottle = sorted({(k, a["cooldown_buff"], a["file"]) for k in rows
                     for a in model[k]["awards"]
                     if a["cooldown_buff"] and not a["cooldown_defined"]})
nomax = [k for k in rows if not model[k]["max_level"]]
noxp = [k for k in rows if all(is_dead(a) for a in model[k]["awards"])]

F = []
F.append("# LBD static audit - findings\n")
F.append(f"_Generated by `Testing/lbd_audit.py`. "
         f"{len(docs)} XML files, {len(consts)} tuning cvars, "
         f"{len(rows)} progressions, {sum(len(model[k]['awards']) for k in rows)} award paths._\n")

if parse_errors:
    F.append("## XML that failed to parse\n")
    for p, e in parse_errors:
        F.append(f"- `{rel(p)}` - {e}")
    F.append("")

F.append("## 1. Award paths that can never pay out\n")
F.append("The value expression resolves to no cvar, and an unset cvar reads as 0, "
         "so the trigger fires and grants nothing.\n")
F.append("| progression | trigger | value | file |")
F.append("|---|---|---|---|")
for k, a in sorted(dead, key=lambda x: (x[0], x[1]["trigger"])):
    F.append(f"| `{model[k]['progression']}` | `{a['trigger']}` | `{a['value_expr']}` | `{a['file']}` |")

F.append("\n## 2. Progressions with no working XP path at all\n")
if noxp:
    for k in noxp:
        F.append(f"- `{model[k]['progression']}` - every award path resolves to 0")
else:
    F.append("_(none)_")

F.append("\n## 3. Progression names with no vanilla definition\n")
if nomax:
    for k in nomax:
        F.append(f"- `{model[k]['progression']}` - not present in vanilla `progression.xml`; "
                 f"`AddProgressionLevel` cannot resolve it")
else:
    F.append("_(none)_")

F.append("\n## 3b. progression_name references the game does not know\n")
F.append("Checked against vanilla `progression.xml` across every site - level-up, decay "
         "and requirements. An unknown name is inert: `AddProgressionLevel` does nothing "
         "and `ProgressionLevel` reads 0.\n")
if badrefs:
    F.append("| referenced name | sites | files |")
    F.append("|---|--:|---|")
    for nm, hits in sorted(badrefs.items()):
        fl = ", ".join(sorted({h[0] for h in hits}))
        F.append(f"| `{nm}` | {len(hits)} | `{fl}` |")
else:
    F.append("_(none)_")

F.append("\n## 3c. progression_name casing that differs from vanilla\n")
F.append("Lower severity - inert only if the game's progression lookup is "
         "case-sensitive. Worth normalising either way.\n")
if caserefs:
    F.append("| referenced name | sites | files |")
    F.append("|---|--:|---|")
    for nm, hits in sorted(caserefs.items()):
        fl = ", ".join(sorted({h[0] for h in hits}))
        F.append(f"| `{nm}` | {len(hits)} | `{fl}` |")
else:
    F.append("_(none)_")

F.append("\n## 4. Throttle buffs referenced but never defined\n")
F.append("`NotHasBuff` against a buff that does not exist is always true, so these "
         "award paths have no cooldown and score on every trigger.\n")
if nothrottle:
    F.append("| progression | missing buff | file |")
    F.append("|---|---|---|")
    for k, b, f_ in nothrottle:
        F.append(f"| `{model[k]['progression']}` | `{b}` | `{f_}` |")
else:
    F.append("_(none)_")

F.append("\n## 4b. Buff references that do not resolve\n")
F.append("Buff names are case sensitive. `AddBuff` against an unresolvable name is a "
         "silent no-op - a `_LevelUpCheck` listed here means that progression can never "
         "level. `NotHasBuff` against one is always true, removing the throttle.\n")
if bad_buffs:
    F.append("| referenced buff | verdict | used as | file |")
    F.append("|---|---|---|---|")
    for nm, (verdict, sites) in sorted(bad_buffs.items()):
        how = ", ".join(sorted({s[1] for s in sites}))
        fl = ", ".join(sorted({s[0] for s in sites}))
        F.append(f"| `{nm}` | {verdict} | {how} | `{fl}` |")
else:
    F.append("_(none)_")

F.append("\n## 4c. Block requirements that match no block\n")
F.append("Tags and Harvest drops are resolved through the Extends chain, the way "
         "BlocksFromXml inherits them - a block with no Tags of its own carries its "
         "parent's. A requirement matching nothing is a silently dead path.\n")
if not block_own:
    F.append("_(vanilla blocks.xml not found - check skipped)_")
elif dead_block_reqs:
    F.append("| kind | value | uses | files |")
    F.append("|---|---|--:|---|")
    for kind, value, files, n in dead_block_reqs:
        F.append(f"| {kind} | `{value}` | {n} | `{', '.join(files)}` |")
else:
    F.append(f"_(none - all {len(block_reqs)} distinct block requirements match at least one block)_")

F.append("\n## 4d. SCore requirement and action names that resolve to no class\n")
F.append("A requirement names its class exactly; an action names its class without the "
         "MinEventAction prefix. An unresolvable name is dropped silently - the effect "
         "still runs, just without that condition.\n")
if not score_classes:
    F.append("_(SCore source not found - check skipped)_")
elif bad_score_refs:
    F.append("| kind | written as | expected class | uses | files |")
    F.append("|---|---|---|--:|---|")
    for (kind, raw, want), files in sorted(bad_score_refs.items()):
        F.append(f"| {kind} | `{raw}` | `{want}` | {len(files)} | `{', '.join(sorted(set(files)))}` |")
else:
    F.append("_(none - every SCore requirement and action resolves)_")


F.append("\n## 4e. Debug lines that can never print\n")
F.append("A log line gated on NotHasBuff of a cooldown must be ordered BEFORE the "
         "AddBuff that applies it. Where it is not, the award still pays but the line "
         "is silently suppressed, so the progression looks dead in the log.\n")
if suppressed_logs:
    F.append("| file | group | trigger | cooldown |")
    F.append("|---|---|---|---|")
    for fl, grp, trig, buf in sorted(set(suppressed_logs)):
        F.append(f"| `{fl}` | {grp} | `{trig}` | `{buf}` |")
else:
    F.append("_(none)_")


F.append("\n## 5. Runtime observability gaps\n")
F.append("All debug output is gated behind `HasBuff god`, so `buff god` enables the whole stream. "
         "These progressions still cannot be fully followed in the log:\n")
F.append("| progression | silent award paths | logs level-up check | logs level-up success |")
F.append("|---|---|---|---|")
for k in rows:
    o = model[k]["obs"]
    silent = o["award_paths"] - o["award_logs"]
    if silent > 0 or not o["levelcheck_log"] or not o["success_log"]:
        F.append(f"| `{model[k]['progression']}` | {silent}/{o['award_paths']} | "
                 f"{'yes' if o['levelcheck_log'] else 'NO'} | "
                 f"{'yes' if o['success_log'] else 'NO'} |")
F.append("\nInstall the generated `SphereII Learn By Doing Test Harness` modlet to close these gaps.\n")

open(os.path.join(args.out, "FINDINGS.md"), "w", encoding="utf-8").write("\n".join(F))

# ------------------------------------------------------------------ TESTPLAN
T = []
T.append("# LBD test plan\n")
T.append("_Generated by `Testing/lbd_audit.py` - do not hand-edit._\n")
T.append("## How to run\n")
T.append("1. New character (the Init buff only runs when `$lbd_xp_melee_base` is 0, "
         "so retuned values need a fresh character, or a Grandpa's Forgettin' Elixir).\n"
         "2. Console: `dm`, then `buff god`. Every LBD debug line is gated behind the "
         "`god` buff - without it the log stays silent.\n"
         "3. Install the `SphereII Learn By Doing Test Harness` modlet so level-ups and "
         "the silent award paths reach the log.\n"
         "4. Work the checklist below. The perk window draws an LBD progress bar under "
         "each perk, attribute and skill row - that is the live visual confirmation.\n"
         "5. Feed the log to `lbd_logparse.py` for coverage and real per-level timings.\n")
T.append("`setcvar $x 0` **deletes** the cvar rather than zeroing it (SCore treats 0 as "
         "remove). An unset cvar reads as 0, so it works for a reset, but it will not "
         "appear in any dump until something writes it again.\n")

T.append("## Expected cost per progression\n")
T.append("`actions` counts the fastest award path only. `floor` is `actions x cooldown` - "
         "the shortest wall-clock time the XP could physically be earned in. Real play is "
         "slower; beating the floor means the throttle is not working.\n")
T.append("| progression | max | paths | fastest trigger | xp | cd | actions to max | floor |")
T.append("|---|--:|--:|---|--:|--:|--:|--:|")
for k in rows:
    m = model[k]
    if not m["levels"]:
        T.append(f"| `{m['progression']}` | {m['max_level'] or '?'} | {len(m['awards'])} | "
                 f"**no working path** | - | - | - | - |")
        continue
    live = [a for a in m["awards"] if a["value"] not in (None, 0.0)]
    b = max(live, key=lambda a: a["value"] / (a["cooldown_s"] or 1.0))
    ta = sum(l["actions"] for l in m["levels"])
    tf = sum(l["floor_s"] for l in m["levels"]) if all(l["floor_s"] is not None for l in m["levels"]) else None
    cdtxt = f"{b['cooldown_s']:g}s" if b["cooldown_s"] else ("none" if b["cooldown_buff"] else "-")
    T.append(f"| `{m['progression']}` | {m['max_level']} | {len(live)} | `{b['trigger']}` | "
             f"{b['value']:g} | {cdtxt} | {fmt_n(ta)} | {fmt_time(tf)} |")

T.append("\n## Per-level thresholds\n")
T.append("Threshold grows by the global curve multiplier "
         f"`$lbd_xp_curve_multiplier` = {CURVE:g} per level.\n")
for k in rows:
    m = model[k]
    if not m["levels"]:
        continue
    T.append(f"<details><summary><code>{m['progression']}</code> - "
             f"{len(m['levels'])} levels</summary>\n")
    T.append("| level | threshold | actions | floor |")
    T.append("|--:|--:|--:|--:|")
    for l in m["levels"]:
        T.append(f"| {l['level']} | {l['threshold']:,.0f} | {fmt_n(l['actions'])} | "
                 f"{fmt_time(l['floor_s'])} |")
    T.append("\n</details>\n")

T.append("\n## Trigger checklist\n")
T.append("Every XP award path in the mod. Fire each one and confirm the bar moves.\n")
cur_file = None
for k in rows:
    m = model[k]
    T.append(f"\n### `{m['progression']}`  \n")
    meta = [f"max **{m['max_level'] or '?'}**",
            f"first threshold **{m['xptonext_start']:g}**" if m["xptonext_start"] else ""]
    if m["gates"]:
        meta.append("gated on " + ", ".join(f"`{g}`" for g in m["gates"]))
    T.append(" - ".join(x for x in meta if x) + "\n")
    T.append("| # | trigger | pays | throttle | conditions to satisfy |")
    T.append("|--:|---|--:|---|---|")
    for i, a in enumerate(m["awards"], 1):
        pays = ("runtime `" + str(a["value_expr"]) + "`") if a.get("runtime") else (f"{a['value']:g}" if a["value"] not in (None, 0.0) else f"**0** (`{a['value_expr']}`)")
        if a["cooldown_buff"]:
            thr = f"{a['cooldown_s']:g}s" if a["cooldown_defined"] else "**missing buff**"
        else:
            thr = "none"
        cond = "<br>".join(f"`{c}`" for c in a["reqs"]) or "_none_"
        T.append(f"| {i} | `{a['trigger']}` | {pays} | {thr} | {cond} |")

open(os.path.join(args.out, "TESTPLAN.md"), "w", encoding="utf-8").write("\n".join(T))

# -------------------------------------------------------------------- console
print(f"parsed {len(docs)} xml files, {len(consts)} tuning cvars, "
      f"{len(rows)} progressions, {sum(len(model[k]['awards']) for k in rows)} award paths")
print(f"  dead award paths ........ {len(dead)}")
print(f"  no working XP path ...... {len(noxp)}  {[model[k]['progression'] for k in noxp]}")
print(f"  unknown progression ..... {len(nomax)}  {[model[k]['progression'] for k in nomax]}")
print(f"  missing throttle buffs .. {len(nothrottle)}")
print(f"  no success log .......... {sum(1 for k in rows if not model[k]['obs']['success_log'])}/{len(rows)}")
print(f"  unknown progression_name  {len(badrefs)}  {sorted(badrefs)}")
print(f"  case-mismatched names ... {len(caserefs)}  {sorted(caserefs)}")
print(f"  unresolvable buff refs .. {len(bad_buffs)}")
print(f"  bad SCore class refs .... {len(bad_score_refs)}" + (f"  {sorted({k[1] for k in bad_score_refs})}" if bad_score_refs else ""))
print(f"  dead block requirements . {len(dead_block_reqs)}" + (f"  {[d[1] for d in dead_block_reqs]}" if dead_block_reqs else ""))
print(f"  suppressed debug lines .. {len(set(suppressed_logs))}")
print(f"wrote TESTPLAN.md, FINDINGS.md, lbd_model.json -> {args.out}")
