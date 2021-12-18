namespace UAI
{
    /// <summary>
    /// This is a common interface for filtering targets for considerations in Utility AI.
    /// The target's type will be an <see cref="Entity"/> (for target entities)
    /// or a <see cref="Vector3d"/> (for waypoints).
    /// </summary>
    /// <typeparam name="T">The type of target.</typeparam>
    public interface IUAITargetFilter<T>
    {
        /// <summary>
        /// Test the target.
        /// </summary>
        /// <param name="target">The target to test.</param>
        /// <returns>True if the target passes, false otherwise.</returns>
        bool Test(T target);
    }
}