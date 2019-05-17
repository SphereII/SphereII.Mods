using UnityEngine;
class XUiC_HireInformationPopupSDX : XUiController
{
    public XUiV_Panel hireInformationPanel;
    public XUiV_Label hireInformationLabel;


    public override void Init()
    {
        base.Init();
        hireInformationPanel = (XUiV_Panel)base.GetChildById("HireInformationPopup").ViewComponent;
        ((XUiC_SimpleButton)hireInformationPanel.Controller.GetChildById("btnCancel")).OnPressed += BtnCancelHireInformation_OnPressed;
        ((XUiC_SimpleButton)hireInformationPanel.Controller.GetChildById("btnConfirm")).OnPressed += BtnConfirmHireInformation_OnPressed;
        hireInformationLabel = (XUiV_Label)hireInformationPanel.Controller.GetChildById("HireInformationLabel").ViewComponent;
    }

    public override void OnOpen()
    {
        EntityPlayer player = base.xui.playerUI.entityPlayer;

        int entityID = 0;
        if(player.Buffs.HasCustomVar("CurrentNPC"))
            entityID = (int)player.Buffs.GetCustomVar("CurrentNPC");

        if(entityID == 0)
            return;

        EntityAliveSDX myEntity = player.world.GetEntity(entityID) as EntityAliveSDX;
        if(myEntity != null)
            hireInformationLabel.Text = "Hire " + myEntity.EntityName + " for " + EntityUtilities.GetHireCost( entityID ) + " " + EntityUtilities.GetHireCurrency(entityID).ItemClass.GetLocalizedItemName() + "?";

        base.OnOpen();

    }

    private void BtnConfirmHireInformation_OnPressed(XUiController _sender, OnPressEventArgs _onPressEventArgs)
    {
        EntityPlayer player = base.xui.playerUI.entityPlayer;

        int entityID = 0;
        if(player.Buffs.HasCustomVar("CurrentNPC"))
            entityID = (int)player.Buffs.GetCustomVar("CurrentNPC");

        if(entityID == 0)
            return;

        EntityAliveSDX myEntity = player.world.GetEntity(entityID) as EntityAliveSDX;
        if(myEntity != null)
            EntityUtilities.Hire(entityID, player as EntityPlayerLocal);

        base.xui.playerUI.windowManager.Close(windowGroup.ID);
    }

    private void BtnCancelHireInformation_OnPressed(XUiController _sender, OnPressEventArgs _onPressEventArgs)
    {
        hireInformationPanel.IsVisible = false;
        base.xui.playerUI.windowManager.Close(windowGroup.ID);
    }

    public override void OnClose()
    {
        if(base.xui.playerUI.windowManager.Contains("dialog") && base.xui.playerUI.windowManager.IsWindowOpen("dialog"))
            base.xui.playerUI.windowManager.Close("dialog");
        base.OnClose();
    }



}

