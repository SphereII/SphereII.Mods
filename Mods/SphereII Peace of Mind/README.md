SphereII Peace of Mind
======================

Combines the earlier PG13 and Peace of Mind modlets into one. It softens the game's harsher
imagery and language without touching how the game plays.

## Scope

The target is narrow and deliberate: **imagery where one person has killed another, or where
someone has taken their own life.** A body strung up on a post, or a noose left hanging in an
empty room, carries an implication about who put it there. That implication is what gets
removed.

**Death on its own is not the target.** Corpses, blood, bones, body bags, skulls and animal
remains are all left exactly as vanilla ships them - someone who fell to the outbreak, to
injury or to bad luck says nothing about anyone having done it to them. Stripping those out
would flatten the game's atmosphere without serving the purpose.

This is worth stating plainly because the vanilla block list has plenty of the second kind
(`goreBlockHumanCorpse*`, `bodyBag*`, `bloodDecor*`, `goreBlockHumanBones`, `cntBathTubGore`,
the bloodied spike traps) and none of it is in scope. Their presence is a decision, not an
oversight.

## What it changes

### Scenery

- **Bodies hanging from posts and ropes are gone.** The posts remain as scenery with nothing
  on them, using the corpse-free models the game already ships
  (`noCorpseHangingLog1/2Prefab`), which match the originals in dimension, offset and
  placement. All 24 blocks in the family are covered - see the note on inheritance below.
- **The noose is an ordinary rope.** `modularRopeNoose` is rebuilt on `modularRopeTiled` with
  the plain tiled rope model and icon. It keeps its place in the world and is still climbable.
- **The names are changed too.** All 25 of those blocks are renamed for what they now are -
  "Red Hanging Log 1", "White Hanging Rope 1", "Rope Tiled" - following vanilla's own wording
  for `noCorpseHangingLog1`. Without this the crosshair still reads "Red Hanging Corpse Log 1"
  over an empty post.
- **Flickering and pulsating lights are switched off**, if SCore is installed.

### People

- **The Party Girl zombie uses the Marlene model**, across all four variants (standard, feral,
  radiated, charged). Entity names are kept, so quests, spawn lists and rewards are unaffected.
- **Trader Rekt is quieted.** His `voice_set` is removed and his stance softened from Dislike
  to Neutral, the same footing as Trader Hugh.

## Implementation notes

**Models inherit, names do not.** Only four of the 24 hanging-corpse blocks carry a `Model` of
their own; every colour variant extends its White parent and inherits it.
`BlocksFromXml.CreateProperties` resolves `Extends` by copying from the parent's already-parsed
`Block`, and XPath patching runs against the document before any of that, so patching the four
parents covers all 24. Localization is not inherited - it is keyed per block name - which is
why the rename list spells out every variant individually.

**The SCore-dependent part is guarded.** `mod_loaded('0-SCore_sphereii')` wraps the flickering
light setting, so the modlet is fully functional without SCore.

**Help screen.** `Config/XUi_InGame/windows.xml` contributes a Peace of Mind section to the
0-Help Screens window, guarded by `mod_loaded('sphereii_help_screens')`. Load order makes this
work: modlets apply in alphabetical order of folder name, and a modlet patching a node that
does not exist yet is skipped in full, silently.

## Known gaps

- `trader_rekt_rumor_08` - *"That White River outfit all ought to be hanged."* - is a lynching
  reference in Rekt's dialogue, which is squarely in scope. Removing his `voice_set` stops the
  audio, but whether the subtitle still renders when no voice set resolves has not been
  confirmed in game. If it does, that line needs a localization override.
