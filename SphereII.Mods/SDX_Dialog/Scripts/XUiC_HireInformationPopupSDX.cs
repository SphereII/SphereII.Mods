using UnityEngine;
class XUiC_HireInformationPopupSDX : XUiController
{
    public XUiV_Panel hireInformationPanel;
    public XUiV_Label hireInformationLabel;

  
    public override void Init()
    {
        base.Init();
        this.hireInformationPanel = (XUiV_Panel)base.GetChildById("HireInformationPopup").ViewComponent;
        ((XUiC_SimpleButton)this.hireInformationPanel.Controller.GetChildById("btnCancel")).OnPressed += this.BtnCancelHireInformation_OnPressed;
        ((XUiC_SimpleButton)this.hireInformationPanel.Controller.GetChildById("btnConfirm")).OnPressed += this.BtnConfirmHireInformation_OnPressed;
        this.hireInformationLabel = (XUiV_Label)this.hireInformationPanel.Controller.GetChildById("HireInformationLabel").ViewComponent;
    }

    public override void OnOpen()
    {
        LocalPlayerUI uiforPlayer = base.xui.playerUI;

        Debug.Log("HireInformationPopupSDX");
        Debug.Log(" Player: " + uiforPlayer.entityPlayer.ToString());
        // The respondent is an EntityNPC, and we don't have that. Check for the patch scripted otherEntitySDX.
        Debug.Log(" HireInformationPopUpSDX:  Respondent: " + uiforPlayer.xui.Dialog.Respondent);
        Entity respondent = uiforPlayer.xui.Dialog.Respondent;
        if (respondent != null)
        {
            Debug.Log(" Respondent: " + respondent.ToString());
            EntityAliveSDX myEntity = (EntityAliveSDX)respondent;//uiforPlayer.entityPlayer.world.GetEntity(respondent.entityId) as EntityAliveSDX;
            if (myEntity != null)
            {
                Debug.Log(" Setting Hire Information");
                this.hireInformationLabel.Text = "Hire " + myEntity.EntityName + " for " + myEntity.GetHireCost() + " " + myEntity.GetHireCurrency().ItemClass.GetLocalizedItemName() + "?";
            }
        }
        else
            Debug.Log(" Respondent is null");

      
        base.OnOpen();
       
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
                myEntity.Hire(uiforPlayer.entityPlayer as EntityPlayerLocal);
            }
        }

        base.xui.playerUI.windowManager.Close(this.windowGroup.ID);
    }

    private void BtnCancelHireInformation_OnPressed(XUiController _sender, OnPressEventArgs _onPressEventArgs)
    {
        this.hireInformationPanel.IsVisible = false;
        base.xui.playerUI.windowManager.Close(this.windowGroup.ID);
    }

    public override void OnClose()
    {
        if (base.xui.playerUI.windowManager.Contains("dialog") && base.xui.playerUI.windowManager.IsWindowOpen("dialog"))
        {
            base.xui.playerUI.windowManager.Close("dialog");
        }
        base.OnClose();
    }


   
}

