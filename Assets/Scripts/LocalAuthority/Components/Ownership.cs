using LocalAuthority.Message;
using TabletopCardCompanion;
using UnityEngine.Networking;
using MsgType = LocalAuthority.Message.MsgType;

namespace LocalAuthority.Components
{
    /// <summary>
    /// Tracks player ownership of a GameObject across the network. It acts like a networked mutex, except code won't
    /// hang if the mutex is down.
    /// <para>
    /// To run code with the protection of the mutex, wrap it with:
    ///     - call <see cref="RequestOwnership"/>
    ///     - if (myOwnership.IsOwnedByLocal)
    ///           // execute your code over as many frames as you'd like
    ///     - call <see cref="ReleaseOwnership"/>
    /// </para>
    /// </summary>
    /// <seealso cref="NetworkPosition"/>
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
            // TODO: Decouple from TabletopCardCompanion.PlayerInfo
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
            SendCommand<TwoNetIdMessage>((short) MsgType.RequestOwnership, PlayerInfo.LocalPlayer.netId);

            // Give immediate control (client-side prediction).
            if (IsOwnedByNone)
            {
                owner = PlayerInfo.LocalPlayer.NetIdentity;
            }
        }

        /// <summary>
        /// Release ownership of the object so that it has no owner.
        /// </summary>
        public void ReleaseOwnership()
        {
            SendCommand<TwoNetIdMessage>((short)MsgType.ReleaseOwnership, PlayerInfo.LocalPlayer.netId);

            // Immediately release control (client-side prediction).
            if (IsOwnedByLocal)
            {
                owner = null;
            }
        }


        // Message Commands ----------------------------------------------------
        private static void CmdRequestOwnership(NetworkMessage netMsg)
        {
            var msg = netMsg.ReadMessage<TwoNetIdMessage>();
            var ownership = FindLocalComponent<Ownership>(msg.netId);
            var requester = FindLocalComponent<NetworkIdentity>(msg.netId2);

            // Prevent players from stealing ownership.
            if (ownership.IsOwnedByNone)
            {
                ownership.Owner = requester;
            }
        }

        private static void CmdReleaseOwnership(NetworkMessage netMsg)
        {
            var msg = netMsg.ReadMessage<TwoNetIdMessage>();
            var ownership = FindLocalComponent<Ownership>(msg.netId);
            var requester = FindLocalComponent<NetworkIdentity>(msg.netId2);

            if (ownership.Owner == requester)
            {
                ownership.Owner = null;
            }
        }


        // Initialization ------------------------------------------------------
        [SyncVar]
        private NetworkIdentity owner;

        protected override void RegisterCallbacks()
        {
            RegisterCallback((short)MsgType.RequestOwnership, CmdRequestOwnership);
            RegisterCallback((short)MsgType.ReleaseOwnership, CmdReleaseOwnership);
        }
    }
}