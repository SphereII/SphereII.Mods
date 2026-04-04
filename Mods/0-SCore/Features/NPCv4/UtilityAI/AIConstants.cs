namespace UAI
{
    /// <summary>
    /// Named constants for V4 UtilityAI tasks and considerations.
    /// Replaces magic numbers previously scattered across individual task and consideration files.
    /// </summary>
    public static class AIConstants
    {
        // ── Vision ───────────────────────────────────────────────────────────────

        /// <summary>View cone half-angle (degrees) for wide checks such as attack range.</summary>
        public const float ViewConeAngleWide = 70f;

        /// <summary>View cone half-angle (degrees) for narrow / facing checks.</summary>
        public const float ViewConeAngleNarrow = 30f;

        /// <summary>Squared distance below which a target is considered "close" (attack/revenge).</summary>
        public const float CloseDistanceSq = 20f;

        /// <summary>Squared distance beyond which the view-cone check is skipped on initial CanSee.</summary>
        public const float ViewConeSkipDistSq = 100f;

        /// <summary>Squared distance at which the secondary view-cone re-check is applied in CanSee.</summary>
        public const float ViewConeReCheckDistSq = 30f;

        /// <summary>Offset applied along the ray direction to avoid self-intersection on raycasts.</summary>
        public const float RaycastOriginNudge = 0.2f;

        // ── Combat ───────────────────────────────────────────────────────────────

        /// <summary>Angle threshold (degrees) used to determine attack alignment.</summary>
        public const float AttackAngleThreshold = 45f;

        // ── Movement / Pathing ───────────────────────────────────────────────────

        /// <summary>Y-delta above which ladder edge-alignment is applied in GetMoveToLocation.</summary>
        public const float LadderYDeltaThreshold = 1.5f;

        /// <summary>Minimum drop distance (units) before a low jump is preferred over a standard one.</summary>
        public const float LowJumpDropThreshold = 2f;

        /// <summary>Downward raycast length used to check for ground below the entity's feet.</summary>
        public const float GroundCheckRayLength = 3.4f;

        /// <summary>Maximum ground-ray distance that still indicates solid ground (no gap ahead).</summary>
        public const float GroundCheckMaxHitDist = 2.2f;

        /// <summary>Forward nudge applied to foot position when sampling the ground for jump detection.</summary>
        public const float JumpForwardNudge = 0.2f;

        /// <summary>Vertical nudge applied to foot position when sampling the ground for jump detection.</summary>
        public const float JumpVerticalNudge = 0.4f;

        /// <summary>Y offset (above entity base) used to check headroom when deciding jump height.</summary>
        public const float JumpHeadroomYOffset = 2.35f;

        /// <summary>Downward raycast length used to confirm a landing surface in GetMoveToLocation.</summary>
        public const float LandingCheckRayLength = 1.02f;

        // ── Teleport ─────────────────────────────────────────────────────────────

        /// <summary>Minimum spawn radius when placing the entity near the leader after a teleport.</summary>
        public const int TeleportSpawnRadiusMin = 1;

        /// <summary>Maximum spawn radius when placing the entity near the leader after a teleport.</summary>
        public const int TeleportSpawnRadiusMax = 2;

        /// <summary>Preferred clearance radius passed to GetRandomSpawnPositionMinMaxToPosition.</summary>
        public const int TeleportSpawnClearance = 3;

        // ── Containers / Looting ─────────────────────────────────────────────────

        /// <summary>Chunk search radius (±n chunks) used when scanning for tile entities.</summary>
        public const int TileEntityScanChunkRadius = 1;

        /// <summary>Raycast length used in CheckContainer to validate line-of-sight to the container.</summary>
        public const float ContainerRaycastLength = 3f;

        /// <summary>Squared distance below which a container is considered within reach.</summary>
        public const float ContainerReachDistSq = 1f;

        /// <summary>Time in seconds before a cached tile-entity scan result is considered stale.</summary>
        public const float TileEntityCacheTtl = 2f;

        // ── Doors ────────────────────────────────────────────────────────────────

        /// <summary>Delay in milliseconds before automatically closing a door the NPC opened.</summary>
        public const int DoorAutoCloseDelayMs = 2000;

        // ── Entity Search ────────────────────────────────────────────────────────

        /// <summary>Default radius (units) for nearby-entity bounds queries.</summary>
        public const float DefaultNearbySearchRadius = 20f;

        /// <summary>Initial capacity of the shared pooled entity-search buffer.</summary>
        public const int EntityBufferInitialCapacity = 32;

        // ── IsFacing ─────────────────────────────────────────────────────────────

        /// <summary>Angle in degrees within which an entity is considered to be facing a target.</summary>
        public const float FacingAngleThreshold = 10f;
    }
}
