using TabletopCardCompanion;
using TabletopCardCompanion.Debug;
using UnityEngine.Networking;

namespace LocalAuthority.Message
{
    /// <summary>
    /// This tracks player ownership of a GameObject. This does NOT give actual authority, it merely offers server
    /// authoritative way to track and change ownership.
    /// </summary>
    /// <remarks>It is required that the Player prefab has "PlayerInfo" attached.</remarks>
    public class Ownership : NetworkBehaviour
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
            NetworkManager.singleton.client.Send((short)MsgType.RequestOwnership, msg);
        }

        /// <summary>
        /// Release ownership of the object so that it has no owner.
        /// </summary>
        public void ReleaseOwnership()
        {
            var msg = new TwoNetIdMessage(netId, PlayerInfo.LocalPlayer.netId);
            NetworkManager.singleton.client.Send((short)MsgType.ReleaseOwnership, msg);
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



        [SyncVar(hook = nameof(HookOwner))]
        private NetworkIdentity owner;

        private void HookOwner(NetworkIdentity networkIdentity)
        {
            // TODO: broadcast event (maybe use unity networking SyncEvent?) <------------------------------------------
            owner = networkIdentity;
            var str = owner != null ? owner.netId.ToString() : " ";
            DebugStreamer.AddMessage("new owner: " + str);
        }



        private void Awake()
        {
            RegisterMessageCallbacks();
        }

        private void RegisterMessageCallbacks()
        {
            NetworkServer.RegisterHandler((short)MsgType.RequestOwnership, OnRequestOwnership);
            NetworkServer.RegisterHandler((short)MsgType.ReleaseOwnership, OnReleaseOwnership);
        }

        private enum MsgType : short
        {
            RequestOwnership = UnityEngine.Networking.MsgType.Highest + 10,
            ReleaseOwnership,
        }
    }
}