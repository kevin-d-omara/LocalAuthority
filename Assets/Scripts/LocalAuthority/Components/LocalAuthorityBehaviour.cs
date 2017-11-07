using System.Reflection;
using UnityEngine;
using UnityEngine.Networking;

namespace LocalAuthority.Components
{
    /// <summary>
    /// Extend this class instead of <see cref="NetworkBehaviour"/> to enable message-based commands and rpcs. Place the
    /// attributes <see cref="MessageCommand"/> and <see cref="MessageRpc"/> above methods and then Invoke them with
    /// <see cref="SendCallback"/>. <seealso cref="MessageBasedCallback"/>
    /// </summary>
    public abstract class LocalAuthorityBehaviour : NetworkBehaviour
    {
        /// <summary>
        /// A unique number to identify messages sent through LocalAuthority.
        /// </summary>
        public const short MessageCallback = MsgType.Highest + 1;

        /// <summary>
        /// Invoke a <see cref="MessageCommand"/> or <see cref="MessageRpc"/>.
        /// Commands are run on the server; Rpcs are run on all clients.
        /// </summary>
        /// <param name="methodName">Name of a method marked with a <see cref="MessageBasedCallback"/> attribute. Use <c>nameof(MyCallback)</c>.</param>
        /// <param name="values">An argument list for the method. Contains objects with the same number, order, and type
        /// as the parameters of the method. Omit this if the method has no parameters.</param>
        /// <returns>True if the callback was sent.</returns>
        protected bool SendCallback(string methodName, params object[] values)
        {
            var hash = Registration.GetCallbackHashcode(GetType(), methodName);
            var msg = new VarArgsNetIdMessasge(netId, hash, values);

            // Execute immediately if client-side prediction is enabled.
            MethodInfo method;
            if (Registration.PredictedCallbacks.TryGetValue(hash, out method))
            {
                method.Invoke(this, msg.args);
            }

            return NetworkManager.singleton.client.Send(MessageCallback, msg);
        }

        /// <summary>
        /// Find a component of type <c>TComp</c> attached to a game object with the given network id.
        /// </summary>
        /// <returns>
        /// The component attached to the game object with matching netId.
        /// Throws a fatal error and returns default(T) if the object or component are not found.
        /// </returns>
        public static TComp FindLocalComponent<TComp>(NetworkInstanceId netId)
        {
            var foundObject = ClientScene.FindLocalObject(netId);
            if (foundObject == null)
            {
                if (LogFilter.logError) { Debug.LogError("No GameObject exists for the given NetworkInstanceId: " + netId); }
                return default(TComp);
            }

            var foundComponent = foundObject.GetComponent<TComp>();
            if (foundComponent == null)
            {
                if (LogFilter.logError) { Debug.LogError("The GameObject " + foundObject + " does not have a " + typeof(TComp) + " component attached."); }
                return default(TComp);
            }

            return foundComponent;
        }

        protected virtual void Awake()
        {
            RegisterMessageHandler();
            var classType = GetType();
            Registration.RegisterCommands(classType);
        }

        /// <summary>
        /// Register the primary <see cref="ExecuteCallback"/> message handler on the server and client.
        /// This only needs to be done once, per player, per game.
        /// </summary>
        private void RegisterMessageHandler()
        {
            NetworkServer.RegisterHandler(MessageCallback, ExecuteCallback);
            NetworkManager.singleton.client.RegisterHandler(MessageCallback, ExecuteCallback);
        }

        /// <summary>
        /// Receiving end for all Command/Rpc messages sent with <see cref="SendCallback"/>.
        /// </summary>
        private static void ExecuteCallback(NetworkMessage netMsg)
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
