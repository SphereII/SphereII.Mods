using Audio;
using UnityEngine;

namespace Harmony.SoundFeatures
{
    /**
     * SCoreGiveBuffOrQuestBySound
     * 
     * This class includes a Harmony patch to hook the delivery of a buff or quest through the sound system.
     * 
     * Usage XML:
     * <!-- Adds the buffCursed to any entity that hears the screamer -->
     * <append xpath="/Sounds/SoundDataNode[@name='zombiefemalescoutalert']">
     *     <buff value="buffCursed" />
     *     <Quest value="myQuest" />
     * </append>
     */
    public class GiveBuffOrQuestBySound
    {
        private static readonly string AdvFeatureClass = "AdvancedSoundFeatures";

        public static void CheckForBuffOrQuest(string soundGroupName, Vector3 position)
        {
            AdvLogging.DisplayLog(AdvFeatureClass, "Searching for " + soundGroupName);

            // Dictionary search for substring
            if (!SoundDataNodeClassSDX.SoundDataSdxInfo.ContainsKey(soundGroupName)) return;
            AdvLogging.DisplayLog(AdvFeatureClass, "Found Sound Node. Checking for buffs");


            var radius = 1;
            // use xmlData to grab the sound node information, which can contain how far away the sound can be heard.
            if (Manager.audioData.TryGetValue(soundGroupName.ToLower(), out var xmlData))
            {
                AdvLogging.DisplayLog(AdvFeatureClass, $"SoundGroup Name: {soundGroupName} Found in Manager.Audio Data. Grabbing radius");
                radius = Utils.Fastfloor(xmlData.distantFadeEnd);
            }
            else
            {
                AdvLogging.DisplayLog(AdvFeatureClass, $"SoundGroup Name: {soundGroupName} not found in Manager.audioData");

            }

            var data = SoundDataNodeClassSDX.SoundDataSdxInfo[soundGroupName];
            if (data.Buff != null)
            {
                AdvLogging.DisplayLog(AdvFeatureClass, ": Found Buff for Sound Node: " + data.Buff + $" Radius: {radius}");
                EntityUtilities.AddBuffToRadius(data.Buff, position, radius);
            }

            AdvLogging.DisplayLog(AdvFeatureClass, "Scanning for quest");
            if (data.Quest == null) return;

            AdvLogging.DisplayLog(AdvFeatureClass, "Adding Quest " + data.Quest + " to surrounding entities");
            EntityUtilities.AddQuestToRadius(data.Quest, position, radius);
        }
    }
}