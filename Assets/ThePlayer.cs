using System;
using System.Collections;
using System.Collections.Generic;
using TabletopCardCompanion.Utils;
using UnityEngine;
using UnityEngine.Networking;

namespace TabletopCardCompanion
{
    public class ThePlayer : NetworkBehaviour
    {
        /// <summary>
        /// The currently instantiated CommandAuthorizer. Is created during OnStartLocalPlayer.
        /// </summary>
        public static ThePlayer Instance { get; private set; }

        public override void OnStartLocalPlayer()
        {
            base.OnStartLocalPlayer();
            Instance = this;
        }
    }
}