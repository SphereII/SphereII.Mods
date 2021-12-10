public class XUiC_PathFindingKeypadWindow : XUiController
{
    public static string ID = "";
    public static EntityAliveSDX myEntity;
    private XUiC_TextInput txtPassword;

    public override void Init()
    {
        ID = windowGroup.ID;
        base.Init();
        txtPassword = (XUiC_TextInput)GetChildById("txtPassword");
        txtPassword.OnSubmitHandler += TxtPassword_OnSubmitHandler;
        txtPassword.OnInputAbortedHandler += TextInput_OnInputAbortedHandler;
        ((XUiC_SimpleButton)GetChildById("btnCancel")).OnPressed += BtnCancel_OnPressed;
        ((XUiC_SimpleButton)GetChildById("btnOk")).OnPressed += BtnOk_OnPressed;
    }

    private void TextInput_OnInputAbortedHandler(XUiController _sender)
    {
        xui.playerUI.windowManager.Close(WindowGroup.ID);
    }

    private void TxtPassword_OnSubmitHandler(XUiController _sender, string _text)
    {
        BtnOk_OnPressed(_sender, 0);
    }

    private void BtnOk_OnPressed(XUiController _sender, int _mouseButton)
    {
        var text = txtPassword.Text;
        var code = 0f;
        StringParsers.TryParseFloat(text, out code);
        myEntity.Buffs.AddCustomVar("PathingCode", code);

        SphereCache.RemovePaths(myEntity.entityId);
        EntityUtilities.GetNewPositon(myEntity.entityId);
        EntityUtilities.SetCurrentOrder(myEntity.entityId, EntityUtilities.Orders.Wander);
        GameManager.ShowTooltip(xui.playerUI.entityPlayer, "Pathing Code Set");
        xui.playerUI.windowManager.Close(WindowGroup.ID);
    }

    private void BtnCancel_OnPressed(XUiController _sender, int _mouseButton)
    {
        xui.playerUI.windowManager.Close(WindowGroup.ID);
    }

    public override void OnOpen()
    {
        EntityPlayer player = xui.playerUI.entityPlayer;

        var entityID = 0;
        if (player.Buffs.HasCustomVar("CurrentNPC"))
            entityID = (int)player.Buffs.GetCustomVar("CurrentNPC");

        if (entityID == 0)
            return;

        myEntity = player.world.GetEntity(entityID) as EntityAliveSDX;
        base.OnOpen();

        txtPassword.Text = "";
        txtPassword.SelectOrVirtualKeyboard();
        xui.playerUI.entityPlayer.PlayOneShot("open_sign");
    }

    public override void OnClose()
    {
        base.OnClose();
        xui.playerUI.entityPlayer.PlayOneShot("close_sign");
        myEntity = null;
    }
}