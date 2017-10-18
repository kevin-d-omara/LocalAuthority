using UnityEngine.Networking;

namespace NonPlayerClientAuthority
{
    public static class NetworkingUtilities
    {
        /// <summary>
        /// Find the local NetworkIdentity object with the specified network Id, and return the
        /// component attached to it of type T.
        /// </summary>
        /// <typeparam name="T">Type of the component to find.</typeparam>
        /// <param name="netId">The netId of then networked object.</param>
        /// <returns>The component attached to the game object with matching netId.</returns>
        public static T FindLocalComponent<T>(NetworkInstanceId netId)
        {
            var gameObject = ClientScene.FindLocalObject(netId);
            return gameObject.GetComponent<T>();
        }
    }
}