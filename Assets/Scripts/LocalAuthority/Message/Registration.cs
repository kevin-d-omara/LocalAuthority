using System;
using System.Collections.Generic;
using System.Reflection;
using UnityEngine.Networking;

namespace LocalAuthority.Message
{
    /// <summary>
    /// Static class for creating message-based command/rpc callbacks.
    /// </summary>
    public static class Registration
    {
        /// <summary>
        /// Use reflection to register methods marked with the <see cref="MessageCommand"/> or <see cref="MessageRpc"/> attribute.
        /// </summary>
        public static void RegisterCommands(Type classType)
        {
            if (AlreadyRegistered.Contains(classType)) return;

            // TODO: What about inheritence? I.e. DoubleSidedCardController : CardController?
            var methods = classType.GetMethods(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);

            foreach (var method in methods)
            {
                var attribute = method.GetCustomAttribute<Message>(true);
                if (attribute != null)
                {
                    var callbackHashcode = Utility.GetCallbackHashcode(classType, method.Name);
                    CacheParameterTypeList(callbackHashcode, method);

                    var callback = attribute.GetCallback2(method, classType);
                    Callbacks.Add(callbackHashcode, callback);

                    if (attribute.ClientSidePrediction)
                    {
                        RegisterPredictedRpc(callbackHashcode, method);
                    }

                    AlreadyRegistered.Add(classType);
                }
            }
        }


        #region Private

        /// <summary>
        /// Record a list of Types for this method's parameters.
        /// </summary>
        private static void CacheParameterTypeList(int callbackHashcode, MethodInfo method)
        {
            if (!ParameterTypes.ContainsKey(callbackHashcode))
            {
                var parameters = method.GetParameters();
                if (parameters.Length > 0)
                {
                    var types = new Type[parameters.Length];
                    for (int i = 0; i < parameters.Length; ++i)
                    {
                        types[i] = parameters[i].ParameterType;
                    }
                    ParameterTypes.Add(callbackHashcode, types);
                }
                else
                {
                    ParameterTypes.Add(callbackHashcode, null);
                }
            }
        }

        /// <summary>
        /// Store methods which have client-side prediction enabled.
        /// </summary>
        internal static void RegisterPredictedRpc(int callbackHashcode, MethodInfo method)
        {
            if (!ClientSidePrediction.ContainsKey(callbackHashcode))
            {
                ClientSidePrediction.Add(callbackHashcode, method);
            }
        }


        // Data Cache ----------------------------------------------------------

        /// <summary>
        /// Mapping from callback hashcode to callback.
        /// </summary>
        internal static Dictionary<int, Action<NetworkMessage, VarArgsNetIdMessasge>> Callbacks = new Dictionary<int, Action<NetworkMessage, VarArgsNetIdMessasge>>();

        /// <summary>
        /// Mapping from callback hashcode to a list of Types for the callback's parameters.
        /// </summary>
        internal static readonly Dictionary<int, Type[]> ParameterTypes = new Dictionary<int, Type[]>();

        /// <summary>
        /// Mapping from callback hashcode to method info for methods with client-side prediction enabled.
        /// </summary>
        internal static readonly Dictionary<int, MethodInfo> ClientSidePrediction = new Dictionary<int, MethodInfo>();

        /// <summary>
        /// Classes that have already had their callbacks created.
        /// </summary>
        internal static readonly HashSet<Type> AlreadyRegistered = new HashSet<Type>();

        #endregion
    }
}
