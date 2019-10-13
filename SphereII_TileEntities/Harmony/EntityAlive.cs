using Harmony;

public class SphereII_Mods_TileEntities
{
    // Make all entities succeptible to AoE from Tile Entities. Use the Buffs and tags to determine if the buff will apply
    [HarmonyPatch(typeof(EntityAlive))]
    [HarmonyPatch("OnUpdateLive")]
    public class SphereII_EntityAlive_OnUpdateLive
    {
        public static void Postfix(EntityAlive __instance)
        {
            // Don't run from the server.
            if (__instance.isEntityRemote)
                return;

            // Player already checks for this
            if (__instance is EntityPlayerLocal)
                return;

            Vector3i blockPosition = __instance.GetBlockPosition();
            int num = World.toChunkXZ(blockPosition.x);
            int num2 = World.toChunkXZ(blockPosition.z);
            for (int i = -1; i < 2; i++)
            {
                for (int j = -1; j < 2; j++)
                {
                    Chunk chunk = (Chunk)__instance.world.GetChunkSync(num + j, num2 + i);
                    if (chunk != null)
                    {
                        DictionaryList<Vector3i, TileEntity> tileEntities = chunk.GetTileEntities();
                        for (int k = 0; k < tileEntities.list.Count; k++)
                        {
                            TileEntity tileEntity = tileEntities.list[k];
                            if (tileEntity.IsActive(__instance.world))
                            {
                                BlockValue block = __instance.world.GetBlock(tileEntity.ToWorldPos());
                                Block block2 = Block.list[block.type];

                                if (block2.RadiusEffects != null)
                                {
                                    float distanceSq = __instance.GetDistanceSq(tileEntity.ToWorldPos().ToVector3());
                                    for (int l = 0; l < block2.RadiusEffects.Length; l++)
                                    {
                                        BlockRadiusEffect blockRadiusEffect = block2.RadiusEffects[l];
                                        if (distanceSq <= blockRadiusEffect.radius * blockRadiusEffect.radius && !__instance.Buffs.HasBuff(blockRadiusEffect.variable))
                                        {
                                            __instance.Buffs.AddBuff(blockRadiusEffect.variable, -1, true, false);
                                        }
                                    }
                                }
                            }
                        }
                    }
                }
            }

        }
    }


    // hook into the ItemStack, which should cover all types of containers. This will run in the update task.
    [HarmonyPatch(typeof(XUiC_ItemStack))]
    [HarmonyPatch("Update")]
    public class SphereII_XUiC_ItemStack_Update
    {
        public static void Postfix(XUiC_ItemStack __instance)
        {
            // Make sure we are dealing with legitimate stacks.
            if (__instance.ItemStack.IsEmpty())
                return;

            if (__instance.ItemStack.itemValue == null)
                return;

            // Check if the Item has a CanActivate 
            if (__instance.ItemStack.itemValue.ItemClass != null && __instance.ItemStack.itemValue.ItemClass.Properties.Contains("CanActivate"))
            {
                bool canActivate = false;
                canActivate = __instance.ItemStack.itemValue.ItemClass.Properties.GetBool("CanActivate");
                if (!canActivate)
                    return;

                // if the container is a loot container, and the meta tag isn't already above 0, set it.
                TileEntityLootContainer container = __instance.xui.lootContainer;
                if (container != null && __instance.StackLocation == XUiC_ItemStack.StackLocationTypes.LootContainer)
                {
                    BlockValue Container = GameManager.Instance.World.GetBlock(container.ToWorldPos());
                    if (Container.meta > 0)
                    {
                        Container.meta = 1;
                        GameManager.Instance.World.SetBlockRPC(0, container.ToWorldPos(), Container);
                    }
                }
            }
        }
    }

    // Reset the meta on the storage container
    [HarmonyPatch(typeof(XUiC_LootContainer))]
    [HarmonyPatch("OnOpen")]
    public class SphereII_XUiC_LootContainer_OnOpen
    {
        public static void Postfix(TileEntityLootContainer ___localTileEntity)
        {
            BlockValue blockValue = GameManager.Instance.World.GetBlock(___localTileEntity.ToWorldPos());
            if (blockValue.meta > 0)
            {
                blockValue.meta = 0;
                GameManager.Instance.World.SetBlockRPC(0, ___localTileEntity.ToWorldPos(), blockValue);

            }
        }
    }
}
