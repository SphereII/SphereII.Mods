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
        LocalPlayerUI uiforPlayer = base.xui.playerUI;

        // The respondent is an EntityNPC, and we don't have that. Check for the patch scripted otherEntitySDX.
        Entity respondent = uiforPlayer.xui.Dialog.Respondent;
        if (respondent != null)
        {
            EntityAliveSDX myEntity = uiforPlayer.entityPlayer.world.GetEntity(respondent.entityId) as EntityAliveSDX;
            if (myEntity != null)
            {
                Debug.Log(GetType() + " Entity is EntityAliveSDX. Displaying message");

                this.hireInformationLabel.Text = Localization.Get("HireOffer_" + myEntity.EntityName, "");
                if ( this.hireInformationLabel.Text == "Hire_Offer_" + myEntity.EntityName )
                {
                    this.hireInformationLabel.Text = "I would like to join you. Will you accept me?";
                }
            }
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
        LocalPlayerUI uiforPlayer = base.xui.playerUI;

        // The respondent is an EntityNPC, and we don't have that. Check for the patch scripted otherEntitySDX.
        Entity respondent = uiforPlayer.xui.Dialog.Respondent;
        if (respondent != null)
        {
            EntityAliveSDX myEntity = uiforPlayer.entityPlayer.world.GetEntity(respondent.entityId) as EntityAliveSDX;
            if (myEntity != null)
            {
                myEntity.SetOwner(uiforPlayer.entityPlayer as EntityPlayerLocal);
            }
        }

        base.xui.playerUI.windowManager.Close(this.windowGroup.ID);
    }

  
   
}

