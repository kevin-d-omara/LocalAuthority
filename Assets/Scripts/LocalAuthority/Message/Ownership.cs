using TabletopCardCompanion;
using UnityEngine.Networking;
using MsgType = TabletopCardCompanion.MsgType;

namespace LocalAuthority.Message
{
    /// <summary>
    /// This tracks player ownership of a GameObject. This does NOT give actual authority, it merely offers server
    /// authoritative way to track and change ownership.
    /// </summary>
    /// <remarks>It is required that the Player prefab has "PlayerInfo" attached.</remarks>
    public class Ownership : LocalAuthorityBehaviour
    {
        /// <summary>
        /// NetworkIdentity attached to the owning player, or null if no player has ownership.
        /// </summary>
        public NetworkIdentity Owner { get { return owner; } set { owner = value; } }

        /// <summary>
        /// True if owned by the local player.
        /// </summary>
        public bool IsOwnedByLocal
        {
            get { return Owner == PlayerInfo.LocalPlayer.NetIdentity; }
        }

        /// <summary>
        /// True if owned by another player.
        /// </summary>
        public bool IsOwnedByRemote
        {
            get { return Owner != null && Owner != PlayerInfo.LocalPlayer.NetIdentity; }
        }


        /// <summary>
        /// True if owned by no player.
        /// </summary>
        public bool IsOwnedByNone
        {
            get { return Owner == null; }
        }

        /// <summary>
        /// Give this client ownership of the object if it is not already owned.
        /// </summary>
        public void RequestOwnership()
        {
            var msg = new TwoNetIdMessage(netId, PlayerInfo.LocalPlayer.netId);
            SendCommand((short)MsgType.RequestOwnership, msg);

            // Give immediate control (client-side prediction).
            owner = PlayerInfo.LocalPlayer.NetIdentity;
        }

        /// <summary>
        /// Release ownership of the object so that it has no owner.
        /// </summary>
        public void ReleaseOwnership()
        {
            var msg = new TwoNetIdMessage(netId, PlayerInfo.LocalPlayer.netId);
            SendCommand((short)MsgType.ReleaseOwnership, msg);

            // Immediately release control (client-side prediction).
            owner = null;
        }


        // Message Commands ----------------------------------------------------
        private static void CmdRequestOwnership(NetworkMessage netMsg)
        {
            var msg = netMsg.ReadMessage<TwoNetIdMessage>();
            var ownable = NetworkingUtilities.FindLocalComponent<Ownership>(msg.netId);
            var requester = NetworkingUtilities.FindLocalObject(msg.netId2);

            // Prevent players from stealing ownership.
            if (ownable.Owner == null)
            {
                ownable.Owner = requester.GetComponent<NetworkIdentity>();
            }
        }

        private static void CmdReleaseOwnership(NetworkMessage netMsg)
        {
            var msg = netMsg.ReadMessage<TwoNetIdMessage>();
            var ownable = NetworkingUtilities.FindLocalComponent<Ownership>(msg.netId);
            var requester = NetworkingUtilities.FindLocalObject(msg.netId2);

            if (ownable.Owner == requester.GetComponent<NetworkIdentity>())
            {
                ownable.Owner = null;
            }
        }


        // Initialization ------------------------------------------------------
        private NetworkIdentity owner;

        protected override void RegisterCallbacks()
        {
            RegisterCallback((short)MsgType.RequestOwnership, CmdRequestOwnership);
            RegisterCallback((short)MsgType.ReleaseOwnership, CmdReleaseOwnership);
        }
    }
}