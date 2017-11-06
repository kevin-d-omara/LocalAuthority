using UnityEngine.Networking;

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
            InvokeCommand(nameof(CmdRequestOwnership), LocalPlayer.netId);
        }

        /// <summary>
        /// Release ownership of the object so that it has no owner.
        /// </summary>
        public void ReleaseOwnership()
        {
            InvokeCommand(nameof(CmdReleaseOwnership), LocalPlayer.netId);
        }


        // Message Callbacks ---------------------------------------------------

        [MessageCommand(ClientSidePrediction = true)]
        private void CmdRequestOwnership(NetworkInstanceId requesterNetId)
        {
            var requester = FindLocalComponent<NetworkIdentity>(requesterNetId);

            // Prevent players from stealing ownership.
            if (IsOwnedByNone)
            {
                Owner = requester;
            }
        }

        [MessageCommand(ClientSidePrediction = true)]
        private void CmdReleaseOwnership(NetworkInstanceId requesterNetId)
        {
            var requester = FindLocalComponent<NetworkIdentity>(requesterNetId);

            // Prevent players from dropping someone else's ownership.
            if (Owner == requester)
            {
                Owner = null;
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
    }
}