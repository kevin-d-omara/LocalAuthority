namespace TabletopCardCompanion
{
    /// <summary>
    /// Container Enum for all TabletopCardCompanion message types.
    /// </summary>
    public enum MsgType : short
    {
        Lowest = UnityEngine.Networking.MsgType.Highest + 1,

        // CardController
        ToggleColor,
        Rotate,
        Scale,

        // Ownership
        RequestOwnership,
        ReleaseOwnership,
    }
}
