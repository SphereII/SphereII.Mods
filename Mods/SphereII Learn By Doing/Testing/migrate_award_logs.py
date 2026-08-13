#!/usr/bin/env python3
"""
One-shot migration, kept for the record.

Adds the missing `Attempting XP` debug line to the 11 progressions that logged
nothing at all when they awarded XP - all five attributes and the six flurry /
healing perks, 46 award paths between them. Without these a session could not
show which triggers were feeding an attribute, only that it eventually levelled.

Scope is deliberately limited to progressions whose award-log count is ZERO, so
there is no question of which existing line belongs to which award. The three
partially-logged perks (perkMediumArmor, perkSlowMetabolism, perkTurrets) are
done by hand.

Each new line carries the same <requirement> set as the award it shadows plus
the $lbd_debug gate, so a line in the log means XP was really granted and not
just that the trigger fired. The trigger is appended in brackets so
lbd_logparse.py can report per-trigger coverage.

Text-level edits: ElementTree would drop comments and reflow every file.
"""
import os, re, json, html

HERE = os.path.dirname(os.path.abspath(__file__))
BASE = os.path.normpath(os.path.join(HERE, "..", "Config"))

GATE = '<requirement name="CVarCompare" cvar="$lbd_debug" operation="GTE" value="1"/>'
XP_ADD = re.compile(r'action="ModifyCVar"\s+cvar="\$([a-z0-9_]+)_lbd_xp"\s+operation="add"\s+value="([^"]*)"')
TE_OPEN = re.compile(r"<triggered_effect\b")

model = json.load(open(os.path.join(HERE, "lbd_model.json"), encoding="utf-8"))
P = model["progressions"]
TARGETS = {k: m for k, m in P.items()
           if m["obs"]["award_logs"] == 0 and m["obs"]["award_paths"] > 0}


def label_for(key):
    prog = P[key]["progression"]
    if key.startswith("att"):
        return re.sub(r"^att", "", prog) + " Attribute"
    n = re.sub(r"^(perk|crafting)", "", prog)
    return re.sub(r"(?<!^)(?=[A-Z])", " ", n).strip()


COMMENT = re.compile(r"<!--.*?-->", re.S)


def elements(text):
    """(start, end, body) for every live <triggered_effect>, in document order.

    Commented-out blocks are skipped: HealingFactor.xml keeps two disabled awards
    inside <!-- --> and instrumenting those would put dead lines in the file.
    """
    holes = [(m.start(), m.end()) for m in COMMENT.finditer(text)]

    def commented(pos):
        return any(a <= pos < b for a, b in holes)

    for m in TE_OPEN.finditer(text):
        if commented(m.start()):
            continue
        i, j, q = m.start(), m.end(), None
        while j < len(text):
            c = text[j]
            if q:
                if c == q:
                    q = None
            elif c in "\"'":
                q = c
            elif c == ">":
                break
            j += 1
        if j >= len(text):
            continue
        if text[j - 1] == "/":
            yield i, j + 1, text[i:j + 1]
        else:
            close = text.find("</triggered_effect>", j)
            if close != -1:
                yield i, close + len("</triggered_effect>"), text[i:close + len("</triggered_effect>")]


added, touched, skipped = 0, {}, []

for root, _, names in os.walk(BASE):
    for n in sorted(names):
        if not n.lower().endswith(".xml"):
            continue
        p = os.path.join(root, n)
        src = open(p, encoding="utf-8").read()
        inserts = []

        for s, e, body in elements(src):
            m = XP_ADD.search(body)
            if not m:
                continue
            key, value = m.group(1), m.group(2)
            if key not in TARGETS:
                continue
            tm = re.search(r'trigger="([^"]+)"', body)
            if not tm:
                continue
            trigger = tm.group(1)

            line_start = src.rfind("\n", 0, s) + 1
            indent = src[line_start:s]
            if indent.strip():
                skipped.append((os.path.relpath(p, BASE), key, "award not at line start"))
                continue

            reqs = [r for r in re.findall(r"<requirement\b[^>]*/>", body)
                    if "$lbd_debug" not in r]
            msg = html.escape(f"LBD DEBUG: {label_for(key)} - Attempting XP "
                              f"(+{value}) [{trigger}]", quote=True)

            first = f'<triggered_effect trigger="{trigger}" action="LogMessageCVars, SCore" message="{msg}">'
            rest = [f"    {r}" for r in reqs] + [f"    {GATE}", "</triggered_effect>"]
            block = first + "\n" + "\n".join(indent + r for r in rest) + "\n" + indent
            inserts.append((s, block))
            added += 1

        if inserts:
            out = src
            for pos, txt in reversed(inserts):
                out = out[:pos] + txt + out[pos:]
            open(p, "w", encoding="utf-8").write(out)
            touched[os.path.relpath(p, BASE).replace("\\", "/")] = len(inserts)

for f, c in sorted(touched.items()):
    print(f"   {f}: +{c}")
print(f"\nadded {added} award log lines across {len(touched)} files")
print(f"targets: {len(TARGETS)} progressions, "
      f"{sum(m['obs']['award_paths'] for m in TARGETS.values())} award paths")
if skipped:
    print("SKIPPED:", skipped)
