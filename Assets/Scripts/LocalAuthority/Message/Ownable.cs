using UnityEngine;
using UnityEngine.Networking;

namespace LocalAuthority.Message
{
    public class Ownable : NetworkBehaviour
    {
        public enum Ownership
        {
            None, Remote, Local
        }

        public Ownership Owner { get; private set; }

        public void RequestOwnership()
        {
            
        }
    }
}
