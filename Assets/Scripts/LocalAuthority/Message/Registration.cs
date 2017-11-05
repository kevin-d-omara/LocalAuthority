using System;
using System.Collections.Generic;
using System.Reflection;

namespace LocalAuthority.Message
{
    /// <summary>
    /// Static class for registering message commands.
    /// </summary>
    public static class Registration
    {
        /// <summary>
        /// Use reflection to register methods marked with the <see cref="MessageCommand"/> attribute.
        /// </summary>
        public static void RegisterCommands(Type classType)
        {
            if (AlreadyRegistered(classType)) return;

            // TODO: What about inheritence? I.e. DoubleSidedCardController : CardController?
            var methods = classType.GetMethods(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);

            foreach (var method in methods)
            {
                var attribute = method.GetCustomAttribute<Message>(true);
                if (attribute != null)
                {
                    CacheRegisteredClass(classType, attribute);
                    CacheMessageType(method, classType, attribute);
                    CacheParameterTypeList(method, attribute);

                    attribute.RegisterMessage(method, classType);
                }
            }
        }


        #region Private

        /// <summary>
        /// True if the class has already been registered on the server and clients.
        /// </summary>
        private static bool AlreadyRegistered(Type classType)
        {
            short msgType;
            if (RegisteredClasses.TryGetValue(classType, out msgType))
            {
                if (Message.HasBeenRegistered(msgType)) return true;
            }
            return false;
        }

        /// <summary>
        /// Record that this class has been registered.
        /// </summary>
        private static void CacheRegisteredClass(Type classType, Message attribute)
        {
            // Store the first method. We only need one value per class to check Message.HasBeenRegistered().
            if (!RegisteredClasses.ContainsKey(classType))
            {
                RegisteredClasses.Add(classType, attribute.MsgType);
            }
        }

        /// <summary>
        /// Record mapping from method name to message id.
        /// </summary>
        private static void CacheMessageType(MethodInfo method, Type classType, Message attribute)
        {
            var methodName = Utility.GetFullyQualifiedMethodName(classType, method.Name);
            if (!MsgTypes.ContainsKey(methodName))
            {
                MsgTypes.Add(methodName, attribute.MsgType);
            }
        }

        /// <summary>
        /// Record a list of Types for this method's parameters.
        /// </summary>
        private static void CacheParameterTypeList(MethodInfo method, Message attribute)
        {
            if (!ParameterTypes.ContainsKey(attribute.MsgType))
            {
                var parameters = method.GetParameters();
                if (parameters.Length > 0)
                {
                    var types = new Type[parameters.Length];
                    for (int i = 0; i < parameters.Length; ++i)
                    {
                        types[i] = parameters[i].ParameterType;
                    }
                    ParameterTypes.Add(attribute.MsgType, types);
                }
                else
                {
                    ParameterTypes.Add(attribute.MsgType, null);
                }
            }
        }

        /// <summary>
        /// Store methods which have client-side prediction enabled.
        /// </summary>
        internal static void RegisterPredictedRpc(short msgType, MethodInfo method)
        {
            if (!ClientSidePrediction.ContainsKey(msgType))
            {
                ClientSidePrediction.Add(msgType, method);
            }
        }


        // Data Cache ----------------------------------------------------------

        /// <summary>
        /// Mapping from registered class to message id of its first method.
        /// Only one message id is needed per class to check <see cref="Message.HasBeenRegistered"/>
        /// </summary>
        private static readonly Dictionary<Type, short> RegisteredClasses = new Dictionary<Type, short>();

        /// <summary>
        /// Mapping from method name to message id.
        /// </summary>
        internal static readonly Dictionary<string, short> MsgTypes = new Dictionary<string, short>();

        /// <summary>
        /// Mapping from message id to a list of Types for the callback's parameters.
        /// </summary>
        internal static readonly Dictionary<short, Type[]> ParameterTypes = new Dictionary<short, Type[]>();

        /// <summary>
        /// Mapping from message id to method info for methods with client-side prediction enabled.
        /// </summary>
        internal static readonly Dictionary<short, MethodInfo> ClientSidePrediction = new Dictionary<short, MethodInfo>();

        #endregion
    }
}
