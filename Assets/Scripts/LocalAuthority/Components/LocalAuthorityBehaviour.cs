using System;
using System.Reflection;
using LocalAuthority.Message;
using UnityEngine;
using UnityEngine.Networking;

namespace LocalAuthority.Components
{
    /// <summary>
    /// Extend this class instead of <see cref="NetworkBehaviour"/> to enable message-based commands and rpcs. Place the
    /// attributes <see cref="MessageCommand"/> and <see cref="MessageRpc"/> above methods. Invoke these with
    /// <see cref="InvokeCommand"/> and <see cref="InvokeRpc"/>
    /// </summary>
    public abstract class LocalAuthorityBehaviour : NetworkBehaviour
    {
        /// <summary>
        /// A unique number to identify messages sent through LocalAuthority.
        /// </summary>
        public const short Callback = MsgType.Highest + 1;

        /// <summary>
        /// Invoke a message-based Command on the server.
        /// </summary>
        /// <param name="methodName">Use nameof(Command) on a method tagged with the <see cref="MessageCommand"/> attribute.</param>
        /// <param name="values">An argument list for the method. This is an array of objects with the same number,
        /// order, and type as the parameters of the method. Omit this if the method has no parameters.</param>
        /// <returns>True if the command was sent.</returns>
        protected bool InvokeCommand(string methodName, params object[] values)
        {
            return InvokeCommandOrRpc(methodName, values);
        }

        /// <summary>
        /// Invoke a message-based Rpc on all clients.
        /// </summary>
        /// <param name="methodName">Use nameof(Rpc) on a method tagged with the <see cref="MessageRpc"/> attribute.</param>
        /// <param name="values">An argument list for the method. This is an array of objects with the same number,
        /// order, and type as the parameters of the method. Omit this if the method has no parameters.</param>
        /// <returns>True if the rpc was sent.</returns>
        protected bool InvokeRpc(string methodName, params object[] values)
        {
            return InvokeCommandOrRpc(methodName, values);
        }

        /// <summary>
        /// Invoke the message-based command or rpc on the server or clients.
        /// <remarks>
        /// Commands and Rpcs are treated the same in LocalAuthority. The difference is stored in the lambda function
        /// captured during registration.
        /// </remarks>
        /// </summary>
        private bool InvokeCommandOrRpc(string methodName, params object[] values)
        {
            var callbackHash = Utility.GetCallbackHashcode(GetType(), methodName);
            var msg = new VarArgsNetIdMessasge(netId, callbackHash, values);

            // Execute immediately if client-side prediction is enabled.
            MethodInfo method;
            if (Registration.ClientSidePrediction.TryGetValue(callbackHash, out method))
            {
                method.Invoke(this, msg.args);
            }

            return NetworkManager.singleton.client.Send(Callback, msg);
        }

        protected virtual void Awake()
        {
            // Placing this here guarantees the message handler is always registered.
            RegisterMessageHandler();

            var classType = GetType();
            Registration.RegisterCommands(classType);
        }

        /// <summary>
        /// Register the primary <see cref="RedirectCallback"/> message handler on the server and client.
        /// </summary>
        private void RegisterMessageHandler()
        {
            NetworkServer.RegisterHandler(Callback, RedirectCallback);
            NetworkManager.singleton.client.RegisterHandler(Callback, RedirectCallback);
        }

        /// <summary>
        /// Pitstop for all Command/Rpc messages sent through LocalAuthorityBehaviour.
        /// </summary>
        private static void RedirectCallback(NetworkMessage netMsg)
        {
            var msg = netMsg.ReadMessage<VarArgsNetIdMessasge>();

            Action<NetworkMessage, VarArgsNetIdMessasge> callback;
            if (Registration.Callbacks.TryGetValue(msg.callbackHash, out callback))
            {
                callback(netMsg, msg);
            }
            else
            {
                if (LogFilter.logFatal) { Debug.LogError("No callback registered for callback hash: " + msg.callbackHash); }
            }
        }
    }
}
