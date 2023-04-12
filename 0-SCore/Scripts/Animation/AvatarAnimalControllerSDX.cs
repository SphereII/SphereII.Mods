using UnityEngine;

/// <summary>
/// Custom Animation Class for animals. Deprecated.
/// </summary>
internal class AvatarAnimalControllerSDX : AvatarAnimalController
{
    private Transform _rightHand;

    // This controls the animations if we are holding a weapon.
    private Animator _rightHandAnimator;
    private Transform _rightHandItemTransform;

    public override void SwitchModelAndView(string _modelName, bool _bFPV, bool _bMale)
    {
        base.SwitchModelAndView(_modelName, _bFPV, _bMale);
        _rightHand = bipedTransform.FindInChilds(entity.GetRightHandTransformName());
        if (_rightHandItemTransform == null) return;
        _rightHandItemTransform.parent = _rightHand;
        var position = AnimationGunjointOffsetData.AnimationGunjointOffset[entity.inventory.holdingItem.HoldType.Value].position;
        var rotation = AnimationGunjointOffsetData.AnimationGunjointOffset[entity.inventory.holdingItem.HoldType.Value].rotation;
        _rightHandItemTransform.localPosition = position;
        _rightHandItemTransform.localEulerAngles = rotation;
        if (entity.inventory.holdingItem.HoldingItemHidden)
            _rightHandItemTransform.localScale = new Vector3(0, 0, 0);
    }

    public override void SetInRightHand(Transform _transform)
    {
        idleTime = 0f;
        if (_transform != null) _transform.parent = _rightHand;
        _rightHandItemTransform = _transform;
        _rightHandAnimator = _transform != null ? _transform.GetComponent<Animator>() : null;
        if (_rightHandAnimator != null) _rightHandAnimator.logWarnings = false;
        if (_rightHandItemTransform != null) Utils.SetLayerRecursively(_rightHandItemTransform.gameObject, 0);
    }

    public override Transform GetRightHandTransform()
    {
        return _rightHandItemTransform;
    }

    public override void StartAnimationReloading()
    {
        idleTime = 0f;
        if (bipedTransform == null || !bipedTransform.gameObject.activeInHierarchy)
            return;
        var value = EffectManager.GetValue(PassiveEffects.ReloadSpeedMultiplier, entity.inventory.holdingItemItemValue, 1f, entity);
        SetBool("Reload", true);
        SetFloat("ReloadSpeed", value);

        // Work around for the Ranged2 EAI Task that needs meta to be greater than 0 to fire.
        // The same EAI task decrements the meta flag for each bullet it consumes.
        entity.inventory.holdingItemItemValue.Meta = 1;
    }

    public override void StartAnimationFiring()
    {
        base.StartAnimationFiring();
        SetTrigger("WeaponFire");
    }
}