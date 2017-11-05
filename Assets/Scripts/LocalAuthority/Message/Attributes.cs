using System;
using System.Collections.Generic;
using System.Reflection;
using LocalAuthority.Components;
using UnityEngine;
using UnityEngine.Networking;

namespace LocalAuthority.Message
{
    /// <summary>
    /// This is an attribute that can be put on methods of a <see cref="LocalAuthorityBehaviour"/> class to allow them
    /// to be invoked on the server or clients by sending a message from a client.
    /// <para>
    /// [Message] functions may be invoked from any LocalAuthorityBehaviour, even those not attached to the
    /// player GameObject. Invoke with <see cref="LocalAuthorityBehaviour.InvokeCommand"/> or <see cref="LocalAuthorityBehaviour.InvokeRpc"/>.
    /// </para>
    /// <para>
    /// Client-side prediction is supported and may be enabled by adding ClientSidePrediction = true in the attribute constructor.
    /// </para>
    /// </summary>
    [AttributeUsage(AttributeTargets.Method)]
    public abstract class Message : Attribute
    {
        // Data ----------------------------------------------------------------

        /// <summary>
        /// A number unique to this callback. <see cref="MsgType"/> and <see cref="UnityEngine.Networking.MsgType"/>
        /// </summary>
        public short MsgType { get; set; }

        /// <summary>
        /// True if client-side prediction is enabled. The attributed method will be run immediately on the caller.
        /// </summary>
        public bool ClientSidePrediction { get; set; }


        // Methods -------------------------------------------------------------

        /// <summary>
        /// Register a message-based callback on the server and clients.
        /// </summary>
        /// <param name="classType">Type of script where the method code is written.</param>
        public void RegisterMessage(MethodInfo method, Type classType)
        {
            if (!Utility.IsSameOrSubclass(typeof(LocalAuthorityBehaviour), classType))
            {
                if (LogFilter.logFatal) { Debug.LogError("Cannot register method " + method + ". Containing clas " + classType + " must inherit from " + typeof(LocalAuthorityBehaviour)); }
                return;
            }

            // Same number, order, and type as parameters to RegisterMessage<TMsg, TComp>().
            var args = new object[] { method };

            // Call RegisterMessage<TComp> with correct generic type.
            var registerMessage = RegisterMessageInfo.MakeGenericMethod(classType);
            registerMessage.Invoke(this, args);
        }

        private void RegisterMessage<TComp>(MethodInfo method) where TComp : LocalAuthorityBehaviour
        {
            var callback = GetCallback<TComp>(method);
            RegisterWithServer(callback);
            RegisterWithClient(callback);

            if (ClientSidePrediction)
            {
                Registration.RegisterPredictedRpc(MsgType, method);
            }
        }

        /// <summary>
        /// Return a callback that behaves like a <see cref="CommandAttribute"/> or <see cref="ClientRpcAttribute"/>.
        /// </summary>
        protected abstract NetworkMessageDelegate GetCallback<TComp>(MethodInfo callback)
            where TComp : LocalAuthorityBehaviour;

        /// <summary>
        /// Return true if the specified message id has already been registered to a callback on the server or client.
        /// <remarks>
        /// Each time a player leaves a networked match, their NetworkServer and NetworKClient handlers get erased.
        /// </remarks>
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
        /// Cached MethodInfo for <see cref="RegisterMessage{TComp}"/>.
        /// </summary>
        private static MethodInfo RegisterMessageInfo { get; }

        static Message()
        {
            var parameterTypes = new Type[] { typeof(MethodInfo) };
            var flags = BindingFlags.Instance | BindingFlags.NonPublic;
            var info = typeof(Message).GetMethod(nameof(RegisterMessage), flags, null, parameterTypes, null);
            RegisterMessageInfo = info;
        }
    }



    /// <summary>
    /// The attributed method will behave like a <see cref="CommandAttribute"/>.
    /// Invoke with <see cref="LocalAuthorityBehaviour.InvokeCommand"/>.
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
    /// Invoke with <see cref="LocalAuthorityBehaviour.InvokeRpc"/>.
    /// </summary>
    [AttributeUsage(AttributeTargets.Method)]
    public class MessageRpc : Message
    {
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
                InvokeRpcOnClients(obj, rpc, netMsg, msg);
            };
        }

        /// <summary>
        /// Run the action on all clients except for the host and optionally the caller.
        /// </summary>
        /// <param name="action">The action to run on the clients.</param>
        /// <param name="netMsg">The network message passed in to the registered callback.</param>
        /// <param name="msg">The message to forward.</param>
        /// <param name="clientSidePrediction">True if the action has already been run on the caller.</param>
        private void InvokeRpcOnClients(LocalAuthorityBehaviour obj, Action action, NetworkMessage netMsg, MessageBase msg)
        {
            if (obj.isServer)
            {
                ForwardMessage(netMsg, msg);

                if (ClientSidePrediction && NetworkServer.localConnections.Contains(netMsg.conn)) return;
            }

            action();
        }

        /// <summary>
        /// Forward a message to all clients, except for the host and optionally the caller.
        /// </summary>
        private void ForwardMessage(NetworkMessage netMsg, MessageBase msg)
        {
            // TODO: Does this actually work for couch coop?
            var ignoreList = new List<NetworkConnection>(NetworkServer.localConnections);
            if (ClientSidePrediction) ignoreList.Add(netMsg.conn);

            foreach (var conn in NetworkServer.connections)
            {
                if (!ignoreList.Contains(conn))
                {
                    conn.Send(netMsg.msgType, msg);
                }
            }
        }
    }
}
