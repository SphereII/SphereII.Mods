/// <summary>
/// <para>
/// Requires that the dialog's NPC must have been spawned into a prefab sleeper volume.
/// Putting the value "not" into <see cref="BaseDialogRequirement.Value"/> will negate the result.
/// </para>
/// 
/// <example>
/// Example: Hide the response unless the NPC is a sleeper.
/// <code>
/// &lt;requirement type="IsSleeper, SCore" requirementtype="Hide" />
/// </code>
/// </example>
/// 
/// <example>
/// Example: Hide the response unless the NPC is <em>not</em> a sleeper.
/// <code>
/// &lt;requirement type="IsSleeper, SCore" requirementtype="Hide" value="not" />
/// </code>
/// </example>
/// </summary>
public class DialogRequirementIsSleeper : BaseDialogRequirement
{
    public override bool CheckRequirement(EntityPlayer player, EntityNPC talkingTo)
    {
        var isSleeper = talkingTo.IsSleeper;
        return Value.EqualsCaseInsensitive("not") ? !isSleeper : isSleeper;
    }
}
