#!/usr/bin/env python3
"""
One-shot migration, kept for the record.

T0 starter gear carries no UnlockedBy property, so the craft/scrap/repair awards
- which all gate on ItemHasProperty UnlockedBy="craftingX" - never saw it. A new
character's entire crafting output scored nothing.

Vanilla unlocks the T1 recipes for every one of these twelve skills at level 11,
so a T0 path has to pay up to that point or the skill strands: below 11 there is
nothing else craftable, and at 5 (where Clubs/Bows/Armor capped their existing
T0 repair path) you could never bridge to iron. Hence LTE 10.

Adds, per skill:
  * a T0 craft path   (onSelfItemCrafted)
  * a T0 repair path  (onSelfItemRepaired) where one does not already exist
  * the shared T0 cooldown buff, where not already defined
Scrap is deliberately left T1+ only: craft-then-scrap on a stone axe would loop
for double XP with the materials mostly returned.

Also normalises the three existing T0 repair discriminators to the same
"ItemHasTags T0" + "ItemHasTags <skill>Skill" pair. HarvestingTools' matched on
harvestingSkill alone, which also matches the 8 iron/steel tools that carry
UnlockedBy - so repairing a looted iron pickaxe below level 11 paid twice.

Usage:  python migrate_t0_paths.py [--apply] [--only <File.xml>]
"""
import os, re, argparse

HERE = os.path.dirname(os.path.abspath(__file__))
BASE = os.path.normpath(os.path.join(HERE, "..", "Config", "CraftingSkills"))

ap = argparse.ArgumentParser()
ap.add_argument("--apply", action="store_true", help="write files (default: dry run)")
ap.add_argument("--only", help="restrict to one file")
args = ap.parse_args()

# file -> (progression, skill tag)
SKILLS = {
    "CraftingBlades.xml":        ("craftingBlades",         "bladeSkill"),
    "CraftingBows.xml":          ("craftingBows",           "bowSkill"),
    "CraftingClubs.xml":         ("craftingClubs",          "clubSkill"),
    "CraftingHandguns.xml":      ("craftingHandguns",       "handgunSkill"),
    "HarvestingTools.xml":       ("craftingHarvestingTools", "harvestingSkill"),
    "CraftingKnuckles.xml":      ("craftingKnuckles",       "knuckleSkill"),
    "CraftingMachineGuns.xml":   ("craftingMachineGuns",    "machinegunSkill"),
    "CraftingRifles.xml":        ("craftingRifles",         "rifleSkill"),
    "CraftingRobotics.xml":      ("craftingRobotics",       "roboticsSkill"),
    "CraftingShotguns.xml":      ("craftingShotguns",       "shotgunSkill"),
    "CraftingSledgehammers.xml": ("craftingSledgehammers",  "sledgeSkill"),
    "CraftingSpears.xml":        ("craftingSpears",         "spearSkill"),
}

GATE = '<requirement name="CVarCompare" cvar="$lbd_debug" operation="GTE" value="1"/>'


def label_of(text, prog):
    m = re.search(r"LBD DEBUG:\s*(.+?)\s+-\s+Attempting", text)
    if m:
        return m.group(1)
    n = re.sub(r"^crafting", "", prog)
    return "Crafting " + re.sub(r"(?<!^)(?=[A-Z])", " ", n).strip()


def build(prog, tag, label, indent, want_repair):
    cd = f"buffLBD_{prog}_T0RepairCoolDown"
    lu = f"buffLBD_{prog}_LevelUpCheck"
    xp = f"${prog.lower()}_lbd_xp"
    tonext = f"${prog.lower()}_lbd_xptonext"
    I = indent

    def reqs(extra=(), debug=False, cool=True):
        out = [f'<requirement name="ProgressionLevel" progression_name="{prog}" operation="LTE" value="10"/>',
               '<requirement name="ItemHasTags" tags="T0"/>',
               f'<requirement name="ItemHasTags" tags="{tag}"/>']
        out += list(extra)
        if cool:
            out.append(f'<requirement name="NotHasBuff" buff="{cd}"/>')
        if debug:
            out.append(GATE)
        return "\n".join(I + "    " + r for r in out)

    dmg = ('<requirement name="ItemPercentDamaged, SCore" operation="GTE" value="0.5"/>',)
    blocks = []
    blocks.append(f"""{I}<!-- T0 starter gear. These items carry no UnlockedBy property, so the paths
{I}     above cannot see them, and below level 11 there is nothing else craftable -
{I}     vanilla unlocks the T1 recipes for this skill at 11. Pays until then and
{I}     stops, letting the UnlockedBy paths take over. Scrap is deliberately not
{I}     included: craft-then-scrap would loop for double XP. -->
{I}<triggered_effect trigger="onSelfItemCrafted" action="LogMessageCVars, SCore" message="LBD DEBUG: {label} - Attempting T0 Craft XP (+@$lbd_xp_crafting_base)">
{reqs(debug=True)}
{I}</triggered_effect>
{I}<triggered_effect trigger="onSelfItemCrafted" action="ModifyCVar" cvar="{xp}" operation="add" value="@$lbd_xp_crafting_base">
{reqs()}
{I}</triggered_effect>""")

    if want_repair:
        blocks.append(f"""{I}<triggered_effect trigger="onSelfItemRepaired" action="LogMessageCVars, SCore" message="LBD DEBUG: {label} - Attempting T0 Repair XP (+@$lbd_xp_repairing_base)">
{reqs(dmg, debug=True)}
{I}</triggered_effect>
{I}<triggered_effect trigger="onSelfItemRepaired" action="ModifyCVar" cvar="{xp}" operation="add" value="@$lbd_xp_repairing_base">
{reqs(dmg)}
{I}</triggered_effect>""")

    blocks.append(f"""{I}<triggered_effect trigger="onSelfItemCrafted" action="AddBuff" buff="{cd}">
{reqs()}
{I}</triggered_effect>""")
    if want_repair:
        blocks.append(f"""{I}<triggered_effect trigger="onSelfItemRepaired" action="AddBuff" buff="{cd}">
{reqs(dmg)}
{I}</triggered_effect>""")

    thr = (f'<requirement name="CVarCompare" cvar="{xp}" operation="GTE" value="@{tonext}"/>',)
    blocks.append(f"""{I}<triggered_effect trigger="onSelfItemCrafted" action="LogMessageCVars, SCore" message="LBD DEBUG: {label} - Triggering Level Up Check (T0 Craft)">
{reqs(thr, debug=True, cool=False)}
{I}</triggered_effect>
{I}<triggered_effect trigger="onSelfItemCrafted" action="AddBuff" buff="{lu}">
{reqs(thr, cool=False)}
{I}</triggered_effect>""")
    if want_repair:
        blocks.append(f"""{I}<triggered_effect trigger="onSelfItemRepaired" action="AddBuff" buff="{lu}">
{reqs(dmg + thr, cool=False)}
{I}</triggered_effect>""")
    return "\n".join(blocks) + "\n"


changed = {}
for fname, (prog, tag) in sorted(SKILLS.items()):
    if args.only and fname != args.only:
        continue
    p = os.path.join(BASE, fname)
    src = open(p, encoding="utf-8").read()
    out = src

    # 1. normalise an existing T0 repair discriminator onto T0 + skill tag
    normalised = 0
    for old in (f'<requirement name="ItemHasTags" tags="{tag}"/>',
                '<requirement name="ItemHasProperty, SCore" property="RepairTools" prop_value="resourceWood"/>'):
        if old in out:
            new = f'<requirement name="ItemHasTags" tags="T0"/>\n' \
                  f'{" " * 16}<requirement name="ItemHasTags" tags="{tag}"/>'
            normalised += out.count(old)
            out = out.replace(old, new)

    has_repair = "T0Repair" in src or "T0 Repair" in src
    label = label_of(src, prog)

    # 2. insert the T0 block at the end of the manager effect_group
    m = re.search(r"\n([ \t]*)</effect_group>", out)
    indent = m.group(1) + "    "
    block = build(prog, tag, label, indent, want_repair=not has_repair)
    out = out[:m.start()] + "\n\n" + block + out[m.start():]

    # 3. define the shared T0 cooldown if the file lacks it
    cd = f"buffLBD_{prog}_T0RepairCoolDown"
    if f'<buff name="{cd}"' not in out:
        out = out.replace("</configs>",
                          f'    <append xpath="/buffs">\n'
                          f'        <!-- Shared throttle for the T0 craft and repair paths. -->\n'
                          f'        <buff name="{cd}" hidden="true"><duration value="5"/></buff>\n'
                          f'    </append>\n</configs>')

    if out != src:
        changed[fname] = (normalised, not has_repair)
        if args.apply:
            open(p, "w", encoding="utf-8").write(out)

for f, (norm, added_repair) in sorted(changed.items()):
    bits = ["craft"]
    if added_repair:
        bits.append("repair")
    extra = f", normalised {norm} existing discriminator(s)" if norm else ""
    print(f"  {f:28} +T0 {'+'.join(bits)}{extra}")
print(f"\n{'APPLIED' if args.apply else 'DRY RUN'}: {len(changed)} files")
