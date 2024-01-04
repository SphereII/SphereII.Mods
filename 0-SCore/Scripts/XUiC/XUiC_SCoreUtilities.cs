using System.Collections.Generic;
using System.Windows.Forms.VisualStyles;
using UnityEngine;

public class XUiC_SCoreUtilities : XUiController
{
    private XUiC_ToggleButton toggleNPCFootsteps;
    private XUiC_ToggleButton toggleVanillaLockPicking;
    public static string ID = "";
    private EntityPlayerLocal _entityPlayerLocal;


    public override void OnOpen()
    {
        base.OnOpen();
        _entityPlayerLocal = xui.playerUI.entityPlayer;
        if (_entityPlayerLocal == null)
           OnClose();
        
        // Set defaults
        toggleNPCFootsteps.Value = _entityPlayerLocal.Buffs.GetCustomVar("quietNPC") > 0f;
        toggleVanillaLockPicking.Value = _entityPlayerLocal.Buffs.GetCustomVar("LegacyLockPick") > 0f;
    }

    public override void Init()
    {
        base.Init();
        ID = WindowGroup.ID;

        toggleNPCFootsteps = GetChildById("toggleNPCFootsteps").GetChildByType<XUiC_ToggleButton>();
        toggleNPCFootsteps.OnValueChanged += delegate(XUiC_ToggleButton sender, bool b)
        {
            _entityPlayerLocal.Buffs.AddCustomVar("quietNPC", b ? 1 : 0);
        };

        toggleVanillaLockPicking = GetChildById("toggleLockPick").GetChildByType<XUiC_ToggleButton>();
        toggleVanillaLockPicking.OnValueChanged += delegate(XUiC_ToggleButton sender, bool b)
        {
            _entityPlayerLocal.Buffs.AddCustomVar("LegacyLockPick", b ? 1 : 0);
        };

      
    }

  

}