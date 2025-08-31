using System;
using System.Collections;
using UAI;
using UnityEngine;

public class XUiC_SCoreCompanion : XUiController
{
    private float distance;
    private EntityAlive Companion { get; set; }
    private string lastBuff = String.Empty;
    private string currentBuff = String.Empty;

    private readonly CachedStringFormatterXuiRgbaColor itemicontintcolorFormatter = new CachedStringFormatterXuiRgbaColor();

    private readonly CachedStringFormatter<float> distanceFormatter = new CachedStringFormatter<float>(delegate(float _f)
    {
        if (_f > 1000f)
        {
            return (_f / 1000f).ToCultureInvariantString("0.0") + " KM";
        }

        return _f.ToCultureInvariantString("0.0") + " M";
    });

    private XUiC_SimpleButton btnFollow;
    private XUiC_SimpleButton btnStayHere;
    private XUiC_SimpleButton btnStayThere;
    private XUiC_SimpleButton btnDismiss;
    private XUiC_SimpleButton btnTeleport;
    
    private float updateLimiter;


    public override void Init()
    {
        base.Init();
        IsDirty = true;

        btnFollow = (XUiC_SimpleButton)GetChildById("btnFollow");
        if (btnFollow != null)
            btnFollow.OnPressed += BtnFollow_Controller_OnPress;

        btnStayHere = (XUiC_SimpleButton)GetChildById("btnStayHere");
        if (btnStayHere != null)
            btnStayHere.OnPressed += BtnStayHere_Controller_OnPress;

        btnStayThere = (XUiC_SimpleButton)GetChildById("btnStayThere");
        if (btnStayThere != null)
            btnStayThere.OnPressed += BtnStayThere_Controller_OnPress;

        btnDismiss = (XUiC_SimpleButton)GetChildById("btnDismiss");
        if (btnDismiss != null)
            btnDismiss.OnPressed += BtnDismiss_Controller_OnPress;
        
        btnTeleport = (XUiC_SimpleButton)GetChildById("btnTeleport");
        if (btnTeleport != null)
            btnTeleport.OnPressed += BtnTeleport_Controller_OnPress;

    }

    private void BtnTeleport_Controller_OnPress(XUiController _sender, int _mousebutton)
    {
        if (IsInRange())
        {
            if (EntityUtilities.TeleportNow(Companion.entityId, xui.playerUI.entityPlayer, 50))
                return;
        }

        GameManager.ShowTooltip(xui.playerUI.entityPlayer, Localization.Get("xuiSCoreOutOfRange"));
        
    }

    private void BtnDismiss_Controller_OnPress(XUiController _sender, int _mousebutton)
    {
        GameManager.ShowTooltip(xui.playerUI.entityPlayer, Localization.Get("xuiSCoreDismissing"));
        EntityUtilities.ExecuteCMD(Companion.entityId, "Dismiss", xui.playerUI.entityPlayer);
        Companion.Buffs.AddBuff("buffOrderDismiss");
        this.ViewComponent.IsVisible = false;
    }

    private void BtnStayThere_Controller_OnPress(XUiController _sender, int _mousebutton)
    {
        ApplyCommand("buffOrderGuardHere");
    }

    private void BtnStayHere_Controller_OnPress(XUiController _sender, int _mousebutton)
    {
        ApplyCommand("buffOrderStay");
    }

    private void BtnFollow_Controller_OnPress(XUiController _sender, int _mouseButton)
    {
        ApplyCommand("buffOrderFollow");
    }

    private void ApplyCommand(string buff)
    {
        if (IsInRange())
        {
            Companion.Buffs.AddBuff(buff);
            return;
        }

        GameManager.ShowTooltip(xui.playerUI.entityPlayer, Localization.Get("xuiSCoreOutOfRange"));
    }

    public override void Update(float _dt)
    {
        base.Update(_dt);
        updateLimiter -= _dt;
        if (!(updateLimiter < 0f)) return;
        updateLimiter = 1f;
        UpdateStatus();
        RefreshBindings(true);
    }

    public void SetCompanion(EntityAlive entity)
    {
        Companion = entity;

        if (Companion == null)
        {
            RefreshBindings(true);
            return;
        }

        UpdateStatus();
        RefreshBindings(true);
        IsDirty = true;
    }

    private bool IsInRange()
    {
        if (Companion.IsDead()) return true;

        return distance < 50;
    }

    private void UpdateStatus()
    {
        if (Companion == null) return;

        var entityPlayer = xui.playerUI.entityPlayer;
        var magnitude = (Companion.GetPosition() - entityPlayer.GetPosition()).magnitude;
        distance = magnitude;
    }

    public override void OnOpen()
    {
        base.OnOpen();
        IsDirty = true;
        RefreshBindings(true);
    }


    public override bool GetBindingValueInternal(ref string value, string bindingName)
    {
        switch (bindingName)
        {
            case "name" when Companion == null:
                value = "";
                return true;
            case "name":
                value = Companion.EntityName;
                return true;
            case "currentorder":
                if (Companion == null)
                {
                    value = "";
                    return true;
                }

                var currentOrder = EntityUtilities.GetCurrentOrder(Companion.entityId);
                value = $"{currentOrder}";
                return true;

            case "distancecolor":
            {
                var color2 = Color.white;
                if (Companion == null)
                {
                    value = "";
                    return true;
                }

                var leader = EntityUtilities.GetLeaderOrOwner(Companion.entityId);
                if (leader == null)
                {
                    value = "";
                    return true;
                }

                if (distance > 50f)
                {
                    color2 = Color.red;
                }

                value = itemicontintcolorFormatter.Format(color2);
                return true;
            }
            case "distance":
            {
                value = distanceFormatter.Format(this.distance);
                return true;
            }
            default:
                return false;
        }
    }
}