#!/usr/bin/env python3
"""
One-shot migration, kept for the record.

Effects in an effect_group fire in document order. A debug line gated on
NotHasBuff <cooldown> that sits AFTER the AddBuff applying that cooldown can
never print: by the time it is evaluated the buff it tests for is already on.

The award still pays - awards are correctly ordered ahead of the cooldown - so
the progression works while looking completely dead in the log. That is why
Salvage Operations produced nothing across 19 wrench swings in a diagnostic run
even though its requirements are satisfied, and why every block dependent path
looked broken.

Moves each such log line to immediately before the cooldown that blocks it,
which keeps it after its award and inside the same requirement window.

Usage:  python migrate_log_order.py [--apply]
"""
import os, re, argparse

HERE = os.path.dirname(os.path.abspath(__file__))
BASE = os.path.normpath(os.path.join(HERE, "..", "Config"))

ap = argparse.ArgumentParser()
ap.add_argument("--apply", action="store_true")
args = ap.parse_args()

TE_OPEN = re.compile(r"<triggered_effect\b")
ATTR = re.compile(r'(\w+)="([^"]*)"')
NOTHAS = re.compile(r'<requirement name="NotHasBuff" buff="([^"]*CoolDown[^"]*)"')


def spans(text):
    """(start, end) of every <triggered_effect>, quote aware."""
    for m in TE_OPEN.finditer(text):
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
            yield i, j + 1
        else:
            k = text.find("</triggered_effect>", j)
            if k != -1:
                yield i, k + len("</triggered_effect>")


def is_log(a):
    return a.get("action", "").split(",")[0].strip() in ("LogMessage", "LogMessageCVars")


def process(text):
    """Reorder within each contiguous run of triggered_effects. Returns (text, moves)."""
    items = [(a, b) for a, b in spans(text)]
    if not items:
        return text, 0

    # group effects that share a parent by walking runs separated by </effect_group>
    runs, cur = [], []
    for idx, (a, b) in enumerate(items):
        if cur and "</effect_group>" in text[items[idx - 1][1]:a]:
            runs.append(cur)
            cur = []
        cur.append((a, b))
    if cur:
        runs.append(cur)

    moves = 0
    out = []
    pos = 0
    for run in runs:
        bodies = [text[a:b] for a, b in run]
        attrs = [dict(ATTR.findall(x.split(">", 1)[0])) for x in bodies]

        cd_at = {}
        for i, at in enumerate(attrs):
            if at.get("action") == "AddBuff" and "CoolDown" in at.get("buff", ""):
                cd_at.setdefault((at.get("trigger"), at.get("buff")), i)

        order = list(range(len(bodies)))
        for i, at in enumerate(attrs):
            if not is_log(at):
                continue
            tgt = None
            for buf in NOTHAS.findall(bodies[i]):
                j = cd_at.get((at.get("trigger"), buf))
                if j is not None and j < i:
                    tgt = j if tgt is None else min(tgt, j)
            if tgt is None:
                continue
            order.remove(i)
            order.insert(order.index(tgt), i)
            moves += 1

        if order != list(range(len(bodies))):
            # rewrite the run in the new order, reusing the original separators
            seps = [text[run[k][1]:run[k + 1][0]] for k in range(len(run) - 1)]
            out.append(text[pos:run[0][0]])
            for n, k in enumerate(order):
                out.append(bodies[k])
                if n < len(order) - 1:
                    out.append(seps[n])
            pos = run[-1][1]
    out.append(text[pos:])
    return "".join(out), moves


total, touched = 0, {}
for root, _, names in os.walk(BASE):
    for n in sorted(names):
        if not n.lower().endswith(".xml"):
            continue
        p = os.path.join(root, n)
        src = open(p, encoding="utf-8").read()
        new, moves = process(src)
        if moves:
            total += moves
            touched[os.path.relpath(p, BASE).replace("\\", "/")] = moves
            if args.apply:
                open(p, "w", encoding="utf-8").write(new)

for f, c in sorted(touched.items()):
    print(f"  {f:44} {c}")
print(f"\n{'APPLIED' if args.apply else 'DRY RUN'}: {total} log lines across {len(touched)} files")
