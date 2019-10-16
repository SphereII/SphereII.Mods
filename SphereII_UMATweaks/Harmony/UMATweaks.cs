using DMT;
using Harmony;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;
using UnityEngine;

// Add Reference to Assembly-CSharp-firstpass for UMA references to resolve in Visual Studio
public class SphereII_UMATweaks
{
    public class SphereII_UMATweaksInit : IHarmony
    {
        public void Start()
        {
            Debug.Log(" Loading Patch: " + GetType().ToString());
            var harmony = HarmonyInstance.Create(GetType().ToString());
            harmony.PatchAll(Assembly.GetExecutingAssembly());
        }
    }

    // Disable the pregeneration of UMAs, since this can be time consuming and resource intensive, causing menu lag, etc. 
    // Create the directory if it doesn't exist, so when they are populated on demand, they'll be saved.
    [HarmonyPatch(typeof(Archetypes))]
    [HarmonyPatch("PregenStatic")]
    public class SphereII_PregenStatic
    {
        public static bool Prefix(Archetype __instance, ref List<Archetype> ___archetypes)
        {
            // Create the folder if it doesn't exist
            string strUMAFolder = Path.Combine(Application.dataPath, "..", "Data", "UMATextures");
            if (!Directory.Exists(strUMAFolder))
                Directory.CreateDirectory(strUMAFolder);

            // No not pre-generating the UMAs. Let them be created on the fly when needed, but saved for future use.
            return false;
        }
    }

    // Drastically reduce the texture size of the generated UMAs, making them spawn in faster and generate smaller files.
    [HarmonyPatch(typeof(UMA.UMAGeneratorCoroutine))]
    [HarmonyPatch("workerMethod")]
    public class SphereII_ArchetypeTextureCache
    {
        public static bool Prefix(ref UMA.UMAData ___umaData)
        {
            //Debug.Log(" Atlas Size: Prefix Before " + ___umaData.AtlasSize);
            // Changing the Atlas size down  for all UMAs
            if (SkyManager.BloodMoon() || SkyManager.IsDark())
                ___umaData.AtlasSize = 128;
            ___umaData.AtlasSize = 512;

            //Debug.Log(" Atlas Size: Prefix After " + ___umaData.AtlasSize);
            return true;
        }
    }

    [HarmonyPatch(typeof(Archetypes))]
    [HarmonyPatch("GetArchetype")]
    public class SphereII_Archetypes_GetArcheType_Random
    {
        static bool DisplayLogs = false;


        public static bool Prefix(Archetype __result, string _name, List<Archetype> ___archetypes)
        {
            if (DisplayLogs) Debug.Log("GetArchetype(): " + _name);
            int MaxArchetypes = ___archetypes.Count - 1;
            if (_name == "Random")
            {
                GameRandom random = GameRandomManager.Instance.CreateGameRandom();
                if (DisplayLogs) Debug.Log("GetArcheType(): Randomizing UMA. Randomized Pool Size: " + ___archetypes.Count);

                int intRandom = random.RandomRange(0, MaxArchetypes);
                if (DisplayLogs) Debug.Log("Random Range: " + intRandom);
                __result = ___archetypes[intRandom].Clone();

                return false;

                //String strRandomColor = string.Format("{0},{1},{2}", random.RandomRange(0, 256), random.RandomRange(0, 256), random.RandomRange(0, 256));
                //__result.EyeColor = StringParsers.ParseColor32(strRandomColor);

                //strRandomColor = string.Format("{0},{1},{2}", random.RandomRange(0, 256), random.RandomRange(0, 256), random.RandomRange(0, 256));
                //__result.HairColor = StringParsers.ParseColor32(strRandomColor);

                //__result.ExpressionData.BlinkingEnabled = false;


                //for (int x = 0; x < __result.BaseSlots.Count; x++)
                //{
                //    intRandom = random.RandomRange(0, MaxArchetypes);
                //    if (DisplayLogs) Debug.Log("GetArchetype(): Randomizing BaseSlot from " + ___archetypes[intRandom].Name + " Slot: " + __result.BaseSlots[x].Name + " Index: " + intRandom);

                //    if (DisplayLogs) Debug.Log("Index: " + intRandom + " __result.BaseSlots Length: " + __result.BaseSlots.Count + " Target: " + ___archetypes[intRandom].BaseSlots.Count);
                //    Debug.Log(___archetypes[intRandom].Name); 
                //    __result.BaseSlots[x] = ___archetypes[intRandom].BaseSlots[x];
                //}

                //intRandom = random.RandomRange(0, MaxArchetypes);
                //if (DisplayLogs) Debug.Log("GetArcheType(): Randoming Expression from " + ___archetypes[intRandom].Name);
                //__result.ExpressionData = ___archetypes[intRandom].ExpressionData;

                //intRandom = random.RandomRange(0, MaxArchetypes);
                //if (DisplayLogs) Debug.Log("GetArcheType(): Randoming DNA from " + ___archetypes[intRandom].Name);
                //__result.Dna = ___archetypes[intRandom].Dna;

                //__result.Dna.height = random.RandomRange(__result.Dna.height - 0.2f, __result.Dna.height + 0.2f);
                //__result.Dna.headSize = random.RandomRange(__result.Dna.headSize - 0.1f, __result.Dna.headSize + 0.1f);

                //if (DisplayLogs) Debug.Log("GetArcheType(): Generated Random Archetype");

            }

            return true;
        }
    }

    //[HarmonyPatch(typeof(EModelZombieUMA))]
    //[HarmonyPatch("BuildUMA")]
    //public class SphereII_UMATweaks_EModelZombieUMA_BuildUMA
    //{
    //    public static bool Prefix(ref Entity ___entity)
    //    {
    //        EntityAlive entity = ___entity as EntityAlive;
    //        if (entity)
    //        {
    //            EntityClass entityClass = EntityClass.list[___entity.entityClass];
    //            if (entityClass.Properties.Values.ContainsKey("Equipment"))
    //            {
    //                foreach (string text in entityClass.Properties.Values["Equipment"].Split(new char[] { ',' }))
    //                {
    //                    ItemStack itemStack = ItemStack.FromString(text.Trim());
    //                    if (itemStack.itemValue.IsEmpty())
    //                    {
    //                        continue;
    //                    }

    //                    ItemClass itemClass = itemStack.itemValue.ItemClass;
    //                    if (itemClass.Properties.Contains("EquipSlot"))
    //                    {
    //                        int slotIndex = Equipment.GetSlotIndex((int)itemClass.EquipSlot, (int)itemClass.UmaSlotData.EquipmentLayer);
    //                        (___entity as EntityAlive).equipment.SetSlotItem(slotIndex, itemStack.itemValue.Clone(), true);
    //                    }
    //                    else
    //                    {
    //                        (___entity as EntityAlive).inventory.SetSlots(new ItemStack[] { itemStack });
    //                        (___entity as EntityAlive).inventory.SetBareHandItem(itemStack.itemValue);
    //                    }
    //                }
    //            }
    //            else
    //            {
    //                GameRandom random = GameRandomManager.Instance.CreateGameRandom();
    //                if (random.RandomRange(0, 100) < 5)
    //                {

    //                    ItemStack item = LootContainer.GetRewardItem("groupApparelClothes", 1f);
    //                    int slotIndex = Equipment.GetSlotIndex((int)item.itemValue.ItemClass.EquipSlot, (int)item.itemValue.ItemClass.UmaSlotData.EquipmentLayer);
    //                    if (item.itemValue.ItemClass.EquipSlot != XMLData.Item.EnumEquipmentSlot.Head) // some head stuff looks weird.
    //                        (___entity as EntityAlive).equipment.SetSlotItem(slotIndex, item.itemValue.Clone(), true);
    //                }

    //            }
    //        }
    //            return true;
    //    }
    //}



}


