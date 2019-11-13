using Harmony;
using System;
using System.Xml;
using UnityEngine;

// This class populates a static variable that will help us link Sound Data with Buff / Quests.
public class SphereII_GiveBuffOrQuestBySound
{
    private static string AdvFeatureClass = "AdvancedSoundFeatures";
    public static bool CheckForBuffOrQuest(Audio.Manager __instance, string soundGroupName, EntityPlayerLocal entity)
    {
        if (entity == null)
        {
            AdvLogging.DisplayLog(AdvFeatureClass, "Entity is null");
            return true;
        }
       // EntityAlive myEntity = entity as EntityAlive;
        if (entity)
        {
            AdvLogging.DisplayLog(AdvFeatureClass, "Searching for " + soundGroupName);

            if (SoundDataNodeClassSDX.SoundDataSDXInfo.ContainsKey(soundGroupName))
            {
                AdvLogging.DisplayLog(AdvFeatureClass, "Found Sound Node. Checking for buffs");

                SoundDataNodeClassSDX.SoundDataSDX data = SoundDataNodeClassSDX.SoundDataSDXInfo[soundGroupName];

                if (data.Buff != null )
                {
                    AdvLogging.DisplayLog(AdvFeatureClass, "Adding Buff " + data.Buff.Name + " to " + entity.EntityName);
                    entity.Buffs.AddBuff(data.Buff.Name);
                }
                AdvLogging.DisplayLog(AdvFeatureClass, "Scanning for quest");

                if (data.Quest != null)
                {
                         AdvLogging.DisplayLog(AdvFeatureClass, "Adding Quest " + data.Quest.ID+ " to " + entity.EntityName);
                    entity.QuestJournal.AddQuest(data.Quest);
 
                    //EntityAliveSDX entityAliveSDX = entity as EntityAliveSDX;
                    //if (entityAliveSDX != null)
                    //{
                    //    AdvLogging.DisplayLog(AdvFeatureClass, "Adding Quest " + data.Quest.Name + " to " + entityAliveSDX.EntityName);
                    //    player.QuestJournal.AddQuest(data.Quest.CreateQuest());
                    //}

                }
            }
        }
        return true;
    }
}


// This class populates a static variable that will help us link Sound Data with Buff / Quests.
[HarmonyPatch(typeof(Audio.Manager))]
[HarmonyPatch("Play")]
[HarmonyPatch(new Type[] { typeof(Entity), typeof(string), typeof(float) , typeof(bool)})]
public class SphereII_Audio_Server_Play
{
    private static string AdvFeatureClass = "AdvancedSoundFeatures";

    static bool Prefix(Audio.Manager __instance, Entity entity, string soundGroupName, float volumeScale = 1f, bool wantHandle = false)
    {

        SphereII_GiveBuffOrQuestBySound.CheckForBuffOrQuest(__instance, soundGroupName, GameManager.Instance.World.GetPrimaryPlayer());
        return true;
    }
}

// This class populates a static variable that will help us link Sound Data with Buff / Quests.
[HarmonyPatch(typeof(Audio.Manager))]
[HarmonyPatch("Play")]
[HarmonyPatch(new Type[] { typeof(Vector3), typeof(string), typeof(int) })]
public class SphereII_GiveBuffOrQuestBySound_Play2
{
    private static string AdvFeatureClass = "AdvancedSoundFeatures";

    static bool Prefix(Audio.Manager __instance, Entity ___localPlayer, Vector3 position, string soundGroupName, int entityId = -1)
    {
        // Sound system may not be initialized yet
        if (___localPlayer == null)
            return true;
        SphereII_GiveBuffOrQuestBySound.CheckForBuffOrQuest(__instance, soundGroupName, ___localPlayer as EntityPlayerLocal);

        return true;
    }
}
