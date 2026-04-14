/// <summary>
/// Dialog requirement that passes only when the talking player is the registered farm owner
/// of the NPC — i.e. the player who issued the "FarmHere" command.
///
/// Usage:
///   Show "Recall Farmer" only for the owner:
///     &lt;requirement type="IsFarmOwner, SCore" requirementtype="Hide" /&gt;
///
///   Show "Assign to Farm" only when NPC is NOT already in farm mode:
///     &lt;requirement type="IsFarmOwner, SCore" requirementtype="Hide" value="not" /&gt;
/// </summary>
public class DialogRequirementIsFarmOwner : BaseDialogRequirement
{
    public override bool CheckRequirement(EntityPlayer player, EntityNPC talkingTo)
    {
        var entityId = 0;
        if (player.Buffs.HasCustomVar("CurrentNPC"))
            entityId = (int)player.Buffs.GetCustomVar("CurrentNPC");

        if (entityId == 0) return false;

        var npc = GameManager.Instance.World.GetEntity(entityId) as EntityAlive;
        if (npc == null) return false;

        bool inFarmMode = npc.Buffs.HasCustomVar("FarmOwnerEntityId") &&
                          npc.Buffs.GetCustomVar("FarmOwnerEntityId") > 0;

        // value="not" → true when the NPC is NOT in farm mode (safe to offer FarmHere)
        if (Value.EqualsCaseInsensitive("not"))
            return !inFarmMode;

        // Default → true only when this player is the registered owner
        if (!inFarmMode) return false;
        return (int)npc.Buffs.GetCustomVar("FarmOwnerEntityId") == player.entityId;
    }
}
