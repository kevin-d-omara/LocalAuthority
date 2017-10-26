using UnityEngine.Networking;

namespace LocalAuthority.Message
{
    /// <summary>
    /// This tracks player ownership of a GameObject. This does NOT give actual authority, it merely offers server
    /// authoritative way to track and change ownership.
    /// </summary>
    /// <remarks>This method of manipulating ownership is NOT resilient to cheating. That's fine, this is for use in a
    /// multiplayer game where cheating is "acceptable".</remarks>
    public class Ownership : NetworkBehaviour
    {
        /// <summary>
        /// NetworkIdentity attached to the owning player, or null if no player has ownership.
        /// </summary>
        public NetworkIdentity Owner { get { return owner; } set { owner = value; } }

        /// <summary>
        /// Give this client ownership of the object if it is not already owned.
        /// </summary>
        /// <param name="requester">NetworkIdentity attached to the player's game object.</param>
        public void RequestOwnership(NetworkIdentity requester)
        {
            if (requester.isLocalPlayer)
            {
                var msg = new TwoNetIdMessage(netId, requester.netId);
                NetworkManager.singleton.client.Send(MsgType.RequestOwnership, msg);
            }
        }

        /// <summary>
        /// Release ownership of the object so that it has no owner.
        /// </summary>
        /// <param name="requester">NetworkIdentity attached to the player's game object.</param>
        public void ReleaseOwnership(NetworkIdentity requester)
        {
            if (requester.isLocalPlayer)
            {
                var msg = new TwoNetIdMessage(netId, requester.netId);
                NetworkManager.singleton.client.Send(MsgType.ReleaseOwnership, msg);
            }
        }

        private static void OnRequestOwnership(NetworkMessage netMsg)
        {
            var msg = netMsg.ReadMessage<TwoNetIdMessage>();
            var ownable = NetworkingUtilities.FindLocalComponent<Ownership>(msg.netId);
            var requester = NetworkingUtilities.FindLocalObject(msg.netId2);

            if (ownable.Owner == null)
            {
                ownable.Owner = requester.GetComponent<NetworkIdentity>();
            }
        }

        private static void OnReleaseOwnership(NetworkMessage netMsg)
        {
            var msg = netMsg.ReadMessage<TwoNetIdMessage>();
            var ownable = NetworkingUtilities.FindLocalComponent<Ownership>(msg.netId);
            var requester = NetworkingUtilities.FindLocalObject(msg.netId2);

            if (ownable.Owner == requester.GetComponent<NetworkIdentity>())
            {
                ownable.Owner = null;
            }
        }

        [SyncVar]
        private NetworkIdentity owner;

        private void Awake()
        {
            RegisterMessageCallbacks();
        }

        private void RegisterMessageCallbacks()
        {
            NetworkServer.RegisterHandler(MsgType.RequestOwnership, OnRequestOwnership);
            NetworkServer.RegisterHandler(MsgType.ReleaseOwnership, OnReleaseOwnership);
        }

        private static class MsgType
        {
            public static readonly short RequestOwnership = MsgTypeUid.Next;
            public static readonly short ReleaseOwnership = MsgTypeUid.Next;
        }
    }
}