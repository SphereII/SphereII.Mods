/*
 *
 *      Test Class, Please Ignore
 *
 * This class currently will add flocking logic to walking zombies, causing each zombie to spawn multiple other entities of the same type.
 *
 *
 */


using System;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using Random = System.Random;

// Extending HAL9000's Zombies Run In Dark mod by adding speed variation
public class EntityZombieFlockSDX : EntityZombie
{
    // flocking logic constants and variables
    private const float BS = 0.25f;
    private const float ES = 10f;
    private const float EE = 80f;

    public static Random random = new Random();

    // Globa value for the light threshold in which zombies run in the dark
    public byte LightThreshold = 5;

    // Frequency of check to determine the light level.
    public float CheckDelay = 1f;

    public float WalkTypeDelay = 10f;
    public float NextWalkCheck;

    // debugging
    private readonly bool debug = false;
    private int AttackTimeout;
    private bool blHeadShotsMatter;

    // Default value
    private bool blIdleSleep;
    private bool blRandomSpeeds;
    private int CourseCheck;
    private string[] deflectorBlocks; // if there's a deflector block nearby, entity (master if flock) will avoid flying nearby
    private Vector3 deflectorPosition = Vector3.zero;
    private DateTime dtaNextSpawn = DateTime.MinValue;

    // Set the default scale of the zombies
    private float flScale = 1f;
    private bool followPlayers; // TODO - the entity will follow players or the owner
    private bool hasFlock;
    private bool HasWaypoint;

    // Caching the walk types and approach speed
    private int intWalkType;
    protected bool isHunting;
    private Vector3 landPosition = Vector3.zero;
    private byte lightLevel = 10;

    // flocking properties
    private EntityAlive masterEntity;
    private int maxHeight = 80; // maximum height modifier, used to prevent the birds from flying too high
    private int maxToSpawn;

    private string[] naturalEnemies; // list of natural enemies. Entity (master if flock) will try to hunt found enemy

    // Stores when to do the next light check and what the current light level is
    // Determines if they run or not.
    private float nextCheck;
    private int numberSpawned;

    private int oldmasterID;

    // these 2 are used to rebuild the flock
    private int previousID;
    private bool retaliateAttack; // the entity will retaliate if attacked or if the master is attacked / killed
    private string[] strFlockEntities; // optional flocking entities
    private string[] targetBlocks; // list of landing blocks. Entity (master if flock) will try to fly near them, if nearby
    private int TargetInterval;
    private bool targetPlayers; // the entity will target players on sight
    private Vector3 Waypoint;

    public override void Init(int _entityClass)
    {
        base.Init(_entityClass);

        var entityClass = EntityClass.list[_entityClass];

        // If true, reduces body damage, and requires headshots
        if (entityClass.Properties.Values.ContainsKey("HeadShots"))
            bool.TryParse(entityClass.Properties.Values["HeadShots"], out blHeadShotsMatter);

        // If true, allows the zombies to move faster or slower
        if (entityClass.Properties.Values.ContainsKey("RandomSpeeds"))
            bool.TryParse(entityClass.Properties.Values["RandomSpeeds"], out blRandomSpeeds);

        // If true, puts the zombie to sleep, rather than Idle animation
        if (entityClass.Properties.Values.ContainsKey("IdleSleep"))
            bool.TryParse(entityClass.Properties.Values["IdleSleep"], out blIdleSleep);

        if (entityClass.Properties.Values.ContainsKey("FlockSize"))
        {
            if (int.TryParse(entityClass.Properties.Values["FlockSize"], out maxToSpawn) == false) maxToSpawn = 10;
            if (maxToSpawn > 0)
                // randomize it
                maxToSpawn = UnityEngine.Random.Range(0, maxToSpawn);
        }

        // Sets how high the birds are allowed to fly, defaulting to 80
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
        if (entityClass.Properties.Values.ContainsKey("LandingBlocks")) targetBlocks = entityClass.Properties.Values["LandingBlocks"].Split(',');
        if (entityClass.Properties.Values.ContainsKey("DeflectorBlocks")) deflectorBlocks = entityClass.Properties.Values["DeflectorBlocks"].Split(',');
        if (entityClass.Properties.Values.ContainsKey("NaturalEnemies")) naturalEnemies = entityClass.Properties.Values["NaturalEnemies"].Split(',');

        if (entityClass.Properties.Values.ContainsKey("FlockEntities")) strFlockEntities = entityClass.Properties.Values["FlockEntities"].Split(',');

        GetWalkType();
        //  GetApproachSpeed();

        // Sets the hand value, so we can give our entities ranged weapons.
        inventory.SetSlots(new[]
        {
            new ItemStack(inventory.GetBareHandItemValue(), 1)
        });


        // If the idle sleep flag is set, put the zombie to sleep
        if (blIdleSleep)
            SetRandomSleeperPose();

        // This is the distributed random heigh multiplier. Add or adjust values as you see fit. By default, it's just a small adjustment.
        var numbers = new float[9] { 0.8f, 0.8f, 0.9f, 0.9f, 1.0f, 1.0f, 1.0f, 1.1f, 1.1f };
        var randomIndex = random.Next(0, numbers.Length);
        flScale = numbers[randomIndex];

        // scale down the zombies, or upscale them
        gameObject.transform.localScale = new Vector3(flScale, flScale, flScale);
    }


    // Randomly assign a sleeper pose for each spawned in zombie, with it weighted to the standing sleeper pose.
    public void SetRandomSleeperPose()
    {
        // We wait for the standing pose, since there's no animation to make them fall down to sleep
        var numbers = new int[9] { 0, 1, 2, 3, 4, 5, 5, 5, 5 };
        var randomNumber = random.Next(0, numbers.Length);
        lastSleeperPose = numbers[randomNumber];
    }

    // Returns a random walk type for the spawned entity
    public static int GetRandomWalkType()
    {
        /**************************
         *  Walk Types - A16.x
         * 1 - female fat, moe
         * 2 - zombieWightFeral, zombieBoe
         * 3 - Arlene
         * 4 - crawler
         * 5 - zombieJoe, Marlene
         * 6 - foot ball player, steve
         * 7 - zombieTemplateMale, business man
         * 8 - spider
         * 9 - zombieBehemoth
         * *****************************/

        // Distribution of Walk Types in an array. Adjust the numbers as you want for distribution. The 9 in the default int[9] indicates how many walk types you've specified.
        var numbers = new int[9] { 1, 2, 2, 3, 4, 5, 6, 7, 8 };

        // Randomly generates a number between 0 and the maximum number of elements in the numbers.
        var randomNumber = random.Next(0, numbers.Length);


        // return the randomly selected walk type
        return numbers[randomNumber];
    }


    // Update the Approach speed, and add a randomized speed to it
    //public override float GetApproachSpeed()
    //{
    //    // default approach speed of this new class is 0, so if we are already above that, just re-use the value.
    //    if (flApproachSpeed > 0.0f)
    //        return flApproachSpeed;

    //    // Find the default approach speed from the base class to give us a reference.
    //    float fDefaultSpeed = base.GetApproachSpeed();

    //    // If random run is disables, return the base speed
    //    if (!blRandomSpeeds)
    //        return fDefaultSpeed;

    //    // if it's greater than 1, just use the base value in the XML. 
    //    // This would otherwise make the football and wights run even faster than they do now.
    //    if (fDefaultSpeed > 1.0f)
    //        return fDefaultSpeed;

    //    // new way to generate the multiplier to control their speeds
    //    float[] numbers = new float[9] {-0.2f, -0.2f, -0.1f, -0.1f, 0.0f, 0.0f, 0.0f, 0.1f, 0.1f };
    //    int randomIndex = random.Next(0, numbers.Length);

    //    float fRandomMultiplier = numbers[randomIndex];

    //    // If the zombies are set never to run, still apply the multiplier, but don't bother doing calulations based on the night speed.
    //    if (GamePrefs.GetInt(EnumGamePrefs.ZombiesRun) == 1)
    //    {
    //        flApproachSpeed = this.speedApproach + fRandomMultiplier;
    //    }
    //    else
    //    {
    //        // Rnadomize the zombie speeds types If you have the blRunInDark set to true, then it'll randomize it too.
    //        if (blRunInDark && this.world.IsDark() || lightLevel < LightThreshold || this.Health < this.GetMaxHealth() * 0.4)
    //        {
    //            flApproachSpeed = this.speedApproachNight + fRandomMultiplier;
    //        }
    //        else if (this.world.IsDark())
    //        {
    //            flApproachSpeed = this.speedApproachNight + fRandomMultiplier;
    //        }
    //        else
    //        {
    //            flApproachSpeed = this.speedApproach + fRandomMultiplier;
    //        }
    //    }

    //    // If the approach speed is too low, set it to default speed
    //    if (flApproachSpeed <= 0)
    //        flApproachSpeed = base.GetApproachSpeed();

    //    // Cap the top end of the speed to be 1.35 or less, otherwise animations may go wonky.
    //    return Math.Min(flApproachSpeed, 1.1f);

    //}

    // Randomize the Walk types.
    public new int GetWalkType()
    {
        // Grab the current walk type in the baes class
        var WalkType = base.GetWalkType();

        // If the WalkType is 4, then just return, since this is the crawler animation
        if (WalkType == 4) return WalkType;
        // If the WalkType is greater than the default, then return the already randomized one
        if (intWalkType > 0) return intWalkType;
        // Grab a random walk type, and store it for this instance.
        intWalkType = GetRandomWalkType();


        // Grab a random walk type
        return intWalkType;
    }


    // Calls the base class, but also does an update on how much light is on the current entity.
    // This only determines if the zombies run in the dark, if enabled.
    public override void OnUpdateLive()
    {
        base.OnUpdateLive();
        flockTasks(); // flocking logic, if needed
        if (nextCheck < Time.time)
        {
            nextCheck = Time.time + CheckDelay;
            var v = new Vector3i(position);
            if (v.x < 0) v.x -= 1;
            if (v.z < 0) v.z -= 1;
            lightLevel = GameManager.Instance.World.ChunkClusters[0].GetLight(v, Chunk.LIGHT_TYPE.SUN);

            // If the Idle Sleep flag is set, then we'll do a check to see if the zombie can go to sleep or not.
            if (blIdleSleep)
                try
                {
                    // If its not alert, and not already sleeping, put it to sleep.
                    if (!IsAlert) ResumeSleeperPose();
                }
                catch (Exception)
                {
                    // No Sleeper code!
                    blIdleSleep = false;
                }
        }
    }

    public override int DamageEntity(DamageSource _damageSource, int _strength, bool _criticalHit, float _impulseScale)
    {
        if (blHeadShotsMatter)
        {
            var bodyPart = _damageSource.GetEntityDamageBodyPart(this);
            if (bodyPart == EnumBodyPartHit.Head)
            {
                // Apply a damage multiplier for the head shot, and bump the dismember bonus for the head shot
                // This will allow the heads to go explode off, which according to legend, if the only want to truly kill a zombie.
                _damageSource.DamageMultiplier = 1f;
                _damageSource.DismemberChance = 0.08f;
            }
            // Reducing the damage to the torso will prevent the entity from being killed by torso shots, while also maintaining de-limbing.
            else if (bodyPart == EnumBodyPartHit.Torso)
            {
                _damageSource.DamageMultiplier = 0.1f;
            }
        }

        return base.DamageEntity(_damageSource, _strength, _criticalHit, _impulseScale);
    }

    //protected override string GetSoundRandom()
    //{
    //    this.emodel.avatarController.StartTaunt();
    //    return base.GetSoundRandom();
    //}

    /* underground 
     * ------------
     * Use Blight code to make underrground caverns, x, y, z  with random sizes
     * over-ride stone block to allow random growth:
     * Top:  stalactites, cobwebs
     * Bottom: stalactites, moss, water block
     * Random spawn entities, like bats, creatures
     * 
    /**************************** Test Code for reading and loading entity data from an asset bundle
     public void SetCustomTextures()
     {
         if (strCustomUMA != "None ")
         {
             try
             {
                 Debug.Log("Checking Main Texture");

                 Texture2D mainTexture = ResourceWrapper.Load(this.strCustomUMA + "_d", typeof(Texture2D)) as Texture2D;
                 Texture2D Bump = ResourceWrapper.Load(this.strCustomUMA + "_n", typeof(Texture2D)) as Texture2D;
                 Texture2D Spec = ResourceWrapper.Load(this.strCustomUMA + "_s", typeof(Texture2D)) as Texture2D;

                 if (mainTexture == null)
                     Debug.Log("main Text is null");
                 if (Bump == null)
                     Debug.Log("Bump is null ");
                 if (Spec == null)
                     Debug.Log("Light is null ");
                 Debug.Log("Found external texture");

                 Renderer[] componentsInChildren2 = this.GetComponentsInChildren<Renderer>();
                 foreach (var r in componentsInChildren2)
                 {
                     Debug.Log("Found a Renderer: " + r.name.ToString());
                     Debug.Log("MainText was: " + r.material.mainTexture.name.ToString());
                     Debug.Log(r.material.GetTexture("_MainTex").name.ToString());
                     Debug.Log(r.material.GetTexture("_SpecGlossMap").name.ToString());
                     r.material.SetTexture("_MainTex", mainTexture);
                     r.material.SetTexture("_BumpMap", Bump);
                     r.material.SetTexture("_SpecGlossMap", Spec);
                     r.material.mainTexture = mainTexture;
                     Debug.Log("MainText is: " + r.material.mainTexture.name.ToString());
                 }

                 Debug.Log("New Main Texture");
             }
             catch (Exception ex)
             {
                 Debug.Log("Error parsing main texture: " + ex.ToString());
             }
         }
     }
     public void CheckForNull(object source, String strTag)
     {
         if (source == null)
             Debug.Log("This is null: " + strTag);
         else
             Debug.Log("This is NOT null: " + strTag);
     }

     */
    public int GetLastId()
    {
        return previousID;
    }

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
            if (previousID == 0 && entityId > 0)
            {
                previousID = entityId;
                if (debug) Debug.Log("Saving my ID of " + previousID);
            }

            if (previousID > 0)
                _bw.Write(previousID);
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
            if (_br.BaseStream.Position == _br.BaseStream.Length) return; // still doesn't have previous ID for some reason
            previousID = _br.ReadInt32();
            //Debug.Log("My Old id is " + previousID + " and my old master ID is " + this.entityId);
        }
        catch
        {
        }
    }

    private void FindOldMaster()
    {
        if (oldmasterID > 0 && previousID > 0)
            // no need to go through all this trouble if its remote world
            if (masterEntity == null)
            {
                // if it has a master entity saved, and not yet assigned, will try to do it here
                // searches for a EntityZombieFlyingSDX that has this id stored
                // but only if the saved id still exists, and is of the same type.                    
                // look for the entity that had this masterID previously
                foreach (var entity in GameManager.Instance.World.Entities.list)
                    if (entity is EntityZombieFlockSDX && entity.IsAlive())
                        if ((entity as EntityZombieFlockSDX).GetLastId() == oldmasterID)
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

    public void setMasterEntity(EntityAlive masterE)
    {
        masterEntity = masterE;
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
                    hasFlock = false;
                }
                else if (masterEntity.IsDead())
                {
                    if (debug) Debug.Log("Master entity died");
                    masterEntity = null;
                    hasFlock = false;
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
        if (DateTime.Now < dtaNextSpawn) return;
        // spawn them right by the parent        
        // separates the spawning a few miliseconds just to avoid overlapping
        var newPos = position + Vector3.up;
        if (debug) Debug.Log("SPAWNING CHILDREN FOR ID " + entityId);

        Debug.Log("Creating entity");

        var intIndex = UnityEngine.Random.Range(0, strFlockEntities.Length - 1);
        var intEntity = EntityClass.FromString(strFlockEntities[intIndex]);
        Debug.Log("Spawning: " + strFlockEntities[intIndex]);
        //Entity spawnEntity = EntityFactory.CreateEntity(EntityClass.FromString(this.EntityName), newPos);
        var spawnEntity = EntityFactory.CreateEntity(intEntity, newPos);
        spawnEntity.SetSpawnerSource(EnumSpawnerSource.Unknown);
        Debug.Log("Spawning entity");
        GameManager.Instance.World.SpawnEntityInWorld(spawnEntity);

        (spawnEntity as EntityZombieFlockSDX).setMasterEntity(this);
        // flying entities spawning is a bit odd, so we "override" the position to where we want the children to be
        // which is by its parent
        spawnEntity.position = newPos;
        numberSpawned++;
        if (numberSpawned > maxToSpawn)
            hasFlock = true;
        dtaNextSpawn = DateTime.Now.AddMilliseconds(50);
    }

    public new EntityAlive GetRevengeTarget()
    {
        return base.GetRevengeTarget();
    }

    // look for a landing block nearby
    private bool FindLandSpot()
    {
        //Debug.Log("FIND LANDING BLOCK");
        //landPosition = FindBlock(landPosition, targetBlocks, 16, new Vector3i(this.GetPosition()));
        landPosition = FindLandSpot(landPosition, targetBlocks, 30);
        if (landPosition != Vector3.zero)
            //Debug.Log("!!!FOUND LANDING BLOCK!!!");
            // if there's a landposition, first check if there isn't a deflectorblock near it           
            if (FindNearBlock(deflectorBlocks, landPosition, 5) != Vector3.zero)
                //Debug.Log("FOUND LANDING BLOCK BUT THERE'S A DEFLECTOR");
                landPosition = Vector3.zero;
        if (landPosition != Vector3.zero) return true;
        return false;
    }

    public Vector3 GetLandingSpot()
    {
        return landPosition;
    }

    // searches for a block inside the given range
    private Vector3 FindLandSpot(Vector3 position, string[] blockList, int range)
    {
        if (blockList == null) return Vector3.zero;
        if (blockList.Length == 0) return Vector3.zero;
        if (position != Vector3.zero)
        {
            var blockAux = GameManager.Instance.World.GetBlock(new Vector3i(position));
            if (Array.Exists(blockList, s => s.Equals(Block.list[blockAux.type].GetBlockName())))
                if (Vector3.Distance(GetPosition(), position) <= range)
                    return position;
        }

        return FindNearBlock(blockList, GetPosition(), range);
    }

    public Vector3 FindNearBlock(string[] blockList, Vector3 position, int checkRange)
    {
        var result = Vector3.zero;
        if (blockList == null) return result;
        if (blockList.Length == 0) return result;
        for (var i = (int)position.x - checkRange; i <= position.x + checkRange; i++)
            for (var j = (int)position.z - checkRange; j <= position.z + checkRange; j++)
                for (var k = (int)position.y - checkRange; k <= position.y + checkRange; k++)
                {
                    var blockF = GameManager.Instance.World.GetBlock(i, k, j);
                    if (Array.Exists(blockList, s => s.Equals(Block.list[blockF.type].GetBlockName())))
                    {
                        result = new Vector3(i, k, j);
                        return result;
                    }
                }

        return result;
    }

    private void FindNaturalEnemy()
    {
        if (naturalEnemies == null) return;
        if (naturalEnemies.Length == 0) return;
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
                if (Array.Exists(naturalEnemies, s => s.Equals(_other.EntityName)))
                    if (_other.IsAlive())
                        //if (base.CanSee(_other))
                        if (_other.Water < 0.5f)
                        {
                            if (debug) Debug.Log("Found natural enemy!");
                            SetRevengeTarget(_other);
                            isHunting = true;
                            return;
                        }
            }
        }
    }

    private void FindClosestPlayer()
    {
        var closestPlayer = world.GetClosestPlayer(this, 80f, false);
        if (CanSee(closestPlayer))
            if (closestPlayer.Water < 0.5f)
                SetRevengeTarget(closestPlayer);
    }

    // flocking logic
    private void flockTasks()
    {
        if (GamePrefs.GetBool(EnumGamePrefs.DebugStopEnemiesMoving) || GameStats.GetInt(EnumGameStats.GameState) == 2) return;
        if (world.IsRemote()) return;
        if (IsDead()) return;
        if (masterEntity == null && !hasFlock && maxToSpawn > 0)
            SpawnFlock();
        else if (masterEntity == null && hasFlock && oldmasterID > 0) FindOldMaster();
        GetEntitySenses().ClearIfExpired();
        if (AttackTimeout > 0) AttackTimeout--;
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
                        if (debug) Debug.Log("Going Home");
                        Waypoint = ye;
                        HasWaypoint = true;
                    }
                }
                else
                {
                    HasWaypoint = false;
                    if (base.GetRevengeTarget() != null && (base.GetRevengeTarget().GetDistanceSq(this) < 6400f && UnityEngine.Random.value <= 0.5f || isHunting))
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
                                Waypoint = landPosition + new Vector3((float)((rand.RandomDouble * 2.0 - 1.0) * 3.0), (float)((rand.RandomDouble * 2.0 - 1.0) * 3.0),
                                    (float)((rand.RandomDouble * 2.0 - 1.0) * 3.0));
                            }
                            else
                            {
                                // chooses a random waypoint - vanilla code
                                Waypoint = GetPosition() + new Vector3((float)((rand.RandomDouble * 2.0 - 1.0) * 16.0), (float)((rand.RandomDouble * 2.0 - 1.0) * 16.0),
                                    (float)((rand.RandomDouble * 2.0 - 1.0) * 16.0));
                                // maximum Y. Just to avoid them going too high (out of sight, out of heart)
                                var maxY = world.GetHeight((int)Waypoint.x, (int)Waypoint.z) + maxHeight;
                                if (Waypoint.y > maxY)
                                {
                                    Waypoint.y = maxY;
                                    if (debug) Debug.Log("Prevented it from going higher");
                                }
                            }
                        }
                        else
                        {
                            if ((GetMasterEntity() as EntityZombieFlockSDX).GetRevengeTarget() != null)
                            {
                                // attacks the same target as master
                                SetRevengeTarget((GetMasterEntity() as EntityZombieFlockSDX).GetRevengeTarget());
                                Waypoint = (GetMasterEntity() as EntityZombieFlockSDX).GetRevengeTarget().GetPosition() + Vector3.up;
                            }
                            else
                            {
                                // if the master has a landing spot, it goes to random position near the landing spot, otherwise just follows master
                                if ((GetMasterEntity() as EntityZombieFlockSDX).GetLandingSpot() == Vector3.zero)
                                    Waypoint = GetMasterEntity().GetPosition() + Vector3.up;
                                else
                                    Waypoint = (GetMasterEntity() as EntityZombieFlockSDX).GetLandingSpot() + new Vector3((float)((rand.RandomDouble * 2.0 - 1.0) * 3.0),
                                        (float)((rand.RandomDouble * 2.0 - 1.0) * 3.0), (float)((rand.RandomDouble * 2.0 - 1.0) * 3.0));
                            }
                        }
                    }

                    var num = 255;
                    // if waypoint is not in the air, change it up
                    while (world.GetBlock(new Vector3i(Waypoint)).type != BlockValue.Air.type && num > 0)
                    {
                        Waypoint.y = Waypoint.y + 1f;
                        num--;
                    }
                }

                Waypoint.y = Mathf.Min(Waypoint.y, 250f);
            }

            if (CourseCheck-- <= 0)
            {
                CourseCheck += rand.RandomRange(5) + 2;
                //if (base.isCourseTraversable(this.Waypoint, out sqrMagnitude))
                //{
                //    this.motion += a / sqrMagnitude * 0.1f;
                //}
                //else
                //{
                Waypoint = GetPosition();
                //}
            }
        }

        if (base.GetRevengeTarget() != null)
        {
            if (!retaliateAttack && !targetPlayers && !isHunting)
            {
                SetRevengeTarget(null);
            }
            else if (base.GetRevengeTarget().IsDead())
            {
                SetRevengeTarget(null);
                isHunting = false;
            }
        }

        // if it's a parent and has no target, then it will look for one.
        if (base.GetRevengeTarget() == null && GetMasterEntity() == null)
            if (TargetInterval-- <= 0)
            {
                isHunting = false;
                if (targetPlayers)
                    // if it's an agressive animal, will look for a player to attack
                    FindClosestPlayer();
                if (base.GetRevengeTarget() == null)
                    if (naturalEnemies != null)
                        if (naturalEnemies.Length > 0)
                            // if it has natural enemies, will look for one to attack
                            FindNaturalEnemy();
                if (base.GetRevengeTarget() != null) TargetInterval = 20;
            }

        float intendedRotation;
        if (!HasWaypoint)
            if (base.GetRevengeTarget() != null)
            {
                float distanceSq;
                if ((distanceSq = base.GetRevengeTarget().GetDistanceSq(this)) < 6400f)
                {
                    var y = base.GetRevengeTarget().position.x - position.x;
                    var x = base.GetRevengeTarget().position.z - position.z;
                    if (distanceSq < 5f)
                    {
                        intendedRotation = Mathf.Atan2(y, x) * 180f / 3.14159274f;
                    }
                    else
                    {
                        intendedRotation = (float)Math.Atan2(motion.x, motion.z) * 180f / 3.14159274f;
                        if (motion.magnitude < 0.25f) motion = motion.normalized * 0.25f;
                    }

                    if (AttackTimeout <= 0)
                        if (distanceSq < 2.8f)
                            if (position.y >= base.GetRevengeTarget().position.y)
                            {
                                if (!isHunting)
                                {
                                    if (position.y <= base.GetRevengeTarget().getHeadPosition().y - 0.25f)
                                        if (Attack(false))
                                        {
                                            AttackTimeout = GetAttackTimeoutTicks();
                                            Attack(true);
                                        }
                                }
                                else
                                {
                                    // just marks the "target" to unload, as if it had captured it.
                                    if (UnityEngine.Random.value <= 0.5f)
                                    {
                                        if (debug) Debug.Log("Eating the target!!!");
                                        base.GetRevengeTarget().MarkToUnload();
                                        // stops hunting to look for another suitable target after a bit
                                        isHunting = false;
                                        SetRevengeTarget(null);
                                        TargetInterval = 50;
                                    }
                                }
                            }

                    rotation.y = UpdateRotation(rotation.y, intendedRotation, 10f);
                    return;
                }
            }

        intendedRotation = (float)Math.Atan2(motion.x, motion.z) * 180f / 3.14159274f;
        rotation.y = UpdateRotation(rotation.y, intendedRotation, 10f);
    }
}