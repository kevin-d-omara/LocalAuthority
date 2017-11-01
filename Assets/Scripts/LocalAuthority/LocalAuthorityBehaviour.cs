using System;
using System.Collections.Generic;
using LocalAuthority.Message;
using UnityEngine.Networking;

namespace LocalAuthority
{
    /// <summary>
    /// Extend this class instead of <see cref="NetworkBehaviour"/> to enable message-based commands.
    /// </summary>
    // TODO: Better class description.
    public abstract class LocalAuthorityBehaviour : NetworkBehaviour
    {
        /// <summary>
        /// Run a message-based command on the server.
        /// </summary>
        /// <param name="values">Values to load the message with, besides netId</param>
        /// <returns>True if the command was sent.</returns>
        protected bool SendCommand<TMsg>(short msgType, params object[] values) where TMsg : NetIdMessage, new()
        {
            var msg = new TMsg();
            msg.netId = netId;
            msg.VarargsSetter(values);
            return NetworkManager.singleton.client.Send(msgType, msg);
        }

        /// <summary>
        /// Run the action on all clients (like an RPC), except for the host and optionally the caller.
        /// </summary>
        /// <param name="action">Closure holding a function and it's arguments.</param>
        /// <param name="netMsg">The network message received in the method registered with RegisterCallback().</param>
        /// <param name="msg">The message unpacked with netMsg.ReadMessage().</param>
        /// <param name="ignoreSender">True if the action should NOT be run on the caller (i.e. for client-side prediction).</param>
        protected void RunNetworkAction(Action action, NetworkMessage netMsg, MessageBase msg, bool ignoreSender = true)
        {
            if (isServer)
            {
                ForwardMessage(netMsg, msg, ignoreSender);

                if (ignoreSender && NetworkServer.localConnections.Contains(netMsg.conn)) return;
            }

            action();
        }


        protected virtual void Awake()
        {
            RegisterCallbacks();
        }

        /// <summary>
        /// Fill this with calls to:
        ///     RegisterCallback(CustomMessageType, SomeCallback, registerClient = true/false);
        /// </summary>
        protected abstract void RegisterCallbacks();

        /// <summary>
        /// Register a message-based command on the server and optionally on the client.
        /// <para>
        /// Registering on the server enables <see cref="SendCommand"/> to reach the server, like a [Command].
        /// Registering on the client enables the <paramref name="callback"/> to reach the clients, like a [ClientRpc].
        /// </para>
        /// </summary>
        /// <param name="msgType">A number unique to this callback. <see cref="Message.MsgType"/></param>
        /// <param name="callback">The function containing server code, like a [Command].</param>
        /// <param name="registerClient">True if the client should be able to receive the callback, like a [ClientRpc].</param>
        protected void RegisterCallback(short msgType, NetworkMessageDelegate callback, bool registerClient = false)
        {
            NetworkServer.RegisterHandler(msgType, callback);

            if (registerClient)
            {
                NetworkManager.singleton.client.RegisterHandler(msgType, callback);
            }
        }


        /// <summary>
        /// Forward a message to all clients, except for the host and optionally omitting the caller.
        /// </summary>
        /// <param name="netMsg">The network message received in the method registered with RegisterCallback().</param>
        /// <param name="msg">The message unpacked with netMsg.ReadMessage().</param>
        /// <param name="ignoreSender">True if the action should NOT be run on the caller (i.e. for client-side prediction).</param>
        private void ForwardMessage(NetworkMessage netMsg, MessageBase msg, bool ignoreSender = true)
        {
            // TODO: Does this actually work for couch coop?
            var ignoreList = new List<NetworkConnection>(NetworkServer.localConnections);
            if (ignoreSender) ignoreList.Add(netMsg.conn);

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
