using NonPlayerClientAuthority;
using UnityEngine;
using UnityEngine.Networking;

namespace TabletopCardCompanion
{
    [RequireComponent(typeof(SpriteRenderer))]
    [RequireComponent(typeof(CommandExecutor))]
    public class Flip : NetworkBehaviour
    {
        private SpriteRenderer spriteRenderer;
        private bool isFlipped;                                     // TODO: match color on late join match

        private CommandExecutor cmdExecutor;

        private void Awake()
        {
            spriteRenderer = GetComponent<SpriteRenderer>();
            cmdExecutor = GetComponent<CommandExecutor>();
        }

        private void OnMouseOver()
        {
            if (Input.GetButtonDown("Vertical"))
            {
                cmdExecutor.CallAsyncWithAuthority(() => CmdFlip());
            }
            if (Input.GetButtonDown("Horizontal"))
            {
                var x = 0.25f;
                cmdExecutor.CallAsyncWithAuthority(() => CmdShift(x));
            }
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