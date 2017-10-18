using UnityEngine.Networking;

namespace TabletopCardCompanion
{
    public static class MsgTypeUid
    {
        /// <summary>
        /// Return the next available MsgType ID.
        /// </summary>
        public static short Next
        {
            get { return ++next; }
        }

        private static short next = MsgType.Highest;
    }
}