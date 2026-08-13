# Learn By Doing - test harness

Three scripts. The point is to make a claim like "all 80 progressions move, and
here is how long each level took" checkable rather than believed.

The mod is fully data-driven, so most of the answer is derivable from the XML
without launching the game. What genuinely needs a session is whether each
trigger *fires* under real conditions - that is what the log parser is for.

## The pieces

| file | what it does |
|---|---|
| `lbd_audit.py` | Parses `Config/**/*.xml`, resolves the tuning cvars, joins against vanilla `progression.xml` and `buffs.xml`. Writes `TESTPLAN.md`, `FINDINGS.md`, `lbd_model.json`. |
| `lbd_logparse.py` | Reads a `Player.log` from a session and reports coverage, real per-level timings, and any level-up failures. |
| `TESTPLAN.md` | Generated. Every XP award path: trigger, conditions, payout, throttle; plus expected actions and floor time per level to max. |
| `FINDINGS.md` | Generated. Award paths that can never pay, throttles and buffs that do not resolve, progression names the game does not know, block requirements that match no block. |
| `migrate_*.py` | One-shot migrations, kept for the record. Already applied; do not re-run. |

## Turning the log on

```
setcvar $lbd_debug 1     in the F1 console
```

Every `LBD DEBUG` line in the mod is gated on this cvar. `setcvar $lbd_debug 0`
turns it off (SCore treats 0 as a delete, and an unset cvar reads as 0, so that
is a genuine off).

This used to be gated on `HasBuff god`. That buff carries
`PhysicalDamageResist` and `ElementalDamageResist +200` and
`CarryCapacity base_set 45`, so switching the log on also made the player
invulnerable - and `perkLightArmor`, `perkHeavyArmor`, `perkHardTarget` and
`perkPainTolerance` earn XP *only* from taking damage. They could never be
watched while being exercised. A plain cvar keeps observation separate from
gameplay.

## Log vocabulary

```
LBD DEBUG: <name> - ... XP (+n)                  an award fired
LBD DEBUG: <name> - Triggering Level Up Check    xp crossed the threshold
LBD DEBUG: <name> - Level Up SUCCESS (<prog>)    the level landed
LBD DEBUG: <name> - Level Up FAILED. ...         the grant was refused
```

The SUCCESS line is what makes per-level timing possible. Before it existed, a
level that landed and a level that silently did nothing produced identical
logs - which is exactly why seven progressions whose `AddBuff` never resolved
went unnoticed for so long.

Note the mod uses 55+ phrasings for its award lines ("Attempting 'Craft' XP",
"General Use Synergy XP", "COMBO HIT! Applying Bonus XP"). `lbd_audit.py`
therefore pairs awards to log lines *structurally* - same trigger, log
requirements a subset of the award's - never by message text. Award lines
deliberately omit the cooldown check, which is why they say "Attempting".

## Values in the log

The mod logs through `LogMessageCVars, SCore` rather than vanilla `LogMessage`.
Vanilla writes the message verbatim, so `(+@$lbd_xp_melee_base)` reached the log
with the cvar name in it and a line could not tell you how much XP it was worth.
The SCore action expands `@cvar` tokens against the player before logging, so the
same message now reads `(+1)`. Token syntax matches what the configs already
wrote, so no message text had to change.

A token naming a cvar the player does not have renders as `<unset:name>` rather
than 0. That is deliberate: the game treats an unset cvar as 0, which is how four
award paths in this mod paid nothing for so long without anyone noticing.

## Running a pass

```sh
# 1. static model + expectations  (re-run after any config change)
python lbd_audit.py                       # --game "<7dtd install>" if not the Steam default

# 2. play a session with $lbd_debug 1, then
python lbd_logparse.py "%APPDATA%\..\LocalLow\The Fun Pimps\7 Days To Die\Player.log" --md session.md
```

In game:

1. **Start a new character.** The Init buff only runs when `$lbd_xp_melee_base`
   is 0 or less, so it fires once per character. Retuned values and new cvars do
   not reach an existing save - use a Grandpa's Forgettin' Elixir, which the mod
   hooks to re-run Init.
2. `setcvar $lbd_debug 1`.
3. Work the checklist in `TESTPLAN.md`. The perk window draws an LBD progress
   bar under each perk, attribute and skill row; that is the live confirmation
   that something moved.
4. Run the log parser. Anything under "progressions that never fired" is your
   to-do list for the next session.

### Reading the timing numbers

`TESTPLAN.md` gives, per level, the **actions** needed on the fastest-paying
award path and a **floor** time - `actions x cooldown`, the shortest wall-clock
time in which that XP could physically be earned. It is a lower bound, not a
prediction: real play is slower because you rarely trigger on the cooldown edge.

Its use is as a falsifier. `lbd_logparse.py` flags any level that arrived
*faster* than its floor, which means a throttle is not applying.

### A trap worth knowing

`setcvar $x 0` **removes** the cvar rather than setting it to zero - SCore's
`ConsoleCmdAdjustCVar` treats 0 as a delete. An unset cvar reads as 0, so it
still works as a reset, but the cvar will not exist again until something
writes it.

## Blocks and Extends

`lbd_audit.py` resolves `Extends` when it reads vanilla `blocks.xml`.
`BlocksFromXml.CreateProperties` inherits a parent block's properties, so a block
with no `Tags` of its own still carries its parent's - `shotgunTurret` looks
untagged but inherits `trapsSkill` from `autoTurret`. Reading the raw
`<property name="Tags">` invents gaps that are not there. Harvest drops inherit
the same way, and those are what `RequirementBlockHasHarvestTags` reads: it tests
the block's `<drop event="Harvest">` entries, not its `Tags`.

Two rules the resolver encodes, both easy to get wrong:

* inheritance applies only where the child does **not** define the property. An
  own `Tags` **replaces** the parent's list rather than adding to it, so patching
  a tag onto an inheriting child silently drops everything else it had.
* `param1` on the `Extends` property names the properties that are *not*
  inherited.

Every block requirement in the mod is then checked against the resolved list -
`RequirementIsTargetBlock`, `RequirementCraftedBlockHasTags`,
`RequirementBlockHasHarvestTags` and `BlockHasName` - and any matching zero
blocks is reported. That is the same failure mode as an undefined cvar or an
unresolvable buff name: the requirement is simply never true, and nothing says so
at load time.

## Scope

`lbd_audit.py` reasons about the XML as written. It does not know whether a
`HoldingItemHasTags` or `RequirementBlockHasHarvestTags` condition can be met by
any item or block that actually exists in the game - a path can be live in the
model and still unreachable in play. That is the gap the session and the log
parser close.
