using System;
using UnityEngine;
using UnityEngine.Networking;

namespace LocalAuthority.Message
{
    /// <summary>
    /// Message that can hold any type readable and writeable by <see cref="NetworkReader"/> and <see cref="NetworkWriter"/>.
    /// </summary>
    public class VarArgsNetIdMessasge : MessageBase
    {
        /// <summary>
        /// The NetworkInstanceId of the object that sent this message. Used to find the object within the scene.
        /// </summary>
        public NetworkInstanceId netId;

        /// <summary>
        /// Hashcode of the fully qualified method name of the callback.
        /// </summary>
        public int callbackHash;

        /// <summary>
        /// An argument list for a callback method. This is an array of objects with the same number, order, and type as
        /// the parameters of the method. It will be null if the method has no parameters.
        /// </summary>
        public object[] args;

        public VarArgsNetIdMessasge()
        {
        }

        public VarArgsNetIdMessasge(NetworkInstanceId id, int callbackHashcode, params object[] values)
        {
            netId = id;
            callbackHash = callbackHashcode;
            args = values;
        }

        public override void Deserialize(NetworkReader reader)
        {
            base.Deserialize(reader);

            netId = reader.ReadNetworkId();
            callbackHash = reader.ReadInt32();

            Type[] types;
            if (Registration.ParameterTypes.TryGetValue(callbackHash, out types))
            {
                if (types == null) return;

                var length = types.Length;
                args = new object[length];

                for (int i = 0; i < length; ++i)
                {
                    args[i] = reader.Read(types[i]);
                }
            }
            else
            {
                if (LogFilter.logFatal) { Debug.LogError("Cannot deserialize message: callback hash " + callbackHash + " not found. Perhaps the method is being invoked from the wrong class."); }
            }
        }

        public override void Serialize(NetworkWriter writer)
        {
            base.Serialize(writer);

            writer.Write(netId);
            writer.Write(callbackHash);

            if (args == null) return;
            for (int i = 0; i < args.Length; ++i)
            {
                writer.Write(args[i]);
            }
        }
    }
}