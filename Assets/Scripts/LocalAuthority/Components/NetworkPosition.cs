using LocalAuthority.Message;
using UnityEngine;
using UnityEngine.Networking;
using MsgType = LocalAuthority.Message.MsgType;

namespace LocalAuthority.Components
{
    /// <summary>
    /// Enables any client to move the object and update other clients. To broadcast a continuous movement:
    ///     - call <see cref="BeginMovement"/>
    ///     - update transform.position over as many frames as you'd like
    ///     - call <see cref="EndMovement"/>
    ///
    ///     - note: to prevent movement when ownership is denied by the server, wrap movement updates in:
    ///             if (ownership.IsOwnedByLocal)
    ///             {
    ///                 // update transform.position
    ///             }
    /// </summary>
    [RequireComponent(typeof(Ownership))]
    public class NetworkPosition : LocalAuthorityBehaviour
    {
        public void BeginMovement()
        {
            ownership.RequestOwnership();
        }

        public void EndMovement()
        {
            targetSyncPosition = transform.position;
            BroadcastCurrentTransform();
            ownership.ReleaseOwnership();
        }

        private void Update()
        {
            if (ownership.IsOwnedByLocal)
            {
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
                // BUG: If object is moving when client joins late, the client doesn't update at all.

                // Interpolate.
                transform.position = targetSyncPosition;
            }
        }

        private void BroadcastCurrentTransform()
        {
            SendCommand<Vector3NetIdMessage>((short) MsgType.UpdateTargetSyncPosition, transform.position);
        }

        private static void CmdUpdateTargetSyncPosition(NetworkMessage netMsg)
        {
            var msg = netMsg.ReadMessage<Vector3NetIdMessage>();
            var netPosition = FindLocalComponent<NetworkPosition>(msg.netId);
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

        protected override void RegisterCommands()
        {
            RegisterCommand((short)MsgType.UpdateTargetSyncPosition, CmdUpdateTargetSyncPosition);
        }
    }
}