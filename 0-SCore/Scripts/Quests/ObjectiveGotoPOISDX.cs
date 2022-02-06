using System.Collections.Generic;
using UnityEngine;
using Random = System.Random;

/*
 * 	<objective type="GotoPOISDX, SCore" value="500-800" phase="1">
			<property name="completion_distance" value="50" />
            <property name="PrefabName" value="prefabName" />
		</objective>
*/
internal class ObjectiveGotoPOISDX : ObjectiveRandomPOIGoto
{
    public string strPOIname = "";

    protected override bool useUpdateLoop => true;

    public override BaseObjective Clone()
    {
        var objectiveGotoPOI = new ObjectiveGotoPOISDX();
        CopyValues(objectiveGotoPOI);
        objectiveGotoPOI.strPOIname = strPOIname;
        return objectiveGotoPOI;
    }


    private void SetDistanceOffset(Vector3 POISize)
    {
        if (POISize.x > POISize.z)
            distanceOffset = POISize.x;
        else
            distanceOffset = POISize.z;
    }

  
    protected override Vector3 GetPosition(EntityNPC ownerNPC = null, EntityPlayer entityPlayer = null, List<Vector2> usedPOILocations = null, int entityIDforQuests = -1)
    {
        if (OwnerQuest.GetPositionData(out position, Quest.PositionDataTypes.POIPosition))
        {
            Vector3 vector;
            OwnerQuest.GetPositionData(out vector, Quest.PositionDataTypes.POISize);
            var vector2 = new Vector2(position.x + vector.x / 2f, position.z + vector.z / 2f);
            var num = (int)vector2.x;
            var num2 = (int)GameManager.Instance.World.GetHeightAt(vector2.x, vector2.y);
            var num3 = (int)vector2.y;
            position = new Vector3(num, num2, num3);
            OwnerQuest.Position = position;
            SetDistanceOffset(vector);
            positionSet = true;
            OwnerQuest.HandleMapObject(Quest.PositionDataTypes.POIPosition, NavObjectName);
            CurrentValue = 2;
            return position;
        }

        EntityAlive entityAlive = ownerNPC;

        if (ownerNPC == null)
            entityAlive = OwnerQuest.OwnerJournal.OwnerPlayer;

        var randomPOINearWorldPos = QuestUtils.FindPrefab(strPOIname, entityAlive.position, ref usedPOILocations, biomeFilterType, biomeFilter);
        if (randomPOINearWorldPos != null)
        {
            var vector = new Vector2(randomPOINearWorldPos.boundingBoxPosition.x + randomPOINearWorldPos.boundingBoxSize.x / 2f,
                randomPOINearWorldPos.boundingBoxPosition.z + randomPOINearWorldPos.boundingBoxSize.z / 2f);
            if (vector.x == -0.1f && vector.y == -0.1f)
                return Vector3.zero;

            var num = (int)vector.x;
            var num2 = (int)GameManager.Instance.World.GetHeightAt(vector.x, vector.y);
            var num3 = (int)vector.y;
            position = new Vector3(num, num2, num3);
            if (SingletonMonoBehaviour<ConnectionManager>.Instance.IsServer)
            {
                OwnerQuest.Position = position;
                FinalizePoint(new Vector3(randomPOINearWorldPos.boundingBoxPosition.x, randomPOINearWorldPos.boundingBoxPosition.y, randomPOINearWorldPos.boundingBoxPosition.z),
                    new Vector3(randomPOINearWorldPos.boundingBoxSize.x, randomPOINearWorldPos.boundingBoxSize.y, randomPOINearWorldPos.boundingBoxSize.z));
                OwnerQuest.QuestPrefab = randomPOINearWorldPos;
                OwnerQuest.DataVariables.Add("POIName", OwnerQuest.QuestPrefab.location.Name);
                if (usedPOILocations != null)
                    usedPOILocations.Add(new Vector2(randomPOINearWorldPos.boundingBoxPosition.x, randomPOINearWorldPos.boundingBoxPosition.z));

                OwnerQuest.HandleMapObject(Quest.PositionDataTypes.POIPosition, NavObjectName);
                return position;
            }

            else
            {
                SingletonMonoBehaviour<ConnectionManager>.Instance.SendToServer(NetPackageManager.GetPackage<NetPackageGotoPOISDX>().Setup(entityAlive.entityId, OwnerQuest.QuestTags,
                    OwnerQuest.QuestCode, NetPackageQuestGotoPoint.QuestGotoTypes.RandomPOI, OwnerQuest.QuestClass.DifficultyTier, (int)position.x, (int)position.z, 0f, 0f, 0f, -1f, biomeFilterType, biomeFilter, strPOIname));
                CurrentValue = 1;
            }
        }
        else
        {
            OwnerQuest.MarkFailed();
        }
        

        return Vector3.zero;
    }


    public override void ParseProperties(DynamicProperties properties)
    {
        if (properties.Values.ContainsKey("PrefabName"))
            strPOIname = properties.Values["PrefabName"];
        if (properties.Values.ContainsKey("PrefabNames"))
        {
            var TempList = new List<string>();
            var strTemp = properties.Values["PrefabNames"];

            var array = strTemp.Split(',');
            for (var i = 0; i < array.Length; i++)
            {
                if (TempList.Contains(array[i]))
                    continue;
                TempList.Add(array[i]);
            }

            var random = new Random();
            var index = random.Next(TempList.Count);

            if (TempList.Count == 0)
                Debug.Log(" ObjectiveGoToPOISDX PrefabNames Contains no prefabs.");
            else
                strPOIname = TempList[index];
        }

        base.ParseProperties(properties);
    }
}