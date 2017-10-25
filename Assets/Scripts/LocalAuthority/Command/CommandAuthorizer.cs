using UnityEngine.Networking;

namespace LocalAuthority.Command
{
    /// <summary>
    /// Attach this component to the player prefab to allow <see cref="CommandExecutor"/> to work.
    /// </summary>
    public class CommandAuthorizer : NetworkBehaviour
    {
        /// <summary>
        /// The currently instantiated CommandAuthorizer. Is created during OnStartLocalPlayer.
        /// </summary>
        public static CommandAuthorizer Instance { get; private set; }

        /// <summary>
        /// Give this client ownership of the object if it is available.
        /// </summary>
        [Command]
        public void CmdRequestOwnership(NetworkIdentity identity)
        {
            if (identity.clientAuthorityOwner == null)
            {
                identity.AssignClientAuthority(connectionToClient);
            }
        }

        /// <summary>
        /// Give the server back ownership of the object.
        /// </summary>
        [Command]
        public void CmdReleaseOwnership(NetworkIdentity identity)
        {
            if (identity.clientAuthorityOwner != null)
            {
                identity.RemoveClientAuthority(connectionToClient);
            }
        }

        public override void OnStartLocalPlayer()
        {
            base.OnStartLocalPlayer();
            if (isLocalPlayer) Instance = this;
        }
    }
}