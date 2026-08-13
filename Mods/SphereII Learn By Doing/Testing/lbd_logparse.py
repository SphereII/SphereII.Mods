#!/usr/bin/env python3
"""
Turns a 7 Days to Die Player.log into an LBD coverage and timing report.

Reads the mod's own `LBD DEBUG:` output, which is gated on $lbd_debug:

    setcvar $lbd_debug 1        turn logging on
    setcvar $lbd_debug 0        turn it off

The lines that matter here:

  LBD DEBUG: <name> - ... XP (+n)                 an award fired
  LBD DEBUG: <name> - Triggering Level Up Check   xp crossed the threshold
  LBD DEBUG: <name> - Level Up SUCCESS (<prog>)   the level landed
  LBD DEBUG: <name> - Level Up FAILED. ...        the grant was refused

Reports:
  * which of the model's progressions gained XP, and which never fired
  * per-level wall-clock time, measured between LEVELUP lines
  * measured time vs the cooldown floor from lbd_audit.py - beating the floor
    means a throttle is not doing its job
  * triggers that never appeared at all, as a to-do list for the next session

Usage:
  python lbd_logparse.py <Player.log> [--model lbd_model.json] [--md report.md]

Log location (Windows):
  %APPDATA%\\..\\LocalLow\\The Fun Pimps\\7 Days To Die\\Player.log
"""
import os, re, sys, json, argparse
from collections import defaultdict, OrderedDict

HERE = os.path.dirname(os.path.abspath(__file__))

ap = argparse.ArgumentParser()
ap.add_argument("log")
ap.add_argument("--model", default=os.path.join(HERE, "lbd_model.json"))
ap.add_argument("--md", default=None, help="also write a markdown report here")
args = ap.parse_args()

M = json.load(open(args.model, encoding="utf-8"))
P = M["progressions"]

# 7DTD log lines start:  2024-05-01T12:34:56 1234.567 INF <msg>
TS = re.compile(r"^\s*\S+\s+(\d+\.\d+)\s+\w+\s+(.*)$")
# SUCCESS carries the progression name verbatim, so it needs no name guessing
DBG_SUCCESS = re.compile(r"LBD DEBUG:\s+(.+?)\s+-\s+Level Up SUCCESS\s+\(([A-Za-z0-9_]+)\)")
DBG_LEVELCHK = re.compile(r"LBD DEBUG:\s+(.+?)\s+-\s+Triggering Level Up Check")
DBG_FAIL = re.compile(r"LBD DEBUG:\s+(.+?)\s+-\s+Level Up FAILED\.\s*(.*)")
# award lines have 55+ phrasings; the payload marker is what they share
DBG_AWARD = re.compile(r"LBD DEBUG:\s+(.+?)\s+-\s+.*?XP.*?\(\+")
# lines the migration added carry the trigger in brackets
DBG_TRIGGER = re.compile(r"\[([A-Za-z]+)\]\s*$")

# display-name -> model key, so DEBUG lines ("Lucky Looter") can be matched too
def spaced(name):
    n = re.sub(r"^(perk|att|crafting)", "", name)
    return re.sub(r"(?<!^)(?=[A-Z])", " ", n).lower().strip()

by_display, by_prog = {}, {}
for k, m in P.items():
    by_display[spaced(m["progression"])] = k
    by_display[m["progression"].lower()] = k
    by_prog[m["progression"].lower()] = k
    # the labels the mod actually prints, harvested from the XML by lbd_audit.py.
    # They are not derivable: attFortitude writes "Fortitude Attribute", and
    # perkFlurryOfStrength still writes "Flurry of Blows".
    for lbl in m.get("labels", []):
        by_display.setdefault(lbl.lower(), k)

def to_key(label):
    l = label.strip().lower()
    if l in by_display:
        return by_display[l]
    l2 = l.replace("crafting ", "").strip()
    return by_display.get(l2)

awards_seen = defaultdict(lambda: defaultdict(int))   # key -> trigger -> count
levelups = defaultdict(list)                          # key -> [t, ...]
levelchecks = defaultdict(list)
failures = defaultdict(lambda: defaultdict(int))      # key -> reason -> count
unmatched = defaultdict(int)
first_t, last_t = None, None
lines = 0

with open(args.log, encoding="utf-8", errors="replace") as fh:
    for raw in fh:
        if "LBD " not in raw:
            continue
        lines += 1
        m = TS.match(raw)
        t, msg = (float(m.group(1)), m.group(2)) if m else (None, raw.strip())
        if t is not None:
            first_t = t if first_t is None else min(first_t, t)
            last_t = t if last_t is None else max(last_t, t)

        mm = DBG_SUCCESS.search(msg)
        if mm:
            k = by_prog.get(mm.group(2).lower()) or to_key(mm.group(1))
            if k:
                levelups[k].append(t)
            else:
                unmatched[mm.group(2)] += 1
            continue
        mm = DBG_LEVELCHK.search(msg)
        if mm:
            k = to_key(mm.group(1))
            if k:
                levelchecks[k].append(t)
            continue
        mm = DBG_FAIL.search(msg)
        if mm:
            k = to_key(mm.group(1))
            if k:
                failures[k][mm.group(2).strip() or "locked"] += 1
            continue
        mm = DBG_AWARD.search(msg)
        if mm:
            k = to_key(mm.group(1))
            if k:
                tm = DBG_TRIGGER.search(msg)
                awards_seen[k][tm.group(1) if tm else "(untagged)"] += 1
            else:
                unmatched[mm.group(1)] += 1

# ------------------------------------------------------------------- reporting
def fmt(s):
    if s is None:
        return "-"
    if s < 90:
        return f"{s:.0f}s"
    if s < 5400:
        return f"{s/60:.1f}m"
    return f"{s/3600:.2f}h"

out = []
def emit(s=""):
    out.append(s)
    print(s)

session = (last_t - first_t) if (first_t is not None and last_t is not None) else None
emit(f"# LBD session report\n")
emit(f"log: `{args.log}`")
emit(f"LBD lines: {lines}   session span: {fmt(session)}\n")

touched = sorted(set(awards_seen) | set(levelups) | set(levelchecks))
untouched = sorted(set(P) - set(touched))

emit(f"## Coverage: {len(touched)}/{len(P)} progressions saw activity\n")

emit(f"### Progressions that never fired ({len(untouched)})\n")
if untouched:
    # grouped by tree so the next session's to-do list reads by area
    bytree = defaultdict(list)
    for k in untouched:
        f0 = P[k]["awards"][0]["file"] if P[k]["awards"] else "?"
        bytree[f0.split("/")[0]].append(P[k]["progression"])
    for tree, names in sorted(bytree.items()):
        emit(f"- **{tree}** ({len(names)}): " + ", ".join(f"`{n}`" for n in sorted(names)))
else:
    emit("_(all covered)_")
emit("")

emit("### Levelling observed\n")
emit("| progression | levels | measured per level | floor (expected min) | verdict |")
emit("|---|--:|---|---|---|")
for k in touched:
    m = P[k]
    ts = [t for t in levelups.get(k, []) if t is not None]
    if len(ts) < 1:
        continue
    deltas = [b - a for a, b in zip(ts, ts[1:])]
    shown = ", ".join(fmt(d) for d in deltas[:8]) or "(first level only)"
    floors = [l["floor_s"] for l in m["levels"][1:1 + len(deltas)]]
    verdict = ""
    if deltas and all(f is not None for f in floors) and floors:
        faster = [i for i, (d, f) in enumerate(zip(deltas, floors), start=2) if d < f * 0.98]
        verdict = f"**faster than floor at level {faster[0]}**" if faster else "ok"
    fl = ", ".join(fmt(f) for f in floors[:8]) if floors else "-"
    emit(f"| `{m['progression']}` | {len(ts)} | {shown} | {fl} | {verdict} |")
emit("")

emit("### XP activity by trigger\n")
emit("| progression | triggers seen | distinct triggers | never fired |")
emit("|---|---|--:|---|")
for k in touched:
    m = P[k]
    seen = {t for t in awards_seen.get(k, {}) if t != "(untagged)"}
    expected = {a["trigger"] for a in m["awards"]}
    missing = sorted(expected - seen)
    emit(f"| `{m['progression']}` | {', '.join(sorted(seen)) or '(untagged only)'} | "
         f"{len(expected)} | {', '.join(f'`{x}`' for x in missing) or '-'} |")
emit("")

if failures:
    emit("### Level-up failures reported\n")
    emit("| progression | reason | count |")
    emit("|---|---|--:|")
    for k, d in sorted(failures.items()):
        for reason, c in sorted(d.items(), key=lambda x: -x[1]):
            emit(f"| `{P[k]['progression']}` | {reason} | {c} |")
    emit("")

if unmatched:
    emit("### Log labels not matched to the model\n")
    for lbl, c in sorted(unmatched.items(), key=lambda x: -x[1]):
        emit(f"- `{lbl}` x{c}")
    emit("")

if args.md:
    open(args.md, "w", encoding="utf-8").write("\n".join(out))
    print(f"\nwrote {args.md}")
