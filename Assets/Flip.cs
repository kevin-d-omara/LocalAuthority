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

        private Queue<ParameterlessMethod> callbacks = new Queue<ParameterlessMethod>();

        private void Awake()
        {
            spriteRenderer = GetComponent<SpriteRenderer>();
            networkIdentity = GetComponent<NetworkIdentity>();
        }

        public override void OnStartAuthority()
        {
            base.OnStartAuthority();
            if (callbacks.Count > 0)    // TODO: need authority??  && hasAuthority
            {
                var callback = callbacks.Dequeue();
                callback();
                Player.Singleton.CmdReleaseOwnership(networkIdentity);
            }
        }

        private void OnMouseOver()
        {
            if (Input.GetButtonDown("Vertical"))
            {
                CallAsyncWithAuthority(() => CmdFlip());
            }
        }

        private void CallAsyncWithAuthority(ParameterlessMethod callback)
        {
            callbacks.Enqueue(callback);
            Player.Singleton.CmdRequestOwnership(networkIdentity);
        }

        [ClientRpc]
        private void RpcFlip()
        {
            spriteRenderer.color = isFlipped ? Color.white : Color.red;
            isFlipped = !isFlipped;
        }

        [Command]
        private void CmdFlip() { RpcFlip(); }
    }
}