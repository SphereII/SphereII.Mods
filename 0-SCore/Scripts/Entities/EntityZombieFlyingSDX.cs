using System;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using Random = UnityEngine.Random;

internal class EntityZombieFlyingSDX : EntityFlying
{
    // flocking logic constants and variables
    private const float BS = 0.25f;
    private const float ES = 10f;
    private const float EE = 80f;
    public static EAIFactory AiFactory = new EAIFactory();
    public int maxHeight = 80; // maximum height modifier, used to prevent the birds from flying too high

    public Vector3 Waypoint;


    // debugging
    private readonly bool debug = false;
    private readonly string returnEntity = "";
    public bool useVanillaAI = false;
    private int AttackTimeout;
    private bool calledBack;

    private int CourseCheck;

    private HashSet<string>
        deflectorBlocks; // if there's a deflector block nearby, entity (master if flock) will avoid flying nearby

    private Vector3 deflectorPosition = Vector3.zero;
    private DateTime dtaNextSpawn = DateTime.MaxValue;
    private bool followPlayers; // TODO - the entity will follow players or the owner
    private bool hadFirstRead;
    private bool hasFlock;
    private bool HasWaypoint;
    private int hunterLevel;
    private bool isHunting;
    private Vector3 landPosition = Vector3.zero;

    private Vector3 lastSpawnPosition = Vector3.zero;

    // flocking properties
    private EntityAlive masterEntity;
    private int maxToSpawn;
    private float meshScale = 1;

    private HashSet<string>
        naturalEnemies; // list of natural enemies. Entity (master if flock) will try to hunt found enemy

    private int numberSpawned;
    private int oldmasterID;

    private EntityPlayer ownerEntity;

    // pet required information
    private int ownerID;

    // these 2 are used to rebuild the flock
    private int previousID;
    private bool retaliateAttack; // the entity will retaliate if attacked or if the master is attacked / killed
    private string tameItem = "";

    private HashSet<string>
        targetBlocks; // list of landing blocks. Entity (master if flock) will try to fly near them, if nearby

    // optional AI
    private bool targetPlayers; // the entity will target players on sight


    protected override void Awake()
    {
        var component = gameObject.GetComponent<BoxCollider>();
        if (component)
        {
            component.center = new Vector3(0f, 0.35f, 0f);
            component.size = new Vector3(0.4f, 0.4f, 0.4f);
        }

        base.Awake();
        var transform = this.transform.Find("Graphics/BlobShadowProjector");
        if (transform) transform.gameObject.SetActive(false);
    }

    // pet properties
    public override void Init(int _entityClass)
    {
        base.Init(_entityClass);
        emodel.SetVisible(true, true);
        if (SingletonMonoBehaviour<ConnectionManager>.Instance.IsServer)
            getNavigator().setCanDrown(true);
        //base.getNavigator().setInWater(false);
        // Sets the hand value, so we can give our entities ranged weapons.
        inventory.SetSlots(new[]
        {
            new ItemStack(inventory.GetBareHandItemValue(), 1)
        });

        string[] auxList = null;
        var entityClass = EntityClass.list[_entityClass];
        if (entityClass.Properties.Values.ContainsKey("FlockSize"))
        {
            if (int.TryParse(entityClass.Properties.Values["FlockSize"], out maxToSpawn) == false) maxToSpawn = 10;
            if (maxToSpawn > 0) maxToSpawn = Random.Range(0, maxToSpawn);
        }

        if (entityClass.Properties.Values.ContainsKey("MaxHeight"))
            if (int.TryParse(entityClass.Properties.Values["MaxHeight"], out maxHeight) == false)
                maxHeight = 80;
        if (entityClass.Properties.Values.ContainsKey("IsAgressive"))
            if (bool.TryParse(entityClass.Properties.Values["IsAgressive"], out targetPlayers) == false)
                targetPlayers = false;
        if (entityClass.Properties.Values.ContainsKey("FollowPlayer"))
            if (bool.TryParse(entityClass.Properties.Values["FollowPlayer"], out followPlayers) == false)
                followPlayers = false;
        if (entityClass.Properties.Values.ContainsKey("RetaliateAttack"))
            if (bool.TryParse(entityClass.Properties.Values["RetaliateAttack"], out retaliateAttack) == false)
                retaliateAttack = false;
        if (entityClass.Properties.Values.ContainsKey("LandingBlocks"))
        {
            auxList = entityClass.Properties.Values["LandingBlocks"].Split(',');
            targetBlocks = new HashSet<string>(auxList);
        }

        if (entityClass.Properties.Values.ContainsKey("DeflectorBlocks"))
        {
            auxList = entityClass.Properties.Values["DeflectorBlocks"].Split(',');
            deflectorBlocks = new HashSet<string>(auxList);
        }

        if (entityClass.Properties.Values.ContainsKey("NaturalEnemies"))
        {
            auxList = entityClass.Properties.Values["NaturalEnemies"].Split(',');
            naturalEnemies = new HashSet<string>(auxList);
        }

        if (entityClass.Properties.Values.ContainsKey("TameItem")) tameItem = entityClass.Properties.Values["TameItem"];
        if (entityClass.Properties.Values.ContainsKey("MeshScale"))
        {
            var meshScaleStr = entityClass.Properties.Values["MeshScale"];
            var parts = meshScaleStr.Split(',');

            float minScale = 1;
            float maxScale = 1;

            if (parts.Length == 1)
            {
                maxScale = minScale = float.Parse(parts[0]);
            }
            else if (parts.Length == 2)
            {
                minScale = float.Parse(parts[0]);
                maxScale = float.Parse(parts[1]);
            }

            meshScale = Random.Range(minScale, maxScale);
            gameObject.transform.localScale = new Vector3(meshScale, meshScale, meshScale);
        }

        if (entityClass.Properties.Values.ContainsKey("UseVanillaAI"))
            bool.TryParse(entityClass.Properties.Values["UseVanillaAI"], out useVanillaAI);
                
        auxList = null;
    }

    public override void InitFromPrefab(int _entityClass)
    {
        base.InitFromPrefab(_entityClass);
        emodel.SetVisible(true, true);
        getNavigator().setCanDrown(true);
        //base.getNavigator().setInWater(false);
    }

    public override void PostInit()
    {
        //if (debug && !world.IsRemote()) Debug.Log("POST INIT BEFORE: OWNER ID = " + (int)this.lifetime);
        if (IsAlive() && (int)lifetime > 0 && (int)lifetime < 9999999 && !world.IsRemote())
        {
            // find the owner with the same id
            var player = world.GetEntity((int)lifetime);
            if (player != null)
            {
                if (debug)
                    Debug.Log("Found PLAYER holding item " + (player as EntityPlayer).inventory.holdingItem.Name +
                              " with UID = " + (player as EntityPlayer).inventory.holdingItemData.itemValue.Meta +
                              " and quality = " + (player as EntityPlayer).inventory.holdingItemData.itemValue.Quality +
                              " and usetimes = " +
                              (player as EntityPlayer).inventory.holdingItemData.itemValue.UseTimes);
                setOwnerID((int)lifetime);
                hunterLevel = (int)(player as EntityPlayer).inventory.holdingItemData.itemValue.UseTimes;
                (player as EntityPlayer).inventory.holdingItemItemValue.UseTimes = 0;
                (player as EntityPlayer).inventory.holdingItemItemValue.Meta = 1;
                (player as EntityPlayer).inventory.ForceHoldingItemUpdate(); // see if this refreshes the item
                (player as EntityPlayer).inventory.CallOnToolbeltChangedInternal();
            }

            //GetOwnership((int)this.lifetime);
            lifetime = float.MaxValue;
        }

        base.PostInit();
        hadFirstRead = false;
    }

    // find the damn owner by item UID
    // MIGHT STILL NEED THIS (USING USETIMES
    private bool GetOwnership(int uid)
    {
        if (debug) Debug.Log("FIND OWNERSHIP FOR UID = " + uid);
        var baseDistance = 999;
        Entity _other = null;
        using (
            var enumerator =
                world.GetEntitiesInBounds(typeof(EntityPlayer),
                    BoundsUtils.BoundsForMinMax(position.x - baseDistance, position.y - baseDistance,
                        position.z - baseDistance, position.x + baseDistance,
                        position.y + baseDistance,
                        position.z + baseDistance), new List<Entity>()).GetEnumerator())
        {
            while (enumerator.MoveNext())
            {
                _other = enumerator.Current as EntityPlayer;
                if (debug)
                    Debug.Log("Found PLAYER holding item " + (_other as EntityPlayer).inventory.holdingItem.Name +
                              " with UID = " + (_other as EntityPlayer).inventory.holdingItemItemValue.Meta +
                              " and quality = " + (_other as EntityPlayer).inventory.holdingItemItemValue.Quality +
                              " and usetimes = " + (_other as EntityPlayer).inventory.holdingItemItemValue.UseTimes);
                if ((_other as EntityPlayer).inventory.holdingItem.Name == tameItem)
                    if ((_other as EntityPlayer).inventory.holdingItemItemValue.Meta == uid)
                    {
                        ownerID = _other.entityId;
                        hunterLevel = (int)(_other as EntityPlayer).inventory.holdingItemItemValue.UseTimes;
                        if (debug) Debug.Log("Found owner with ID = " + ownerID + " and huntinglevel = " + hunterLevel);
                        (_other as EntityPlayer).inventory.holdingItemItemValue.UseTimes = 0;
                        (_other as EntityPlayer).inventory.holdingItemItemValue.Meta = 1;
                        (_other as EntityPlayer).inventory.ForceHoldingItemUpdate();
                        (_other as EntityPlayer).inventory.CallOnToolbeltChangedInternal();
                        return true;
                    }
            }
        }

        return false;
    }

    // cheating a netpackage for commands, using the damageresponse
    public override void ProcessDamageResponse(DamageResponse _dmResponse)
    {
        if (ProccessCommands(_dmResponse)) return;
        base.ProcessDamageResponse(_dmResponse);
    }

    public override void ProcessDamageResponseLocal(DamageResponse _dmResponse)
    {
        if (ProccessCommands(_dmResponse)) return;
        base.ProcessDamageResponseLocal(_dmResponse);
    }

    private bool ProccessCommands(DamageResponse _dmResponse)
    {
        // Disabling for A17 patch
        //  if (_dmResponse.Source.GetName() == EnumDamageSourceType.Disease)
        if (_dmResponse.Strength >= 1000 && _dmResponse.Strength < 2000)
        {
            // it's a command, i take out 1000 from it to obtain the command "bits"
            var commands = _dmResponse.Strength - 1000;

            if (commands == 1)
            {
                // return to player
                callBack();
            }
            else if (commands == 2)
            {
                // despawn the entity, captured
                MarkToUnload();
            }
            else if (commands == 3)
            {
                // start taming
                TameBird();
            }
            else if (commands == 4)
            {
                // failed taming, wild bird
                ownerID = 0;
                SetRevengeTarget(null);
            }

            return true;
        }

        return false;
    }


    // Since the Vulture class doesn't inherit any of the AI tasks that we want and need, we'll over-ride and take control
    public override void CopyPropertiesFromEntityClass()
    {
        var entityClass = EntityClass.list[this.entityClass];
        base.CopyPropertiesFromEntityClass();
        if (SingletonMonoBehaviour<ConnectionManager>.Instance.IsServer) aiManager.CopyPropertiesFromEntityClass(entityClass);
    }
    //    var num = 1;
    //    // Grab all the AITasks avaiable
    //    while (entityClass.Properties.Values.ContainsKey(EntityClass.PropAITask + num))
    //    {
    //        var text = "EAI" + entityClass.Properties.Values[EntityClass.PropAITask + num];
    //        var eaibase = (EAIBase)AiFactory.Instantiate(text);
    //        if (eaibase == null) throw new Exception("Class '" + text + "' not found!");

    //        eaibase.SetEntity(this);
    //        if (entityClass.Properties.Params1.ContainsKey(EntityClass.PropAITask + num))
    //            eaibase.SetParams1(entityClass.Properties.Params1[EntityClass.PropAITask + num]);
    //        if (entityClass.Properties.Params2.ContainsKey(EntityClass.PropAITask + num))
    //            eaibase.SetParams2(entityClass.Properties.Params2[EntityClass.PropAITask + num]);
    //        TaskList.AddTask(num, eaibase);
    //        num++;
    //    }

    //    var num2 = 1;
    //    // Grab all the AI Targets
    //    while (entityClass.Properties.Values.ContainsKey(EntityClass.PropAITargetTask + num2))
    //    {
    //        var text2 = "EAI" + entityClass.Properties.Values[EntityClass.PropAITargetTask + num2];
    //        var eaibase2 = (EAIBase)AiFactory.Instantiate(text2);
    //        if (eaibase2 == null) throw new Exception("Class '" + text2 + "' not found!");
    //        eaibase2.SetEntity(this);
    //        if (entityClass.Properties.Params1.ContainsKey(EntityClass.PropAITargetTask + num2))
    //            eaibase2.SetParams1(entityClass.Properties.Params1[EntityClass.PropAITargetTask + num2]);
    //        if (entityClass.Properties.Params2.ContainsKey(EntityClass.PropAITargetTask + num2))
    //            eaibase2.SetParams2(entityClass.Properties.Params2[EntityClass.PropAITargetTask + num2]);
    //        TargetList.AddTask(num2, eaibase2);
    //        num2++;
    //    }
    //}


    // The vulture updateTasks() doesn't call down the chain, so it never does any checks on the AI Tasks.
    protected override void updateTasks()
    {
        if (!useVanillaAI)
        {
            flockTasks(); // flocking logic, if needed
        }
        else
        {
            LegacyTask();
            base.updateTasks();
        }
    }

    // TODO - i may eventually use this to assign a new leader to the flock
    public override void OnEntityDeath()
    {
        // TODO - give the leadership to one of the children OR let the flock scatter
        // if we want to assign a new leader then do this:
        // find all crows in the vicinity
        // if they have himself as master, change them to the new one
        base.OnEntityDeath();
    }

    // resyncing the flock
    public int GetLastId()
    {
        return previousID;
    }

    // binary read
    public override void Write(BinaryWriter _bw, bool bNetworkWrite)
    {
        base.Write(_bw, bNetworkWrite);
        // persisting current state
        try
        {
            _bw.Write(hasFlock);
            _bw.Write(numberSpawned);
            var masterID = 0;
            if (masterEntity != null) masterID = masterEntity.entityId;
            _bw.Write(masterID);
            if (previousID == 0 && entityId > 0) previousID = entityId;
            _bw.Write(previousID);
            _bw.Write(ownerID);
            _bw.Write(hunterLevel);
            _bw.Write(calledBack);
        }
        catch
        {
        }
    }

    public override void Read(byte _version, BinaryReader _br)
    {
        base.Read(_version, _br);
        if (_br.BaseStream.Position == _br.BaseStream.Length)
            return; //probably a vanilla entity so just return.
        try
        {
            hasFlock = _br.ReadBoolean();
            numberSpawned = _br.ReadInt32();
            oldmasterID = _br.ReadInt32();
            if (_br.BaseStream.Position == _br.BaseStream.Length)
                return; // still doesn't have previous ID for some reason
            previousID = _br.ReadInt32();
            if (_br.BaseStream.Position == _br.BaseStream.Length)
                return; // still doesn't have previous ID for some reason
            ownerID = _br.ReadInt32();
            hunterLevel = _br.ReadInt32();
            calledBack = _br.ReadBoolean();
        }
        catch
        {
        }

        if (!hadFirstRead) hadFirstRead = true;
    }

    // find the old master to reflock on reload
    private void FindOldMaster()
    {
        if (oldmasterID > 0 && previousID > 0)
            if (masterEntity == null)
            {
                // if it has a master entity saved, and not yet assigned, will try to do it here
                // searches for a EntityZombieFlyingSDX that has this id stored
                // but only if the saved id still exists, and is of the same type.                    
                // look for the entity that had this masterID previously
                foreach (var entity in GameManager.Instance.World.Entities.list)
                    if (entity is EntityZombieFlyingSDX && entity.IsAlive())
                        if ((entity as EntityZombieFlyingSDX).GetLastId() == oldmasterID)
                        {
                            oldmasterID = entity.entityId;
                            setMasterEntity(entity as EntityAlive);
                            if (debug) Debug.Log("Found master entity with new id = " + oldmasterID);
                            break;
                        }

                if (masterEntity == null)
                {
                    if (debug) Debug.Log("Did NOT find master entity with old id = " + oldmasterID);
                    // i need to keep hasFlock = true or it will spawn a new flock.
                    oldmasterID = 0;
                }
            }
    }

    // set ownership by a player, if pet
    public void setOwnerID(int _ownerID)
    {
        ownerID = _ownerID;
    }

    // get owner entityID, if a pet
    public int getOwnerID()
    {
        return ownerID;
    }

    // get current hunting level
    public int getHunterLevel()
    {
        return hunterLevel;
    }

    // call pet back to owner
    public void callBack()
    {
        if (ownerID > 0) calledBack = true;
    }

    // TODO - taming procedure
    public void TameBird()
    {
        // has a success change of actually doing it, AND make the animal 
        hunterLevel = Random.Range(5, 10);
        var tamer = GameManager.Instance.World.GetEntity(ownerID);
        SetRevengeTarget(tamer as EntityAlive);
    }

    public void setMasterEntity(EntityAlive masterE)
    {
        masterEntity = masterE;
        oldmasterID = masterE.entityId;
        hasFlock = true;
    }

    public EntityAlive GetMasterEntity()
    {
        if (masterEntity != null)
            try
            {
                if (masterEntity.IsDespawned)
                {
                    // master entity despawned
                    if (debug) Debug.Log("Master entity despawned");
                    masterEntity = null;
                    //hasFlock = false; -> never does this, or it would start spawning new children
                }
                else if (masterEntity.IsDead())
                {
                    if (debug) Debug.Log("Master entity died");
                    masterEntity = null;
                    //hasFlock = false; -> never does this, or it would start spawning new children
                }
            }
            catch (Exception ex)
            {
                if (debug) Debug.Log("WARNING: Master entity doesn't exist? - " + ex.Message);
            }

        return masterEntity;
    }

    // spawns a children next to the parent.
    private void SpawnFlock()
    {
        if (previousID <= 0) return; // does NOT do this yet.
        if (hasFlock) return;
        if (dtaNextSpawn > DateTime.Now.AddSeconds(30)) dtaNextSpawn = DateTime.Now.AddMilliseconds(500);
        if (DateTime.Now < dtaNextSpawn) return;
        // spawn them right by the parent        
        // separates the spawning a few miliseconds just to avoid overlapping
        var newPos = position + Vector3.up;
        var maxDistance = new Vector3(5, 5, 5);
        int x;
        int y;
        int z;
        if (world.FindRandomSpawnPointNearPosition(position, 15, out x, out y, out z, maxDistance, false, true))
        {
            newPos = new Vector3(x, y, z);
            if (lastSpawnPosition != newPos)
            {
                lastSpawnPosition = newPos;
                if (debug) Debug.Log("SPAWNING CHILDREN FOR ID " + entityId);
                var spawnEntity = EntityFactory.CreateEntity(EntityClass.FromString(EntityName), newPos);
                spawnEntity.SetSpawnerSource(EnumSpawnerSource.Unknown);
                GameManager.Instance.World.SpawnEntityInWorld(spawnEntity);
                (spawnEntity as EntityZombieFlyingSDX).setMasterEntity(this);
                // flying entities spawning is a bit odd, so we "override" the position to where we want the children to be
                // which is by its parent
                // spawnEntity.position = newPos;
                numberSpawned++;
                if (numberSpawned > maxToSpawn)
                    hasFlock = true;
            }
        }
        else if (debug)
        {
            Debug.Log("No place to spawn");
        }

        dtaNextSpawn = DateTime.Now.AddMilliseconds(200);
    }

    public new EntityAlive GetRevengeTarget()
    {
        return base.GetRevengeTarget();
    }

    // look for a landing block nearby
    private bool FindLandSpot()
    {
        landPosition = FindLandSpot(landPosition, targetBlocks, 30);
        if (landPosition != Vector3.zero)
            if (FindNearBlock(deflectorBlocks, landPosition, 5) != Vector3.zero)
                landPosition = Vector3.zero;
        if (landPosition != Vector3.zero) return true;
        return false;
    }

    public Vector3 GetLandingSpot()
    {
        return landPosition;
    }

    // searches for a block inside the given range
    private Vector3 FindLandSpot(Vector3 position, HashSet<string> blockList, int range)
    {
        if (blockList == null) return Vector3.zero;
        if (blockList.Count == 0) return Vector3.zero;
        if (position != Vector3.zero)
        {
            var blockAux = GameManager.Instance.World.GetBlock(new Vector3i(position));
            //if (Array.Exists(blockList, s => s.Equals(Block.list[blockAux.type].GetBlockName()))) - removed for performance reasons
            if (blockList.Contains(Block.list[blockAux.type].GetBlockName()))
                if (Vector3.Distance(GetPosition(), position) <= range)
                    return position;
        }


        return FindNearBlock(blockList, GetPosition(), range);
    }

    public Vector3 FindNearBlock(HashSet<string> blockList, Vector3 position, int checkRange)
    {
        var result = Vector3.zero;
        if (blockList == null) return result;
        if (blockList.Count == 0) return result;
        for (var i = (int)position.x - checkRange; i <= position.x + checkRange; i++)
            for (var j = (int)position.z - checkRange; j <= position.z + checkRange; j++)
                for (var k = (int)position.y - checkRange; k <= position.y + checkRange; k++)
                {
                    var blockF = GameManager.Instance.World.GetBlock(i, k, j);
                    if (blockList.Contains(Block.list[blockF.type].GetBlockName()))
                        if (tameItem != "")
                        {
                            //if (Block.list[blockF.type] is BlockBirdTrap)
                            //{
                            //    // its a pet and it's a bird trap, then the landblock needs to be "baited"
                            //    if (BlockBirdTrap.isBaited(blockF))
                            //    {
                            //        result = new Vector3(i, k, j);
                            //        return result;
                            //    }
                            //}
                            //else
                            //{
                            result = new Vector3(i, k, j);
                            return result;
                            //  }
                        }
                        else
                        {
                            result = new Vector3(i, k, j);
                            return result;
                        }
                }

        return result;
    }

    // looks for a natural enemy to hunt, if any defined
    private void FindNaturalEnemy()
    {
        if (naturalEnemies == null) return;
        if (naturalEnemies.Count == 0) return;
        using (
            var enumerator =
                world.GetEntitiesInBounds(typeof(EntityAlive),
                    BoundsUtils.BoundsForMinMax(position.x - 50f, position.y - 50f,
                        position.z - 50f, position.x + 50f,
                        position.y + 50f,
                        position.z + 50f), new List<Entity>()).GetEnumerator())
        {
            while (enumerator.MoveNext())
            {
                var _other = enumerator.Current as EntityAlive;
                //if (Array.Exists(naturalEnemies, s => s.Equals(_other.EntityName)))
                if (naturalEnemies.Contains(_other.EntityName))
                    if (_other.IsAlive())
                        if (_other.Water < 0.5f)
                        {
                            //if (debug) Debug.Log("Found natural enemy!");
                            SetRevengeTarget(_other);
                            isHunting = true;
                            return;
                        }
            }
        }
    }

    // if agressive looks for the closest player to attack
    private void FindClosestPlayer()
    {
        var closestPlayer = world.GetClosestPlayer(this, 80f, false);
        if (CanSee(closestPlayer))
            if (closestPlayer.Water < 0.5f)
                SetRevengeTarget(closestPlayer);
    }

    // if pet, searches for the owner
    private bool FindOwner()
    {
        // the higher the level the further it can fly
        if (hunterLevel == 0) hunterLevel = Random.Range(5, 10);
        var baseDistance = 50 * hunterLevel;
        if (isHunting || returnEntity != "")
            baseDistance =
                baseDistance *
                4; // the pet is hunting, so i let it go a bit further away... or it's constantly failing.
        Entity _other = null;
        using (
            var enumerator =
                world.GetEntitiesInBounds(typeof(EntityPlayer),
                    BoundsUtils.BoundsForMinMax(position.x - baseDistance, position.y - baseDistance,
                        position.z - baseDistance, position.x + baseDistance,
                        position.y + baseDistance,
                        position.z + baseDistance), new List<Entity>()).GetEnumerator())
        {
            while (enumerator.MoveNext())
            {
                _other = enumerator.Current as EntityAlive;
                // see if it is the owner.
                if (_other.entityId == ownerID)
                {
                    ownerEntity = _other as EntityPlayer;
                    return true;
                }
            }
        }

        return false;
    }

    // flocking logic
    private void flockTasks()
    {
        if (GamePrefs.GetBool(EnumGamePrefs.DebugStopEnemiesMoving) ||
            GameStats.GetInt(EnumGameStats.GameState) == 2) return;
        if (world.IsRemote()) return;
        if (IsDead()) return;
        if (masterEntity == null && !hasFlock && maxToSpawn > 0)
        {
            SpawnFlock();
        }
        else
        {
            if (masterEntity == null && hasFlock && oldmasterID > 0)
                FindOldMaster();
        }

        GetEntitySenses().ClearIfExpired();
        if (AttackTimeout > 0) 
            AttackTimeout--;
        if (AttackTimeout <= 0)
        {
            var a = Waypoint - position;
            var sqrMagnitude = a.sqrMagnitude;
            if (sqrMagnitude < 1f || sqrMagnitude > 6400f)
            {
                if (!isWithinHomeDistanceCurrentPosition() && GetMasterEntity() == null && !isHunting)
                {
                    // uses vanilla code to stay near "home position" if its a "master"
                    var ye = RandomPositionGenerator.CalcTowards(this, 2 * getMaximumHomeDistance(), 2 * getMaximumHomeDistance(), 2 * getMaximumHomeDistance(),
                        getHomePosition().position.ToVector3());

                    if (!ye.Equals(Vector3.zero))
                    {
                        // going home
                        Waypoint = ye;
                        HasWaypoint = true;
                    }
                }
                else
                {
                    HasWaypoint = false;

                    if (!HasWaypoint)
                    {
                        if (base.GetRevengeTarget() != null &&
                            (base.GetRevengeTarget().GetDistanceSq(this) < 6400f && Random.value <= 0.5f || isHunting))
                        {
                            // if it's targeting an enemy. Notice that if it's hunting I just want it to get it done.
                            Waypoint = base.GetRevengeTarget().GetPosition() + Vector3.up;
                        }
                        else
                        {
                            if (GetMasterEntity() == null)
                            {
                                // if it finds a target block nearby, it will go for it. Not attack it, just go near it
                                // this will make them "destroy" crops for example, if we make them target crop blocks.
                                if (FindLandSpot())
                                {
                                    // going for landing spot
                                    Waypoint = landPosition +
                                               new Vector3((float) ((rand.RandomDouble * 2.0 - 1.0) * 3.0),
                                                   (float) ((rand.RandomDouble * 2.0 - 1.0) * 3.0),
                                                   (float) ((rand.RandomDouble * 2.0 - 1.0) * 3.0));
                                }
                                else
                                {
                                    // chooses a random waypoint - vanilla code
                                    Waypoint = GetPosition() +
                                               new Vector3((float) ((rand.RandomDouble * 2.0 - 1.0) * 16.0),
                                                   (float) ((rand.RandomDouble * 2.0 - 1.0) * 16.0),
                                                   (float) ((rand.RandomDouble * 2.0 - 1.0) * 16.0));
                                    // maximum Y. Just to avoid them going too high (out of sight, out of heart)
                                    var maxY = world.GetHeight((int) Waypoint.x, (int) Waypoint.z) + maxHeight;
                                    if (Waypoint.y > maxY) 
                                        Waypoint.y = maxY;
                                }
                            }
                            else
                            {
                                // if the master has a landing spot, it goes to random position near the landing spot, otherwise just follows master
                                if ((GetMasterEntity() as EntityZombieFlyingSDX)?.GetLandingSpot() == Vector3.zero)
                                    Waypoint = GetMasterEntity().GetPosition() + Vector3.up;
                                else
                                    Waypoint = (GetMasterEntity() as EntityZombieFlyingSDX).GetLandingSpot() +
                                               new Vector3((float) ((rand.RandomDouble * 2.0 - 1.0) * 3.0),
                                                   (float) ((rand.RandomDouble * 2.0 - 1.0) * 3.0),
                                                   (float) ((rand.RandomDouble * 2.0 - 1.0) * 3.0));
                                // }
                            }
                        }
                    }

                    AdjustWayPoint();
                    // if waypoint is not in the air, change it up
                    //                    while (world.GetBlock(new Vector3i(Waypoint)).type != BlockValue.Air.type && num > 0)
                    //                  {
                    //                    Waypoint.y = Waypoint.y + 1f;
                    //                  num--;
                    //            } 
                }

                Waypoint.y = Mathf.Min(Waypoint.y, 250f);
            }

            if (CourseCheck-- <= 0)
            {
                CourseCheck += rand.RandomRange(5) + 2;
                if (IsCourseTraversable(Waypoint, out sqrMagnitude))
                    motion += a / sqrMagnitude * 0.1f;
                else
                    Waypoint = GetPosition();
            }
        }


        float intendedRotation;

        intendedRotation = (float)Math.Atan2(motion.x, motion.z) * 180f / 3.14159274f;
        rotation.y = UpdateRotation(rotation.y, intendedRotation, 10f);
    }

    public virtual void AdjustWayPoint()
    {
        var num = 255;
        // if waypoint is not in the air, change it up
        while (world.GetBlock(new Vector3i(Waypoint)).type != BlockValue.Air.type && num > 0)
        {
            Waypoint.y = Waypoint.y + 1f;
            num--;
        }
    }

    #region BaseHornetClass

    // These variables were grabbed from A17, and changes are isolated to the below methods. 
    private int LV;
    private Vector3 HV;
    private bool AV;
    private int EV;
    private int MV;
    private const float SV = 48f;
    private readonly List<Bounds> YV = new List<Bounds>();

    private void LegacyTask()
    {
        if (GamePrefs.GetBool(EnumGamePrefs.DebugStopEnemiesMoving)) return;
        if (GameStats.GetInt(EnumGameStats.GameState) == 2) return;

        GetEntitySenses().ClearIfExpired();
        if (MV > 0) MV--;
        if (MV <= 0)
        {
            var vector = HV - position;
            var sqrMagnitude = vector.sqrMagnitude;
            if (sqrMagnitude is < 1f or > 2304f)
            {
                if (!isWithinHomeDistanceCurrentPosition())
                {
                    var hv = RandomPositionGenerator.CalcTowards(this, 2 * getMaximumHomeDistance(), 2 * getMaximumHomeDistance(), 2 * getMaximumHomeDistance(),
                        getHomePosition().position.ToVector3());
                    if (!hv.Equals(Vector3.zero))
                    {
                        HV = hv;
                        AV = true;
                    }
                }
                else
                {
                    AV = false;
                    if (base.GetRevengeTarget() != null && base.GetRevengeTarget().GetDistanceSq(this) < 2304f && Random.value <= 0.5f)
                        HV = base.GetRevengeTarget().GetPosition() + Vector3.up;
                    else
                        HV = GetPosition() + new Vector3((float)((rand.RandomDouble * 2.0 - 1.0) * 16.0), (float)((rand.RandomDouble * 2.0 - 1.0) * 16.0),
                            (float)((rand.RandomDouble * 2.0 - 1.0) * 16.0));
                }

                HV.y = Mathf.Min(HV.y, 250f);
            }

            if (LV-- <= 0)
            {
                LV += rand.RandomRange(5) + 2;
                if (IsCourseTraversable(HV, out sqrMagnitude))
                    motion += vector / sqrMagnitude * 0.1f;
                else
                    HV = GetPosition();
            }
        }

        if (base.GetRevengeTarget() != null && base.GetRevengeTarget().IsDead()) 
            SetRevengeTarget(null);
        if (base.GetRevengeTarget() == null || EV-- <= 0)
        {
            var closestPlayer = world.GetClosestPlayer(this, 48f, false);
            if (CanSee(closestPlayer)) SetRevengeTarget(closestPlayer);
            if (base.GetRevengeTarget() != null) 
                EV = 20;
        }

        float distanceSq;
        if (!AV && base.GetRevengeTarget() != null && (distanceSq = base.GetRevengeTarget().GetDistanceSq(this)) < 2304f)
        {
            var num = base.GetRevengeTarget().position.x - position.x;
            var num2 = base.GetRevengeTarget().position.z - position.z;
            rotation.y = Mathf.Atan2(num, num2) * 180f / 3.14159274f;
            if (MV <= 0 && distanceSq < 2.8f && position.y >= base.GetRevengeTarget().position.y && position.y <= base.GetRevengeTarget().getHeadPosition().y && Attack(false))
            {
                MV = GetAttackTimeoutTicks();
                Attack(true);
            }
        }
        else
        {
            rotation.y = (float)Math.Atan2(motion.x, motion.z) * 180f / 3.14159274f;
        }
    }


    protected override bool isDetailedHeadBodyColliders()
    {
        return true;
    }

    protected override bool isRadiationSensitive()
    {
        return false;
    }

    public override float GetEyeHeight()
    {
        return 0.5f;
    }

    private bool IsCourseTraversable(Vector3 pos, out float distance)
    {
        var num = pos.x - position.x;
        var num2 = pos.y - position.y;
        var num3 = pos.z - position.z;
        distance = Mathf.Sqrt(num * num + num2 * num2 + num3 * num3);
        if (distance < 1.5f) return true;
        num /= distance;
        num2 /= distance;
        num3 /= distance;
        var box = boundingBox;
        YV.Clear();
        var num4 = 1;
        while (num4 < distance - 1f)
        {
            box.center += new Vector3(num, num2, num3);
            world.GetCollidingBounds(this, box, YV);
            if (YV.Count > 0) return false;
            num4++;
        }

        return true;
    }

    public override int DamageEntity(DamageSource _damageSource, int _strength, bool _criticalHit, float impulseScale)
    {
        if (base.GetRevengeTarget() != null || _damageSource.getEntityId() == -1)
            return base.DamageEntity(_damageSource, _strength, _criticalHit, impulseScale);
        
        var entityAlive = world.GetEntity(_damageSource.getEntityId()) as EntityAlive;
        if (entityAlive != null && entityAlive.IsCrouching && (_damageSource.GetDamageType() == EnumDamageTypes.Piercing || _damageSource.GetDamageType() == EnumDamageTypes.Bashing ||
                                                               _damageSource.GetDamageType() == EnumDamageTypes.Slashing ||
                                                               _damageSource.GetDamageType() == EnumDamageTypes.Crushing)) _damageSource.DamageMultiplier = Constants.cSneakDamageMultiplier;

        return base.DamageEntity(_damageSource, _strength, _criticalHit, impulseScale);
    }

    #endregion
}