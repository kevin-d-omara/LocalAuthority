using LocalAuthority.Command;
using TabletopCardCompanion.Utils;
using UnityEngine;
using UnityEngine.Networking;

namespace LocalAuthority
{
    public class NetworkPosition : NetworkBehaviour
    {
        private Vector3 targetPosition;
        private NetworkInstanceId owner;

        private float sendRate = 9f / 60f;
        private float lastSendTime = 0f;


        public void RequestOwnership()
        {
            owner = CommandAuthorizer.Instance.netId;
            // broadcast
        }


        private void FixedUpdate()
        {
            if (isOwner())
            {
                FixedUpdateOwner();
            }
            else
            {
                FixedUpdateClient();
            }
        }

        private bool isOwner()
        {
            return owner == CommandAuthorizer.Instance.netId;
        }

        private void FixedUpdateOwner()
        {
            DebugStreamer.AddMessage("Owner");

            if (Time.time - lastSendTime > sendRate)
            {
                DebugStreamer.AddMessage("Send");
                lastSendTime = Time.time;
            }
        }

        private void FixedUpdateClient()
        {
            DebugStreamer.AddMessage("Client");
        }
    }
}