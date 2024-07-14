/// <summary>
/// Sets the guard position to where the entity is standing.
/// 
/// <example>
/// <code>
/// &lt;triggered_effect trigger="onSelfBuffUpdate" action="GuardThere, SCore" /&gt;
/// </code>
/// </example>
/// </summary>
public class MinEventActionGuardThere : MinEventActionTargetedBase
{
    public override void Execute(MinEventParams _params)
    {
        if (!(_params.Self is IEntityOrderReceiverSDX entityOrderReceiver)) return;

        entityOrderReceiver.GuardPosition = _params.Self.position;
        entityOrderReceiver.GuardLookPosition = _params.Self.position + _params.Self.GetLookVector();
    }
}