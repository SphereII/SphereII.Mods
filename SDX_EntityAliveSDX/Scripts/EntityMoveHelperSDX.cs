using System;
using UnityEngine;

class EntityMoveHelperSDX : EntityMoveHelper
{
    public EntityAlive entity;
    private float blockedDistSq;
    private Vector3 tempMoveToPos;
    private Vector3 moveToPos;
    private bool isTempMove;
    private int obstacleCheckTickDelay;
    private float sideStepAngle;
    private int moveToTicks;
    private int moveToFailCnt;
    private float moveToDistance;

    private bool blDisplayLog = true;
    public void DisplayLog(String strMessage)
    {
        if(blDisplayLog )
            Debug.Log(this.GetType() + ": " + strMessage);
    }

    
    public EntityMoveHelperSDX(EntityAlive _entity) : base(_entity)
    {
        DisplayLog("Initializing");
        entity = _entity;
    }
    private void CheckBlocked(Vector3 pos, Vector3 endPos, int baseY)
    {
        DisplayLog(" CheckedBlocked() ");
        IsBlocked = false;
        endPos.y -= 0.01f;
        Vector3 vector = endPos - pos;
        float num = vector.magnitude + 0.001f;
        vector *= 1f / num;
        Ray ray = new Ray(pos - vector * 0.375f, vector);
        if(num > 1f)
        {
            num = 1f;
        }
        if(Voxel.Raycast(this.entity.world, ray, num - 0.125f + 0.375f, 1082195968, 128, 0.125f))
        {
            if(baseY == 0 && Voxel.phyxRaycastHit.normal.y > 0.643f)
            {
                Vector2 vector2;
                vector2.x = Voxel.phyxRaycastHit.normal.x;
                vector2.y = Voxel.phyxRaycastHit.normal.z;
                vector2.Normalize();
                Vector2 vector3;
                vector3.x = vector.x;
                vector3.y = vector.z;
                vector3.Normalize();
                float num2 = vector3.x * vector2.x + vector3.y * vector2.y;
                if(num2 < -0.7f)
                {
                    return;
                }
            }
            Block block = Block.list[Voxel.voxelRayHitInfo.hit.blockValue.type];
            if(block is BlockDamage)
            {
                return;
            }
            HitInfo = Voxel.voxelRayHitInfo.Clone();
            IsBlocked = true;
            Vector3 a = pos - HitInfo.hit.pos;
            float sqrMagnitude = a.sqrMagnitude;
            if(sqrMagnitude < this.blockedDistSq)
            {
                this.blockedDistSq = sqrMagnitude;
                float num3 = 1f / Mathf.Sqrt(sqrMagnitude);
                float num4 = this.entity.m_characterController.GetRadius() + 0.4f;
                this.tempMoveToPos = a * (num3 * num4) + HitInfo.hit.pos;
                this.tempMoveToPos.y = this.moveToPos.y;
                this.isTempMove = true;
                this.obstacleCheckTickDelay = 12;
                this.ResetStuckCheck();
            }
        }
    }
    private void ResetStuckCheck()
    {
        this.sideStepAngle = 0f;
        this.moveToTicks = 0;
        this.moveToFailCnt = 0;
        if(this.isTempMove)
        {
            this.moveToDistance = this.CalcTempMoveDist();
        }
        else
        {
            this.moveToDistance = this.CalcMoveDist();
        }
    }

    private float CalcMoveDist()
    {
        Vector3 position = this.entity.position;
        float num = this.moveToPos.x - position.x;
        float num2 = this.moveToPos.z - position.z;
        float num3 = this.moveToPos.y - position.y;
        return Mathf.Sqrt(num * num + num3 * num3 + num2 * num2);
    }

    private float CalcTempMoveDist()
    {
        Vector3 position = this.entity.position;
        float num = this.tempMoveToPos.x - position.x;
        float num2 = this.tempMoveToPos.z - position.z;
        float num3 = this.tempMoveToPos.y - position.y;
        return Mathf.Sqrt(num * num + num3 * num3 + num2 * num2);
    }
}

