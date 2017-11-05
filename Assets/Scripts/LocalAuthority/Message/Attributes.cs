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
    /// player GameObject. Invoke a [Message] by using <see cref="LocalAuthorityBehaviour.SendCommand"/>.
    /// </para>
    /// </summary>
    [AttributeUsage(AttributeTargets.Method)]
    public abstract class Message : Attribute
    {
        /// <summary>
        /// A number unique to this callback. <see cref="MsgType"/> and <see cref="UnityEngine.Networking.MsgType"/>
        /// </summary>
        public short MsgType { get; set; }


        // TODO: documentation
        public void RegisterMessage(MethodInfo method, Type classType)
        {
            // Same number, order, and type as parameters to RegisterMessage<TMsg, TComp>().
            var args = new object[] { method };

            // Call RegisterMessage<TComp> with correct generic type.
            var registerMessage = RegisterMessageInfo.MakeGenericMethod(classType);
            registerMessage.Invoke(this, args);
        }

        /// <summary>
        /// Register a message-based command on the server and clients.
        /// <para>
        /// Registering on the server enables the method to be called on the server, like a [Command].
        /// Registering on the client enables the method to be called on all clients, like a [ClientRpc].
        /// </para>
        /// </summary>
        /// <typeparam name="TComp">Type of component where the method code is written.</typeparam>
        /// <param name="method">The function to register.</param>
        public virtual void RegisterMessage<TComp>(MethodInfo method) where TComp : LocalAuthorityBehaviour
        {
            var callback = GetCallback<TComp>(method);
            RegisterWithServer(callback);
            RegisterWithClient(callback);
        }


        /// <summary>
        /// Return a callback that behaves like a <see cref="CommandAttribute"/> or <see cref="ClientRpcAttribute"/>.
        /// </summary>
        protected abstract NetworkMessageDelegate GetCallback<TComp>(MethodInfo callback)
            where TComp : LocalAuthorityBehaviour;

        /// <summary>
        /// Return true if the specified message id has already been registered to a callback on the server or client.
        /// </summary>
        public static bool HasBeenRegistered(short msgType)
        {
            return NetworkManager.singleton.client.handlers.ContainsKey(msgType);
        }

        /// <summary>
        /// Register the callback so that it may be invoked on the server from a client.
        /// </summary>
        protected void RegisterWithServer(NetworkMessageDelegate callback)
        {
            NetworkServer.RegisterHandler(MsgType, callback);
        }

        /// <summary>
        /// Register the callback so that it may be invoked on a client from the server.
        /// </summary>
        protected void RegisterWithClient(NetworkMessageDelegate callback)
        {
            NetworkManager.singleton.client.RegisterHandler(MsgType, callback);
        }


        // Initialization ------------------------------------------------------

        /// <summary>
        /// Cached MethodInfo for <see cref="RegisterMessage{TMsg,TComp}"/>.
        /// </summary>
        private static MethodInfo RegisterMessageInfo { get; }

        static Message()
        {
            var parameterTypes = new Type[] { typeof(MethodInfo) };
            var flags = BindingFlags.Instance | BindingFlags.Public;
            var info = typeof(Message).GetMethod(nameof(RegisterMessage), flags, null, parameterTypes, null);
            RegisterMessageInfo = info;
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
        /// When invoked with <see cref="LocalAuthorityBehaviour.SendCommand"/>, it will run only on the server.
        /// </summary>
        protected override NetworkMessageDelegate GetCallback<TComp>(MethodInfo callback)
        {
            return netMsg =>
            {
                var msg = new VarArgsNetIdMessasge();
                msg.msgType = netMsg.msgType;
                msg.Deserialize(netMsg.reader);

                var obj = Utility.FindLocalComponent<TComp>(msg.netId);

                callback.Invoke(obj, msg.args);
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
        /// True if the Rpc should be run immediately on the caller for client-side prediction.
        /// </summary>
        public bool ClientSidePrediction { get; set; }

        public MessageRpc(short msgType)
        {
            MsgType = msgType;
        }

        /// <summary>
        /// Return a callback that behaves like a [ClientRpc].
        /// When invoked with <see cref="LocalAuthorityBehaviour.SendCommand"/>, it will run on all clients.
        /// </summary>
        protected override NetworkMessageDelegate GetCallback<TComp>(MethodInfo callback)
        {
            return netMsg =>
            {
                var msg = new VarArgsNetIdMessasge();
                msg.msgType = netMsg.msgType;
                msg.Deserialize(netMsg.reader);

                var obj = Utility.FindLocalComponent<TComp>(msg.netId);

                Action rpc = () => callback.Invoke(obj, msg.args);
                obj.InvokeMessageRpc(rpc, netMsg, msg, ClientSidePrediction);
            };
        }

        public override void RegisterMessage<TComp>(MethodInfo method)
        {
            base.RegisterMessage<TComp>(method);

            if (ClientSidePrediction)
            {
                Registration.RegisterPredictedRpc(MsgType, method);
            }
        }
    }
}
