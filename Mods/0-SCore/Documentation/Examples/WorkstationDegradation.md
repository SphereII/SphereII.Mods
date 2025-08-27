### Workstation Degradation

In addition to items, SCore now supports the degradation of **workstations**. Patches to the `TileEntityWorkstation`'s
`UpdateTick` allow a workstation to degrade over time.

This is done by adding an `effect_group` to the `block.xml` definition. The SCore system keeps track of which blocks
have these effect groups, allowing the `ItemModificationDegradation` system to check if a block is valid for degradation
and then trigger an `onSelfItemDegrade` effect. This approach is better than having the workstation degrade on every
tick, as it allows for more fine-grained requirements, such as a random roll.

**Example: Campfire Degradation**

Here's an example of a campfire that can degrade into a `cntCollapsedCampfire` and then be upgraded back.

```xml

<append xpath="//block[@name='campfire']">
    <property name="DowngradeBlock" value="cntCollapsedCampfire"/>
    <property name="DegradationPerUse" value="1"/>
    <effect_group name="DamageHooks Degrade">
        <triggered_effect trigger="onSelfItemDegrade" action="DamageBlock, SCore">
            <requirement name="RandomRoll" seed_type="Random" min_max="0,100" operation="LTE" value="10"/>
        </triggered_effect>
    </effect_group>
</append>

<append xpath="//block[@name='cntCollapsedCampfire']">
<property class="UpgradeBlock">
    <property name="ToBlock" value="campfire"/>
    <property name="Item" value="resourceCobblestones"/>
    <property name="ItemCount" value="5"/>
    <property name="UpgradeHitCount" value="4"/>
</property>
</append>

<csv xpath="//block[@name='cntCollapsedCampfire']/property[@name='Extends']/@param1" op="add">DowngradeBlock</csv>
```