using System;
using System.Collections.Generic;
using System.Reflection;
using LocalAuthority.Message;
using UnityEngine.Networking;

namespace LocalAuthority.Components
{
    /// <summary>
    /// Extend this class instead of <see cref="NetworkBehaviour"/> to enable message-based commands.
    /// </summary>
    // TODO: Better class description.
    public abstract class LocalAuthorityBehaviour : NetworkBehaviour
    {
        /// <summary>
        /// Invoke the message-based command on the server, or rpc on all clients.
        /// </summary>
        /// <param name="values">Values to load the message with, besides netId.</param>
        /// <returns>True if the command was sent.</returns>
        protected bool SendCommand<TMsg>(short msgType, params object[] values) where TMsg : NetIdMessage, new()
        {
            var msg = MessageFactory.New<TMsg>(netId, values);

//            Registration.InvokePrediction(msgType, values);
            MethodInfo method;
            if (Registration.RpcsWithPrediction.TryGetValue(msgType, out method))
            {
                var args = typeof(TMsg) == typeof(NetIdMessage) ? null : new object[] { msg };
                method.Invoke(this, args);
            }

            return NetworkManager.singleton.client.Send(msgType, msg);
        }


        #region Private

        /// <summary>
        /// Run the action on all clients (like an RPC), except for the host and optionally the caller.
        /// </summary>
        /// <param name="action">A closure with all arguments filled in. Ex: Action action = () => Foo(bar);</param>
        /// <param name="netMsg">The network message received in the method registered with RegisterCommand().</param>
        /// <param name="msg">The message unpacked with netMsg.ReadMessage().</param>
        /// <param name="ignoreSender">True if the action should NOT be run on the caller (i.e. for client-side prediction).</param>
        internal void InvokeMessageRpc(Action action, NetworkMessage netMsg, MessageBase msg, bool ignoreSender = false)
        {
            if (isServer)
            {
                ForwardMessage(netMsg, msg, ignoreSender);

                if (ignoreSender && NetworkServer.localConnections.Contains(netMsg.conn)) return;
            }

            action();
        }

        /// <summary>
        /// Forward a message to all clients, except for the host and optionally omitting the caller.
        /// </summary>
        /// <param name="netMsg">The network message received in the method registered with RegisterCommand().</param>
        /// <param name="msg">The message unpacked with netMsg.ReadMessage().</param>
        /// <param name="ignoreSender">True if the action should NOT be run on the caller (i.e. for client-side prediction).</param>
        private void ForwardMessage(NetworkMessage netMsg, MessageBase msg, bool ignoreSender = false)
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

        protected virtual void Awake()
        {
            var classType = GetType();
            Registration.RegisterCommands(classType);
        }

        #endregion
    }
}
