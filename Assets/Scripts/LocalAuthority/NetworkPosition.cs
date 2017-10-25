using System.Collections;
using LocalAuthority.Command;
using TabletopCardCompanion;
using UnityEngine;
using UnityEngine.Networking;

namespace LocalAuthority
{
    [RequireComponent(typeof(NetworkTransform))]
    public class NetworkPosition : NetworkBehaviour
    {
        private static float waitTime = 0.5f;

        private NetworkTransform netTransform;
        private NetworkIdentity networkIdentity;

        public void ReleaseMovement()
        {
            PrivateAccess.SetInstanceField(typeof(NetworkTransform), netTransform, "m_TargetSyncPosition", transform.position);
            // TODO: broadcast "finished" message
            netTransform.enabled = false;
            CommandAuthorizer.Instance.CmdReleaseOwnership(networkIdentity);
            StartCoroutine(ReEnableNetworkTransform(waitTime));
        }

        private IEnumerator ReEnableNetworkTransform(float afterSeconds)
        {
            yield return new WaitForSeconds(afterSeconds);
            netTransform.enabled = true;
        }

        // TODO: receive message

        private void Awake()
        {
            netTransform = GetComponent<NetworkTransform>();
            networkIdentity = GetComponent<NetworkIdentity>();
        }
    }
}