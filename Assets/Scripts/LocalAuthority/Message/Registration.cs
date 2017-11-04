using System;
using System.Collections.Generic;
using System.Reflection;
using UnityEngine;
using UnityEngine.Networking;

namespace LocalAuthority.Message
{
    /// <summary>
    /// Static class for registering message commands.
    /// </summary>
    // TODO: Lots and lots of error reporting for when people use incompatible method signatures (i.e. float vs FloatNetIdMessage, etc).
    public static class Registration
    {
        /// <summary>
        /// Use reflection to register methods marked with the <see cref="MessageCommand"/> attribute.
        /// </summary>
        public static void RegisterCommands(Type classType)
        {
            // If the class has already registered with the server, exit early to avoid expensive reflection operations.
            short msgType;
            if (messageTypes.TryGetValue(classType, out msgType))
            {
                if (Message.HasBeenRegistered(msgType)) return;
            }

            // TODO: What about inheritence? I.e. DoubleSidedCardController : CardController?
            var methods = classType.GetMethods(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);

            foreach (var method in methods)
            {
                var attribute = method.GetCustomAttribute<Message>(true);
                if (attribute != null)
                {
                    var error = HasValidSignature(method);
                    if (error.Length != 0)
                    {
                        if (LogFilter.logFatal) { Debug.LogError(error); }
                        return;
                    }

                    // Store only the first method's message type, because we only need a single value
                    // per class to check Message.HasBeenRegistered().
                    if (!messageTypes.ContainsKey(classType))
                    {
                        messageTypes.Add(classType, attribute.MsgType);
                    }

                    Type messageType = GetValidParameterType(method);
                    var types = new Type[] { messageType, classType };
                    attribute.RegisterMessage(method, types);
                }
            }
        }

        /// <summary>
        /// Run the client-side prediction callback for this message id, if it exists. <see cref="MessageRpc.Predicted"/>
        /// </summary>
        public static void InvokePrediction(short msgType, NetIdMessage msg)
        {
            Action<NetIdMessage> callback;
            if (predictionCallbacks.TryGetValue(msgType, out callback))
            {
                callback(msg);
            }
        }


        #region Private

        // Validation ----------------------------------------------------------

        /// <summary>
        /// Check if the method has a valid signature:
        ///     Zero or one parameters
        ///     Parameter derives from <see cref="NetIdMessage"/>
        /// </summary>
        /// <returns>Empty string if valid signature, error message otherwise.</returns>
        private static string HasValidSignature(MethodInfo method)
        {
            var parameters = method.GetParameters();

            // Should only take 0 or 1 arguments.
            if (parameters.Length > 1)
            {
                return "Cannot register method: " + method + ", because it has more than 1 parameter. It can only take zero or one parameters.";
            }

            // The first parameter must derive from NetIdMessage.
            if (parameters.Length == 1)
            {
                var argType = parameters[0].ParameterType;
                if (!Utility.IsSameOrSubclass(typeof(NetIdMessage), argType))
                {
                    return "Cannot register method: " + method + ", because its first argument does not derive from: " + typeof(NetIdMessage);
                }
            }

            return "";
        }

        /// <summary>
        /// Return the type of the first parameter of the method, or <see cref="NetIdMessage"/> if there are no parameters.
        /// </summary>
        private static Type GetValidParameterType(MethodInfo method)
        {
            var parameters = method.GetParameters();

            return parameters.Length == 0 ? typeof(NetIdMessage) : parameters[0].ParameterType;
        }

        /// <summary>
        /// Store the client-side prediction callback for a specific message id.
        /// </summary>
        internal static void RegisterPrediction(short msgType, Action<NetIdMessage> callback)
        {
            if (!predictionCallbacks.ContainsKey(msgType))
            {
                predictionCallbacks.Add(msgType, callback);
            }
        }


        // Data ----------------------------------------------------------------

        /// <summary>
        /// A cache with one message id per class. Used to prevent uneccessary and expensive registrations.
        /// </summary>
        private static readonly Dictionary<Type, short> messageTypes = new Dictionary<Type, short>();

        /// <summary>
        /// Callbacks for message ids with client-side prediction enabled.
        /// </summary>
        private static Dictionary<short, Action<NetIdMessage>> predictionCallbacks = new Dictionary<short, Action<NetIdMessage>>();

        #endregion
    }
}
