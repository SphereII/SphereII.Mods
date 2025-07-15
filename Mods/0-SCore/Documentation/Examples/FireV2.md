
## FireV2 System: A Modder's Guide to SCore's Fire Features

The FireV2 system, integrated through SCore, offers comprehensive tools for modders to control and customize fire
mechanics within the game. From ignitable materials and propagation rules to specialized fire-based entities and
extinguishing tools, FireV2 provides deep control over the game's pyro-dynamics.

### 1\. Core Fire Mechanics and Global Settings

The fundamental behavior of fire in your game is controlled through the `FireManagement` class within the
`ConfigFeatureBlock` in `Config/blocks.xml`. Modders can adjust these global settings to define how fire spreads, deals
damage, and appears visually.

* **Enabling and Timing**:
    * **`FireEnable`**: Set to `true` to activate all FireV2 mechanics.
    * **`CheckInterval`**: Defines the frequency (in game ticks/seconds) at which the system checks for fire propagation
      and updates. A lower value leads to faster, more aggressive fire behavior.
    * **`FireSpread`**: Allows fire to spread to neighbor blocks.
    * **Example (`Config/blocks.xml`)**:
      ```xml
      <set xpath="/blocks/block[@name='ConfigFeatureBlock']/property[@class='FireManagement']/property[@name='FireEnable']/@value">true</set>
      <set xpath="/blocks/block[@name='ConfigFeatureBlock']/property[@class='FireManagement']/property[@name='CheckInterval']/@value">20</set>
      <set xpath="/blocks/block[@name='ConfigFeatureBlock']/property[@class='FireManagement']/property[@name='FireSpread']/@value">true</set>

      ```
* **Damage and Susceptible Materials**:
    * **`FireDamage`**: Sets the base damage dealt by fire to both blocks and entities per check interval.
    * **`MaterialDamage`**: A comma-separated list of material types that fire can destroy or damage. If a block's
      material is not on this list, it will not take fire damage.
    * **`MaterialSurface`**: A list of materials on which fire particle effects can visually render or spread.
    * **Example (`Config/blocks.xml`)**:
      ```xml
      <set xpath="/blocks/block[@name='ConfigFeatureBlock']/property[@class='FireManagement']/property[@name='FireDamage']/@value">50</set>
      <set xpath="/blocks/block[@name='ConfigFeatureBlock']/property[@class='FireManagement']/property[@name='MaterialDamage']/@value">wood, cloth, corn, grass, plastic, leaves, cactus, mushroom, hay, paper, trash, backpack, organic</set>
      <set xpath="/blocks/block[@name='ConfigFeatureBlock']/property[@class='FireManagement']/property[@name='MaterialSurface']/@value">wood, cloth, corn, grass, plastic, leaves, cactus, mushroom, hay, paper, trash, backpack, organic</set>
      ```
* **Visuals and Particle Customization**:
    * **`SmokeTime`**: Duration for which smoke particles persist after a fire event.
    * **`SmokeParticle`**: The default particle prefab for smoke.
    * **`RandomFireParticle` & `RandomSmokeParticle`**: These allow for dynamic and varied fire/smoke visuals. Provide a
      comma-separated list of Unity3D prefab paths. Including "NoParticle" gives a chance for no particle effect.
    * **Example (`Config/blocks.xml`)**:
      ```xml
      <set xpath="/blocks/block[@name='ConfigFeatureBlock']/property[@class='FireManagement']/property[@name='SmokeTime']/@value">30</set>
      <set xpath="/blocks/block[@name='ConfigFeatureBlock']/property[@class='FireManagement']/property[@name='SmokeParticle']/@value">#@modfolder:Resources/guppySmokeParticles.unity3d?gupSmoke1.prefab</set>
      <append xpath="/blocks/block[@name='ConfigFeatureBlock']/property[@class='FireManagement']">    
        <property name="RandomFireParticle" value="#@modfolder(0-SCore_sphereii):Resources/gupFireParticles.unity3d?gupBeavis05-Heavy,#@modfolder(0-SCore_sphereii):Resources/gupFireParticles.unity3d?gupBeavis05-Heavy,#@modfolder(0-SCore_sphereii):Resources/gupFireParticles.unity3d?gupBeavis05-Heavy,#@modfolder(0-SCore_sphereii):Resources/gupFireParticles.unity3d?gupBeavis05-Heavy,#@modfolder(0-SCore_sphereii):Resources/gupFireParticles.unity3d?gupBeavis05-Heavy,#@modfolder:Resources/guppySmokeParticles.unity3d?gupSmoke1"/>
        <property name="RandomSmokeParticle" value="NoParticle,NoParticle,NoParticle,NoParticle,#@modfolder:Resources/guppySmokeParticles.unity3d?gupSmoke1,#@modfolder:Resources/guppySmokeParticles.unity3d?gupSmoke5,#@modfolder:Resources/guppySmokeParticles.unity3d?gupSmoke2"/>
      </append>
      ```

### 2\. Crafting and Integrating Flammable Items & Weapons

Modders can introduce custom flammable items and fire-generating weapons.

* **Flamethrower (`Config/items.xml`, `Config/recipes.xml`, `Config/Localization.txt`, `Config/sounds.xml`,
  `Config/progression.xml`)**:
    * **Item Definition**: Define a ranged weapon with `DisplayType="rangedGun"`. Use `Magazine_items` to specify its
      fuel type (e.g., `ammoNapalm`). The `Sound_start` and `Sound_loop` properties define the burning sounds.
    * **SCore Action**: The
      `triggered_effect trigger="onSelfPrimaryActionStart" action="AnimatorSetBool" target="self" property="Fire" value="true"`
      line, while an animator control, works in conjunction with FireV2's visual system.
    * **Example (`Config/items.xml`)**:
      ```xml
      <item name="guppyFlamethrower">
          <property name="Tags" value="weapon,ranged"/>
          <property name="DisplayType" value="rangedGun"/>
          <property name="HoldType" value="1"/>
          <property name="Meshfile" value="#@modfolder:Resources/guppyFlamethrower.unity3d?guppyFlamethrower.Prefab"/>
          <property class="Action0">
              <property name="Class" value="Ranged"/>
              <property name="Delay" value=".150"/>
              <property name="AutoReload" value="true"/>
              <property name="Magazine_items" value="ammoNapalm"/>
              <property name="Sound_start" value="gupFireBBQ"/>
              <property name="Sound_loop" value="gupFireBBQ"/>
              </property>
          <effect_group name="FlameThrower">
              <triggered_effect trigger="onSelfPrimaryActionStart" action="AnimatorSetBool" target="self" property="Fire" value="true"/>
              </effect_group>
      </item>
      ```
    * **Napalm Ammo (`ammoNapalm`)**: This item is crucial for flamethrowers. Its `effect_group` contains specific
      FireV2 actions:
        * `triggered_effect trigger="onSelfAttackedOther" action="AddBuff" target="other" buff="buffBurningMolotov"`:
          Applies a burning buff to entities hit.
        * `triggered_effect trigger="onSelfDamagedBlock" action="AddFireDamage, SCore"`: Directly calls SCore's
          `AddFireDamage` method on blocks hit, causing them to ignite or take fire damage.
        * **Example (`Config/items.xml`)**:
          ```xml
          <item name="ammoNapalm">
              <property name="Tags" value="gasoline"/>
              <effect_group name="Fire Proc" tiered="false">
                  <triggered_effect trigger="onSelfAttackedOther" action="AddBuff" target="other" buff="buffBurningMolotov"/>
                  <passive_effect name="EntityDamage" operation="base_set" value="10"/>
                  <passive_effect name="BlockDamage" operation="base_set" value="1"/>
                  <passive_effect name="DamageModifier" operation="perc_add" value="50" tags="wood"/> <triggered_effect trigger="onSelfDamagedBlock" action="AddFireDamage, SCore" />
              </effect_group>
          </item>
          ```
    * **Crafting & Sounds**: Define recipes in `Config/recipes.xml` (e.g., `ammoNapalm`, `guppyFlamethrower`). Add
      custom sounds for these items in `Config/sounds.xml` (e.g., `gupFireBBQ`). Integrate into progression skills in
      `Config/progression.xml`.
    * **Localization**: Add `Key,English` entries for your items and their descriptions in `Config/Localization.txt`.

### 3\. Fire-Prone Blocks and Environmental Hazards

Create blocks that can ignite or act as fire hazards.

* **Oil Slicks (`guppyOilSlickGoesInDitch`, `gupOilSlickGoesOnBlock`)**:
    * Define blocks with a custom `Material` (e.g., `Mguppyoil` from `Config/materials.xml`) that is designated as
      `organic` or a material susceptible to fire damage.
    * Set `FireDamage` property on the block to make it ignitable.
    * **Example (`Config/blocks.xml`)**:
      ```xml
      <block name="gupOilSlickGoesOnBlock">
          <property name="Material" value="Mguppyoil"/>
          <property name="Shape" value="ModelEntity"/>
          <property name="Model" value="#@modfolder:Resources/guppyOilSlick.unity3d?gupOilSlickGoesOnBlock.prefab"/>
          <property name="FireDamage" value="1" />
      </block>
      ```
    * **Material Definition (`Config/materials.xml`)**:
      ```xml
      <material id="Mguppyoil">
          <property name="damage_category" value="organic"/>
          <property name="surface_category" value="organic"/>
          </material>
      ```
* **Dynamic Campfires (`guppyCampfireRocks`, `guppyCampfireLogsOff`, `guppyCampfireLogsOn`, `guppyCampfireLogsOffPot`,
  `guppyCampfireLogsOnPot`)**:
    * These multi-stage blocks (`Config/blocks.xml`) demonstrate `FireDamage` for ignition and `FireDowngradeBlock` for
      transitioning states (e.g., logs becoming lit).
    * They utilize `FireParticle` to display flames.
    * **Workstations**: The related `workstation_guppyCampfireLogsOnPot` and `workstation_guppyCampfireLogsOn` entries
      in `Config/XUi/xui.xml` show how to make these blocks functional crafting stations when lit.
    * **Example (`Config/blocks.xml`)**:
      ```xml
      <block name="guppyCampfireLogsOff">
          <property name="Material" value="MguppyCampfire"/>
          <property name="FireDamage" value="1500" /> <property name="FireDowngradeBlock" value="guppyCampfireLogsOn" /> <property name="FireParticle" value="#@modfolder:Resources/gupCampfire.unity3d?guppyCampfireParticle" />
      </block>
      ```

### 4\. Extinguishing Fires

Modders can create custom tools and methods for fire suppression.

* **Fire Extinguisher Item (`guppyFireExtinguisherItem`)**:
    * Defined as a ranged weapon in `Config/items.xml` that uses `ammoCO2`.
    * **SCore Action**: The `ammoCO2` item's `effect_group` contains `triggered_effect` actions directly calling
      `RemoveFire, SCore` in an AOE, as well as `RemoveBuff` `buffIsOnFire`. This is the core of its extinguishing
      functionality.
    * **Example (`Config/items.xml`)**:
      ```xml
      <item name="ammoCO2">
          <property name="Tags" value="fireretardent"/>
          <effect_group name="CO2 Proc" tiered="false">
              <triggered_effect trigger="onSelfAttackedOther" action="RemoveBuff" target="other" buff="buffIsOnFire"/>
              <passive_effect name="EntityDamage" operation="base_set" value="0"/>
              <triggered_effect trigger="onSelfPrimaryActionRayHit" action="RemoveFire, SCore" target="positionAOE" range="5"/>
              <triggered_effect trigger="onSelfSecondaryActionEnd" action="RemoveFire, SCore" target="positionAOE" range="5"/>
          </effect_group>
      </item>
      ```
* **Water Buckets (`guppyWaterBucket`)**:
    * The `guppyWaterBucket` in `Config/items.xml` triggers the `buffWaterSpray` buff upon use.
    * **SCore Action**: `buffWaterSpray` (`Config/buffs.xml`) directly calls `RemoveFire, SCore` in an AOE.
    * **Example (`Config/buffs.xml`)**:
      ```xml
      <buff name="buffWaterSpray">
          <effect_group>
              <triggered_effect trigger="onSelfBuffStart" action="RemoveFire, SCore" target="positionAOE" range="5">
                  <requirement name="IsFPV"/>
              </triggered_effect>
              </effect_group>
      </buff>
      ```
* **Loot & Placement**: `guppyFireExtinguisherItem` is added to various loot groups in `Config/loot.xml` and can also
  appear as a placable block through `Config/blockplaceholders.xml`.

### 5\. Fire-Reactive Entities and Buffs

Entities can be set on fire, and you can define specific entities that interact with the fire system.

* **Burning Buffs (`guppyBurning`, `guppyIsOnFire`)**:
    * `guppyBurning` (`Config/buffs.xml`) is a hidden buff that applies damage over time and sets the visible
      `guppyIsOnFire` buff. `guppyIsOnFire` handles particle effects (`p_onFire`) and sounds (`buff_burn_lp`).
    * **Example (`Config/buffs.xml`)**:
      ```xml
      <buff name="guppyBurning" hidden="true">
          <damage_type value="heat"/>
          <effect_group>
              <triggered_effect trigger="onSelfBuffStart" action="AddBuff" buff="guppyIsOnFire"/>
              </effect_group>
      </buff>

      <buff name="guppyIsOnFire">
          <damage_type value="Heat"/>
          <effect_group name="run particles, cleanup">
              <triggered_effect trigger="onSelfBuffStart" action="AttachParticleEffectToEntity" particle="p_onFire" local_offset="0,0,0" parent_transform="LOD0" shape_mesh="true"/>
              <triggered_effect trigger="onSelfBuffStart" action="PlaySound" sound="buff_burn_lp"/>
              </effect_group>
      </buff>
      ```
* **Fire-Spreading Entities (`guppyFireNado`)**:
    * Define a new `entity_class` in `Config/entityclasses.xml` that extends an existing zombie type.
    * Assign it a specific buff, like `buffDiein30Fire` (`Config/buffs.xml`), which includes `AddFireDamage, SCore` to
      burn entities around it.
    * Integrate this entity into existing `entitygroups` (`Config/entitygroups.xml`) to make it spawn in specific biomes
      or events.
    * **Example (`Config/entityclasses.xml`)**:
      ```xml
      <entity_class name="guppyFireNado" extends="zombieSoldier">
          <property name="Tags" value="firenado"/>
          <property name="Mesh" value="#@modfolder:Resources/guppyFirenadoEntity.unity3d?guppyFireNadoNew.prefab"/>
          <property name="Buffs" value="buffDiein30Fire"/>
          </entity_class>
      ```
* **Burnt Entities (`guppyVultureBurnt`, `zombieBurnt`)**:
    * Create `entity_class` entries for burnt versions of entities. These often have `buffGupQuickFireEffectOnly` on
      spawn for a brief visual fire effect.
    * **CVar Control**: Modifiers to `zombieBuffStatusCheck01` and `buffStatusCheck01` in `Config/buffs.xml` use CVars (
      `spawnOverridezombieBurnt`, `NoSpawnOnDeath`) to manage particle effects and prevent re-spawning of burnt versions
      if they are already on fire.
    * **Example (`Config/entityclasses.xml`)**:
      ```xml
      <append xpath="/entity_classes/entity_class[@name='zombieBurnt']">
          <property name="Buffs" value="buffGupQuickFireEffectOnly"/>
          <effect_group>
              <triggered_effect trigger="onSelfFirstSpawn" action="ModifyCVar" target="self" cvar="alreadyBurned" operation="set" value="1"/>
          </effect_group>
      </append>
      ```

### 6\. Advanced Fire Management: The Water Plane

The FireV2 system can be extended to include larger-scale fire suppression mechanics.

* **`guppyWaterPlane` Entity (`Config/entityclasses.xml`)**:
    * This is a non-moving entity designed to trigger a large-area fire extinguishing effect.
    * It uses `buffStartWaterTimer` (`Config/buffs.xml`) which then adds `buffWaterPlaneSpray` (`Config/buffs.xml`).
    * **SCore Action**: `buffWaterPlaneSpray` directly calls `RemoveFire, SCore` with a wide `range="25"`.
    * **Example (`Config/entityclasses.xml`)**:
      ```xml
      <entity_class name="guppyWaterPlane" extends="zombieSoldier">
          <property name="Buffs" value="buffStartWaterTimer"/>
          <effect_group name="Base Effects">
              <passive_effect name="HealthMax" operation="base_set" value="100"/>
          </effect_group>
      </entity_class>
      ```
* **Triggering the Water Plane (`bDub_FlareGun`, `Config/items.xml`, `spawn_guppywaterplane`, `Config/gameevents.xml`)
  **:
    * A custom item (e.g., `bDub_FlareGun`) can be configured to trigger a `CallGameEvent`.
    * The `spawn_guppywaterplane` `action_sequence` in `Config/gameevents.xml` then spawns the `guppyWaterPlane` entity.
    * **CVar Control**: The `canCallDaPlane` CVar (`Config/buffs.xml`, `Config/items.xml`, `Config/entityclasses.xml`)
      is used to control cooldowns or conditions for calling the water plane.
    * **Example (`Config/items.xml`)**:
      ```xml
      <item name="bDub_FlareGun">
          <property class="Action0">
              <property name="Class" value="Launcher"/>
              </property>
          <effect_group name="gunFlareGun">
              <triggered_effect trigger="onSelfPrimaryActionStart" action="CallGameEvent" event="spawn_guppywaterplane">
                  <requirement name="CVarCompare" cvar="canCallDaPlane" operation="GT" value="1"/>
              </triggered_effect>
              <triggered_effect trigger="onReloadStop" action="ModifyCVar" cvar="canCallDaPlane" operation="set" value="2"/>
              <triggered_effect trigger="onSelfPrimaryActionEnd" action="ModifyCVar" cvar="canCallDaPlane" operation="set" value="1"/>
          </effect_group>
      </item>
      ```
    * **Game Event (`Config/gameevents.xml`)**:
      ```xml
      <action_sequence name="spawn_guppywaterplane">
          <property name="allow_user_trigger" value="false" />
          <property name="category" value="twitch_actions" />
          <action class="SpawnEntity">
              <property name="entity_names" value="guppyWaterPlane" param1="zombiename" />
              <property name="spawn_count" value="1" param1="spawncount" />
              <property name="air_spawn" value="false" param1="airspawn" />
          </action>
      </action_sequence>
      ```

### 7\. Entity Physics for Fire Interactions

* **`gupVultureStandard` (`Config/physicsbodies.xml`)**: This defines detailed colliders for a "burnt vulture" entity,
  ensuring accurate hit detection for fire damage and interactions with the environment. Modders defining new
  fire-susceptible entities with custom meshes should create similar physics body definitions.

### Summary

The FireV2 system, via SCore, provides a powerful and flexible framework for modders to implement detailed fire
mechanics. By understanding and modifying the XML configurations, particularly the `FireManagement` properties, using
SCore actions like `AddFireDamage, SCore` and `RemoveFire, SCore`, and integrating custom items, buffs, and entities,
modders can create immersive and dynamic fire experiences in their game worlds.