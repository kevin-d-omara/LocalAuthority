using System.Reflection;
using UnityEngine;
using UnityEngine.Networking;

namespace LocalAuthority.Components
{
    /// <summary>
    /// Extend this class instead of <see cref="NetworkBehaviour"/> to enable message-based commands and rpcs. Place the
    /// attributes <see cref="MessageCommand"/> and <see cref="MessageRpc"/> above methods and then Invoke them with
    /// <see cref="InvokeCommand"/> and <see cref="InvokeRpc"/>. <seealso cref="MessageBasedCallback"/>
    /// </summary>
    public abstract class LocalAuthorityBehaviour : NetworkBehaviour
    {
        /// <summary>
        /// A unique number to identify messages sent through LocalAuthority.
        /// </summary>
        public const short MessageCallback = MsgType.Highest + 1;

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
            var hash = Registration.GetCallbackHashcode(GetType(), methodName);
            var msg = new VarArgsNetIdMessasge(netId, hash, values);

            // Execute immediately if client-side prediction is enabled.
            MethodInfo method;
            if (Registration.ClientSidePrediction.TryGetValue(hash, out method))
            {
                method.Invoke(this, msg.args);
            }

            return NetworkManager.singleton.client.Send(MessageCallback, msg);
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
            NetworkServer.RegisterHandler(MessageCallback, RedirectCallback);
            NetworkManager.singleton.client.RegisterHandler(MessageCallback, RedirectCallback);
        }

        /// <summary>
        /// Pitstop for all Command/Rpc messages sent through LocalAuthorityBehaviour.
        /// </summary>
        private static void RedirectCallback(NetworkMessage netMsg)
        {
            var msg = netMsg.ReadMessage<VarArgsNetIdMessasge>();

            MessageCallback callback;
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
