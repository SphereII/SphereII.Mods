	[ Crop Management ]	
		- Initial Implementation of Advanced Crop Management features. This will likely be buggy.

		- There's a lot to unpack here. Buckle up.

		- When enabled, crops will need to be within a 5x5 block radious of water block. 
			This is configurable with the WaterRange on a per-plant basis. Default is 5.
			Each plant will record where it's water blocks are. 
			The CheckInterval is used to determine how often the plant will check its water source.
				- If the water is gone when checked, the plant will rescan for a new block.
				- If no new water block is found, it will wilt.
				- Whenever a plant successfully checks in with water, it will do 1 point of damage to the water.
					- When a water block's damage exceeds its max damage (100 by default), it will turn into air.
			Crop data is not persisted to disk. Rather, if data is missing, it simply rechecks and re-discovers.

		- SCore's blocks.xml contains a new entry:
			<!-- Turns on support for the PlantGrowingSDX, SCore features for more advanced crop -->
			<property class="CropManagement" >
				<!-- Turns on logging to help debug -->
				<property name="Logging" value="false"/>

				<!-- Controls if crops are to be managed -->
				<property name="CropEnable" value="false"/>
				
				<!-- How often the crops should check for water -->
				<property name="CheckInterval" value="600" />

				<!-- How many pipes to scan -->
				<property name="MaxPipeLength" value="500" />
			</property>

			As always, it is not recommended to change the SCore's blocks.xml directly when adjusting, but rather use a modlet:
				<!-- Turning on Crops-->
				<set xpath="/blocks/block[@name='ConfigFeatureBlock']/property[@class='CropManagement']/property[@name='CropEnable']/@value">true</set>

				<!-- Checks how often the Crop Manager will check in with the plants. -->
				<set xpath="/blocks/block[@name='ConfigFeatureBlock']/property[@class='CropManagement']/property[@name='CheckInterval']/@value">120</set>

			
		- Crops that you want to manage under the advanced crop management must use the new classes:
			BlockPlantGrowingSDX

			It supports the following properties:
				RequireWater: [ true/false ]
					Allows invidiaul plants to control whether they need water or not. 
					- This can be read using extends, but overridden by the individual block
				WaterRange: [ 5 ]
					Allows individual plants to control how far away their water can be.
					- This can be read using extends, but overridden by the individual block
				PlantGrowing.Wilt: [ treeDeadPineLeaf ]
					If a plant goes without water for tool long, it will wilt into this block.
					- Can be air as well.
				Wilt: [ true / false ]: default false.
					- If a plant cannot find a water source, it will wilt and destroy itself.

			Example XML to convert all growable crops to use water.

				<!-- Setting up defaults -->
				<!-- Changing all crops to using the PlantGrowingSDX class -->
				<set xpath="/blocks/block[@name='cropsGrowingMaster']/property[@name='Class']/@value">PlantGrowingSDX, SCore</set>
				<append xpath="/blocks/block[@name='cropsGrowingMaster']">
					<!-- If the crop needs water to survive. Default: false -->
					<property name="RequireWater" value="true" /> 

					<!-- how far away that block can be from a water source: Default is 5 -->
					<property name="WaterRange" value="5" />  

					<!-- The block the crop downgrades too if its dead. Default: Air-->
					<property name="PlantGrowing.Wilt" value="treeDeadPineLeaf"/>

					<!-- if set to true, the plant will die when there is no water near by. -->
					<property name="Wilt" value="true" />
				</append>

		- In order to better support crops, and make them more fun, support for water pipes have been added.
			Two new classes have been written:
				BlockWaterPipeSDX: This block designates a block as a water carrier. It is not a water source itself, but can allow water to 'move' through. 
					- One section of the pipe must touch a water source block.

					- In this example, I convert all metalPipe's to be used as water pipes
						<append xpath="/blocks/block[starts-with(@name, 'metalPipe')]">
							<property name="Class" value="WaterPipeSDX, SCore" />
						</append>

				BlockWaterSourceSDX: This block designates a block as a water distributor. It is not a water source itself.
					- if the BlockWaterSourceSDX is connected to a series of BlockWaterPipeSDX which is connected to a water source block, it will act like a water source block.

					- In this example, I'm using the metalPipeValue as a distributor
						<set xpath="/blocks/block[@name='metalPipeValve']/property[@name='Class']/@value">WaterSourceSDX, SCore</set>


			A water source block is currently defined as:
				BlockLiquidV2 : Water from a river, lake, or dumped from a bucket
				terrBedRock: if you connect a pipe to bedrock, it will act as an unlimited water source.

				Any block is the property WaterSource is set on it, and set to true
					<property name="WaterSource" value="true" />
