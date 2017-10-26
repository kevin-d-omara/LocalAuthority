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
        private static float waitTime = .5f;

        private NetworkTransform netTransform;
        private NetworkIdentity networkIdentity;

        private void Update()
        {
            // BUG: velocity becomes non-zero when Ownership is released (if other peers are still interpolating).
            GetComponent<Rigidbody2D>().velocity = Vector2.zero;
        }

        public void ReleaseMovement()
        {
            CmdMovementCompleted(transform.position);

            netTransform.enabled = false;
            CommandAuthorizer.Instance.CmdReleaseOwnership(networkIdentity);
            StartCoroutine(ReEnableNetworkTransform(waitTime));
            SetTargetSyncPosition(transform.position);
        }

        [Command]
        private void CmdMovementCompleted(Vector3 finalPosition)
        {
            RpcMovementCompleted(finalPosition);
        }

        [ClientRpc]
        private void RpcMovementCompleted(Vector3 finalPosition)
        {
            SetTargetSyncPosition(finalPosition);
        }

        private IEnumerator ReEnableNetworkTransform(float afterSeconds)
        {
            yield return new WaitForSeconds(afterSeconds);
            netTransform.enabled = true;
        }

        private void SetTargetSyncPosition(Vector3 targetPosition)
        {
            PrivateAccess.SetInstanceField(typeof(NetworkTransform), netTransform, "m_TargetSyncPosition", targetPosition);
        }

        private void Awake()
        {
            netTransform = GetComponent<NetworkTransform>();
            networkIdentity = GetComponent<NetworkIdentity>();
        }
    }
}