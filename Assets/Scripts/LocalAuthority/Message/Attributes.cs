using System;
using System.Reflection;
using LocalAuthority.Components;
using UnityEngine.Networking;

namespace LocalAuthority.Message
{
    /// <summary>
    /// This is an attribute that can be put on methods of a <see cref="LocalAuthorityBehaviour"/> class to allow them
    /// to be invoked on the server or clients by sending a message from a client.
    /// <para>
    /// [Message] functions may be invoked from any LocalAuthorityBehaviour, even those not attached to the
    /// player GameObject. Invoke a [Message] by using <see cref="LocalAuthorityBehaviour.SendCommand{TMsg}"/>.
    /// </para>
    /// <para>
    /// These functions must accept zero or one parameters. That parameter must be derived from <see cref="NetIdMessage"/>.
    /// </para>
    /// </summary>
    [AttributeUsage((AttributeTargets.Method))]
    public abstract class Message : Attribute
    {
        /// <summary>
        /// A number unique to this callback. <see cref="MsgType"/> and <see cref="UnityEngine.Networking.MsgType"/>
        /// </summary>
        public short MsgType { get; set; }

        /// <summary>
        /// Return a callback that behaves like a <see cref="CommandAttribute"/> or <see cref="ClientRpcAttribute"/>.
        /// </summary>
        public abstract NetworkMessageDelegate GetCallback<TMsg, TComp>(MethodInfo callback)
            where TMsg : NetIdMessage, new()
            where TComp : LocalAuthorityBehaviour;

        public virtual void RegisterMessage(NetworkMessageDelegate callback)
        {
            RegisterWithServer(callback);
        }

        protected void RegisterWithServer(NetworkMessageDelegate callback)
        {
            NetworkServer.RegisterHandler(MsgType, callback);
        }

        protected void RegisterWithClient(NetworkMessageDelegate callback)
        {
            NetworkManager.singleton.client.RegisterHandler(MsgType, callback);
        }
    }

    /// <summary>
    /// The attributed method will behave like a <see cref="CommandAttribute"/>.
    /// </summary>
    [AttributeUsage(AttributeTargets.Method)]
    public class MessageCommand : Message
    {
        public MessageCommand(short msgType)
        {
            MsgType = msgType;
        }

        /// <summary>
        /// Return a callback that behaves like a [Command].
        /// When invoked with <see cref="LocalAuthorityBehaviour.SendCommand{TMsg}"/>, it will run only on the server.
        /// </summary>
        public override NetworkMessageDelegate GetCallback<TMsg, TComp>(MethodInfo callback)
        {
            return netMsg =>
            {
                var msg = netMsg.ReadMessage<TMsg>();
                var obj = LocalAuthorityBehaviour.FindLocalComponent<TComp>(msg.netId);

                if (typeof(TMsg) == typeof(NetIdMessage))
                {
                    // The method takes no arguments. NetIdMessage was only used for locating the object.
                    callback.Invoke(obj, null);
                }
                else
                {
                    callback.Invoke(obj, new object[] { msg });
                }
            };
        }
    }

    /// <summary>
    /// The attributed method will behave like a <see cref="ClientRpcAttribute"/>.
    /// </summary>
    [AttributeUsage(AttributeTargets.Method)]
    public class MessageRpc : Message
    {
        /// <summary>
        /// True if this Rpc should not be invoked on the caller, because you are using client-side prediction.
        /// </summary>
        public bool Predicted { get; set; }

        public MessageRpc(short msgType)
        {
            MsgType = msgType;
        }

        /// <summary>
        /// Return a callback that behaves like a [ClientRpc].
        /// When invoked with <see cref="LocalAuthorityBehaviour.SendCommand{TMsg}"/>, it will run on all clients.
        /// </summary>
        public override NetworkMessageDelegate GetCallback<TMsg, TComp>(MethodInfo callback)
        {
            return netMsg =>
            {
                var msg = netMsg.ReadMessage<TMsg>();
                var obj = LocalAuthorityBehaviour.FindLocalComponent<TComp>(msg.netId);

                Action rpc;
                if (typeof(TMsg) == typeof(NetIdMessage))
                {
                    // The method takes no arguments. NetIdMessage was only used for locating the object.
                    rpc = () => callback.Invoke(obj, null);
                }
                else
                {
                    rpc = () => callback.Invoke(obj, new object[] { msg });
                }

                obj.InvokeMessageRpc(rpc, netMsg, msg, Predicted);
            };
        }

        public override void RegisterMessage(NetworkMessageDelegate callback)
        {
            base.RegisterMessage(callback);
            RegisterWithClient(callback);
        }
    }
}
