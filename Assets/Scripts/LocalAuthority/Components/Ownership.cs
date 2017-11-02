using LocalAuthority.Message;
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
    /// <para>
    /// For an example of use, <see cref="NetworkPosition"/>.
    /// </para>
    /// </summary>
    /// <remarks>The Player prefab must have <see cref="LocalAuthorityPlayer"/> attached for this to work.</remarks>
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
            get { return Owner == LocalPlayer; }
        }

        /// <summary>
        /// True if owned by another player.
        /// </summary>
        public bool IsOwnedByRemote
        {
            get { return Owner != null && Owner != LocalPlayer; }
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
            SendCommand<TwoNetIdMessage>((short) MsgType.RequestOwnership, LocalPlayer.netId);

            // Give immediate control (client-side prediction).
            if (IsOwnedByNone)
            {
                owner = LocalPlayer;
            }
        }

        /// <summary>
        /// Release ownership of the object so that it has no owner.
        /// </summary>
        public void ReleaseOwnership()
        {
            SendCommand<TwoNetIdMessage>((short)MsgType.ReleaseOwnership, LocalPlayer.netId);

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

            // Prevent players from dropping someone else's ownership.
            if (ownership.Owner == requester)
            {
                ownership.Owner = null;
            }
        }


        // Initialization ------------------------------------------------------
        [SyncVar]
        private NetworkIdentity owner;

        /// <summary>
        /// Wrapper reference to the Player game object owned by this client.
        /// </summary>
        private static NetworkIdentity LocalPlayer
        {
            get { return LocalAuthorityPlayer.LocalPlayer.NetIdentity; }
        }

        protected override void RegisterCommands()
        {
            RegisterCommand((short)MsgType.RequestOwnership, CmdRequestOwnership);
            RegisterCommand((short)MsgType.ReleaseOwnership, CmdReleaseOwnership);
        }
    }
}