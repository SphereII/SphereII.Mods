using System;
using System.Linq;
using UnityEngine;
using System.Collections.Generic;
public static class BlockUtilitiesSDX
{
    public static void CheckAndLoadParticles(DynamicProperties dynamicProperties, string property)
    {
        if (string.IsNullOrEmpty(property)) return;
        if (!dynamicProperties.Values.ContainsKey(property)) return;
        var particleName = dynamicProperties.Values[property];
        CheckAndLoadParticles(particleName);
    }
    
    public static void CheckAndLoadParticles(string particleName)
    {
        if (string.IsNullOrEmpty(particleName)) return;
        foreach (var particle in particleName.Split(','))
        {
            if ( ParticleEffect.IsAvailable(particle)) continue;
            Log.Out($"SCore: Loading Particle: {particle}");
            ParticleEffect.LoadAsset(particle);
        }

    }
    public static void AddRadiusEffect(string strItemClass, ref Block myBlock)
    {
        var itemClass = ItemClass.GetItemClass(strItemClass);
        if (itemClass.Properties.Values.ContainsKey("ActivatedBuff"))
        {
            var strBuff = itemClass.Properties.Values["ActivatedBuff"];
            var array5 = strBuff.Split(new[] { ";" }, StringSplitOptions.RemoveEmptyEntries);

            // Grab the current radius effects
            var list2 = myBlock.RadiusEffects.OfType<BlockRadiusEffect>().ToList();
            foreach (var text4 in array5)
            {
                var num12 = text4.IndexOf('(');
                var num13 = text4.IndexOf(')');
                var item = default(BlockRadiusEffect);
                if (num12 != -1 && num13 != -1 && num13 > num12 + 1)
                {
                    item.radiusSq = StringParsers.ParseFloat(text4.Substring(num12 + 1, num13 - num12 - 1));
                    item.variable = text4.Substring(0, num12);
                }
                else
                {
                    item.radiusSq = 1f;
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

    public static void RemoveRadiusEffect(string strItemClass, ref Block myBlock)
    {
        var itemClass = ItemClass.GetItemClass(strItemClass);
        if (itemClass.Properties.Values.ContainsKey("ActivatedBuff"))
        {
            var strBuff = itemClass.Properties.Values["ActivatedBuff"];
            var array5 = strBuff.Split(new[] { ";" }, StringSplitOptions.RemoveEmptyEntries);

            // Grab the current radius effects
            var list2 = myBlock.RadiusEffects.OfType<BlockRadiusEffect>().ToList();
            foreach (var text4 in array5)
            {
                var num12 = text4.IndexOf('(');
                var num13 = text4.IndexOf(')');
                var item = default(BlockRadiusEffect);
                if (num12 != -1 && num13 != -1 && num13 > num12 + 1)
                {
                    item.radiusSq = StringParsers.ParseFloat(text4.Substring(num12 + 1, num13 - num12 - 1));
                    item.variable = text4.Substring(0, num12);
                }
                else
                {
                    item.radiusSq = 1f;
                    item.variable = text4;
                }

                if (list2.Contains(item)) list2.Remove(item);
            }

            myBlock.RadiusEffects = list2.ToArray();
        }
    }

  
    public static void addParticles(string strParticleName, Vector3i position)
    {
        if (GameManager.IsDedicatedServer) return;

        if (string.IsNullOrEmpty(strParticleName))
            strParticleName = "#@modfolder(0-SCore_sphereii):Resources/PathSmoke.unity3d?P_PathSmoke_X";

        if (!ParticleEffect.IsAvailable(strParticleName))
            ParticleEffect.LoadAsset(strParticleName);

        var blockValue = GameManager.Instance.World.GetBlock(position);
        GameManager.Instance.World.GetGameManager().SpawnBlockParticleEffect(position,
            new ParticleEffect(strParticleName, position.ToVector3() + Vector3.up, blockValue.Block.shape.GetRotation(blockValue), 1f, Color.white));
    }

    public static void addParticlesCentered(string strParticleName, Vector3i position)
    {
        if (string.IsNullOrEmpty(strParticleName))
            strParticleName = "#@modfolder(0-SCore_sphereii):Resources/PathSmoke.unity3d?P_PathSmoke_X";

        if (strParticleName == "NoParticle")
            return;

        
        if (!ParticleEffect.IsAvailable(strParticleName))
        {
            if (ThreadManager.IsMainThread())
            {
                ParticleEffect.LoadAsset(strParticleName);
            }
            else
            {
                Log.Out($"Trying to load {strParticleName} but the call is not on the main thread: {Environment.StackTrace}. Failing.");
                return;
            }
        }
        
        if (GameManager.Instance.HasBlockParticleEffect(position)) return;

        if (GameManager.IsDedicatedServer) return;
        
        var centerPosition = EntityUtilities.CenterPosition(position);
        var blockValue = GameManager.Instance.World.GetBlock(position);
        var rotation = Quaternion.identity;
        //rotation = blockValue.Block.shape.GetRotation(blockValue);
        var particle = new ParticleEffect(strParticleName, centerPosition, rotation, 1f, Color.white);
   
       
        GameManager.Instance.SpawnBlockParticleEffect(position, particle);
    }
    
    public static void addParticlesCenteredServer(string strParticleName, Vector3i position)
    {
        if (string.IsNullOrEmpty(strParticleName))
            strParticleName = "#@modfolder(0-SCore_sphereii):Resources/PathSmoke.unity3d?P_PathSmoke_X";

        if (strParticleName == "NoParticle")
            return;
        
        if (!ParticleEffect.IsAvailable(strParticleName))
        {
            Log.Out($"PArticle not available: {strParticleName}");
            if (ThreadManager.IsMainThread())
            {
                ParticleEffect.LoadAsset(strParticleName);
            }
            else
            {
                Log.Out($"Trying to load {strParticleName} but the call is not on the main thread: {Environment.StackTrace}. Failing.");
                return;
            }
        }
        
       // if (GameManager.Instance.HasBlockParticleEffect(position)) return;
        
        var centerPosition = EntityUtilities.CenterPosition(position);
        var blockValue = GameManager.Instance.World.GetBlock(position);
        var rotation = Quaternion.identity;
        //rotation = blockValue.Block.shape.GetRotation(blockValue);
        var particle = new ParticleEffect(strParticleName, centerPosition, rotation, 1f, Color.white);
        GameManager.Instance.SpawnParticleEffectServer(particle, -1);
    }
    
   
    public static void removeParticles(Vector3i position)
    {
        if (GameManager.IsDedicatedServer) return;
        GameManager.Instance.World.GetGameManager().RemoveBlockParticleEffect(position);
    }
}