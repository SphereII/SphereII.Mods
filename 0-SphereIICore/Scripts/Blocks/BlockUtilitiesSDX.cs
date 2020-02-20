using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using UnityEngine;


public static class BlockUtilitiesSDX
{
    public static void AddRadiusEffect(String strItemClass, ref Block myBlock)
    {
        ItemClass itemClass = ItemClass.GetItemClass(strItemClass, false);
        if (itemClass.Properties.Values.ContainsKey("ActivatedBuff"))
        {
            String strBuff = itemClass.Properties.Values["ActivatedBuff"];
            string[] array5 = strBuff.Split(new string[] { ";" }, StringSplitOptions.RemoveEmptyEntries);

            // Grab the current radius effects
            List<BlockRadiusEffect> list2 = myBlock.RadiusEffects.OfType<BlockRadiusEffect>().ToList();
            foreach (string text4 in array5)
            {
                int num12 = text4.IndexOf('(');
                int num13 = text4.IndexOf(')');
                BlockRadiusEffect item = default(BlockRadiusEffect);
                if (num12 != -1 && num13 != -1 && num13 > num12 + 1)
                {
                    item.radius = StringParsers.ParseFloat(text4.Substring(num12 + 1, num13 - num12 - 1), 0, -1, NumberStyles.Any);
                    item.variable = text4.Substring(0, num12);
                }
                else
                {
                    item.radius = 1f;
                    item.variable = text4;
                }
                if (!list2.Contains(item))
                {
                    Debug.Log("Adding Buff for " + item.variable);
                    list2.Add(item);
                }
            }

            myBlock.RadiusEffects = list2.ToArray();

        }
    }
    public static void RemoveRadiusEffect(String strItemClass, ref Block myBlock)
    {
        ItemClass itemClass = ItemClass.GetItemClass(strItemClass, false);
        if (itemClass.Properties.Values.ContainsKey("ActivatedBuff"))
        {
            String strBuff = itemClass.Properties.Values["ActivatedBuff"];
            string[] array5 = strBuff.Split(new string[] { ";" }, StringSplitOptions.RemoveEmptyEntries);

            // Grab the current radius effects
            List<BlockRadiusEffect> list2 = myBlock.RadiusEffects.OfType<BlockRadiusEffect>().ToList();
            foreach (string text4 in array5)
            {
                int num12 = text4.IndexOf('(');
                int num13 = text4.IndexOf(')');
                BlockRadiusEffect item = default(BlockRadiusEffect);
                if (num12 != -1 && num13 != -1 && num13 > num12 + 1)
                {
                    item.radius = StringParsers.ParseFloat(text4.Substring(num12 + 1, num13 - num12 - 1), 0, -1, NumberStyles.Any);
                    item.variable = text4.Substring(0, num12);
                }
                else
                {
                    item.radius = 1f;
                    item.variable = text4;
                }
                if (list2.Contains(item))
                {
                    list2.Remove(item);
                }
            }

            myBlock.RadiusEffects = list2.ToArray();

        }
    }

    public static void addParticles(String strParticleName, Vector3i position)
    {
        if (strParticleName == null || strParticleName == "")
            return;
        BlockValue blockValue = GameManager.Instance.World.GetBlock(position);
        GameManager.Instance.World.GetGameManager().SpawnBlockParticleEffect(position, new ParticleEffect(strParticleName,  position.ToVector3() + Vector3.up, blockValue.Block.shape.GetRotation(blockValue), 1f, Color.white));
    }

    public static void removeParticles(Vector3i position)
    {
        GameManager.Instance.World.GetGameManager().RemoveBlockParticleEffect(position);
    }
}

