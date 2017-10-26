using LocalAuthority.Message;
using UnityEngine;
using UnityEngine.Networking;

namespace LocalAuthority
{
    [RequireComponent(typeof(Ownership))]
    public class NetworkPosition : NetworkBehaviour
    {
        private void Update()
        {
            if (ownership.IsOwnedByRemote || ownership.IsOwnedByNone)
            {
                // interpolate
            }
        }

        private Ownership ownership;

        private void Awake()
        {
            ownership = GetComponent<Ownership>();
        }
    }
}