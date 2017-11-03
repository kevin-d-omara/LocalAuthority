﻿using System;
using System.Collections.Generic;
using LocalAuthority.Message;
using UnityEngine;
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
            Registration.InvokePrediction(msgType, msg);
            return NetworkManager.singleton.client.Send(msgType, msg);
        }

        /// <summary>
        /// Find a component of type T attached to a game object with the given network id.
        /// </summary>
        /// <typeparam name="T">Type of the component to find.</typeparam>
        /// <param name="netId">The netId of the networked object.</param>
        /// <returns>The component attached to the game object with matching netId, or default(T) if the object or
        /// component are not found.</returns>
        public static T FindLocalComponent<T>(NetworkInstanceId netId)
        {
            var foundObject = ClientScene.FindLocalObject(netId);
            if (foundObject == null)
            {
                if (LogFilter.logError) { Debug.LogError("No GameObject exists for the given NetworkInstanceId: " + netId); }
                return default(T);
            }

            var foundComponent = foundObject.GetComponent<T>();
            if (foundComponent == null)
            {
                if (LogFilter.logError) { Debug.LogError("The GameObject " + foundObject + " does not have a " + typeof(T) + " component attached."); }
                return default(T);
            }

            return foundComponent;
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
