// Token: 0x0200097B RID: 2427

public class XUiC_DialogWindowGroupSDX : XUiController
{
    // Token: 0x04003AE1 RID: 15073
    private XUiC_DialogResponseList responseWindow;

    // Token: 0x04003AE0 RID: 15072
    private XUiC_DialogStatementWindow statementWindow;

    // Token: 0x17000794 RID: 1940
    // (get) Token: 0x060049BE RID: 18878 RVA: 0x001F5E10 File Offset: 0x001F4010
    // (set) Token: 0x060049BF RID: 18879 RVA: 0x001F5E18 File Offset: 0x001F4018
    public Dialog CurrentDialog { get; private set; }

    // Token: 0x060049C0 RID: 18880 RVA: 0x001F5E24 File Offset: 0x001F4024
    public override void Init()
    {
        base.Init();
        statementWindow = GetChildByType<XUiC_DialogStatementWindow>();
        responseWindow = GetChildByType<XUiC_DialogResponseList>();
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
        if (xui.playerUI.windowManager.IsWindowOpen("windowpaging")) xui.playerUI.windowManager.Close("windowpaging");
        if (xui.playerUI.windowManager.Contains("compass") && xui.playerUI.windowManager.IsWindowOpen("compass")) xui.playerUI.windowManager.Close("compass");
        if (xui.playerUI.windowManager.Contains("toolbelt") && xui.playerUI.windowManager.IsWindowOpen("toolbelt")) xui.playerUI.windowManager.Close("toolbelt");
        CurrentDialog = Dialog.DialogList["humanEveBandit"];

        CurrentDialog.CurrentOwner = xui.Dialog.Respondent;
        CurrentDialog.RestartDialog(xui.playerUI.entityPlayer);
        statementWindow.CurrentDialog = CurrentDialog;
        responseWindow.CurrentDialog = CurrentDialog;
        GameManager.Instance.SetToolTipPause(xui.playerUI.nguiWindowManager, true);
    }

    // Token: 0x060049C3 RID: 18883 RVA: 0x001F5FEC File Offset: 0x001F41EC
    public override void OnClose()
    {
        base.OnClose();
        if (xui.playerUI.windowManager.Contains("questOffer") && xui.playerUI.windowManager.IsWindowOpen("questOffer")) xui.playerUI.windowManager.Close("questOffer");
        if (xui.playerUI.windowManager.Contains("compass") && !xui.playerUI.windowManager.IsWindowOpen("compass")) xui.playerUI.windowManager.Open("compass", false);
        if (xui.playerUI.windowManager.Contains("toolbelt") && !xui.playerUI.windowManager.IsWindowOpen("toolbelt")) xui.playerUI.windowManager.Open("toolbelt", false);
        xui.Dialog.Respondent = null;
        GameManager.Instance.SetToolTipPause(xui.playerUI.nguiWindowManager, false);
    }

    // Token: 0x060049C4 RID: 18884 RVA: 0x001F6128 File Offset: 0x001F4328
    public void RefreshDialog()
    {
        statementWindow.Refresh();
        if (CurrentDialog.CurrentStatement != null)
        {
            statementWindow.Refresh();
            responseWindow.Refresh();
            return;
        }

        xui.playerUI.windowManager.Close("dialog");
    }

    // Token: 0x060049C5 RID: 18885 RVA: 0x001F6180 File Offset: 0x001F4380
    public void ShowResponseWindow(bool isVisible)
    {
        responseWindow.Parent.ViewComponent.IsVisible = isVisible;
    }
}