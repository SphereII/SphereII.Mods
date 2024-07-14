using UnityEngine;

public class PushOutOfBlocksUtils {
    private EntityAlive _entityAlive;

    public PushOutOfBlocksUtils(EntityAlive entityAlive) {
        _entityAlive = entityAlive;
    }
      private bool ShouldPushOutOfBlock(int _x, int _y, int _z, bool pushOutOfTerrain) {
          var shape = _entityAlive.world.GetBlock(_x, _y, _z).Block.shape;
        if (shape.IsSolidSpace && !shape.IsTerrain())
        {
            return true;
        }

        if (!pushOutOfTerrain || !shape.IsSolidSpace || !shape.IsTerrain()) return false;
        var shape2 = _entityAlive.world.GetBlock(_x, _y + 1, _z).Block.shape;
        return shape2.IsSolidSpace && shape2.IsTerrain();
    }

    private bool PushOutOfBlocks(float _x, float _y, float _z)
    {
        var num = Utils.Fastfloor(_x);
        var num2 = Utils.Fastfloor(_y);
        var num3 = Utils.Fastfloor(_z);
        var num4 = _x - (float)num;
        var num5 = _z - (float)num3;
        var result = false;
        if (!this.ShouldPushOutOfBlock(num, num2, num3, false) &&
            (!this.ShouldPushOutOfBlock(num, num2 + 1, num3, false))) return false;
        var flag2 = !this.ShouldPushOutOfBlock(num - 1, num2, num3, true) &&
                    !this.ShouldPushOutOfBlock(num - 1, num2 + 1, num3, true);
        var flag3 = !this.ShouldPushOutOfBlock(num + 1, num2, num3, true) &&
                    !this.ShouldPushOutOfBlock(num + 1, num2 + 1, num3, true);
        var flag4 = !this.ShouldPushOutOfBlock(num, num2, num3 - 1, true) &&
                    !this.ShouldPushOutOfBlock(num, num2 + 1, num3 - 1, true);
        var flag5 = !this.ShouldPushOutOfBlock(num, num2, num3 + 1, true) &&
                    !this.ShouldPushOutOfBlock(num, num2 + 1, num3 + 1, true);
        var b = byte.MaxValue;
        var num6 = 9999f;
        if (flag2 && num4 < num6)
        {
            num6 = num4;
            b = 0;
        }

        if (flag3 && 1.0 - (double)num4 < (double)num6)
        {
            num6 = 1f - num4;
            b = 1;
        }

        if (flag4 && num5 < num6)
        {
            num6 = num5;
            b = 4;
        }

        if (flag5 && 1f - num5 < num6)
        {
            b = 5;
        }

        var num7 = 0.1f;
        switch (b)
        {
            case 0:
                _entityAlive.motion.x = -num7;
                break;
            case 1:
                _entityAlive.motion.x = num7;
                break;
            case 4:
                _entityAlive.motion.z = -num7;
                break;
            case 5:
                _entityAlive.motion.z = num7;
                break;
        }

        if (b != 255)
        {
            result = true;
        }

        return result;
    }

    public virtual void CheckStuck(Vector3 position, float width, float depth)
    {
        _entityAlive.IsStuck = false;
        if (_entityAlive.IsFlyMode.Value) return;
        var num = _entityAlive.boundingBox.min.y + 0.5f;
        _entityAlive.IsStuck = PushOutOfBlocks(position.x - width * 0.3f, num, position.z + depth * 0.3f);
        _entityAlive.IsStuck = (PushOutOfBlocks(position.x - width * 0.3f, num, position.z - depth * 0.3f) || _entityAlive.IsStuck);
        _entityAlive.IsStuck = (PushOutOfBlocks(position.x + width * 0.3f, num, position.z - depth * 0.3f) || _entityAlive.IsStuck);
        _entityAlive.IsStuck = (PushOutOfBlocks(position.x + width * 0.3f, num, position.z + depth * 0.3f) || _entityAlive.IsStuck);
        if (_entityAlive.IsStuck) return;

        var x = Utils.Fastfloor(position.x);
        var num2 = Utils.Fastfloor(num);
        var z = Utils.Fastfloor(position.z);
        if (!ShouldPushOutOfBlock(x, num2, z, true) ||
            !CheckNonSolidVertical(new Vector3i(x, num2 + 1, z), 4, 2)) return;
        _entityAlive.IsStuck = true;
        _entityAlive.motion = new Vector3(0f, 1.6f, 0f);
        Log.Warning($"{_entityAlive.EntityName} ({_entityAlive.entityId}) is stuck. Unsticking.");
    }
    
    private bool CheckNonSolidVertical(Vector3i blockPos, int maxY, int verticalSpace)
    {
        for (int i = 0; i < maxY; i++)
        {
            if (!_entityAlive.world.GetBlock(blockPos.x, blockPos.y + i + 1, blockPos.z).Block.shape.IsSolidSpace)
            {
                bool flag = true;
                for (int j = 1; j < verticalSpace; j++)
                {
                    if (_entityAlive.world.GetBlock(blockPos.x, blockPos.y + i + 1 + j, blockPos.z).Block.shape.IsSolidSpace)
                    {
                        flag = false;
                        break;
                    }
                }

                if (flag)
                {
                    return true;
                }
            }
        }

        return false;
    }

}