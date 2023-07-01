using System;
using System.IO;
using UnityEngine;

public class EntityBackpackNPC : EntityItem
{
    public override void CopyPropertiesFromEntityClass()
    {
        base.CopyPropertiesFromEntityClass();
        EntityClass entityClass = EntityClass.list[this.entityClass];
        float num = 5f;
        entityClass.Properties.ParseFloat(EntityClass.PropTimeStayAfterDeath, ref num);
        this.ticksStayAfterDeath = (int)(num * 20f);
    }

    protected override void Start()
    {
        base.Start();
        foreach (Collider collider in base.transform.GetComponentsInChildren<Collider>())
        {
            collider.gameObject.tag = "E_BP_Body";
            collider.gameObject.layer = 13;
            collider.enabled = true;
            collider.gameObject.AddMissingComponent<RootTransformRefEntity>().RootTransform = base.transform;
        }
        this.SetDead();
        if (this.lootContainer != null)
        {
            this.lootContainer.entityId = this.entityId;
        }
    }

    public override void OnUpdateEntity()
    {
        base.OnUpdateEntity();
        if (this.deathUpdateTicks > 0)
        {
            if (!this.bRemoved && this.lootContainer != null && !this.lootContainer.IsUserAccessing() && this.lootContainer.IsEmpty())
            {
                this.RemoveBackpack("empty");
            }
            if (!this.bRemoved && this.deathUpdateTicks >= this.ticksStayAfterDeath - 1)
            {
                this.RemoveBackpack("old");
            }
        }
        this.deathUpdateTicks++;
        if (!this.bRemoved && !this.isEntityRemote && base.transform.position.y + Origin.position.y < 0f)
        {
            Vector3 vector = new Vector3(this.position.x, (float)(this.world.GetHeight(Utils.Fastfloor(this.position.x), Utils.Fastfloor(this.position.z)) + 5) + this.rand.RandomFloat * 20f, this.position.z);
            this.SetPosition(vector, true);
            base.transform.position = vector - Origin.position;
            int num = this.safetyCounter + 1;
            this.safetyCounter = num;
            if (num > 500)
            {
                this.RemoveBackpack("retries");
            }
        }
    }

    protected override void createMesh()
    { 
    }
    private void RemoveBackpack(string reason)
    {
        this.deathUpdateTicks = this.ticksStayAfterDeath;
        this.bRemoved = true;
    }

    public override void Write(BinaryWriter _bw, bool bNetworkWrite)
    {
        base.Write(_bw, bNetworkWrite);
        _bw.Write(this.RefPlayerId);
    }

    public override void Read(byte _version, BinaryReader _br)
    {
        base.Read(_version, _br);
        this.RefPlayerId = _br.ReadInt32();
    }

    public override bool IsMarkedForUnload()
    {
        return base.IsMarkedForUnload() && this.deathUpdateTicks >= this.ticksStayAfterDeath;
    }

    public override string GetLootList()
    {
        return this.lootListOnDeath;
    }


    public int RefPlayerId = -1;

    private int deathUpdateTicks;

    private int ticksStayAfterDeath;

    private bool bRemoved;

    private int safetyCounter;
}



