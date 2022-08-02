using System;
using System.Linq;
using UnityEngine;
using System.Collections.Generic;
public static class BlockUtilitiesSDX
{
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
                    item.radius = StringParsers.ParseFloat(text4.Substring(num12 + 1, num13 - num12 - 1));
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
                    item.radius = StringParsers.ParseFloat(text4.Substring(num12 + 1, num13 - num12 - 1));
                    item.variable = text4.Substring(0, num12);
                }
                else
                {
                    item.radius = 1f;
                    item.variable = text4;
                }

                if (list2.Contains(item)) list2.Remove(item);
            }

            myBlock.RadiusEffects = list2.ToArray();
        }
    }

    public static void addParticles(string strParticleName, Vector3i position)
    {
        if (strParticleName == null || strParticleName == "")
            strParticleName = "#@modfolder(0-SCore):Resources/PathSmoke.unity3d?P_PathSmoke_X";

        if (!ParticleEffect.IsAvailable(strParticleName))
            ParticleEffect.RegisterBundleParticleEffect(strParticleName);

        var blockValue = GameManager.Instance.World.GetBlock(position);
        GameManager.Instance.World.GetGameManager().SpawnBlockParticleEffect(position,
            new ParticleEffect(strParticleName, position.ToVector3() + Vector3.up, blockValue.Block.shape.GetRotation(blockValue), 1f, Color.white));
    }

    public static void addParticlesCentered(string strParticleName, Vector3i position)
    {
        if (strParticleName == null || strParticleName == "")
            strParticleName = "#@modfolder(0-SCore):Resources/PathSmoke.unity3d?P_PathSmoke_X";

        if (strParticleName == "NoParticle")
            return;

        if (!ParticleEffect.IsAvailable(strParticleName))
            ParticleEffect.RegisterBundleParticleEffect(strParticleName);

        if (GameManager.Instance.HasBlockParticleEffect(position)) 
            return;

        var centerPosition = EntityUtilities.CenterPosition(position);
        var blockValue = GameManager.Instance.World.GetBlock(position);
        var particle = new ParticleEffect(strParticleName, centerPosition, blockValue.Block.shape.GetRotation(blockValue), 1f, Color.white);
        GameManager.Instance.SpawnBlockParticleEffect(position, particle);
    }

    public static void addParticlesCenteredNetwork(string strParticleName, Vector3i position)
    {
        if (strParticleName == null || strParticleName == "")
            strParticleName = "#@modfolder(0-SCore):Resources/PathSmoke.unity3d?P_PathSmoke_X";

        if (strParticleName == "NoParticle")
            return;

        if (!ParticleEffect.IsAvailable(strParticleName))
            ParticleEffect.RegisterBundleParticleEffect(strParticleName);

        var centerPosition = EntityUtilities.CenterPosition(position);
        var particle = new ParticleEffect(strParticleName, centerPosition, Quaternion.LookRotation(position, Vector3.up), 1f, Color.white);



        //if (!GameManager.IsDedicatedServer)
        {
            BlockUtilitiesSDX.SCoreParticles[position] = GameManager.Instance.SpawnParticleEffectClient(particle, -1);

        }
        if (!SingletonMonoBehaviour<ConnectionManager>.Instance.IsServer)
        {
            SingletonMonoBehaviour<ConnectionManager>.Instance.SendToServer(NetPackageManager.GetPackage<NetPackageParticleEffect>().Setup(particle, -1), false);
            return;
        }
        SingletonMonoBehaviour<ConnectionManager>.Instance.SendPackage(NetPackageManager.GetPackage<NetPackageParticleEffect>().Setup(particle, -1), false, -1, -1, -1, -1);

    }

    private static Dictionary<Vector3i, Transform> SCoreParticles = new Dictionary<Vector3i, Transform>();
    public static void removeParticlesNetPackage(Vector3i position)
    {
        if (BlockUtilitiesSDX.SCoreParticles.TryGetValue(position, out Transform particle))
        {
            RemoveParticleEffectServer(position, -1);
        }

        ////removeParticles(position);

        //if (!GameManager.IsDedicatedServer)
        //{
        //    removeParticles(position);
        //}
        //if (!SingletonMonoBehaviour<ConnectionManager>.Instance.IsServer)
        //{
        //    SingletonMonoBehaviour<ConnectionManager>.Instance.SendToServer(NetPackageManager.GetPackage<NetPackageRemoveParticleEffect>().Setup(position, -1), false);
        //    return;
        //}
        //SingletonMonoBehaviour<ConnectionManager>.Instance.SendPackage(NetPackageManager.GetPackage<NetPackageRemoveParticleEffect>().Setup(position, -1), false, -1, -1, -1, -1);
    }

    public static void RemoveParticleEffectServer(Vector3i position, int _entityId)
    {
        if (GameManager.Instance.World == null)
        {
            return;
        }
        if (!GameManager.IsDedicatedServer)
        {
            RemoveParticleEffectClient(position, _entityId);
        }
        if (!SingletonMonoBehaviour<ConnectionManager>.Instance.IsServer)
        {
            SingletonMonoBehaviour<ConnectionManager>.Instance.SendToServer(NetPackageManager.GetPackage<NetPackageRemoveParticleEffect>().Setup(position, _entityId), false);
            return;
        }
        SingletonMonoBehaviour<ConnectionManager>.Instance.SendPackage(NetPackageManager.GetPackage<NetPackageRemoveParticleEffect>().Setup(position, _entityId), false, -1, _entityId, -1, -1);
    }

    public static void RemoveParticleEffectClient(Vector3i position, int _entityThatCausedIt)
    {
        //return this.spawnParticleEffect(_pe, _entityThatCausedIt);
        //removeParticles(new Vector3i(_pe.pos));
        if (BlockUtilitiesSDX.SCoreParticles.ContainsKey(position))
        {
            GameObject.Destroy(BlockUtilitiesSDX.SCoreParticles[position].gameObject);
            BlockUtilitiesSDX.SCoreParticles.Remove(position);
        }
    }
    public static void removeParticles(Vector3i position)
    {
        GameManager.Instance.World.GetGameManager().RemoveBlockParticleEffect(position);
    }
}