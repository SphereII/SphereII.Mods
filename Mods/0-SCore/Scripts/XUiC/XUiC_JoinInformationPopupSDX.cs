internal class XUiC_JoinInformationPopupSDX : XUiController
{
    public XUiV_Label hireInformationLabel;

    public XUiV_Panel hireInformationPanel;


    public override void Init()
    {
        base.Init();
        hireInformationPanel = (XUiV_Panel)GetChildById("JoinInformationPopup").ViewComponent;
        ((XUiC_SimpleButton)hireInformationPanel.Controller.GetChildById("btnCancel")).OnPressed += BtnCancelHireInformation_OnPressed;
        ((XUiC_SimpleButton)hireInformationPanel.Controller.GetChildById("btnConfirm")).OnPressed += BtnConfirmHireInformation_OnPressed;
        hireInformationLabel = (XUiV_Label)hireInformationPanel.Controller.GetChildById("JoinInformationLabel").ViewComponent;
    }

    public override void OnOpen()
    {
        EntityPlayer player = xui.playerUI.entityPlayer;

        var entityID = 0;
        if (player.Buffs.HasCustomVar("CurrentNPC"))
            entityID = (int)player.Buffs.GetCustomVar("CurrentNPC");

        if (entityID == 0)
            return;

        var myEntity = player.world.GetEntity(entityID) as EntityAliveSDX;
        if (myEntity != null)
        {
            hireInformationLabel.Text = Localization.Get("HireOffer_" + myEntity.EntityName);
            if (hireInformationLabel.Text == "Hire_Offer_" + myEntity.EntityName)
                hireInformationLabel.Text = "I would like to join you. Will you accept me?";
        }

        base.OnOpen();
    }

    private void BtnCancelHireInformation_OnPressed(XUiController _sender, int _mouseButton)
    {
        hireInformationPanel.IsVisible = false;
        xui.playerUI.windowManager.Close(windowGroup.ID);
    }

    private void BtnConfirmHireInformation_OnPressed(XUiController _sender, int _mouseButton)
    {
        EntityPlayer player = xui.playerUI.entityPlayer;

        var entityID = 0;
        if (player.Buffs.HasCustomVar("CurrentNPC"))
            entityID = (int)player.Buffs.GetCustomVar("CurrentNPC");

        if (entityID == 0)
            return;

        var myEntity = player.world.GetEntity(entityID) as EntityAliveSDX;
        if (myEntity != null)
            EntityUtilities.SetOwner(entityID, player.entityId);

        xui.playerUI.windowManager.Close(windowGroup.ID);
    }
}