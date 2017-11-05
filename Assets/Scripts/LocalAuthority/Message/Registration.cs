using System;
using System.Collections.Generic;
using System.Reflection;

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
            // Registration ----------------------------------------------------

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
                    // Store only the first method's message type, because we only need a single value
                    // per class to check Message.HasBeenRegistered().
                    if (!messageTypes.ContainsKey(classType))
                    {
                        messageTypes.Add(classType, attribute.MsgType);
                    }

                    // Store map from method name to message id.
                    var methodName = method.Name;
                    if (!MsgTypes.ContainsKey(methodName))
                    {
                        MsgTypes.Add(methodName, attribute.MsgType); // TODO: Same method name, different class?
                    }

                    // Store Type list for the method parameters.
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

                    attribute.RegisterMessage(method, classType);
                }
            }
        }


        #region Private

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


        // Data ----------------------------------------------------------------

        /// <summary>
        /// A cache with one message id per class. Used to prevent uneccessary and expensive registrations.
        /// </summary>
        private static readonly Dictionary<Type, short> messageTypes = new Dictionary<Type, short>();

        /// <summary>
        /// Mapping from method name to message id.
        /// </summary>
        internal static readonly Dictionary<string, short> MsgTypes = new Dictionary<string, short>();

        /// <summary>
        /// Mapping from message id to Type of each parameter to the method callback.
        /// </summary>
        internal static readonly Dictionary<short, Type[]> ParameterTypes = new Dictionary<short, Type[]>();

        /// <summary>
        /// Mapping from message id to method info for methods with client-side prediction enabled.
        /// </summary>
        internal static readonly Dictionary<short, MethodInfo> ClientSidePrediction = new Dictionary<short, MethodInfo>();

        #endregion
    }
}
