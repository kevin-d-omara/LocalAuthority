namespace TabletopCardCompanion
{
    /// <summary>
    /// Container Enum for all TabletopCardCompanion message types.
    /// </summary>
    public enum MsgType : short
    {
        // Prevent overwritting Unity built-in message id's.
        Lowest = UnityEngine.Networking.MsgType.Highest + 1,

        // Ownership
        RequestOwnership,
        ReleaseOwnership,

        // CardController
        ToggleColor,
        FlipOver,
        Rotate,
        Scale,
    }
}
