using UnityEngine;

class ItemActionLauncherSDX : ItemActionLauncher
{
    public override void StartHolding(ItemActionData _action)
    {
        // Launchers being held by NPCs don't appear to be loading their ammo properly on spawn in, so they are just shooting blanks.
        if (_action.invData.itemValue.Meta == 0)
            _action.invData.itemValue.Meta = 1;
        base.StartHolding(_action);
    }

    public override void ItemActionEffects(GameManager _gameManager, ItemActionData _actionData, int _firingState, Vector3 _startPos, Vector3 _direction, int _userData = 0)
    {
        base.ItemActionEffects(_gameManager, _actionData, _firingState, _startPos, _direction, _userData);
        ItemActionRanged.ItemActionDataRanged itemActionDataRanged = (ItemActionRanged.ItemActionDataRanged)_actionData;
        itemActionDataRanged.invData.itemValue.Meta = 0;
    }

    public override void ExecuteAction(ItemActionData _actionData, bool _bReleased)
    {
        base.ExecuteAction(_actionData, _bReleased);
        if (_bReleased)
            return;
        var itemActionDataRanged = (ItemActionRanged.ItemActionDataRanged)_actionData;
        if (itemActionDataRanged.isReloading)
            return;

        // We were having trouble getting the NPCs to properly reload, so here we go!
        StartHolding(_actionData);
    }
}