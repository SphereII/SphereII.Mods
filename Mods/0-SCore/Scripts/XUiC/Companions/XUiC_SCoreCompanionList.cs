using System.Collections.Generic;

public class XUiC_SCoreCompanionList : XUiController
{
    private List<XUiC_SCoreCompanion> entryList = new List<XUiC_SCoreCompanion>();
    public static string ID = "";
    private EntityPlayerLocal _entityPlayerLocal;
    private XUiC_ToggleButton _toggleNpcFootsteps;

    public override void Init()
    {
        base.Init();
        ID = WindowGroup.ID;

        var childrenByType = GetChildrenByType<XUiC_SCoreCompanion>(null);
        foreach (var t in childrenByType)
        {
            t.ViewComponent.IsVisible = false;
            entryList.Add(t);
        }

        _toggleNpcFootsteps = GetChildById("toggleNPCFootsteps").GetChildByType<XUiC_ToggleButton>();
        _toggleNpcFootsteps.OnValueChanged += delegate(XUiC_ToggleButton sender, bool b)
        {
            _entityPlayerLocal.Buffs.AddCustomVar("quietNPC", b ? 1 : 0);
        };

    }

    public override void OnOpen()
    {
        base.OnOpen();
        _entityPlayerLocal = xui.playerUI.entityPlayer;
        if (_entityPlayerLocal == null)
        {
            OnClose();
            return;
        }
        RefreshCompanions();
        _toggleNpcFootsteps.Value = _entityPlayerLocal.Buffs.GetCustomVar("quietNPC") > 0f;

    }

    private void RefreshCompanions()
    {
        foreach (var t in entryList)
        {
            t.SetCompanion(null);
            t.ViewComponent.IsVisible = false;
        }

        var j = 0;
        // Loop around the cvars instead of Companions, as an NPC who is told to stay will be removed as a companion.
        foreach (var cvar in _entityPlayerLocal.Buffs.CVars)
        {
            if (!cvar.Key.StartsWith("hired_")) continue;
            var entityAlive = GameManager.Instance.World.GetEntity((int)cvar.Value) as EntityAliveSDX;
            if (!entityAlive) continue;
            if (!EntityUtilities.IsHired(entityAlive.entityId)) continue;
            var leader = EntityUtilities.GetLeaderOrOwner(entityAlive.entityId);
            if (leader == null) continue;
            if ( leader.entityId != _entityPlayerLocal.entityId) continue;
            
            entryList[j].SetCompanion(entityAlive);
            entryList[j].ViewComponent.IsVisible = true;
            j++;
        }
    }
}