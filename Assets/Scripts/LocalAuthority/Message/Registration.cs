using System;
using System.Collections.Generic;
using System.Reflection;
using TabletopCardCompanion.Debug;
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
            if (hasRegistered.Contains(classType))
            {
                return;
            }
            hasRegistered.Add(classType);

            DebugStreamer.AddMessage("Registering: " + classType);


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

                    Type messageType = GetValidParameterType(method);
                    var types = new Type[] { messageType, classType };
                    attribute.RegisterMessage(method, types);
                }
            }
        }

        /// <summary>
        /// Call this each time the client joins a new game to clear stale connection data.
        /// </summary>
        public static void ClearCache()
        {
            DebugStreamer.AddMessage("Clearing cache.");
            hasRegistered.Clear();
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
                // TODO: See that method.Name is better than method.
                return "Cannot register method: " + method.Name + ", because it takes more than 1 argument. It should only take a single argument.";
            }

            // The first parameter must derive from NetIdMessage.
            if (parameters.Length == 1)
            {
                var argType = parameters[0].ParameterType;
                if (!Utility.IsSameOrSubclass(typeof(NetIdMessage), argType))
                {
                    return "Cannot register method: " + method.Name + ", because its first argument does not derive from: " + typeof(NetIdMessage);
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
        /// Classes that have registered for message-invoked commands. This is used to prevent duplicate and expensive
        /// reflection operations from running each time a new object is instantiated.
        /// </summary>
        private static readonly HashSet<Type> hasRegistered = new HashSet<Type>();

        /// <summary>
        /// Callbacks for message ids with client-side prediction enabled.
        /// </summary>
        private static Dictionary<short, Action<NetIdMessage>> predictionCallbacks = new Dictionary<short, Action<NetIdMessage>>();

        #endregion
    }
}
