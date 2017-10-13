using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;

namespace TabletopCardCompanion
{
    public class Player : NetworkBehaviour
    {
        public static Player Singleton { get; private set; }

        public override void OnStartLocalPlayer()
        {
            base.OnStartLocalPlayer();
            Singleton = this;
        }

        [Command]
        public void CmdRequestOwnership(NetworkIdentity identity)
        {
            identity.AssignClientAuthority(connectionToClient);
        }

        [Command]
        public void CmdReleaseOwnership(NetworkIdentity identity)
        {
            identity.RemoveClientAuthority(connectionToClient);
        }
    }
}