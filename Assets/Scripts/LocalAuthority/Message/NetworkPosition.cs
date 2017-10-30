using TabletopCardCompanion.Debug;
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
                var deltaTime = Time.time - LastBroadcastTime;
                if (deltaTime > SendRate)
                {
                    LastBroadcastTime = Time.time;
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

            DebugStreamer.AddMessage("Sent! " + Time.time);
        }

        private static void CmdUpdateTargetSyncPosition(NetworkMessage netMsg)
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

        /// <summary>
        /// The target position interpolating towards.
        /// </summary>
        public Vector3 TargetSyncPosition { get { return targetSyncPosition; } set { targetSyncPosition = value; } }

        /// <summary>
        /// Amount of time between network updates.
        /// </summary>
        public float SendRate { get { return 1f / networkSendRate; } }

        /// <summary>
        /// Number of network updates per second.
        /// </summary>
        public int NetworkSendRate { get { return networkSendRate; } set { networkSendRate = value >= 1 ? value : 1; } }

        /// <summary>
        /// If a movement update puts an object further from its current position that this value, it will snap to the position instead of moving smoothly.
        /// </summary>
        public float SnapThreshold { get { return snapThreshold; } set { snapThreshold = value; } }

        /// <summary>
        /// The most recent time when a movement synchronization packet arrived for this object.
        /// </summary>
        public float LastSyncTime { get; private set; }

        /// <summary>
        /// The most recent time when a movement synchronization packet was sent from this object.
        /// </summary>
        public float LastBroadcastTime { get; private set; }

        [SyncVar(hook = nameof(HookTargetSyncPosition))]
        private Vector3 targetSyncPosition;

        [SerializeField]
        [Range(1,60)]
        [Tooltip("Number of network updates per second.")]
        private int networkSendRate = 9;

        [SerializeField]
        [Tooltip("If a movement update puts an object further from its current position that this value, it will snap to the position instead of moving smoothly.")]
        private float snapThreshold = 5f;


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
            RegisterCallback((short)MsgType.UpdateTargetSyncPosition, CmdUpdateTargetSyncPosition);
        }
    }
}