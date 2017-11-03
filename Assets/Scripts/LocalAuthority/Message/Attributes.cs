﻿using System;
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
    [AttributeUsage(AttributeTargets.Method)]
    public abstract class Message : Attribute
    {
        /// <summary>
        /// A number unique to this callback. <see cref="MsgType"/> and <see cref="UnityEngine.Networking.MsgType"/>
        /// </summary>
        public short MsgType { get; set; }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="method"></param>
        /// <param name="types"></param>
        public void RegisterMessage(MethodInfo method, Type[] types)
        {
            // Same number, order, and type as parameters to RegisterMessage<TMsg, TComp>().
            var args = new object[] { method };

            var registerMessage = RegisterMessageInfo.MakeGenericMethod(types);
            registerMessage.Invoke(this, args);
        }

        /// <summary>
        /// Register a message-based command on the server and clients.
        /// <para>
        /// Registering on the server enables the method to be called on the server, like a [Command].
        /// Registering on the client enables the method to be called on all clients, like a [ClientRpc].
        /// </para>
        /// </summary>
        /// <typeparam name="TMsg">Type of network message that the method takes as its only parameter.</typeparam>
        /// <typeparam name="TComp">Type of component where the method code is written.</typeparam>
        /// <param name="method">The function to register.</param>
        public abstract void RegisterMessage<TMsg, TComp>(MethodInfo method) where TMsg : NetIdMessage, new()
                                                                             where TComp : LocalAuthorityBehaviour;


        /// <summary>
        /// Return a callback that behaves like a <see cref="CommandAttribute"/> or <see cref="ClientRpcAttribute"/>.
        /// </summary>
        protected abstract NetworkMessageDelegate GetCallback<TMsg, TComp>(MethodInfo callback)
            where TMsg : NetIdMessage, new()
            where TComp : LocalAuthorityBehaviour;

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
        /// When invoked with <see cref="LocalAuthorityBehaviour.SendCommand{TMsg}"/>, it will run only on the server.
        /// </summary>
        protected override NetworkMessageDelegate GetCallback<TMsg, TComp>(MethodInfo callback)
        {
            return netMsg =>
            {
                var msg = netMsg.ReadMessage<TMsg>();
                var obj = Utility.FindLocalComponent<TComp>(msg.netId);

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

        public override void RegisterMessage<TMsg, TComp>(MethodInfo method)
        {
            var callback = GetCallback<TMsg, TComp>(method);
            RegisterWithServer(callback);
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
        public bool Predicted { get; set; }

        public MessageRpc(short msgType)
        {
            MsgType = msgType;
        }

        /// <summary>
        /// Return a callback that behaves like a [ClientRpc].
        /// When invoked with <see cref="LocalAuthorityBehaviour.SendCommand{TMsg}"/>, it will run on all clients.
        /// </summary>
        protected override NetworkMessageDelegate GetCallback<TMsg, TComp>(MethodInfo callback)
        {
            return netMsg =>
            {
                var msg = netMsg.ReadMessage<TMsg>();
                var obj = Utility.FindLocalComponent<TComp>(msg.netId);

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

        /// <summary>
        /// Return a callback for the callback.
        /// This is used to run the callback immediately on the caller for client-side prediction.
        /// </summary>
        private Action<NetIdMessage> GetPredictedCallback<TMsg, TComp>(MethodInfo callback)
            where TMsg : NetIdMessage, new()
            where TComp : LocalAuthorityBehaviour
        {
            return netIdMsg =>
            {
                var msg = (TMsg)netIdMsg;
                var obj = Utility.FindLocalComponent<TComp>(msg.netId);

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

        public override void RegisterMessage<TMsg, TComp>(MethodInfo method)
        {
            var callback = GetCallback<TMsg, TComp>(method);
            RegisterWithServer(callback);
            RegisterWithClient(callback);

            if (Predicted)
            {
                var predictionCallback = GetPredictedCallback<TMsg, TComp>(method);
                Registration.RegisterPrediction(MsgType, predictionCallback);
            }
        }
    }
}
