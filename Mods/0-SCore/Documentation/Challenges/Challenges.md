# Challenges

### Challenges Overview

Challenges were introduced into the game in the first **V1 release**. The **0-SCore** mod has expanded the available challenges.

### Notes:

Challenges are defined in the `challenges.xml` file. There are 3 parts to a challenge:

1. **Challenge Category**:  
   This creates a new category at the top of the Challenges window.

   ```xml
   <challenge_category name="AbilityCategory" title="Ability Challenges" icon="ui_game_symbol_challenge_category2" />
   ```

2. **Challenge Group**:  
   This creates a new group, which is a row in a new category. Up to 10 challenges can be added to a group.

   ```xml
   <challenge_group category="AbilityCategory" name="PerceptionGroup" title_key="perceptionChallenges_key" reward_text_key="No Rewards" reward_event="NoEvent"  />
   ```

3. **Individual Challenge**:  
   This is an individual challenge that is added to the row.

   ```xml
   <challenge name="rifle01" 
   title_key="gunRifleT0PipeRifle" 
   icon="ui_game_symbol_long_shot" 
   group="PerceptionGroup" 
   short_description_key="challengeKillZombies" 
   description_key="challengeKillZombiesDesc" 
   reward_text_key="challenge_reward_1000xp" 
   reward_event="challenge_reward_1000">
   
      <objective type="KillWithItem, SCore" count="10" item="gunRifleT0PipeRifle" />
   </challenge>
   ```

### Examples:

Examples can be found [here](https://github.com/SphereII/SphereII.Mods/tree/master/Mods/SphereII%20Challenges/Config).

[View Challenges Related to Fire](FireChallenges.md)
[View Challenges Related to Blocks](BlocksChallenges.md)
[View Challenges Related to Kills](KillChallenges.md)
[View Challenges Related to Player Activities](PlayerChallenges.md)
