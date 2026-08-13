#!/usr/bin/env python3
"""
One-shot migration, kept for the record.

1. Replaces the `HasBuff god` gate on every LBD debug line with a CVarCompare on
   $lbd_debug. The god buff carries PhysicalDamageResist/ElementalDamageResist
   +200 and CarryCapacity base_set 45, so switching logging on used to make the
   player invulnerable - which meant perkLightArmor, perkHeavyArmor,
   perkHardTarget and perkPainTolerance, whose only XP comes from taking damage,
   could never be observed while they were being exercised.

2. Adds a `Level Up SUCCESS` line to every granting effect_group in every
   *_LevelUpCheck buff. Only the failure branches logged before, so a level that
   landed and a level that silently did nothing produced identical logs.

Text-level edits on purpose: round-tripping through ElementTree would discard
the comments and reflow every file.
"""
import os, re, sys

HERE = os.path.dirname(os.path.abspath(__file__))
BASE = os.path.normpath(os.path.join(HERE, "..", "Config"))

GOD_REQ = '<requirement name="HasBuff" buff="god"/>'
CVAR_REQ = '<requirement name="CVarCompare" cvar="$lbd_debug" operation="GTE" value="1"/>'

LU_BUFF = re.compile(r'<buff name="([^"]*_LevelUpCheck)"[^>]*>(.*?)</buff>', re.S)
ADD_PROG = re.compile(r'^([ \t]*)<triggered_effect[^>]*action="AddProgressionLevel"', re.M)
FAIL_NAME = re.compile(r'LBD DEBUG:\s*(.+?)\s*-\s*Level Up FAILED')
PROG_NAME = re.compile(r'action="AddProgressionLevel"\s+progression_name="([^"]+)"'
                       r'|progression_name="([^"]+)"\s+level="1"')


def display_name(block, fallback):
    """Human name this buff already uses in its own debug lines."""
    m = FAIL_NAME.search(block)
    if m:
        return m.group(1)
    n = re.sub(r"^(perk|att|crafting)", "", fallback)
    return re.sub(r"(?<!^)(?=[A-Z])", " ", n).strip()


def progression_of(block):
    m = re.search(r'action="AddProgressionLevel"\s+progression_name="([^"]+)"', block)
    if m:
        return m.group(1)
    m = re.search(r'progression_name="([^"]+)"[^>]*level="1"', block)
    return m.group(1) if m else None


gate_hits = 0
success_added = 0
touched = {}

for root, _, names in os.walk(BASE):
    for n in sorted(names):
        if not n.lower().endswith(".xml"):
            continue
        p = os.path.join(root, n)
        src = open(p, encoding="utf-8").read()
        out = src

        # ---- 1. gate swap -------------------------------------------------
        c = out.count(GOD_REQ)
        if c:
            out = out.replace(GOD_REQ, CVAR_REQ)
            gate_hits += c

        # ---- 2. success lines ---------------------------------------------
        def patch_buff(m):
            global success_added
            buffname, block = m.group(1), m.group(2)
            if "Level Up SUCCESS" in block:
                return m.group(0)          # already done, stay idempotent
            prog = progression_of(block)
            if not prog:
                return m.group(0)
            label = display_name(block, prog)

            def insert(am):
                global success_added
                indent = am.group(1)
                success_added += 1
                line = (f'{indent}<triggered_effect trigger="onSelfBuffStart" '
                        f'action="LogMessageCVars, SCore" '
                        f'message="LBD DEBUG: {label} - Level Up SUCCESS ({prog})">\n'
                        f'{indent}    {CVAR_REQ}\n'
                        f'{indent}</triggered_effect>\n')
                return line + am.group(0)

            newblock = ADD_PROG.sub(insert, block)
            return m.group(0).replace(block, newblock)

        out = LU_BUFF.sub(patch_buff, out)

        if out != src:
            open(p, "w", encoding="utf-8").write(out)
            touched[os.path.relpath(p, BASE).replace("\\", "/")] = True

print(f"debug gates rewritten to $lbd_debug : {gate_hits}")
print(f"'Level Up SUCCESS' lines added      : {success_added}")
print(f"files touched                       : {len(touched)}")
