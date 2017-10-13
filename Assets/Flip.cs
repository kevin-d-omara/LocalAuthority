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

        private delegate void ZeroArgMethod();                                                      // TODO: use Action?
        private Queue<ZeroArgMethod> callbacks = new Queue<ZeroArgMethod>();

        private void Awake()
        {
            spriteRenderer = GetComponent<SpriteRenderer>();
            networkIdentity = GetComponent<NetworkIdentity>();
        }

        public override void OnStartAuthority()
        {
            base.OnStartAuthority();
            if (Player.Singleton == null) return;

            foreach (var callback in callbacks)
            {
                callback();
            }
            callbacks.Clear();
            Player.Singleton.CmdReleaseOwnership(networkIdentity);                                  // TODO: Reduce lag by retaining Authority. Thus, only first interaction suffers 2x lag.
        }

        private void OnMouseOver()
        {
            if (Input.GetButtonDown("Vertical"))
            {
                CallAsyncWithAuthority(() => CmdFlip());
            }
            if (Input.GetButtonDown("Horizontal"))
            {
                var x = 0.25f;
                CallAsyncWithAuthority(() => CmdShift(x));
            }
        }

        private void CallAsyncWithAuthority(ZeroArgMethod callback)
        {
            callbacks.Enqueue(callback);
            Player.Singleton.CmdRequestOwnership(networkIdentity);
        }

        [Command]
        private void CmdFlip() { RpcFlip(); }

        [ClientRpc]
        private void RpcFlip()
        {
            spriteRenderer.color = isFlipped ? Color.white : Color.red;
            isFlipped = !isFlipped;
        }

        [Command]
        private void CmdShift(float x) { RpcShift(x); }

        [ClientRpc]
        private void RpcShift(float x)
        {
            spriteRenderer.color += Color.blue * x;
        }
    }
}