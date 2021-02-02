public class XUiC_PathFindingKeypadWindow : XUiController
{
    public override void Init()
    {
        XUiC_PathFindingKeypadWindow.ID = windowGroup.ID;
        base.Init();
        txtPassword = (XUiC_TextInput)base.GetChildById("txtPassword");
        txtPassword.OnSubmitHandler += TxtPassword_OnSubmitHandler;
        txtPassword.OnInputAbortedHandler += TextInput_OnInputAbortedHandler;
        ((XUiC_SimpleButton)base.GetChildById("btnCancel")).OnPressed += BtnCancel_OnPressed;
        ((XUiC_SimpleButton)base.GetChildById("btnOk")).OnPressed += BtnOk_OnPressed;
    }

    private void TextInput_OnInputAbortedHandler(XUiController _sender)
    {
        base.xui.playerUI.windowManager.Close(base.WindowGroup.ID);
    }

    private void TxtPassword_OnSubmitHandler(XUiController _sender, string _text)
    {
        BtnOk_OnPressed(_sender, new OnPressEventArgs(-1));
    }

    private void BtnOk_OnPressed(XUiController _sender, OnPressEventArgs _onPressEventArgs)
    {

        string text = txtPassword.Text;
        float code = 0f;
        StringParsers.TryParseFloat(text, out code);
        myEntity.Buffs.AddCustomVar("PathingCode", code);

        SphereCache.RemovePaths(myEntity.entityId);
        EntityUtilities.GetNewPositon(myEntity.entityId);
        EntityUtilities.SetCurrentOrder(myEntity.entityId, EntityUtilities.Orders.Wander);
        GameManager.ShowTooltip(base.xui.playerUI.entityPlayer, "Pathing Code Set");
        base.xui.playerUI.windowManager.Close(base.WindowGroup.ID);
    }

    private void BtnCancel_OnPressed(XUiController _sender, OnPressEventArgs _e)
    {
        base.xui.playerUI.windowManager.Close(base.WindowGroup.ID);
    }

    public override void OnOpen()
    {
        EntityPlayer player = base.xui.playerUI.entityPlayer;

        int entityID = 0;
        if (player.Buffs.HasCustomVar("CurrentNPC"))
            entityID = (int)player.Buffs.GetCustomVar("CurrentNPC");

        if (entityID == 0)
            return;

        myEntity = player.world.GetEntity(entityID) as EntityAliveSDX;
        base.OnOpen();

        txtPassword.Text = "";
        txtPassword.SelectOrVirtualKeyboard();
        base.xui.playerUI.entityPlayer.PlayOneShot("open_sign", false);

    }

    public override void OnClose()
    {
        base.OnClose();
        base.xui.playerUI.entityPlayer.PlayOneShot("close_sign", false);
        myEntity = null;
    }


    public static string ID = "";
    private XUiC_TextInput txtPassword;
    public static EntityAliveSDX myEntity;
}
