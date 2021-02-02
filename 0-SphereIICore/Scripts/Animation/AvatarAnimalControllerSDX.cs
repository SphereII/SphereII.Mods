using UnityEngine;

class AvatarAnimalControllerSDX : AvatarAnimalController
{
    // This controls the animations if we are holding a weapon.
    protected Animator rightHandAnimator;
    private Transform rightHandItemTransform;
    private Transform rightHand;

    public override void SwitchModelAndView(string _modelName, bool _bFPV, bool _bMale)
    {
        base.SwitchModelAndView(_modelName, _bFPV, _bMale);
        rightHand = bipedTransform.FindInChilds(entity.GetRightHandTransformName(), false);
        if (rightHandItemTransform != null)
        {
            rightHandItemTransform.parent = rightHand;
            Vector3 position = AnimationGunjointOffsetData.AnimationGunjointOffset[entity.inventory.holdingItem.HoldType.Value].position;
            Vector3 rotation = AnimationGunjointOffsetData.AnimationGunjointOffset[entity.inventory.holdingItem.HoldType.Value].rotation;
            rightHandItemTransform.localPosition = position;
            rightHandItemTransform.localEulerAngles = rotation;
            if (entity.inventory.holdingItem.HoldingItemHidden)
                rightHandItemTransform.localScale = new Vector3(0, 0, 0);

        }
    }
    public override void SetInRightHand(Transform _transform)
    {
        idleTime = 0f;
        if (_transform != null)
        {
            _transform.parent = rightHand;
        }
        rightHandItemTransform = _transform;
        rightHandAnimator = ((_transform != null) ? _transform.GetComponent<Animator>() : null);
        if (rightHandAnimator != null)
        {
            rightHandAnimator.logWarnings = false;
        }
        if (rightHandItemTransform != null)
        {
            Utils.SetLayerRecursively(rightHandItemTransform.gameObject, 0);
        }
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
        float value = EffectManager.GetValue(PassiveEffects.ReloadSpeedMultiplier, entity.inventory.holdingItemItemValue, 1f, entity, null, default(FastTags), true, true, true, true, 1, true);
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

