using System.Collections.Generic;
using UnityEngine;

public class NetPackageRandomTaggedPOIGotoSDX : NetPackage
{
    // Private fields from NetPackageQuestGotoPoint

    private int entityId;

    private int questCode;

    private FastTags<TagGroup.Global> questTags;

    private Vector2 position;

    private Vector3 size;

    private byte difficulty;

    private BiomeFilterTypes biomeFilterType;

    private string biomeFilter;

    // Fields needed to get the correct POI

    private FastTags<TagGroup.Poi> includeTags;

    private FastTags<TagGroup.Poi> excludeTags;

    public float maxSearchDistance;

    public float minSearchDistance;

    public override int GetLength()
    {
        return 20;
    }

    public override void ProcessPackage(World _world, GameManager _callbacks)
    {
        if (_world == null)
        {
            return;
        }

        EntityPlayer primaryPlayer = GameManager.Instance.World.GetPrimaryPlayer();

        if (SingletonMonoBehaviour<ConnectionManager>.Instance.IsServer)
        {
            EntityAlive questOwner = GameManager.Instance.World.GetEntity(entityId) as EntityAlive;
            PrefabInstance prefabInstance;
            List<Vector2> usedPoiLocations;

            // Why do this 5 times? Dunno - it's how NetPackageQuestGotoPoint does it
            for (int i = 0; i < 5; i++)
            {
                // Same logic as ObjectiveRandomTaggedPOIGotoSDX
                if (questOwner is EntityTrader trader && trader.traderArea != null)
                {
                    usedPoiLocations = primaryPlayer.QuestJournal.GetUsedPOIs(
                        new Vector2(trader.traderArea.Position.x, trader.traderArea.Position.z),
                        difficulty);

                    prefabInstance = QuestUtils.GetRandomPOINearTrader(
                        trader,
                        questTags,
                        difficulty,
                        includeTags,
                        excludeTags,
                        minSearchDistance,
                        maxSearchDistance,
                        usedPoiLocations,
                        entityId,
                        biomeFilterType,
                        biomeFilter);
                }
                else
                {
                    usedPoiLocations = primaryPlayer.QuestJournal.GetUsedPOIs(
                        new Vector2(questOwner.position.x, questOwner.position.z),
                        difficulty);

                    prefabInstance = QuestUtils.GetRandomPOINearEntityPos(
                        questOwner,
                        questTags,
                        difficulty,
                        includeTags,
                        excludeTags,
                        // The values used in vanilla are the square of the distance; adjusted here
                        minSearchDistance >= 0 ? minSearchDistance : 50,
                        maxSearchDistance >= 0 ? maxSearchDistance : 2000,
                        usedPoiLocations,
                        entityId,
                        biomeFilterType,
                        biomeFilter);
                }

                if (prefabInstance != null)
                {
                    // All the client needs to do is to finalize the point in the objective -
                    // we don't need this package to do that, the GotoPoint package will do fine,
                    // as long as we send RandomPOI so the correct base class Finalize is called
                    SingletonMonoBehaviour<ConnectionManager>.Instance.SendPackage(
                        NetPackageManager.GetPackage<NetPackageQuestGotoPoint>().Setup(
                            entityId,
                            primaryPlayer.entityId,
                            questTags,
                            questCode,
                            NetPackageQuestGotoPoint.QuestGotoTypes.RandomPOI,
                            difficulty,
                            prefabInstance.boundingBoxPosition.x,
                            prefabInstance.boundingBoxPosition.z,
                            prefabInstance.boundingBoxSize.x,
                            prefabInstance.boundingBoxSize.y,
                            prefabInstance.boundingBoxSize.z),
                        _onlyClientsAttachedToAnEntity: false,
                        entityId);
                    break;
                }
            }

            return;
        }

        Quest quest = primaryPlayer.QuestJournal.FindActiveQuest(questCode);
        if (quest == null)
        {
            return;
        }

        for (int j = 0; j < quest.Objectives.Count; j++)
        {
            if (quest.Objectives[j] is ObjectiveRandomTaggedPOIGotoSDX taggedGoto)
            {
                taggedGoto.FinalizePoint(position, size);
            }
        }
    }

    public override void read(PooledBinaryReader _br)
    {
        // from NetPackageQuestGotoPoint
        entityId = _br.ReadInt32();
        questCode = _br.ReadInt32();
        questTags = FastTags<TagGroup.Global>.Parse(_br.ReadString());
        position = new Vector2(_br.ReadInt32(), _br.ReadInt32());
        size = StreamUtils.ReadVector3(_br);
        difficulty = _br.ReadByte();
        biomeFilterType = (BiomeFilterTypes)_br.ReadByte();
        biomeFilter = _br.ReadString();

        // needed to get the correct POI
        string includeTagsString = _br.ReadString();
        includeTags = FastTags<TagGroup.Poi>.Parse(includeTagsString);
        string excludeTagsString = _br.ReadString();
        excludeTags = FastTags<TagGroup.Poi>.Parse(excludeTagsString);
        maxSearchDistance = _br.ReadSingle();
        minSearchDistance = _br.ReadSingle();
    }

    public override void write(PooledBinaryWriter _bw)
    {
        base.write(_bw);

        // from NetPackageQuestGotoPoint
        _bw.Write(entityId);
        _bw.Write(questCode);
        _bw.Write(questTags.ToString());
        _bw.Write((int)position.x);
        _bw.Write((int)position.y);
        StreamUtils.Write(_bw, size);
        _bw.Write(difficulty);
        _bw.Write((byte)biomeFilterType);
        _bw.Write(biomeFilter);

        // needed to get the correct POI
        _bw.Write(includeTags.ToString());
        _bw.Write(excludeTags.ToString());
        _bw.Write(maxSearchDistance);
        _bw.Write(minSearchDistance);
    }

    public NetPackageRandomTaggedPOIGotoSDX Setup(
        int _ownerId,
        FastTags<TagGroup.Global> _questTags,
        int _questCode,
        byte _difficulty,
        FastTags<TagGroup.Poi> _includeTags,
        FastTags<TagGroup.Poi> _excludeTags,
        float _minSearchDistance,
        float _maxSearchDistance,
        int posX = 0, int posZ = -1,
        float sizeX = 0f, float sizeY = 0f, float sizeZ = 0f,
        BiomeFilterTypes _biomeFilterType = BiomeFilterTypes.AnyBiome,
        string _biomeFilter = "")
    {
        entityId = _ownerId;
        questCode = _questCode;
        questTags = _questTags;
        difficulty = _difficulty;
        includeTags = _includeTags;
        excludeTags = _excludeTags;
        minSearchDistance = _minSearchDistance;
        maxSearchDistance = _maxSearchDistance;
        position = new Vector2(posX, posZ);
        size = new Vector3(sizeX, sizeY, sizeZ);
        biomeFilterType = _biomeFilterType;
        biomeFilter = _biomeFilter;
        return this;
    }
}
