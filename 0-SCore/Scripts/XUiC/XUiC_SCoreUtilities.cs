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
            toggle.Value = _entityPlayerLocal.Buffs.GetCustomVar(toggle.CVarName) > 0f;
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
    
    public override bool GetBindingValue(ref string value, string bindingName)
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
        return base.GetBindingValue(ref value, bindingName);
    }

}