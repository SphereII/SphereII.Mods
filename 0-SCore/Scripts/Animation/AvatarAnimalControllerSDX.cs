using UnityEngine;

/// <summary>
/// Custom Animation Class for animals. Deprecated.
/// </summary>
internal class AvatarAnimalControllerSDX : AvatarAnimalController
{
    private Transform rightHand;

    // This controls the animations if we are holding a weapon.
    protected Animator rightHandAnimator;
    private Transform rightHandItemTransform;

    public override void SwitchModelAndView(string _modelName, bool _bFPV, bool _bMale)
    {
        base.SwitchModelAndView(_modelName, _bFPV, _bMale);
        rightHand = bipedTransform.FindInChilds(entity.GetRightHandTransformName());
        if (rightHandItemTransform != null)
        {
            rightHandItemTransform.parent = rightHand;
            var position = AnimationGunjointOffsetData.AnimationGunjointOffset[entity.inventory.holdingItem.HoldType.Value].position;
            var rotation = AnimationGunjointOffsetData.AnimationGunjointOffset[entity.inventory.holdingItem.HoldType.Value].rotation;
            rightHandItemTransform.localPosition = position;
            rightHandItemTransform.localEulerAngles = rotation;
            if (entity.inventory.holdingItem.HoldingItemHidden)
                rightHandItemTransform.localScale = new Vector3(0, 0, 0);
        }
    }

    public override void SetInRightHand(Transform _transform)
    {
        idleTime = 0f;
        if (_transform != null) _transform.parent = rightHand;
        rightHandItemTransform = _transform;
        rightHandAnimator = _transform != null ? _transform.GetComponent<Animator>() : null;
        if (rightHandAnimator != null) rightHandAnimator.logWarnings = false;
        if (rightHandItemTransform != null) Utils.SetLayerRecursively(rightHandItemTransform.gameObject, 0);
    }

    public override Transform GetRightHandTransform()
    {
        return rightHandItemTransform;
    }

    public override void StartAnimationReloading()
    {
        idleTime = 0f;
        if (bipedTransform == null || !bipedTransform.gameObject.activeInHierarchy)
            return;
        var value = EffectManager.GetValue(PassiveEffects.ReloadSpeedMultiplier, entity.inventory.holdingItemItemValue, 1f, entity);
        UpdateBool("Reload", true);
        UpdateFloat("ReloadSpeed", value);

        // Work around for the Ranged2 EAI Task that needs meta to be greater than 0 to fire.
        // The same EAI task decrements the meta flag for each bullet it consumes.
        entity.inventory.holdingItemItemValue.Meta = 1;
    }

    public override void StartAnimationFiring()
    {
        base.StartAnimationFiring();
        TriggerEvent("WeaponFire");
    }
}