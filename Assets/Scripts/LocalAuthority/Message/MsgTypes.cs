namespace LocalAuthority.Message
{
    /// <summary>
    /// Container for Local Authority message types.
    /// </summary>
    public enum MsgType : short
    {
        /// <summary>
        /// Message ids start at this value to prevent overwritting Unity message id's.
        /// </summary>
        Lowest = UnityEngine.Networking.MsgType.Highest + 1,

        // Ownership
        RequestOwnership,
        ReleaseOwnership,

        // NetworkPosition
        UpdateTargetSyncPosition,

        /// <summary>
        /// The highest value of Local Authority message ids. User messages must be above this value.
        /// </summary>
        Highest,
    }
}
