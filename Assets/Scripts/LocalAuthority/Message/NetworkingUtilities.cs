using UnityEngine;
using UnityEngine.Networking;
using Debug = UnityEngine.Debug;

namespace LocalAuthority.Message
{
    public static class NetworkingUtilities
    {
        /// <summary>
        /// Find the networked GameObject with the specified network Id. Logs an error if the object does not exist.
        /// </summary>
        /// <param name="netId">The netId of the networked object.</param>
        /// <returns>The GameObject associated with the network id, or null if the object does not exist.</returns>
        public static GameObject FindLocalObject(NetworkInstanceId netId)
        {
            var foundObject = ClientScene.FindLocalObject(netId);

            if (foundObject == null)
            {
                if (LogFilter.logError) { Debug.LogError("No GameObject exists for the given NetworkInstanceId: " + netId); }
            }

            return foundObject;
        }

        /// <summary>
        /// Find the local NetworkIdentity object with the specified network Id, and return the component attached to
        /// it of type T.
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
    }
}