namespace TabletopCardCompanion
{
    /// <summary>
    /// Container for TabletopCardCompanion message types.
    /// </summary>
    public enum MsgType : short
    {
        /// <summary>
        /// Message ids start at this value to prevent overwritting Local Authority and Unity message ids.
        /// </summary>
        Lowest = LocalAuthority.Message.MsgType.Highest + 1,

        // CardController
        ToggleColor,
        Rotate,
        Scale,

        // Base / Derived
        BaseFlip,
        BaseScale,
        BaseRotate,

        /// <summary>
        /// The highest value of TabletopCardCompanion message ids. Imported assets must be above this value.
        /// </summary>
        Highest,
    }
}
