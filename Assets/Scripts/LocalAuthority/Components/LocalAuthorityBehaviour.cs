using System.Reflection;
using LocalAuthority.Message;
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
            var msgType = Registration.MsgTypes[methodName]; // TODO: Same method name, different class?
            var msg = new VarArgsNetIdMessasge(netId, values);

            // Execute immediately if client-side prediction is enabled.
            MethodInfo method;
            if (Registration.ClientSidePrediction.TryGetValue(msgType, out method))
            {
                method.Invoke(this, msg.args);
            }

            return NetworkManager.singleton.client.Send(msgType, msg);
        }

        protected virtual void Awake()
        {
            var classType = GetType();
            Registration.RegisterCommands(classType);
        }
    }
}
