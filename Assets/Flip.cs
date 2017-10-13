using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;

namespace TabletopCardCompanion
{
    [RequireComponent(typeof(SpriteRenderer))]
    public class Flip : NetworkBehaviour
    {
        private SpriteRenderer spriteRenderer;
        private NetworkIdentity networkIdentity;
        private bool isFlipped;

        private delegate void ParameterlessMethod();
        private ParameterlessMethod callbackCommand;

        private void Awake()
        {
            spriteRenderer = GetComponent<SpriteRenderer>();
            networkIdentity = GetComponent<NetworkIdentity>();
        }

        public override void OnStartAuthority()
        {
            base.OnStartAuthority();
            if (callbackCommand != null && hasAuthority)
            {
//                callbackCommand();
                CmdFlip();
                callbackCommand = null;
                Player.Singleton.CmdReleaseOwnership(networkIdentity);
            }
        }

        private void OnMouseOver()
        {
            if (Input.GetButtonDown("Vertical"))
            {
                callbackCommand = CmdFlip;
                Player.Singleton.CmdRequestOwnership(networkIdentity);
            }
        }

        [Command]
        private void CmdFlip()
        {
            RpcFlip();
        }

        [ClientRpc]
        private void RpcFlip()
        {
            spriteRenderer.color = isFlipped ? Color.white : Color.red;
            isFlipped = !isFlipped;
        }
    }
}