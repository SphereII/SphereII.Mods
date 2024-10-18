The fire manager is a feature in the game that allows blocks to catch on fire, spread to neighboring flammable blocks, and eventually extinguish. 

It uses the CheckInterval time to deal damage to burning blocks and update their state. The fire manager also handles block explosions when they 
take too much damage. You can configure the fire manager's settings for different blocks through the blocks.xml file.

The settings in the blocks.xml related to the FireManager are:

- FireEnable: This property determines whether the fire manager is enabled or not. If it's set to false, the fire manager won't work for that block.
- CheckInterval: This property defines how often the fire manager checks a burning block for damage. A higher value means less frequent checks and less fire spreading. The default value is 20.
- FireDamage: This property defines how much damage a burning block takes from the fire manager. It can be overridden by specific block settings or the default value may apply.
- FireSpread: This property determines if fire can spread to neighboring flammable blocks. If it's set to false, only the burning block will burn and not its neighbors. The default value is true.
- ChanceToExtinguish: This property defines the chance of a burning block to self-extinguish over time. It can be overridden by specific block settings or the default value may apply. A higher value means less likely to extinguish. The default value is 0.05.

These properties can also have block-specific configurations that override the global values for that particular block. 

For example, you can set FireDowngradeBlock to a different block than air to specify what block a burning block will downgrade to when destroyed by fire.