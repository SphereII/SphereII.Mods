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
        this.CopyValues(objectiveGotoPOI);
        objectiveGotoPOI.strPOIname = this.strPOIname;
        return objectiveGotoPOI;
    }



    private void SetDistanceOffset(Vector3 POISize)
    {
        if (POISize.x > POISize.z)
        {
            this.distanceOffset = POISize.x;
        }
        else
        {
            this.distanceOffset = POISize.z;
        }
    }


    protected override Vector3 GetPosition(EntityNPC ownerNPC = null, EntityPlayer entityPlayer = null, List < Vector2> usedPOILocations = null, int entityIDforQuests = -1)
    {
        if (base.OwnerQuest.GetPositionData(out this.position, Quest.PositionDataTypes.POIPosition))
        {
            base.OwnerQuest.Position = this.position;
            Vector3 distanceOffset;
            base.OwnerQuest.GetPositionData(out distanceOffset, Quest.PositionDataTypes.POISize);
            this.SetDistanceOffset(distanceOffset);
            this.positionSet = true;
            base.OwnerQuest.HandleMapObject(Quest.PositionDataTypes.POIPosition, this.icon);
            base.CurrentValue = 2;
            return this.position;
        }

        EntityAlive entityAlive = ownerNPC;

        if (ownerNPC == null)
            entityAlive = base.OwnerQuest.OwnerJournal.OwnerPlayer;

        Debug.Log("GetPosition");
       // if (SingletonMonoBehaviour<ConnectionManager>.Instance.IsServer)
        {
            Debug.Log("Running on Server");
            //PrefabInstance randomPOINearWorldPos = GetRandomPOINearWorldPos(new Vector2(entityAlive.position.x, entityAlive.position.z), 1000, 50000000, base.OwnerQuest.QuestTags, base.OwnerQuest.QuestClass.DifficultyTier, usedPOILocations, entityIDforQuests);
            PrefabInstance randomPOINearWorldPos = GameManager.Instance.World.ChunkClusters[0].ChunkProvider.GetDynamicPrefabDecorator().GetPOIPrefabs().Find( instance => instance.name == this.strPOIname);

            if (randomPOINearWorldPos != null)
            {
                Debug.Log("Random POI Near World Pos is not null");
                Vector2 vector = new Vector2((float)randomPOINearWorldPos.boundingBoxPosition.x + (float)randomPOINearWorldPos.boundingBoxSize.x / 2f, (float)randomPOINearWorldPos.boundingBoxPosition.z + (float)randomPOINearWorldPos.boundingBoxSize.z / 2f);
                if (vector.x == -0.1f && vector.y == -0.1f)
                {
                    return Vector3.zero;
                }
                int num = (int)vector.x;
                int num2 = (int)entityAlive.position.y;
                int num3 = (int)vector.y;
                this.position = new Vector3((float)num, (float)num2, (float)num3);
                if (GameManager.Instance.World.IsPositionInBounds(this.position))
                {
                    Debug.Log("Position is in bounds");
                    base.OwnerQuest.Position = this.position;
                    base.FinalizePoint(new Vector3((float)randomPOINearWorldPos.boundingBoxPosition.x, (float)randomPOINearWorldPos.boundingBoxPosition.y, (float)randomPOINearWorldPos.boundingBoxPosition.z), new Vector3((float)randomPOINearWorldPos.boundingBoxSize.x, (float)randomPOINearWorldPos.boundingBoxSize.y, (float)randomPOINearWorldPos.boundingBoxSize.z));
                    base.OwnerQuest.QuestPrefab = randomPOINearWorldPos;
                    base.OwnerQuest.DataVariables.Add("POIName", base.OwnerQuest.QuestPrefab.name);
                    if (usedPOILocations != null)
                    {
                        Debug.Log("Adding to used POI Location.");
                        usedPOILocations.Add(new Vector2((float)randomPOINearWorldPos.boundingBoxPosition.x, (float)randomPOINearWorldPos.boundingBoxPosition.z));
                    }
                    base.OwnerQuest.HandleMapObject(Quest.PositionDataTypes.POIPosition, this.icon);
                    Debug.Log("Returning Position: " + this.position);
                    return this.position;
                }
            }
            else
                Debug.Log("Random POI Near World Pos is not null");

        }
        //else
        //{
        //    Debug.Log("Server is client.");

        //    SingletonMonoBehaviour<ConnectionManager>.Instance.SendPackage(NetPackageManager.GetPackage<NetPackageQuestGotoPoint>().Setup(entityAlive.entityId, base.OwnerQuest.QuestTags, base.OwnerQuest.QuestCode, NetPackageQuestGotoPoint.QuestGotoTypes.RandomPOI, base.OwnerQuest.QuestClass.DifficultyTier, 0, -1, 0f, 0f, 0f, -1),false);
        //    base.CurrentValue = 1;
        //}
        return Vector3.zero;
    }


    public override void ParseProperties(DynamicProperties properties)
    {
        if (properties.Values.ContainsKey("PrefabName"))
            this.strPOIname = properties.Values["PrefabName"];
        if (properties.Values.ContainsKey("PrefabNames"))
        {
            List<String> TempList = new List<string>();
            string strTemp = properties.Values["PrefabNames"].ToString();

            string[] array = strTemp.Split(new char[] {',' });
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
                this.strPOIname = TempList[index];
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
