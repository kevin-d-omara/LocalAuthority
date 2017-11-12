using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;

namespace LocalAuthority.Components
{
    /// <summary>
    /// Synchronizes the networked object's position and enables any client to move the object.
    /// <para>
    /// There are two modes of synchronization controlled through <see cref="Ownership"/>:
    /// </para>
    /// <para>
    /// Receiving: the object is not owned by this client and is being moved by another client or no client.
    /// Movement is smoothly interpolated between states received from the server.
    /// </para>
    /// <para>
    /// Broadcasting: the object is owned by this client.
    /// Movement state is periodically sent to the server and relayed to all clients.
    /// </para>
    /// <para>
    /// Call <see cref="BeginMovement"/> to enter broadcasting mode.
    /// This will not succeed if the object already has an owner.
    /// To prevent locally moving the object in the case that ownership was denied by the server, wrap movement updates in:
    /// <code>if (ownership.IsOwnedByLocal) { // update transform.position; }</code>
    /// </para>
    /// <para>
    /// Call <see cref="EndMovement"/> to stop broadcasting and release ownership.
    /// </para>
    /// <seealso cref="Ownership"/>
    /// </summary>
    [RequireComponent(typeof(Ownership))]
    public class NetworkPosition : LocalAuthorityBehaviour
    {
        // TODO: make two "Move()" methods, one for absolute, one for delta.

        public void BeginMovement()
        {
            ownership.RequestOwnership();
            samples.Clear();
            RecordPositionSample();
        }

        public void EndMovement()
        {
            waypoints.Clear();
            if (ownership.IsOwnedByLocal)
            {
                BroadcastCurrentPosition();
                ownership.ReleaseOwnership();
            }
        }

        private void Update()
        {
            // Broadcasting mode.
            if (ownership.IsOwnedByLocal)
            {
                RecordPositionSample();

                // Periodically broadcast.
                var elapsedTime = Time.time - LastBroadcastTime;
                if (elapsedTime > SendInterval)
                {
                    BroadcastCurrentPosition();
                }
            }

            // Receiving Mode.
            else
            {
                if (waypoints.Count == 0)
                    return;

                var waypoint = waypoints.Peek();
                targetSyncPosition = waypoint.position;
                targetSyncVelocity = waypoint.velocity;

                // Interpolate or snap.
                var distanceBetween = (waypoint.position - transform.position).magnitude;
                if (distanceBetween > snapThreshold)
                {
                    transform.position = waypoint.position;
                }
                else
                {
                    var newPosition = Vector3.MoveTowards(transform.position, waypoint.position, Time.deltaTime * waypoint.velocity);
                    transform.position = newPosition;
                }

                // Waypoint reached.
                if (transform.position == waypoint.position)
                {
                    waypoints.Dequeue();
                }
            }
        }

        private void BroadcastCurrentPosition()
        {
            LastBroadcastTime = Time.time;
            RecordPositionSample();

            // If one sample, use previous velocity.
            if (samples.Count == 1)
            {
                SendCallback(nameof(RpcUpdateTargetSyncPosition), transform.position, targetSyncVelocity);
                samples.Clear();
                return;
            }

            // Find when the object stopped moving (i.e. earliest position sample matching the current position).
            var start = samples[0];
            var end = samples[samples.Count - 1];
            foreach (var sample in samples)
            {
                if (sample.position == transform.position)
                {
                    end = sample;
                    break;
                }
            }

            // No movement.
            if (end.position == start.position)
            {
                samples.Clear();
                return;
            }

            var duration = end.time - start.time;
            targetSyncVelocity = (end.position - start.position).magnitude / duration;
            SendCallback(nameof(RpcUpdateTargetSyncPosition), end.position, targetSyncVelocity);
            samples.Clear();
        }

        private void RecordPositionSample()
        {
            var sample = new PositionSample(transform.position, Time.time);
            samples.Add(sample);
        }

        [MessageRpc(ClientSidePrediction = true)]
        private void RpcUpdateTargetSyncPosition(Vector3 newTargetSyncPosition, float velocity)
        {
            var waypoint = new Waypoint(newTargetSyncPosition, velocity);
            waypoints.Enqueue(waypoint);
        }


        // Data ----------------------------------------------------------------

        // Broadcasting Mode -------------------------------

        [SerializeField]
        [Range(1, 30)]
        [Tooltip("The number of network updates sent per second.")]
        private int networkSendRate = 9;

        /// <summary>
        /// The number of network updates sent per second.
        /// <para>
        /// If greater than zero, position state updates are sent at most this many times per second.
        /// However, if an object is stationary, no updates are sent.
        /// </para>
        /// <para>
        /// If zero, then no automatic updates are sent. In this case, calling SetDirtyBits() will cause an update to
        /// be sent. This could be used for objects like bullets that have a predictable trajectory.
        /// </para>
        /// </summary>
        public int NetworkSendRate { get { return networkSendRate; } set { networkSendRate = value < 0 ? 0 : value; } }

        /// <summary>
        /// The length of time between sending state updates.
        /// </summary>
        public float SendInterval { get { return 1f / networkSendRate; } }

        /// <summary>
        /// The most recent time when a movement synchronization packet was sent from this object.
        /// </summary>
        public float LastBroadcastTime { get; private set; }

        /// <summary>
        /// Snapshots of where the object has been since the last time it broadcasted.
        /// </summary>
        private List<PositionSample> samples = new List<PositionSample>();


        // Receiving Mode ----------------------------------

        /// <summary>
        /// If a movement update puts an object further from its current position than this value, it will snap to the position instead of moving smoothly.
        /// </summary>
        [Tooltip("If a movement update puts an object further from its current position than this value, it will snap to the position instead of moving smoothly.")]
        public float snapThreshold = 5f;

        /// <summary>
        /// The position being interpolated towards.
        /// </summary>
        public Vector3 TargetSyncPosition { get { return targetSyncPosition; } }

        // Pseudo [SyncVar]; only sent to each client once, when they join the game.
        private Vector3 targetSyncPosition;

        // Pseudo [SyncVar]; only sent to each client once, when they join the game.
        private float targetSyncVelocity;

        /// <summary>
        /// Ordered list of locations to move towards with a specified velocity.
        /// </summary>
        private Queue<Waypoint> waypoints = new Queue<Waypoint>();


        // Data Structs --------------------------------------------------

        /// <summary>
        /// Records a snapshot of where the object was. Used only in broadcasting mode.
        /// </summary>
        private struct PositionSample
        {
            public readonly Vector3 position;
            public readonly float time;

            public PositionSample(Vector3 position, float time)
            {
                this.position = position;
                this.time = time;
            }
        }

        /// <summary>
        /// A destination to move towards with some velocity. Used only in receiving mode.
        /// </summary>
        private struct Waypoint
        {
            public readonly Vector3 position;
            public readonly float velocity;

            public Waypoint(Vector3 position, float velocity)
            {
                this.position = position;
                this.velocity = velocity;
            }
        }
        

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
            targetSyncVelocity = 1f;
        }


        // Serialization -------------------------------------------------------

        // SyncVars are only sent to each client once, when they join the game.
        public override bool OnSerialize(NetworkWriter writer, bool initialState)
        {
            if (initialState)
            {
                // SyncVars
                writer.Write(targetSyncPosition);
                writer.Write(targetSyncVelocity);
                return true;
            }

            // Broadcast if manually set.
            if (syncVarDirtyBits > 0 && ownership.IsOwnedByLocal)
            {
                BroadcastCurrentPosition();
            }

            return false;
        }

        // SyncVars are only read once, when the client joins the game.
        public override void OnDeserialize(NetworkReader reader, bool initialState)
        {
            if (initialState)
            {
                // SyncVars
                targetSyncPosition = reader.ReadVector3();
                targetSyncVelocity = reader.ReadSingle();
            }
        }
    }
}