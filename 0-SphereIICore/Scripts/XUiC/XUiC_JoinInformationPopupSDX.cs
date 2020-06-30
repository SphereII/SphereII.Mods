using UnityEngine;
class XUiC_JoinInformationPopupSDX : XUiController
{

    public XUiV_Panel hireInformationPanel;
    public XUiV_Label hireInformationLabel;


    public override void Init()
    {
        base.Init();
        this.hireInformationPanel = (XUiV_Panel)base.GetChildById("JoinInformationPopup").ViewComponent;
        ((XUiC_SimpleButton)this.hireInformationPanel.Controller.GetChildById("btnCancel")).OnPressed += this.BtnCancelHireInformation_OnPressed;
        ((XUiC_SimpleButton)this.hireInformationPanel.Controller.GetChildById("btnConfirm")).OnPressed += this.BtnConfirmHireInformation_OnPressed;
        this.hireInformationLabel = (XUiV_Label)this.hireInformationPanel.Controller.GetChildById("JoinInformationLabel").ViewComponent;
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
        {
            this.hireInformationLabel.Text = Localization.Get("HireOffer_" + myEntity.EntityName);
            if(this.hireInformationLabel.Text == "Hire_Offer_" + myEntity.EntityName)
                this.hireInformationLabel.Text = "I would like to join you. Will you accept me?";
        }

        base.OnOpen();
    }

    private void BtnCancelHireInformation_OnPressed(XUiController _sender, OnPressEventArgs _onPressEventArgs)
    {
        this.hireInformationPanel.IsVisible = false;
        base.xui.playerUI.windowManager.Close(this.windowGroup.ID);
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
            EntityUtilities.SetOwner(entityID, player.entityId);

        base.xui.playerUI.windowManager.Close(this.windowGroup.ID);
    }

  
   
}

