using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;

namespace LocalAuthority
{
    public static class NetworkWriterExtensions
    {
        /// <summary>
        /// Write an object of any type writeable by <see cref="NetworkWriter"/>.
        /// </summary>
        public static void Write(this NetworkWriter writer, object obj)
        {
            if (obj == null) return;

            Action<NetworkWriter, object> write;
            if (@switch.TryGetValue(obj.GetType(), out write))
            {
                write(writer, obj);
            }
            else
            {
                if (LogFilter.logFatal) { Debug.LogError("NetworkWriter: cannot write " + obj + ", because its type is not writeable: " + obj.GetType()); }
            }
        }

        /// <summary>
        /// A switch statement on Type.
        /// <para>
        /// <code>
        /// switch (value.GetType())
        /// {
        ///     case(typeof(int)):
        ///         writer.write((int) value);
        ///         break;
        ///     ...
        /// }
        /// </code>
        /// </para>
        /// </summary>
        private static readonly Dictionary<Type, Action<NetworkWriter, object>> @switch =
            new Dictionary<Type, Action<NetworkWriter, object>>
            {
                {typeof(NetworkInstanceId), (writer, value) => writer.Write((NetworkInstanceId) value)},
                {typeof(NetworkSceneId),    (writer, value) => writer.Write((NetworkSceneId) value)},
                {typeof(char),              (writer, value) => writer.Write((char) value)},
                {typeof(byte),              (writer, value) => writer.Write((byte) value)},
                {typeof(sbyte),             (writer, value) => writer.Write((sbyte) value)},
                {typeof(short),             (writer, value) => writer.Write((short) value)},
                {typeof(ushort),            (writer, value) => writer.Write((ushort) value)},
                {typeof(int),               (writer, value) => writer.Write((int) value)},
                {typeof(uint),              (writer, value) => writer.Write((uint) value)},
                {typeof(long),              (writer, value) => writer.Write((long) value)},
                {typeof(ulong),             (writer, value) => writer.Write((ulong) value)},
                {typeof(float),             (writer, value) => writer.Write((float) value)},
                {typeof(double),            (writer, value) => writer.Write((double) value)},
                {typeof(decimal),           (writer, value) => writer.Write((decimal) value)},
                {typeof(string),            (writer, value) => writer.Write((string) value)},
                {typeof(bool),              (writer, value) => writer.Write((bool) value)},
                {typeof(Vector2),           (writer, value) => writer.Write((Vector2) value)},
                {typeof(Vector3),           (writer, value) => writer.Write((Vector3) value)},
                {typeof(Vector4),           (writer, value) => writer.Write((Vector4) value)},
                {typeof(Color),             (writer, value) => writer.Write((Color) value)},
                {typeof(Color32),           (writer, value) => writer.Write((Color32) value)},
                {typeof(Quaternion),        (writer, value) => writer.Write((Quaternion) value)},
                {typeof(Rect),              (writer, value) => writer.Write((Rect) value)},
                {typeof(Plane),             (writer, value) => writer.Write((Plane) value)},
                {typeof(Ray),               (writer, value) => writer.Write((Ray) value)},
                {typeof(Matrix4x4),         (writer, value) => writer.Write((Matrix4x4) value)},
                {typeof(NetworkHash128),    (writer, value) => writer.Write((NetworkHash128) value)},
                {typeof(NetworkIdentity),   (writer, value) => writer.Write((NetworkIdentity) value)},
                {typeof(Transform),         (writer, value) => writer.Write((Transform) value)},
                {typeof(GameObject),        (writer, value) => writer.Write((GameObject) value)},
            };
    }
}
