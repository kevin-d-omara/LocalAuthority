using UnityEngine;
using UnityEngine.Networking;

namespace LocalAuthority.Message
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