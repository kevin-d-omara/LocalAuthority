using System;
using System.Collections.Generic;
using System.Reflection;
using LocalAuthority.Components;
using UnityEngine.Networking;

namespace LocalAuthority
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
        /// True if client-side prediction is enabled. The attributed method will be run immediately on the caller.
        /// </summary>
        public bool ClientSidePrediction { get; set; }


        // Methods -------------------------------------------------------------

        /// <summary>
        /// Register a message-based callback on the server and clients.
        /// </summary>
        /// <param name="classType">Type of script where the method code is written.</param>
        // TODO: simplify return type.
        public Action<NetworkMessage, VarArgsNetIdMessasge> GetCallback(MethodInfo method, Type classType)
        {
            // Same number, order, and type as parameters to GetCallback2<TMsg, TComp>().
            var args = new object[] { method };

            // Call GetCallback2<TComp> with correct generic type.
            var registerMessage = CachedInfo.MakeGenericMethod(classType);
            return (Action<NetworkMessage, VarArgsNetIdMessasge>) registerMessage.Invoke(this, args);
        }

        /// <summary>
        /// Return a callback that behaves like a <see cref="CommandAttribute"/> or <see cref="ClientRpcAttribute"/>.
        /// </summary>
        protected abstract Action<NetworkMessage, VarArgsNetIdMessasge> GetCallback<TComp>(MethodInfo callback)
            where TComp : LocalAuthorityBehaviour;


        // Initialization ------------------------------------------------------

        /// <summary>
        /// Cached MethodInfo for <see cref="GetCallback{TComp}"/>.
        /// </summary>
        private static MethodInfo CachedInfo { get; }

        static Message()
        {
            var parameterTypes = new Type[] { typeof(MethodInfo) };
            var flags = BindingFlags.Instance | BindingFlags.NonPublic;
            var info = typeof(Message).GetMethod(nameof(GetCallback), flags, null, parameterTypes, null);
            CachedInfo = info;
        }
    }



    /// <summary>
    /// The attributed method will behave like a <see cref="CommandAttribute"/>.
    /// Invoke with <see cref="LocalAuthorityBehaviour.InvokeCommand"/>.
    /// </summary>
    [AttributeUsage(AttributeTargets.Method)]
    public class MessageCommand : Message
    {
        /// <summary>
        /// Return a callback that behaves like a [Command].
        /// When invoked with <see cref="LocalAuthorityBehaviour.InvokeCommand"/>, it will run only on the server.
        /// </summary>
        protected override Action<NetworkMessage, VarArgsNetIdMessasge> GetCallback<TComp>(MethodInfo callback)
        {
            return (netMsg, msg) =>
            {
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
        /// <summary>
        /// Return a callback that behaves like a [ClientRpc].
        /// When invoked with <see cref="LocalAuthorityBehaviour.InvokeRpc"/>, it will run on all clients.
        /// </summary>
        protected override Action<NetworkMessage, VarArgsNetIdMessasge> GetCallback<TComp>(MethodInfo callback)
        {
            return (netMsg, msg) =>
            {
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
