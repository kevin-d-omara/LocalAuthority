using UnityEngine;
using UnityEngine.Networking;
using MsgType = TabletopCardCompanion.MsgType;

namespace LocalAuthority.Message
{
    /// <summary>
    /// Like Unity's <see cref="NetworkTransform"/>, except with local authority enabled.
    /// </summary>
    [RequireComponent(typeof(Ownership))]
    public class NetworkPosition : LocalAuthorityBehaviour
    {
        /// <summary>
        /// Wrapper for <see cref="Ownership"/> to ensure final targetSyncPosition is sent.
        /// </summary>
        public void ReleaseOwnership()
        {
            targetSyncPosition = transform.position;
            BroadcastCurrentTransform();
            ownership.ReleaseOwnership();
        }


        private void Update()
        {
            if (ownership.IsOwnedByLocal)
            {
                targetSyncPosition = transform.position;

                // Periodically broadcast.
                if (true)
                {
                    BroadcastCurrentTransform();
                }
            }
            else if (targetSyncPosition != transform.position)
            {
                // Interpolate.
                transform.position = targetSyncPosition;
            }
        }

        private void BroadcastCurrentTransform()
        {
            var msg = NewMessage<Vector3CommandRecordMessage>();
            msg.value = transform.position;
            SendCommand((short)MsgType.UpdateTargetSyncPosition, msg);
        }

        private static void MsgCmdUpdateTargetSyncPosition(NetworkMessage netMsg)
        {
            var msg = netMsg.ReadMessage<Vector3CommandRecordMessage>();
            var netPosition = NetworkingUtilities.FindLocalComponent<NetworkPosition>(msg.netId);
            var syncPosition = msg.value;

            netPosition.targetSyncPosition = syncPosition;
        }

        public void HookTargetSyncPosition(Vector3 newSyncPosition)
        {
            // Don't update if owned by local or none, because 'newSyncPosition' is our own historical data.
            if (isServer || ownership.IsOwnedByRemote)
            {
                targetSyncPosition = newSyncPosition;
            }
        }

        // Data ----------------------------------------------------------------

        [SyncVar(hook = nameof(HookTargetSyncPosition))]
        private Vector3 targetSyncPosition;


        // Initialization ------------------------------------------------------
        private Ownership ownership;

        protected override void Awake()
        {
            base.Awake();
            ownership = GetComponent<Ownership>();
        }

        public override void OnStartServer()
        {
            base.OnStartServer();

            targetSyncPosition = transform.position;
        }

        protected override void RegisterCallbacks()
        {
            RegisterCallback((short)MsgType.UpdateTargetSyncPosition, MsgCmdUpdateTargetSyncPosition);
        }
    }
}