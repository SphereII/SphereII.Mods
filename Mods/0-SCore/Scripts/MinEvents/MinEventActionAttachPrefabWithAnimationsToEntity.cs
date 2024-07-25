using UnityEngine;
public class MinEventActionAttachPrefabWithAnimationsToEntity : MinEventActionAttachPrefabToEntity
{
    public override void Execute(MinEventParams _params)
    {
        //if (_params.Self.RootTransform != null )
        if (_params.Self != null || _params.Self.RootTransform != null)
        {
            Animator[] componentsInChildren = _params.Self.RootTransform.GetComponentsInChildren<Animator>();
            for (int i = componentsInChildren.Length - 1; i >= 0; i--)
            {
                Animator animator = componentsInChildren[i];
                animator.enabled = true;
                if (animator.gameObject.GetComponent<BlockClockScript>() == null)
                    animator.gameObject.AddComponent<BlockClockScript>();
            }
        }

        base.Execute(_params);
    }
}
