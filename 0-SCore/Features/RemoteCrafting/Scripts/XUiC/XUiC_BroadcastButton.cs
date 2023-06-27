using System.Linq;
using JetBrains.Annotations;
using SCore.Features.RemoteCrafting.Scripts;

// Code from Laydor slightly modified
public class XUiC_BroadcastButton : XUiController
{
    private static readonly string AdvFeatureClass = "AdvancedRecipes";

    private XUiV_Button _button;

    public override void Init()
    {
        base.Init();
        _button = viewComponent as XUiV_Button;
        OnPress += Grab_OnPress;
    }

    public override void Update(float dt)
    {
        base.Update(dt);
        if (!IsDirty) return;
        IsDirty = false;
        SetupButton();
    }

    public override void OnOpen()
    {
        base.OnOpen();
        //if debug enabled show lootList name of container
        if (Configuration.CheckFeatureStatus(AdvFeatureClass, "Debug"))
        {
            if (xui.lootContainer != null)
            {
                Log.Out("Current Container name: " + xui.lootContainer.lootListName);
            }
        }

        IsDirty = true;
    }

    private void Grab_OnPress(XUiController sender, int mouseButton)
    {
        //Check if Broadcastmanager is running
        if (!Broadcastmanager.HasInstance) return;

        if (Broadcastmanager.Instance.Check(xui.lootContainer.ToWorldPos()))
        {
            //Unselect button
            _button.Selected = true;
            // Remove from Broadcastmanager dictionary
            Broadcastmanager.Instance.remove(xui.lootContainer.ToWorldPos());
        }
        else
        {
            //Select button
            _button.Selected = false;
            // Add to Broadcastmanager dictionary
            Broadcastmanager.Instance.add(xui.lootContainer.ToWorldPos());
        }
    }

    private void SetupButton()
    {
        //Unselect button and disable it
        _button.Enabled = false;
        _button.Selected = false;
        _button.IsVisible = false;

        var disabledsender = Configuration.GetPropertyValue(AdvFeatureClass, "disablesender").Split(',');
        var bindToWorkstation = Configuration.GetPropertyValue(AdvFeatureClass, "bindtoWorkstation").Split(';');
        if (xui.lootContainer == null || !Broadcastmanager.HasInstance ||
            xui.vehicle != null ||
            GameManager.Instance.World.GetEntity(xui.lootContainer.entityId) is EntityAliveSDX ||
            GameManager.Instance.World.GetEntity(xui.lootContainer.entityId) is EntityDrone) return;

        if (disabledsender[0] != null)
        {
            if (RemoteCraftingUtils.DisableSender(disabledsender, xui.lootContainer))
            {
                return;
            }
        }

        if (!string.IsNullOrEmpty(bindToWorkstation[0]))
        {
            var counter = 0;
            foreach (var bind in bindToWorkstation)
            {
                var bindings = bind.Split(':')[1].Split(',');
                if (bindings.Any(x => x.Trim() == xui.lootContainer.lootListName)) counter++;
            }

            if (counter == 0 &&
                bool.Parse(Configuration.GetPropertyValue(AdvFeatureClass, "enforcebindtoWorkstation")))
            {
                return;
            }
        }

        //Enable button and set if button is selected
        _button.IsVisible = true;
        _button.Enabled = true;
        _button.Selected = !Broadcastmanager.Instance.Check(xui.lootContainer.ToWorldPos());
    }
}