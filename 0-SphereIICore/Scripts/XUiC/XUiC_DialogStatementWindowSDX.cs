using System;
using UnityEngine;

// Token: 0x0200097B RID: 2427
public class XUiC_DialogWindowGroupSDX : XUiController
{
    // Token: 0x17000794 RID: 1940
    // (get) Token: 0x060049BE RID: 18878 RVA: 0x001F5E10 File Offset: 0x001F4010
    // (set) Token: 0x060049BF RID: 18879 RVA: 0x001F5E18 File Offset: 0x001F4018
    public Dialog CurrentDialog
    {
        get; private set;
    }

    // Token: 0x060049C0 RID: 18880 RVA: 0x001F5E24 File Offset: 0x001F4024
    public override void Init()
    {
        base.Init();
        this.statementWindow = base.GetChildByType<XUiC_DialogStatementWindow>();
        this.responseWindow = base.GetChildByType<XUiC_DialogResponseList>();
    }

    // Token: 0x060049C1 RID: 18881 RVA: 0x001F5E44 File Offset: 0x001F4044
    public override void Update(float _dt)
    {
        base.Update(_dt);
    }

    // Token: 0x060049C2 RID: 18882
    public override void OnOpen()
    {
        base.OnOpen();
        if (base.xui.playerUI.windowManager.IsWindowOpen("windowpaging"))
        {
            base.xui.playerUI.windowManager.Close("windowpaging");
        }
        if (base.xui.playerUI.windowManager.Contains("compass") && base.xui.playerUI.windowManager.IsWindowOpen("compass"))
        {
            base.xui.playerUI.windowManager.Close("compass");
        }
        if (base.xui.playerUI.windowManager.Contains("toolbelt") && base.xui.playerUI.windowManager.IsWindowOpen("toolbelt"))
        {
            base.xui.playerUI.windowManager.Close("toolbelt");
        }
        this.CurrentDialog = Dialog.DialogList["humanEveBandit"];
     
        this.CurrentDialog.CurrentOwner = base.xui.Dialog.Respondent;
        this.CurrentDialog.RestartDialog(base.xui.playerUI.entityPlayer);
        this.statementWindow.CurrentDialog = this.CurrentDialog;
        this.responseWindow.CurrentDialog = this.CurrentDialog;
        GameManager.Instance.SetToolTipPause(base.xui.playerUI.nguiWindowManager, true);
    }

    // Token: 0x060049C3 RID: 18883 RVA: 0x001F5FEC File Offset: 0x001F41EC
    public override void OnClose()
    {
        base.OnClose();
        if (base.xui.playerUI.windowManager.Contains("questOffer") && base.xui.playerUI.windowManager.IsWindowOpen("questOffer"))
        {
            base.xui.playerUI.windowManager.Close("questOffer");
        }
        if (base.xui.playerUI.windowManager.Contains("compass") && !base.xui.playerUI.windowManager.IsWindowOpen("compass"))
        {
            base.xui.playerUI.windowManager.Open("compass", false, false, true);
        }
        if (base.xui.playerUI.windowManager.Contains("toolbelt") && !base.xui.playerUI.windowManager.IsWindowOpen("toolbelt"))
        {
            base.xui.playerUI.windowManager.Open("toolbelt", false, false, true);
        }
        base.xui.Dialog.Respondent = null;
        GameManager.Instance.SetToolTipPause(base.xui.playerUI.nguiWindowManager, false);
    }

    // Token: 0x060049C4 RID: 18884 RVA: 0x001F6128 File Offset: 0x001F4328
    public void RefreshDialog()
    {
        this.statementWindow.Refresh();
        if (this.CurrentDialog.CurrentStatement != null)
        {
            this.statementWindow.Refresh();
            this.responseWindow.Refresh();
            return;
        }
        base.xui.playerUI.windowManager.Close("dialog");
    }

    // Token: 0x060049C5 RID: 18885 RVA: 0x001F6180 File Offset: 0x001F4380
    public void ShowResponseWindow(bool isVisible)
    {
        this.responseWindow.Parent.ViewComponent.IsVisible = isVisible;
    }

    // Token: 0x04003AE0 RID: 15072
    private XUiC_DialogStatementWindow statementWindow;

    // Token: 0x04003AE1 RID: 15073
    private XUiC_DialogResponseList responseWindow;
}
