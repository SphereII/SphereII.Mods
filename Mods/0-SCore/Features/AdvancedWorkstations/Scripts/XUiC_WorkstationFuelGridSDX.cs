using System;
using Audio;
using Platform;

public class XUiC_WorkstationFuelGridSDX : XUiC_WorkstationFuelGrid
{
    public override void Init()
    {
        base.Init();
        itemControllers = new XUiC_ItemStack[1];
        
    }
    
    public new bool AddItem(ItemClass _itemClass, ItemStack _itemStack)
    {
        return false;
    }

    public new bool TryStackItem(int startIndex, ItemStack _itemStack)
    {
        return false;
            
    }


    public override void Update(float _dt)
    {
        
        if (base.xui.playerUI.playerInput.GUIActions.WindowPagingRight.WasPressed && PlatformManager.NativePlatform.Input.CurrentInputStyle != PlayerInputManager.InputStyle.Keyboard)
        {
            this.Button_OnPress(null, 0);
        }
        //base.Update(_dt);
    }

    public override void SetSlots(ItemStack[] stacks)
    {
        
    }

    public override ItemStack[] GetSlots()
    {
        return new []{ItemStack.Empty };
    }

    public override bool HasRequirement(Recipe recipe)
    {
        return workstationData.GetBurnTimeLeft() > 0f;
    }
}
