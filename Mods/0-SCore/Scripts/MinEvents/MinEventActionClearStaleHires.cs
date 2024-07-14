﻿//        <triggered_effect trigger = "onSelfBuffUpdate" action="ClearStaleHires, SCore"  />

using System.Xml;
using UnityEngine;

public class MinEventActionClearStaleHires : MinEventActionTargetedBase
{
    public override void Execute(MinEventParams _params)
    {
        EntityUtilities.CheckForDanglingHires(_params.Self.entityId);
    }
}