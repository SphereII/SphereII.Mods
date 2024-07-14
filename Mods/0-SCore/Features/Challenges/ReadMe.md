Challenges
----------

All code in SCore related to challenges, including:

Harmony Hook to BaseChallengeObjective's ReadObjective
An Enum to satisfying the ChallengeObjective requirement
Classes adding new challenges

To Create a New SCore Challenge:
- Add new enum entry under Harmony/ChallengeReadObjective
- Update Harmony Patch in Harmony/ChallengesReadObjective to parse it correctly.
- Add a Scripts/ Challenge Objective.
- Add an delegate, event, and harmony patch to add a trigger
  - Optionally, this can be included in the same file as the Script. See ChallengeObjectiveEnterPOI


Example Syntax:

       <challenge name="enterPOI" title_key="EnterPOI" icon="ui_game_symbol_wood" group="ScoreTest" short_description_key="challengeGathererWoodShort" description_key="challengeGathererWoodDesc" reward_text_key="challenge_reward_1000xp" reward_event="challenge_reward_1000">
            <objective type="EnterPOI, SCore" prefab="abandoned_house_02" count="10"/>
        </challenge>
