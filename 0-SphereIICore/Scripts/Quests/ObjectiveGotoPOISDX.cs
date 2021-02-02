using System;
using System.Collections.Generic;
using UnityEngine;

/*
 * 	<objective type="GotoPOISDX, Mods" value="500-800" phase="1">
			<property name="completion_distance" value="50" />
            <property name="PrefabName" value="prefabName" />
		</objective>
*/
class ObjectiveGotoPOISDX : ObjectiveRandomPOIGoto
{
    public String strPOIname = "";

    public override BaseObjective Clone()
    {
        ObjectiveGotoPOISDX objectiveGotoPOI = new ObjectiveGotoPOISDX();
        CopyValues(objectiveGotoPOI);
        objectiveGotoPOI.strPOIname = strPOIname;
        return objectiveGotoPOI;
    }



    private void SetDistanceOffset(Vector3 POISize)
    {
        if (POISize.x > POISize.z)
        {
            distanceOffset = POISize.x;
        }
        else
        {
            distanceOffset = POISize.z;
        }
    }


    protected override Vector3 GetPosition(EntityNPC ownerNPC = null, EntityPlayer entityPlayer = null, List<Vector2> usedPOILocations = null, int entityIDforQuests = -1)
    {
        if (base.OwnerQuest.GetPositionData(out position, Quest.PositionDataTypes.POIPosition))
        {
            Vector3 vector;
            base.OwnerQuest.GetPositionData(out vector, Quest.PositionDataTypes.POISize);
            Vector2 vector2 = new Vector2(position.x + vector.x / 2f, position.z + vector.z / 2f);
            int num = (int)vector2.x;
            int num2 = (int)vector2.y;
            int num3 = (int)GameManager.Instance.World.GetHeightAt(vector2.x, vector2.y);
            position = new Vector3(num, num3, num2);
            base.OwnerQuest.Position = position;
            SetDistanceOffset(vector);
            positionSet = true;
            base.OwnerQuest.HandleMapObject(Quest.PositionDataTypes.POIPosition, NavObjectName, -1);
            base.CurrentValue = 2;
            return position;
        }

        EntityAlive entityAlive = ownerNPC;

        if (ownerNPC == null)
            entityAlive = base.OwnerQuest.OwnerJournal.OwnerPlayer;

        if (SingletonMonoBehaviour<ConnectionManager>.Instance.IsServer)
        {
            PrefabInstance randomPOINearWorldPos = GameManager.Instance.World.ChunkClusters[0].ChunkProvider.GetDynamicPrefabDecorator().GetPOIPrefabs().Find(instance => instance.name.Contains(strPOIname));

            if (randomPOINearWorldPos != null)
            {
                Vector2 vector = new Vector2(randomPOINearWorldPos.boundingBoxPosition.x + randomPOINearWorldPos.boundingBoxSize.x / 2f, randomPOINearWorldPos.boundingBoxPosition.z + randomPOINearWorldPos.boundingBoxSize.z / 2f);
                if (vector.x == -0.1f && vector.y == -0.1f)
                    return Vector3.zero;

                int num = (int)vector.x;
                int num2 = (int)entityAlive.position.y;
                int num3 = (int)vector.y;
                position = new Vector3(num, num2, num3);
                if (GameManager.Instance.World.IsPositionInBounds(position))
                {
                    base.OwnerQuest.Position = position;
                    base.FinalizePoint(new Vector3(randomPOINearWorldPos.boundingBoxPosition.x, randomPOINearWorldPos.boundingBoxPosition.y, randomPOINearWorldPos.boundingBoxPosition.z), new Vector3(randomPOINearWorldPos.boundingBoxSize.x, randomPOINearWorldPos.boundingBoxSize.y, randomPOINearWorldPos.boundingBoxSize.z));
                    base.OwnerQuest.QuestPrefab = randomPOINearWorldPos;
                    base.OwnerQuest.DataVariables.Add("POIName", base.OwnerQuest.QuestPrefab.name);
                    if (usedPOILocations != null)
                        usedPOILocations.Add(new Vector2(randomPOINearWorldPos.boundingBoxPosition.x, randomPOINearWorldPos.boundingBoxPosition.z));

                    base.OwnerQuest.HandleMapObject(Quest.PositionDataTypes.POIPosition, NavObjectName, -1);
                    return position;
                }
            }
        }
        else
        {
            SingletonMonoBehaviour<ConnectionManager>.Instance.SendToServer(NetPackageManager.GetPackage<NetPackageQuestGotoPoint>().Setup(entityAlive.entityId, base.OwnerQuest.QuestTags, base.OwnerQuest.QuestCode, NetPackageQuestGotoPoint.QuestGotoTypes.RandomPOI, base.OwnerQuest.QuestClass.DifficultyTier, 0, -1, 0f, 0f, 0f, -1f, biomeFilterType, strPOIname), false);
            base.CurrentValue = 1;
        }
        return Vector3.zero;
    }


    public override void ParseProperties(DynamicProperties properties)
    {
        if (properties.Values.ContainsKey("PrefabName"))
            strPOIname = properties.Values["PrefabName"];
        if (properties.Values.ContainsKey("PrefabNames"))
        {
            List<String> TempList = new List<string>();
            string strTemp = properties.Values["PrefabNames"].ToString();

            string[] array = strTemp.Split(new char[] { ',' });
            for (int i = 0; i < array.Length; i++)
            {
                if (TempList.Contains(array[i].ToString()))
                    continue;
                TempList.Add(array[i].ToString());
            }
            var random = new System.Random();
            int index = random.Next(TempList.Count);

            if (TempList.Count == 0)
                Debug.Log(" ObjectiveGoToPOISDX PrefabNames Contains no prefabs.");
            else
                strPOIname = TempList[index];
        }
        base.ParseProperties(properties);

    }

    protected override bool useUpdateLoop
    {
        get
        {
            return true;
        }
    }
}
