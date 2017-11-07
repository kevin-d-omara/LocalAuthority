using System;
using System.Collections.Generic;
using System.Reflection;

namespace LocalAuthority
{
    /// <summary>
    /// Static class for creating message-based command/rpc callbacks.
    /// </summary>
    public static class Registration
    {
        /// <summary>
        /// Create and register callbacks for methods marked with the <see cref="MessageBasedCallback"/> attribute in the specified class.
        /// </summary>
        public static void RegisterCommands(Type classType)
        {
            if (AlreadyRegistered.Contains(classType)) return;

            var methods = classType.GetMethods(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
            foreach (var method in methods)
            {
                var attribute = method.GetCustomAttribute<MessageBasedCallback>(true);
                if (attribute != null)
                {
                    var hash = GetCallbackHashcode(classType, method.Name);
                    var callback = attribute.GetCallback(method, classType);

                    Callbacks.Add(hash, callback);
                    CacheParameterTypeList(hash, method);

                    if (attribute.ClientSidePrediction)
                    {
                        CachePredictedCallback(hash, method);
                    }

                    AlreadyRegistered.Add(classType);
                }
            }
        }

        /// <summary>
        /// Return a hashcode created from the class type and method name.
        /// </summary>
        public static int GetCallbackHashcode(Type classType, string methodName)
        {
            return GetHashCode(GetFullyQualifiedMethodName(classType, methodName));
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
        /// Record methods which have client-side prediction enabled.
        /// </summary>
        private static void CachePredictedCallback(int callbackHashcode, MethodInfo method)
        {
            if (!PredictedCallbacks.ContainsKey(callbackHashcode))
            {
                PredictedCallbacks.Add(callbackHashcode, method);
            }
        }

        /// <summary>
        /// Return a concatenation of "namespace" + "class" + "method".
        /// </summary>
        private static string GetFullyQualifiedMethodName(Type classType, string methodName)
        {
            return classType.FullName + "." + methodName;
        }

        /// <summary>
        /// Copied from UNetBehaviourProcessor.cs, which in turn copied from Mono string.GetHashCode(), so that we generate same hashes regardless of runtime (mono/MS .NET).
        /// </summary>
        private static int GetHashCode(string s)
        {
            unsafe
            {
                int length = s.Length;
                fixed (char* c = s)
                {
                    char* cc = c;
                    char* end = cc + length - 1;
                    int h = 0;
                    for (; cc < end; cc += 2)
                    {
                        h = (h << 5) - h + *cc;
                        h = (h << 5) - h + cc[1];
                    }
                    ++end;
                    if (cc < end)
                        h = (h << 5) - h + *cc;
                    return h;
                }
            }
        }

        // Data Cache ----------------------------------------------------------

        /// <summary>
        /// Mapping from callback hashcode to callback.
        /// </summary>
        internal static readonly Dictionary<int, MessageCallback> Callbacks = new Dictionary<int, MessageCallback>();

        /// <summary>
        /// Mapping from callback hashcode to a list of Types for the callback's parameters.
        /// </summary>
        internal static readonly Dictionary<int, Type[]> ParameterTypes = new Dictionary<int, Type[]>();

        /// <summary>
        /// Mapping from callback hashcode to method info for methods with client-side prediction enabled.
        /// </summary>
        internal static readonly Dictionary<int, MethodInfo> PredictedCallbacks = new Dictionary<int, MethodInfo>();

        /// <summary>
        /// Classes that have already had their callbacks created.
        /// </summary>
        private static readonly HashSet<Type> AlreadyRegistered = new HashSet<Type>();

        #endregion
    }
}
