using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;

namespace NonPlayerClientAuthority
{
    /// <summary>
    /// Attach this component to any non-player object which needs authority to execute commands.
    /// </summary>
    /// <example>
    /// For example, a landmine with OnTriggerEnter => CmdExplode(). The landmine does not have
    /// authority to tell the server it exploded (via CmdExplode). It first needs to request
    /// authority, through the player. This component, combined with <see cref="CommandAuthorizer"/>,
    /// abstracts this process.
    /// 
    /// Must use a closure to pass callback:
    /// cmdExecutor.CallAsyncWithAuthority(() => CmdExplode()); // yes
    /// cmdExecutor.CallAsyncWithAuthority(CmdExplode);         // no - doesn't call networked version
    /// </example>
    // TODO: include actual code sample, see https://docs.unity3d.com/ScriptReference/Collider2D.OnTriggerEnter2D.html
    public class CommandExecutor : NetworkBehaviour
    {
        /// <summary>
        /// Execute callback after this object gains authority.
        /// </summary>
        /// <param name="callback">[Command] annotated method to call with authority.</param>
        public void CallAsyncWithAuthority(Action callback)
        {
            callbacks.Enqueue(callback);
            CommandAuthorizer.Instance.CmdRequestOwnership(networkIdentity);
        }

        private Queue<Action> callbacks = new Queue<Action>();
        private NetworkIdentity networkIdentity;

        private void Awake()
        {
            networkIdentity = GetComponent<NetworkIdentity>();
        }

        /// <summary>
        /// Execute all callback methods enqueued since the last time this object gained authority.
        /// Do not call this method, it is a public callback used by Unity.
        /// </summary>
        public override void OnStartAuthority()
        {
            base.OnStartAuthority();

            if (CommandAuthorizer.Instance == null)
            {
                return;     // Object was initialized on host, before CommandAuthorizer exists.
            }
            if (callbacks.Count == 0)
            {
                return;     // Control returned to host after client finished OnStartAuthority.
                            // The object has no owner.
            }

            foreach (var callback in callbacks)
            {
                callback();
            }
            callbacks.Clear();
            CommandAuthorizer.Instance.CmdReleaseOwnership(networkIdentity);                        // TODO: Reduce lag by retaining Authority. Thus, only first interaction suffers 2x lag.
        }
    }
}