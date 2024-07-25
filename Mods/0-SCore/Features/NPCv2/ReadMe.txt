NPCv2
=====
New class format and code designed to allow a replacement EntityAliveSDX for A22.

The goal of this project is to improve performance for the custom entities, as well as clean up the code significantly.

EntityAliveV2 (prototype name only)
=============
- Inherits off EntityNPC 
- Smaller code foot print
	- Base class only has the basic code to:
		- Add items to their inventory
		- Allow Dialog
		- Checks to see if immune to damage
		- etc.

- Support Classes have been configured to handle additional tasks that may be optional for the NPC.
	- Found under Features\NPCv2\Scripts\Entity\SupportClasses
	- Examples:
	    - LeaderUtils handles all logic and checks if the NPC has a leader.
	    - MissionUtils handles all logic and checks if the NPC is away on a mission.
	    - NPCQuest Utils handles all logic and checks if the NPC can give quests.
	    - PushOutOfBounds handles all logic and checks if the Entity is out of bounds.
	    - NPCUtils handles misc logic for generic NPCs
	
	- Some of these utils are added via EntityAliveV2's Init call. 
	- These are considered be useful for all NPCs, regardless of what the NPCs are meant for.
	    public override void Init(int _entityClass) {
			base.Init(_entityClass);
			_pushOutOfBlocksUtils = new PushOutOfBlocksUtils(this);
			_npcUtils = new NPCUtils(this);
		}
	- For more advanced NPCs, there is now an exposed XML format for the entityclass to configure:
		<property class="NPCConfiguration" >
            <property name="Hireable" value="true" />
          	<property name="Missions" value="true"/>
        </property>
	- The XML configurations are handled in the CopyPropertiesFromEntityClass, via a switch statement:
	    case "Hireable":
            if (StringParsers.ParseBool(keyValuePair.Value.ToString()))
				_leaderUtils = new LeaderUtils(this);
            
	- This will create a new reference to the LeaderUtils class.
	- Elsewhere in code, the following hooks have been added. If _leaderUtils is not null, then it'll execute the LeaderUpdate() call.
		- If it does not exist, it does nothing.
		
	    public override void OnUpdateLive() {
			_leaderUtils?.LeaderUpdate();
			_pushOutOfBlocksUtils.CheckStuck(position, width, depth);
			_npcUtils?.CheckCollision();
			_npcUtils?.CheckFallAndGround();
	