using Harmony.Dialog;
using UnityEngine;

public class XUiC_SCoreUtilities : XUiController {
    public static string ID = "";
    private EntityPlayerLocal _entityPlayerLocal;


    public override void OnOpen() {
        base.OnOpen();
        _entityPlayerLocal = xui.playerUI.entityPlayer;
        if (_entityPlayerLocal == null)
            OnClose();

        // Sync up the CVars
        foreach (var toggle in GetChildrenByType<XUiC_ToggleButtonCVar>())
        {
            var result = false;
            if (toggle.CVarName.Contains("_"))
            {
                var className = toggle.CVarName.Split('_')[0];
                var featureName = toggle.CVarName.Split('_')[1];
                result = Configuration.CheckFeatureStatus(className, featureName);
            }

            if (_entityPlayerLocal.Buffs.HasCustomVar(toggle.CVarName))
                toggle.Value = _entityPlayerLocal.Buffs.GetCustomVar(toggle.CVarName) > 0f;
            else
                toggle.Value = result;
        }

    }

    public override void Init() {
        base.Init();
        ID = WindowGroup.ID;

        if (GetChildById("SCorebtnSubmit") is XUiC_SimpleButton xuiC_SimpleButton)
        {
            xuiC_SimpleButton.OnPressed += BtnSubmit_OnPressed;
        }
    }

    private void BtnSubmit_OnPressed(XUiController _sender, int _mouseButton) {
        foreach (var toggle in GetChildrenByType<XUiC_ToggleButtonCVar>())
        {
            _entityPlayerLocal.Buffs.SetCustomVar(toggle.CVarName, toggle.Value ? 1 : 0);
        }

        xui.playerUI.windowManager.Close(this.windowGroup.ID);
    }

    public override bool GetBindingValueInternal(ref string value, string bindingName)
    {
        switch (bindingName)
        {
            case "ingame":
                value = (GameStats.GetInt(EnumGameStats.GameState) != 0).ToString();
                return true;
            case "notingame":
                value = (GameStats.GetInt(EnumGameStats.GameState) == 0).ToString();
                return true;
            case "notreleaseingame":
                value = "false";
                return true;
            case "ingamenoteditor":
                value = "false";
                if (GameStats.GetInt(EnumGameStats.GameState) != 0)
                    value = (!GameManager.Instance.World.IsEditor()).ToString();
                return true;
        }
        return base.GetBindingValueInternal(ref value, bindingName);
    }

}