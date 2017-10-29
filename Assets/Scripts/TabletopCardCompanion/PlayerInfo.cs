using System;
using UnityEngine.Networking;

namespace TabletopCardCompanion
{
    public class PlayerInfo : NetworkBehaviour
    {
        // Data ----------------------------------------------------------------
        public static PlayerInfo LocalPlayer { get; private set; }

        public NetworkIdentity NetIdentity { get; private set; }

        public static event EventHandler<EventArgs> PlayerInitialized;

        // Initialization ------------------------------------------------------
        private void Awake()
        {
            NetIdentity = GetComponent<NetworkIdentity>();
        }

        public override void OnStartLocalPlayer()
        {
            base.OnStartLocalPlayer();

            if (NetIdentity.isLocalPlayer)
            {
                LocalPlayer = this;
                PlayerInitialized?.Invoke(this, EventArgs.Empty);
            }
        }
    }
}
