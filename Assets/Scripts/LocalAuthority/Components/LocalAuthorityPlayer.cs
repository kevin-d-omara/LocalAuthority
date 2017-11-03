using System;
using LocalAuthority.Message;
using UnityEngine.Networking;

namespace LocalAuthority.Components
{
    /// <summary>
    /// Attach to the Player prefab to enable <see cref="Ownership"/>, which is necessary for components like <see cref="NetworkPosition"/>.
    /// </summary>
    public class LocalAuthorityPlayer : NetworkBehaviour
    {
        // Data ----------------------------------------------------------------

        /// <summary>
        /// Singleton reference to the Player object owned by this client.
        /// </summary>
        public static LocalAuthorityPlayer LocalPlayer { get; private set; }

        /// <summary>
        /// Convienience reference to the player object's Network Identity.
        /// </summary>
        public NetworkIdentity NetIdentity { get; private set; }

        /// <summary>
        /// Raised when the singleton reference <see cref="LocalPlayer"/> is set for the first time.
        /// </summary>
        public static event EventHandler<EventArgs> PlayerInitialized;


        // Initialization ------------------------------------------------------
        private void Awake()
        {
            NetIdentity = GetComponent<NetworkIdentity>();
        }

//        public override void OnStartClient()
//        {
//            base.OnStartClient();
//            if (NetIdentity.isLocalPlayer)
//            {
//                Registration.ClearCache();
//            }
//        }

        public override void OnStartLocalPlayer()
        {
            base.OnStartLocalPlayer();

            if (NetIdentity.isLocalPlayer)
            {
                LocalPlayer = this;
                PlayerInitialized?.Invoke(this, EventArgs.Empty);
                Registration.ClearCache();
            }
        }
    }
}
