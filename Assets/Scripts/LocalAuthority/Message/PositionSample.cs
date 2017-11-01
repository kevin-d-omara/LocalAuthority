using LocalAuthority.Components;
using UnityEngine;
using UnityEngine.Networking;
using Object = System.Object;

namespace LocalAuthority.Message
{
    /// <summary>
    /// Data class for <see cref="NetworkPosition"/> movement samples.
    /// </summary>
    public struct PositionSample
    {
        // Data ----------------------------------------------------------------
        public Vector3 Position { get; }

        public float Timestamp { get; }


        // Methods -------------------------------------------------------------
        public PositionSample(Vector3 position, float timestamp)
        {
            Position = position;
            Timestamp = timestamp;
        }

        /// <summary>
        /// Deserialize using Unity networking.
        /// </summary>
        public static PositionSample Read(NetworkReader reader)
        {
            var position = reader.ReadVector3();
            var timestamp = reader.ReadSingle();
            return new PositionSample(position, timestamp);
        }

        /// <summary>
        /// Serialize using Unity networking.
        /// </summary>
        public void Write(NetworkWriter writer)
        {
            writer.Write(Position);
            writer.Write(Timestamp);
        }
        

        // Overrides -----------------------------------------------------------
        public override bool Equals(Object obj)
        {
            return obj is PositionSample && this == (PositionSample)obj;
        }

        public override int GetHashCode()
        {
            return Position.GetHashCode() ^ Timestamp.GetHashCode();
        }

        public static bool operator ==(PositionSample a, PositionSample b)
        {
            return a.Position == b.Position && a.Timestamp == b.Timestamp;
        }

        public static bool operator !=(PositionSample a, PositionSample b)
        {
            return !(a == b);
        }
    }


    // Messages ------------------------------------------------------------
    public class PositionSampleMessage : NetIdMessage
    {
        public PositionSample value;

        public PositionSampleMessage()
        {
        }

        public PositionSampleMessage(NetworkInstanceId id, PositionSample value) : base(id)
        {
            this.value = value;
        }

        public override void Deserialize(NetworkReader reader)
        {
            base.Deserialize(reader);
            value = PositionSample.Read(reader);
        }

        public override void Serialize(NetworkWriter writer)
        {
            base.Serialize(writer);
            value.Write(writer);
        }
    }
}
