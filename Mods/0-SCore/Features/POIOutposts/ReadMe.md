# POI Outposts

A quest can turn its POI into a permanent outpost. When the quest is turned in, the POI is rebuilt
from a variant prefab (`cabin_01` as `cabin_01_outpost`, say), and a settler NPC is stationed in
it. The settler can be a trader. Quests never send anyone to the POI again, so no quest can reset
it back.

The obvious use is after a [Quest Rescue](../QuestRescue/ReadMe.md): the survivor you got out
fortifies the place and starts trading from it.

## How it works

1. The quest's `ConvertPOISDX` action runs when the quest is completed. It marks the POI as an
   outpost. The marks are saved with the world, in `SCore_POIOutposts.xml` in the save folder.
2. The POI is rebuilt from the variant once all of its chunks are loaded and no player is within
   10 blocks of it. Usually the player is back at the trader by then, so the rebuild happens out of
   sight, and the outpost is there on their next visit. If the POI isn't loaded, the rebuild waits
   until it is.
3. After the rebuild, the settler spawns at a random `settler_block` marker in the outpost and is
   set to stay there. If the settler dies or leaves the POI, a new one moves in the next time no
   player is near (turn this off with `settler_respawn="false"`).
4. From then on, the POI is hidden from every quest POI search: vanilla quests, `RandomTaggedPOIGotoSDX`
   and `GotoPOISDX`. If anything else resets it, it's rebuilt as the outpost.

The rebuild uses [Quest POI Variants](../QuestPOIVariants/ReadMe.md), so the same rules apply to the
variant: same footprint (x and z) as the original, any height. An outpost variant should have no
sleeper volumes; otherwise zombies spawn in it.

## The action

```xml
<action type="ConvertPOISDX, SCore">
    <property name="on_complete" value="true"/>
    <property name="poi_variant_suffix" value="_outpost"/>
    <property name="settler" value="npcOutpostTrader"/>
    <property name="settler_block" value="outpostSettlerMarker"/>
</action>
```

| Property | Required | Description |
|---|---|---|
| `on_complete` | yes | `true`, so the action runs when the quest is turned in. Without it, it runs at the start of its `phase`. |
| `poi_variant_suffix` | yes | The variant to rebuild the POI from: its name is the POI's name plus this suffix. A POI without that variant isn't converted (the log says why). |
| `settler` | no | Entity group or entity class of the NPC stationed in the outpost. Without it, the POI is converted but nobody lives there. |
| `settler_block` | no | Block name(s), comma separated. The settler spawns at a random one in the outpost: on top of it if it's solid, inside it if you can walk through it. Without a marker, it spawns at the POI's center. |
| `settler_buff` | no | Buff given to the settler when it spawns. |
| `settler_respawn` | no | `true` (default): a new settler moves in if the old one dies or leaves. `false`: only one ever spawns. |

Every party member's quest runs the action; the first one converts the POI and the rest are
ignored. A POI that's already an outpost is never converted again.

Use it on quests that send the player to a POI with a variant. With `RandomTaggedPOIGotoSDX`, set
`require_poi_variant` and the quest's `poi_variant_suffix` so only those POIs are picked. Note
that's the quest's *own* variant (`_captive`, say); the outpost variant needs to exist for the same
POIs.

## Making the settler a trader

SCore NPCs are traders underneath, so all the settler needs is a `trader_info` with some stock and
a dialog that opens it:

```xml
<!-- traders.xml -->
<append xpath="/traders">
    <trader_info id="6500" reset_interval="1" allow_sell="true">
        <trader_items>
            <item group="cannedfood" count="2,4"/>
            <item group="medicine" count="1,3"/>
            <item group="ammoAll" count="2,4"/>
        </trader_items>
    </trader_info>
</append>

<!-- npc.xml -->
<append xpath="/npc">
    <npc_info id="npcOutpostTrader" name="Outpost Trader" faction="whiteriver" stance="Like"
              dialog_id="OutpostTrader" trader_id="6500"/>
</append>

<!-- dialogs.xml -->
<append xpath="/dialogs">
    <dialog id="OutpostTrader" startstatementid="start">
        <statement id="start" text="Welcome to the outpost.">
            <response_entry id="trade"/>
            <response_entry id="done"/>
        </statement>
        <response id="trade" text="Show me what you have.">
            <action type="Trader" id="trade"/>
        </response>
        <response id="done" text="See you around."/>
    </dialog>
</append>

<!-- entityclasses.xml: any SCore NPC, pointed at the npc_info -->
<append xpath="/entity_classes">
    <entity_class name="npcOutpostTrader" extends="npcBakerPlayerCtrlTest">
        <property name="NPCID" value="npcOutpostTrader"/>
    </entity_class>
</append>
```

To make it look like the survivor you rescued, extend the same entity class the captive uses.
The captive itself doesn't move in: with `on_complete="leave"` it's gone by turn-in, so the
settler is a new NPC.

## Managing outposts

The `outposts` console command (server):

- `outposts list`: every outpost in the world, whether it's built yet, and its settler.
- `outposts remove <n>`: forget outpost `n`. Quests can pick the POI again; it keeps the outpost's
  blocks until a quest resets it. The settler stays until it's removed some other way.

The server log lines start with `[SCore] POIOutposts:`.

## Test quest

`Z-SphereIIDebugging` has `test_rescue_outpost`: the marker rescue quest (`test_rescue_captive_marker`)
plus `ConvertPOISDX`. Its outpost, `cabin_01_outpost`, is `cabin_01` with steel walls, a vault door
and no sleepers, and its chairs are `outpostSettlerMarker` blocks. The settler, `npcOutpostTraderTest`,
is the SDCS test baker with a small trader stock (`trader_info` 6500).

1. `givequest test_rescue_outpost`, and do the rescue as usual.
2. Walk the captive out of the cabin, then turn the quest in (talk to any NPC; `givequest` quests
   have no quest giver). The log shows `... will become an outpost`.
3. Move more than 10 blocks from the cabin. The log shows `rebuilding as cabin_01_outpost`, then
   `is now an outpost`, then `spawned settler`.
4. Go back: the cabin is steel, and the baker sits in one of the chairs. Talk to them and trade.
5. `givequest test_rescue_outpost` again: it picks a different `cabin_01`.
6. `outposts list` shows the outpost; restart the game and check it's still there and still
   skipped.
