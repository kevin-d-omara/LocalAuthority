using UnityEngine.Networking;

namespace NonPlayerClientAuthority
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
        /// Give this client ownership of the object.
        /// </summary>
        [Command]
        public void CmdRequestOwnership(NetworkIdentity identity)
        {
            identity.AssignClientAuthority(connectionToClient);
        }

        /// <summary>
        /// Give the server back ownership of the object.
        /// </summary>
        [Command]
        public void CmdReleaseOwnership(NetworkIdentity identity)
        {
            identity.RemoveClientAuthority(connectionToClient);
        }

        public override void OnStartLocalPlayer()
        {
            base.OnStartLocalPlayer();
            Instance = this;
        }
    }
}