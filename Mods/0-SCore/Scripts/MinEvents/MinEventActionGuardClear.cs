using UnityEngine;

/// <summary>
/// Clears the guard position.
/// 
/// <example>
/// <code>
/// &lt;triggered_effect trigger="onSelfBuffUpdate" action="GuardClear, SCore" /&gt;
/// </code>
/// </example>
/// </summary>
public class MinEventActionGuardClear : MinEventActionTargetedBase
{
    public override void Execute(MinEventParams _params)
    {
        if (!(_params.Self is IEntityOrderReceiverSDX entityOrderReceiver)) return;

        entityOrderReceiver.GuardPosition = Vector3.zero;
        entityOrderReceiver.GuardLookPosition = Vector3.zero;
    }
}