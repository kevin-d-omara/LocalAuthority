using System;
using System.Collections.Generic;
using System.Reflection;
using LocalAuthority.Components;
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
                    RegisterCommand(method, attribute, types);
                }
            }
        }

        /// <summary>
        /// Empty the list of classes that have registered for message-invoked commands.
        /// This is necessary each time the client joins a new game, because cached connection data has gone stale.
        /// </summary>
        public static void ClearRegisteredSet()
        {
            hasRegistered.Clear();
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
                if (!IsSameOrSubclass(typeof(NetIdMessage), argType))
                {
                    return "Cannot register method: " + method.Name + ", because its first argument does not derive from: " + typeof(NetIdMessage);
                }
            }

            return "";
        }

        // TODO: move elsewhere
        public static bool IsSameOrSubclass(Type potentialBase, Type potentialDescendant)
        {
            return potentialDescendant.IsSubclassOf(potentialBase) || potentialDescendant == potentialBase;
        }

        /// <summary>
        /// Return the type of the first parameter of the method, or <see cref="NetIdMessage"/> if there are no parameters.
        /// </summary>
        private static Type GetValidParameterType(MethodInfo method)
        {
            var parameters = method.GetParameters();

            return parameters.Length == 0 ? typeof(NetIdMessage) : parameters[0].ParameterType;
        }


        // Registration --------------------------------------------------------

        /// <summary>
        /// A non-generic version of <see cref="RegisterCommand{TMsg,TComp}"/> that requires information obtained using reflection.
        /// </summary>
        /// <param name="method">The function to register.</param>
        /// <param name="attribute">The attribute attached to the method.</param>
        /// <param name="types">The two types from <see cref="RegisterCommand{TMsg,TComp}"/></param>
        private static void RegisterCommand(MethodInfo method, Message attribute, Type[] types)
        {
            // Same number, order, and type as parameters to RegisterCommand<TMsg, TComp>().
            var args = new object[] { method, attribute };

            var registerCommand = RegisterCommandMethodInfo.MakeGenericMethod(types);
            registerCommand.Invoke(null, args);
        }

        /// <summary>
        /// Register a message-based command on the server and the client.
        /// <para>
        /// Registering on the server enables the method to be called on the server like a [Command], except invoked with <see cref="LocalAuthorityBehaviour.SendCommand{TMsg}"/>.
        /// Registering on the client enables the method to be called on all clients, like a [ClientRpc], except invoked with <see cref="LocalAuthorityBehaviour.InvokeMessageRpc"/>.
        /// </para>
        /// </summary>
        /// <typeparam name="TMsg">Type of network message that the method takes as its only parameter.</typeparam>
        /// <typeparam name="TComp">Type of component where the method code is written.</typeparam>
        /// <param name="method">The function to register.</param>
        /// <param name="attribute">The attribute attached to the method.</param>
        private static void RegisterCommand<TMsg, TComp>(MethodInfo method, Message attribute)
            where TMsg : NetIdMessage, new()
            where TComp : LocalAuthorityBehaviour
        {
            var callback = attribute.GetCallback<TMsg, TComp>(method);
            attribute.RegisterMessage(callback);
        }


        // Initialization ------------------------------------------------------

        /// <summary>
        /// Classes that have registered for message-invoked commands. This is used to prevent duplicate and expensive
        /// reflection operations from running each time a new object is instantiated.
        /// </summary>
        private static readonly HashSet<Type> hasRegistered = new HashSet<Type>();

        /// <summary>
        /// Cached MethodInfo for <see cref="RegisterCommand{TMsg,TComp}"/>.
        /// </summary>
        private static MethodInfo RegisterCommandMethodInfo { get; }

        static Registration()
        {
            var types = new Type[] {typeof(MethodInfo), typeof(Message)};
            var flags = BindingFlags.Static | BindingFlags.NonPublic;
            var info = typeof(Registration).GetMethod(nameof(RegisterCommand), flags, null, types, null);
            RegisterCommandMethodInfo = info;
        }

        #endregion
    }
}
