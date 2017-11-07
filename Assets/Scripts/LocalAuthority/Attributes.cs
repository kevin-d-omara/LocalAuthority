using System;
using System.Collections.Generic;
using System.Reflection;
using LocalAuthority.Components;
using UnityEngine.Networking;

namespace LocalAuthority
{
    /// <summary>
    /// The callback delegate used for message-based command/rpc methods.
    /// </summary>
    public delegate void MessageCallback(NetworkMessage netMsg, VarArgsNetIdMessasge msg);

    /// <summary>
    /// This is an attribute that can be put on methods of a <see cref="LocalAuthorityBehaviour"/> class to allow them
    /// to be invoked on the server or clients by sending a message from a client.
    /// <para>
    /// [MessageBasedCallback] methods may be invoked from any LocalAuthorityBehaviour, even those not attached to the
    /// player GameObject. Invoke with <see cref="LocalAuthorityBehaviour.InvokeCommand"/> or <see cref="LocalAuthorityBehaviour.SendRpc"/>.
    /// </para>
    /// <para>
    /// Client-side prediction may be enabled by adding ClientSidePrediction = true in the attribute constructor.
    /// If enabled, the attributed method will be run immediately on the caller and the caller will not be "called back" by the server.
    /// </para>
    /// </summary>
    [AttributeUsage(AttributeTargets.Method)]
    public abstract class MessageBasedCallback : Attribute
    {
        /// <summary>
        /// True if client-side prediction is enabled. The attributed method will be run immediately on the caller.
        /// </summary>
        public bool ClientSidePrediction { get; set; }

        /// <summary>
        /// Return a callback that behaves like a <see cref="CommandAttribute"/> or <see cref="ClientRpcAttribute"/>.
        /// </summary>
        /// <param name="method">The method to create a callback for.</param>
        /// <param name="classType">The class where the method is defined.</param>
        public MessageCallback GetCallback(MethodInfo method, Type classType)
        {
            // Same number, order, and type as parameters to GetCallback<TMsg, TComp>().
            var args = new object[] { method };

            // Call GetCallback<TComp> with correct generic type.
            var registerMessage = CachedMethodInfo.MakeGenericMethod(classType);
            return (MessageCallback) registerMessage.Invoke(this, args);
        }

        protected abstract MessageCallback GetCallback<TComp>(MethodInfo callback)
            where TComp : LocalAuthorityBehaviour;


        // Initialization ------------------------------------------------------

        /// <summary>
        /// Cached MethodInfo for <see cref="GetCallback{TComp}"/>.
        /// </summary>
        private static MethodInfo CachedMethodInfo { get; }

        static MessageBasedCallback()
        {
            // Cache method info for GetCallback<TComp>().
            var parameterTypes = new Type[] { typeof(MethodInfo) };
            var flags = BindingFlags.Instance | BindingFlags.NonPublic;
            var info = typeof(MessageBasedCallback).GetMethod(nameof(GetCallback), flags, null, parameterTypes, null);
            CachedMethodInfo = info;
        }
    }

    /// <summary>
    /// The attributed method will behave like a <see cref="CommandAttribute"/>.
    /// When invoked with <see cref="LocalAuthorityBehaviour.InvokeCommand"/>, it will run only on the server.
    /// </summary>
    [AttributeUsage(AttributeTargets.Method)]
    public class MessageCommand : MessageBasedCallback
    {
        protected override MessageCallback GetCallback<TComp>(MethodInfo callback)
        {
            return (netMsg, msg) =>
            {
                var obj = LocalAuthorityBehaviour.FindLocalComponent<TComp>(msg.netId);

                if (ClientSidePrediction && obj.isServer && NetworkServer.localConnections.Contains(netMsg.conn))
                {
                    return;
                }

                callback.Invoke(obj, msg.args);
            };
        }
    }

    /// <summary>
    /// The attributed method will behave like a <see cref="ClientRpcAttribute"/>.
    /// When invoked with <see cref="LocalAuthorityBehaviour.SendRpc"/>, it will run on all clients.
    /// </summary>
    [AttributeUsage(AttributeTargets.Method)]
    public class MessageRpc : MessageBasedCallback
    {
        protected override MessageCallback GetCallback<TComp>(MethodInfo callback)
        {
            return (netMsg, msg) =>
            {
                var obj = LocalAuthorityBehaviour.FindLocalComponent<TComp>(msg.netId);

                if (obj.isServer)
                {
                    ForwardMessage(netMsg, msg);

                    if (ClientSidePrediction && NetworkServer.localConnections.Contains(netMsg.conn)) return;
                }

                callback.Invoke(obj, msg.args);
            };
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
