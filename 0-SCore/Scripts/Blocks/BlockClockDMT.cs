using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

public class BlockClockDMT : Block
{
    public override void ForceAnimationState(BlockValue _blockValue, BlockEntityData _ebcd)
    {
        if (_ebcd != null && _ebcd.bHasTransform)
        {
            Animator[] componentsInChildren = _ebcd.transform.GetComponentsInChildren<Animator>();
            for (int i = componentsInChildren.Length - 1; i >= 0; i--)
            {
                Animator animator = componentsInChildren[i];
                animator.enabled = true;
                if (animator.gameObject.GetComponent<BlockClockScript>() == null)
                    animator.gameObject.AddComponent<BlockClockScript>();
            }
        }
    }
}
