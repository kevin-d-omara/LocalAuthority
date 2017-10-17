using TabletopCardCompanion.Utils;
using UnityEngine.Networking;

namespace NonPlayerClientAuthority
{
    public class NetIdMessage : MessageBase
    {
        public NetworkInstanceId netId;

        public NetIdMessage()
        {
            
        }

        public NetIdMessage(NetworkInstanceId id)
        {
            netId = id;
        }
    }

    public class FloatNetIdMessage : MessageBase
    {
        public NetworkInstanceId netId;
        public float value;

        public FloatNetIdMessage()
        {
            
        }

        public FloatNetIdMessage(NetworkInstanceId id)
        {
            netId = id;
        }

        public FloatNetIdMessage(float value)
        {
            this.value = value;
        }

        public FloatNetIdMessage(NetworkInstanceId id, float value) : this(id)
        {
            this.value = value;
        }
    }

    public static class NetUtils
    {
        // TODO: put in util class
        public static T FindLocalComponent<T>(NetworkInstanceId id)
        {
            // TODO: pass in NetIdMessage instead
            var gameObject = ClientScene.FindLocalObject(id);
            return gameObject.GetComponent<T>();    // TODO: check for null
        }
    }
}