using System;
using System.Collections.Generic;
using System.Globalization;
using UnityEngine;
using UnityEngine.Scripting;
using static EntityDrone;
using static UnityEngine.UI.GridLayoutGroup;

[Preserve]
public class EAISetNearestEntityAsTargetCompanion : EAITarget
{
    public override void Init(EntityAlive _theEntity)
    {
        base.Init(_theEntity, 25f, true);
        this.MutexBits = 1;
        this.sorter = new EAISetNearestEntityAsTargetSorter(_theEntity);
    }

    public override void SetData(DictionarySave<string, string> data)
    {
        base.SetData(data);
        this.targetClasses = new List<EAISetNearestEntityAsTargetCompanion.TargetClass>();
        string text;
        if (data.TryGetValue("class", out text))
        {
            string[] array = text.Split(',', StringSplitOptions.None);
            for (int i = 0; i < array.Length; i += 3)
            {
                EAISetNearestEntityAsTargetCompanion.TargetClass targetClass;
                targetClass.type = EntityFactory.GetEntityType(array[i]);
                targetClass.hearDistMax = 0f;
                if (i + 1 < array.Length)
                {
                    targetClass.hearDistMax = StringParsers.ParseFloat(array[i + 1], 0, -1, NumberStyles.Any);
                }
                if (targetClass.hearDistMax == 0f)
                {
                    targetClass.hearDistMax = 50f;
                }
                targetClass.seeDistMax = 0f;
                if (i + 2 < array.Length)
                {
                    targetClass.seeDistMax = StringParsers.ParseFloat(array[i + 2], 0, -1, NumberStyles.Any);
                }
                if (targetClass.type == typeof(EntityPlayer))
                {
                    this.playerTargetClassIndex = this.targetClasses.Count;
                }
                this.targetClasses.Add(targetClass);
            }
        }
    }

    public override bool CanExecute()
    {
        if (this.theEntity.Buffs.GetCustomVar("onMission") == 1f)
        {
            return false;
        }

        //Log.Out("EAISetNearestEntityAsTargetCompanion-CanExecute START");

        float currentOrder = this.theEntity.Buffs.GetCustomVar("CurrentOrder");

        if (currentOrder > 1)
        {
            return false;
        }

        if (this.theEntity.Buffs.HasBuff("FuriousRamsayStandStill"))
        {
            this.theEntity.SetRevengeTarget(null);
            this.theEntity.SetAttackTarget(null, 0);
            return false;
        }

        if (this.theEntity.distraction != null)
        {
            //Log.Out("EAISetNearestEntityAsTargetCompanion-CanExecute 1");
            return false;
        }

        bool stopAttacking = this.theEntity.Buffs.HasBuff("buffNPCModStopAttacking") ||
                                this.theEntity.Buffs.HasBuff("FuriousRamsayStandStill");

        if (stopAttacking)
        {
            //Log.Out("EAISetNearestEntityAsTargetCompanion-CanExecute STOP ATTACKING");
            return false;
        }

        this.FindTarget();
        if (!this.closeTargetEntity)
        {
            //Log.Out("EAISetNearestEntityAsTargetCompanion-CanExecute 2");
            return false;
        }
        this.targetEntity = this.closeTargetEntity;
        this.targetPlayer = (this.closeTargetEntity as EntityPlayer);

        //Log.Out("EAISetNearestEntityAsTargetCompanion-CanExecute this.targetEntity: " + this.targetEntity.EntityClass.entityClassName);
        return true;
    }

    private void FindTarget()
    {
        //Log.Out("EAISetNearestEntityAsTargetCompanion-FindTarget START");
        this.closeTargetDist = float.MaxValue;
        this.closeTargetEntity = null;
        float seeDistance = 10f;  this.theEntity.GetSeeDistance();
        //Log.Out("EAISetNearestEntityAsTargetCompanion-FindTarget this.targetClasses.Count " + this.targetClasses.Count);
        for (int i = 0; i < this.targetClasses.Count; i++)
        {
            //Log.Out("EAISetNearestEntityAsTargetCompanion-FindTarget i: " + i);
            EAISetNearestEntityAsTargetCompanion.TargetClass targetClass = this.targetClasses[i];
            float num = 60;
            /*if (targetClass.seeDistMax != 0f)
            {
                float v = (targetClass.seeDistMax < 0f) ? (-targetClass.seeDistMax) : (targetClass.seeDistMax * this.theEntity.senseScale);
                num = Utils.FastMin(num, v);
            }*/

            //Log.Out("EAISetNearestEntityAsTargetCompanion-FindTarget seeDistance " + seeDistance);
            //Log.Out("EAISetNearestEntityAsTargetCompanion-FindTarget targetClass.seeDistMax " + targetClass.seeDistMax);
            //Log.Out("EAISetNearestEntityAsTargetCompanion-FindTarget num " + num);

            //Log.Out("EAISetNearestEntityAsTargetCompanion-FindTarget this.theEntity.IsSleeping " + this.theEntity.IsSleeping);
            //Log.Out("EAISetNearestEntityAsTargetCompanion-FindTarget this.theEntity.HasInvestigatePosition " + this.theEntity.HasInvestigatePosition);

            bool hasInvestigatePosition = false; // this.theEntity.HasInvestigatePosition;

            if (!this.theEntity.IsSleeping && !hasInvestigatePosition)
            {
                this.theEntity.world.GetEntitiesInBounds(targetClass.type, BoundsUtils.ExpandBounds(this.theEntity.boundingBox, num, 15f, num), EAISetNearestEntityAsTargetCompanion.list);
                EAISetNearestEntityAsTargetCompanion.list.Sort(this.sorter);
                int j = 0;
                //Log.Out("EAISetNearestEntityAsTargetCompanion-FindTarget EAISetNearestEntityAsTargetCompanion.list.Count " + EAISetNearestEntityAsTargetCompanion.list.Count);
                while (j < EAISetNearestEntityAsTargetCompanion.list.Count)
                {
                    EntityAlive entityAlive = (EntityAlive)EAISetNearestEntityAsTargetCompanion.list[j];

                    //Log.Out("EAISetNearestEntityAsTargetCompanion-FindTarget j: " + j + " / entityAlive: " + entityAlive.EntityClass.entityClassName);

                    if (!(entityAlive is EntityDrone))
                    {
                        bool check = base.check(entityAlive);

                        //Log.Out("EAISetNearestEntityAsTargetCompanion-FindTarget check: " + check);



                        if (check && !entityAlive.IsSleeping)
                        {
                            //Log.Out("EAISetNearestEntityAsTargetCompanion-FindTarget entityAlive.EntityClass.entityClassName: " + entityAlive.EntityClass.entityClassName);
                            float distance = this.theEntity.GetDistance(entityAlive);

                            //Log.Out("EAISetNearestEntityAsTargetCompanion-FindTarget this.closeTargetDist: " + this.closeTargetDist);
                            //Log.Out("EAISetNearestEntityAsTargetCompanion-FindTarget distance: " + distance);

                            bool bypassDistance = (entityAlive.Buffs.GetCustomVar("$FuriousRamsayAttacking") == 1f ||
                                                    entityAlive.Buffs.GetCustomVar("$FuriousRamsayAttacked") == 1f ||
                                                    this.theEntity.Buffs.HasBuff("buffNPCModThreatControlMode")
                                                    );

                            if (bypassDistance && distance > 50)
                            {
                                bypassDistance = false;
                            }

                            //Log.Out("EAISetNearestEntityAsTargetCompanion-FindTarget bypassDistance: " + bypassDistance);

                            if (bypassDistance || distance < seeDistance)
                            {
                                //Log.Out("EAISetNearestEntityAsTargetCompanion-FindTarget COMPANION distance: " + distance);
                                this.closeTargetDist = distance;
                                this.closeTargetEntity = entityAlive;
                                this.lastSeenPos = entityAlive.position;
                                break;
                            }
                            else
                            {
                                int OwnerID = (int)this.theEntity.Buffs.GetCustomVar("Leader");

                                //Log.Out("EAISetNearestEntityAsTargetCompanion-FindTarget OwnerID: " + OwnerID);

                                EntityPlayer owner = null;

                                if (OwnerID > 0)
                                {
                                    //Log.Out("EAISetNearestEntityAsTargetCompanion-FindTarget HAS OWNER");
                                    EntityPlayer entityPlayer = (EntityPlayer)this.theEntity.world.GetEntity(OwnerID);
                                    if (entityPlayer != null)
                                    {
                                        //Log.Out("EAISetNearestEntityAsTargetCompanion-FindTarget FOUND OWNER");
                                        owner = entityPlayer;
                                    }
                                }

                                if (owner != null)
                                {
                                    distance = owner.GetDistance(entityAlive);

                                    //Log.Out("EAISetNearestEntityAsTargetCompanion-FindTarget DISTANCE TO OWNER: " + distance);

                                    if (distance < seeDistance + 5)
                                    {
                                        //Log.Out("EAISetNearestEntityAsTargetCompanion-FindTarget OWNER distance: " + distance);
                                        this.closeTargetDist = distance;
                                        this.closeTargetEntity = entityAlive;
                                        this.lastSeenPos = entityAlive.position;
                                        break;
                                    }
                                }
                            }
                            break;
                        }
                        else
                        {
                            j++;
                        }
                    }
                }
                EAISetNearestEntityAsTargetCompanion.list.Clear();
            }
        }
    }

    public override void Start()
    {
        //Log.Out("EAISetNearestEntityAsTargetCompanion-Start START");
        this.theEntity.SetAttackTarget(this.targetEntity, 200);
        this.theEntity.ConditionalTriggerSleeperWakeUp();
        base.Start();
    }

    public override bool Continue()
    {
        if (this.theEntity.Buffs.GetCustomVar("onMission") == 1f)
        {
            return false;
        }

        //Log.Out("EAISetNearestEntityAsTargetCompanion-Continue START");

        float currentOrder = this.theEntity.Buffs.GetCustomVar("CurrentOrder");

        if (currentOrder > 1)
        {
            return false;
        }

        if (this.theEntity.Buffs.HasBuff("FuriousRamsayStandStill"))
        {
            this.theEntity.SetRevengeTarget(null);
            this.theEntity.SetAttackTarget(null, 0);
            return false;
        }

        bool stopAttacking = this.theEntity.Buffs.HasBuff("buffNPCModStopAttacking") ||
                                this.theEntity.Buffs.HasBuff("FuriousRamsayStandStill");

        if (stopAttacking)
        {
            //Log.Out("EAISetNearestEntityAsTargetCompanion-Continue STOP ATTACKING");
            this.theEntity.SetAttackTarget(null, 0);
            return false;
        }

        if (this.targetEntity.IsDead() || this.theEntity.distraction != null)
        {
            //Log.Out("EAISetNearestEntityAsTargetCompanion-Continue 1");
            if (this.theEntity.GetAttackTarget() == this.targetEntity)
            {
                //Log.Out("EAISetNearestEntityAsTargetCompanion-Continue 2");
                this.theEntity.SetAttackTarget(null, 0);
            }
            return false;
        }
        this.findTime += 0.05f;
        if (this.findTime > 2f)
        {
            //Log.Out("EAISetNearestEntityAsTargetCompanion-Continue 3");
            this.findTime = 0f;
            this.FindTarget();
            if (this.closeTargetEntity && this.closeTargetEntity != this.targetEntity)
            {
                //Log.Out("EAISetNearestEntityAsTargetCompanion-Continue 4");
                return false;
            }
        }
        if (this.theEntity.GetAttackTarget() != this.targetEntity)
        {
            //Log.Out("EAISetNearestEntityAsTargetCompanion-Continue 5");
            return false;
        }

        bool flag2 = this.theEntity.CanSee(this.targetEntity);

        //Log.Out("EAISetNearestEntityAsTargetCompanion-Continue BEFORE flag2: " + flag2);

        if (this.theEntity.Buffs.GetCustomVar("Leader") > 0 && !(this.targetEntity is EntityPlayer) && this.targetEntity.Buffs.GetCustomVar("$FuriousRamsayAttacking") == 1f || this.targetEntity.Buffs.GetCustomVar("$FuriousRamsayAttacked") == 1f)
        {
            //Log.Out("EAISetNearestEntityAsTargetCompanion-Continue 6");
            flag2 = true;
        }

        //Log.Out("EAISetNearestEntityAsTargetCompanion-Continue AFTER flag2: " + flag2);

        if (base.check(this.targetEntity) && flag2)
        {
            //Log.Out("EAISetNearestEntityAsTargetCompanion-Continue 7");
            this.theEntity.SetAttackTarget(this.targetEntity, 600);
            this.lastSeenPos = this.targetEntity.position;
            return true;
        }
        if (this.theEntity.GetDistanceSq(this.lastSeenPos) < 2.25f)
        {
            //Log.Out("EAISetNearestEntityAsTargetCompanion-Continue 8");
            this.lastSeenPos = Vector3.zero;
        }
        this.theEntity.SetAttackTarget(null, 0);
        int num = this.theEntity.CalcInvestigateTicks(Constants.cEnemySenseMemory * 20, this.targetEntity);
        if (this.lastSeenPos != Vector3.zero)
        {
            //Log.Out("EAISetNearestEntityAsTargetCompanion-Continue 9");
            this.theEntity.SetInvestigatePosition(this.lastSeenPos, num, true);
        }
        return false;
    }

    public override void Reset()
    {
        this.targetEntity = null;
        this.targetPlayer = null;
    }

    public override string ToString()
    {
        return string.Format("{0}, {1}", base.ToString(), this.targetEntity ? this.targetEntity.EntityName : "");
    }

    private const float cHearDistMax = 50f;
    private List<EAISetNearestEntityAsTargetCompanion.TargetClass> targetClasses;
    private int playerTargetClassIndex = -1;
    private float closeTargetDist;
    private EntityAlive closeTargetEntity;
    private EntityAlive targetEntity;
    private EntityPlayer targetPlayer;
    private Vector3 lastSeenPos;
    private float findTime;
    private float senseSoundTime;
    private EAISetNearestEntityAsTargetSorter sorter;
    private static List<Entity> list = new List<Entity>();
    private struct TargetClass
    {
        public Type type;
        public float hearDistMax;
        public float seeDistMax;
    }
}
