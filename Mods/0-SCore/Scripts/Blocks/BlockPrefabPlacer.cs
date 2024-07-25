using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;


public class BlockPrefabPlacer : Block
{
    private string prefabName = "PortalPrefab";

    public override void Init()
    {
        if (Properties.Values.ContainsKey("Prefab"))
            prefabName = Properties.Values["Prefab"];

        // Reset the Multi block dim for the placer
        var location = PathAbstractions.PrefabsSearchPaths.GetLocation(prefabName);
        var prefabTemplate = new Prefab();
        prefabTemplate.Load(prefabName, true, true, true);
        prefabTemplate.LoadXMLData(location);
        Properties.Values[Block.PropMultiBlockDim] = prefabTemplate.size.ToString();
        Properties.Values[Block.PropPlacementDistance] = prefabTemplate.size.x.ToString() ;

        base.Init();
    }

    // Over-write whatever is there. Screw it!
    public override bool CanPlaceBlockAt(WorldBase _world, int _clrIdx, Vector3i _blockPos, BlockValue _blockValue, bool _bOmitCollideCheck = false)
    {
        return true;
    }
    // When the block is added, set the prefab then.
    public override void OnBlockAdded(WorldBase _world, Chunk _chunk, Vector3i _blockPos, BlockValue _blockValue)
    {
        base.OnBlockAdded(_world, _chunk, _blockPos, _blockValue);
        SetPrefab(_blockPos);
    }

    public void SetPrefab(Vector3i _blockPos )
    {
        var chunk = (Chunk)((World)GameManager.Instance.World).GetChunkFromWorldPos(_blockPos);
        var location = PathAbstractions.PrefabsSearchPaths.GetLocation(prefabName);
        
        var prefabTemplate = GameManager.Instance.GetDynamicPrefabDecorator().GetPrefab(prefabName, true, true, true);
        if (prefabTemplate == null)
        {
            // If it's not in the prefab decorator, load it up.
            prefabTemplate = new Prefab();
            prefabTemplate.Load(prefabName, true, true, true);
            prefabTemplate.LoadXMLData(location);
        }
        var myPrefab = prefabTemplate.Clone();

        myPrefab.CopyBlocksIntoChunkNoEntities(GameManager.Instance.World, chunk, _blockPos, true);
        var entityInstanceIds = new List<int>();
        myPrefab.CopyEntitiesIntoChunkStub(chunk, _blockPos, entityInstanceIds, true);

        var prefabInstance = new PrefabInstance(GameManager.Instance.GetDynamicPrefabDecorator().GetNextId(), location, _blockPos, 0, myPrefab, 0);
        GameManager.Instance.GetDynamicPrefabDecorator().AddPrefab(prefabInstance, false);

    }
}

