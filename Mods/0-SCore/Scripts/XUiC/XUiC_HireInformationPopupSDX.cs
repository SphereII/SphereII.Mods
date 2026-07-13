using System.Collections;

internal class XUiC_HireInformationPopupSDX : XUiController
{
    public XUiV_Label hireInformationLabel;
    public XUiV_Panel hireInformationPanel;


    public override void Init()
    {
        base.Init();
        hireInformationPanel = (XUiV_Panel)GetChildById("HireInformationPopup").ViewComponent;
        ((XUiC_SimpleButton)hireInformationPanel.Controller.GetChildById("btnCancel")).OnPressed += BtnCancelHireInformation_OnPressed;
        ((XUiC_SimpleButton)hireInformationPanel.Controller.GetChildById("btnConfirm")).OnPressed += BtnConfirmHireInformation_OnPressed;
        hireInformationLabel = (XUiV_Label)hireInformationPanel.Controller.GetChildById("HireInformationLabel").ViewComponent;
    }

    public override void OnOpen()
    {
        EntityPlayer player = xui.playerUI.entityPlayer;

        var entityID = 0;
        if (player.Buffs.HasCustomVar("CurrentNPC"))
            entityID = (int)player.Buffs.GetCustomVar("CurrentNPC");

        if (entityID == 0)
            return;

        var myEntity = player.world.GetEntity(entityID) as IEntityAliveSDX;;
        if (myEntity != null)
            hireInformationLabel.Text = "Hire " + myEntity.EntityName + " for " + EntityUtilities.GetHireCost(entityID) + " " +
                                        EntityUtilities.GetHireCurrency(entityID).ItemClass.GetLocalizedItemName() + "?";

        base.OnOpen();
    }

    private void BtnConfirmHireInformation_OnPressed(XUiController _sender, int _mouseButton)
    {
        EntityPlayer player = xui.playerUI.entityPlayer;

        var entityID = 0;
        if (player.Buffs.HasCustomVar("CurrentNPC"))
            entityID = (int)player.Buffs.GetCustomVar("CurrentNPC");

        if (entityID == 0)
            return;

        var myEntity = player.world.GetEntity(entityID) as IEntityAliveSDX;
        if (myEntity != null)
            EntityUtilities.Hire(entityID, player as EntityPlayerLocal);

        xui.playerUI.windowManager.Close(windowGroup);

    }

    private void BtnCancelHireInformation_OnPressed(XUiController _sender, int _mouseButton)
    {
  //      
       xui.playerUI.windowManager.Close(windowGroup);
    }

   
}