using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;

namespace LocalAuthority
{
    public static class NetworkReaderExtensions
    {
        /// <summary>
        /// Read an object of any type readable by <see cref="NetworkReader"/>.
        /// </summary>
        public static object Read(this NetworkReader reader, Type type)
        {
            Func<NetworkReader, object> read;
            if (@switch.TryGetValue(type, out read))
            {
                return read(reader);
            }
            else
            {
                if (LogFilter.logFatal) { Debug.LogError("NetworkReader: type is unreadable: " + type); }
                return null;
            }
        }

        /// <summary>
        /// A switch statement on Type.
        /// <para>
        /// <code>
        /// switch (type)
        /// {
        ///     case(typeof(int)):
        ///         reader.ReadInt32();
        ///         break;
        ///     ...
        /// }
        /// </code>
        /// </para>
        /// </summary>
        private static readonly Dictionary<Type, Func<NetworkReader, object>> @switch =
            new Dictionary<Type, Func<NetworkReader, object>>
            {
                {typeof(NetworkInstanceId), (reader) => reader.ReadNetworkId()},
                {typeof(NetworkSceneId),    (reader) => reader.ReadSceneId()},
                {typeof(char),              (reader) => reader.ReadChar()},
                {typeof(byte),              (reader) => reader.ReadByte()},
                {typeof(sbyte),             (reader) => reader.ReadSByte()},
                {typeof(short),             (reader) => reader.ReadInt16()},
                {typeof(ushort),            (reader) => reader.ReadUInt16()},
                {typeof(int),               (reader) => reader.ReadInt32()},
                {typeof(uint),              (reader) => reader.ReadUInt32()},
                {typeof(long),              (reader) => reader.ReadInt64()},
                {typeof(ulong),             (reader) => reader.ReadUInt64()},
                {typeof(float),             (reader) => reader.ReadSingle()},
                {typeof(double),            (reader) => reader.ReadDouble()},
                {typeof(decimal),           (reader) => reader.ReadDecimal()},
                {typeof(string),            (reader) => reader.ReadString()},
                {typeof(bool),              (reader) => reader.ReadBoolean()},
                {typeof(Vector2),           (reader) => reader.ReadVector2()},
                {typeof(Vector3),           (reader) => reader.ReadVector3()},
                {typeof(Vector4),           (reader) => reader.ReadVector4()},
                {typeof(Color),             (reader) => reader.ReadColor()},
                {typeof(Color32),           (reader) => reader.ReadColor32()},
                {typeof(Quaternion),        (reader) => reader.ReadQuaternion()},
                {typeof(Rect),              (reader) => reader.ReadRect()},
                {typeof(Plane),             (reader) => reader.ReadPlane()},
                {typeof(Ray),               (reader) => reader.ReadRay()},
                {typeof(Matrix4x4),         (reader) => reader.ReadMatrix4x4()},
                {typeof(NetworkHash128),    (reader) => reader.ReadNetworkHash128()},
                {typeof(NetworkIdentity),   (reader) => reader.ReadNetworkIdentity()},
                {typeof(Transform),         (reader) => reader.ReadTransform()},
                {typeof(GameObject),        (reader) => reader.ReadGameObject()},
            };
    }
}
