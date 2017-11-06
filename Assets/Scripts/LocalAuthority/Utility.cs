using System;
using UnityEngine;
using UnityEngine.Networking;

namespace LocalAuthority
{
    /// <summary>
    /// Static class for general use utility methods.
    /// </summary>
    public static class Utility
    {
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

        /// <summary>
        /// Return the hashcode for the fully qualified method.
        /// </summary>
        public static int GetCallbackHashcode(Type classType, string methodName)
        {
            return GetHashCode(GetFullyQualifiedMethodName(classType, methodName));
        }

        /// <summary>
        /// Return the concatenation of "namespace" + "class" + "method".
        /// </summary>
        public static string GetFullyQualifiedMethodName(Type classType, string methodName)
        {
            return classType.FullName + "." + methodName;
        }

        /// <summary>
        /// Copied from UNetBehaviourProcessor.cs, which in turn copied from Mono string.GetHashCode(), so that we generate same hashes regardless of runtime (mono/MS .NET).
        /// </summary>
        public static int GetHashCode(string s)
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
    }
}
