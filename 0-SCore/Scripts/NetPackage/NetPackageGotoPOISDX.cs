using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

public class NetPackageGotoPOISDX : NetPackage
{
    public NetPackageGotoPOISDX Setup(int _playerId, FastTags _questTags, int _questCode, NetPackageQuestGotoPoint.QuestGotoTypes _gotoType, byte _difficulty, int posX = 0, int posZ = -1, float sizeX = 0f, float sizeY = 0f, float sizeZ = 0f, float offset = -1f, BiomeFilterTypes _biomeFilterType = BiomeFilterTypes.AnyBiome, string _biomeFilter = "", string _POIFilter = "")
    {
        this.playerId = _playerId;
        this.questCode = _questCode;
        this.GotoType = _gotoType;
        this.questTags = _questTags;
        this.position = new Vector2((float)posX, (float)posZ);
        this.size = new Vector3(sizeX, sizeY, sizeZ);
        this.difficulty = _difficulty;
        this.biomeFilterType = _biomeFilterType;
        this.biomeFilter = _biomeFilter;
        poiFilter = _POIFilter;
        return this;
    }

    public override void read(PooledBinaryReader _br)
    {
        this.playerId = _br.ReadInt32();
        this.questCode = _br.ReadInt32();
        this.GotoType = (NetPackageQuestGotoPoint.QuestGotoTypes)_br.ReadByte();
        this.questTags = FastTags.Parse(_br.ReadString());
        this.position = new Vector2((float)_br.ReadInt32(), (float)_br.ReadInt32());
        this.size = StreamUtils.ReadVector3(_br);
        this.difficulty = _br.ReadByte();
        this.biomeFilterType = (BiomeFilterTypes)_br.ReadByte();
        this.biomeFilter = _br.ReadString();
        this.poiFilter = _br.ReadString();

    }

    public override void write(PooledBinaryWriter _bw)
    {
        base.write(_bw);
        _bw.Write(this.playerId);
        _bw.Write(this.questCode);
        _bw.Write((byte)this.GotoType);
        _bw.Write(this.questTags.ToString());
        _bw.Write((int)this.position.x);
        _bw.Write((int)this.position.y);
        StreamUtils.Write(_bw, this.size);
        _bw.Write(this.difficulty);
        _bw.Write((byte)this.biomeFilterType);
        _bw.Write(this.biomeFilter);
        _bw.Write(this.poiFilter);

    }

    public override void ProcessPackage(World _world, GameManager _callbacks)
    {
        if (_world == null)
        {
            return;
        }

        var usedPositions = new List<Vector2>();
        if (SingletonMonoBehaviour<ConnectionManager>.Instance.IsServer)
        {
            for (int i = 0; i < 5; i++)
            {
                //  Debug.Log($"Checking Position {this.position}");
                //var prefabInstance = GameManager.Instance.World.ChunkClusters[0].ChunkProvider.GetDynamicPrefabDecorator().GetClosestPOIToWorldPos(this.questTags, new Vector2(position.x, position.y), null, -1, false, BiomeFilterTypes.AnyBiome, "");
                PrefabInstance prefabInstance = QuestUtils.FindPrefab(poiFilter, position, ref usedPositions, biomeFilterType, biomeFilter);
                //new Vector2((float)prefabInstance.boundingBoxPosition.x + (float)prefabInstance.boundingBoxSize.x / 2f, (float)prefabInstance.boundingBoxPosition.z + (float)prefabInstance.boundingBoxSize.z / 2f);
                if (prefabInstance != null)
                {
                    SingletonMonoBehaviour<ConnectionManager>.Instance.SendPackage(NetPackageManager.GetPackage<NetPackageQuestGotoPoint>().Setup(this.playerId, this.questTags, this.questCode, this.GotoType, this.difficulty, prefabInstance.boundingBoxPosition.x, prefabInstance.boundingBoxPosition.z, (float)prefabInstance.boundingBoxSize.x, (float)prefabInstance.boundingBoxSize.y, (float)prefabInstance.boundingBoxSize.z, -1f, biomeFilterType, biomeFilter), false, this.playerId, -1, -1, -1);
                    //SingletonMonoBehaviour<ConnectionManager>.Instance.SendPackage(NetPackageManager.GetPackage<NetPackageQuestGotoPoint>().Setup(this.playerId, this.questTags, this.questCode, this.GotoType, this.difficulty, (int)position.x, (int)position.y, (float)prefabInstance.boundingBoxSize.x, (float)prefabInstance.boundingBoxSize.y, (float)prefabInstance.boundingBoxSize.z, -1f, biomeFilterType, biomeFilter), false, this.playerId, -1, -1, -1);var num2 = (int)GameManager.Instance.World.GetHeightAt(position.x, position.y);

                    return;
                }
            }
            return;
        }

        EntityPlayer primaryPlayer = GameManager.Instance.World.GetPrimaryPlayer();
        Quest quest = primaryPlayer.QuestJournal.FindActiveQuest(this.questCode);
        if (quest != null)
        {
            for (int j = 0; j < quest.Objectives.Count; j++)
            {
                Log.Out($"Objective position: {this.position}");
                var num2 = (int)GameManager.Instance.World.GetHeightAt(position.x, position.y);
                ((ObjectiveGotoPOISDX)quest.Objectives[j]).FinalizePoint(new Vector3(this.position.x, num2, this.position.y), this.size);
            }
        }
    }

    public override int GetLength()
    {
        return 8;
    }


    private byte difficulty;
    private int playerId;

    private int questCode;

    private FastTags questTags;

    private Vector2 position;

    private BiomeFilterTypes biomeFilterType;

    private string biomeFilter;
    private Vector3 size;

    private string poiFilter;

    public NetPackageQuestGotoPoint.QuestGotoTypes GotoType;

}

