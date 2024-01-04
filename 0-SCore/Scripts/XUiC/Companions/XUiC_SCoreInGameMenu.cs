
    using UnityEngine;

    public class XUiC_SCoreInGameMenu : XUiController
    {

        private XUiC_SimpleButton _btnSCoreOptions;
        private XUiC_SimpleButton _btnNpcView;


        public override void Init()
        {
            base.Init();
            XUiC_InGameMenuWindow.ID = WindowGroup.ID;
            _btnSCoreOptions = GetChildById("btnSCoreOptions").GetChildByType<XUiC_SimpleButton>();
            _btnSCoreOptions.OnPressed += BtnSCoreOptions_OnPressed;
            _btnNpcView = GetChildById("btnNPCView").GetChildByType<XUiC_SimpleButton>();
            _btnNpcView.OnPressed += BtnNPCView_OnPressed;
        }

        private void BtnNPCView_OnPressed(XUiController sender, int mousebutton)
        {
            xui.playerUI.windowManager.Open(XUiC_SCoreCompanionList.ID,true);
        }


        private void BtnSCoreOptions_OnPressed(XUiController sender, int mousebutton)
        {
            xui.playerUI.windowManager.Open(XUiC_SCoreUtilities.ID, true, false, true);
        }
    }    
