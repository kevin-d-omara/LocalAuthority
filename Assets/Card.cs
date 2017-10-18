using System;
using NonPlayerClientAuthority;
using TabletopCardCompanion.Utils;
using UnityEngine;
using UnityEngine.Networking;

namespace TabletopCardCompanion
{
    [RequireComponent(typeof(SpriteRenderer))]
    public class Card : NetworkBehaviour
    {
        [ClientRpc]
        public void RpcFlip()
        {
            spriteRenderer.color = isFlipped ? FRONT_COLOR : BACK_COLOR;
            isFlipped = !isFlipped;
        }

        [ClientRpc]
        public void RpcShift(float x)
        {
            spriteRenderer.color += SHIFT_COLOR * x;
        }

        private static class MsgType
        {
            public static readonly short Flip  = MsgTypeUid.Next;
            public static readonly short Shift = MsgTypeUid.Next;
        }

        private Color FRONT_COLOR = Color.white;
        private Color BACK_COLOR = Color.yellow;
        private Color SHIFT_COLOR = Color.blue;

        private SpriteRenderer spriteRenderer;
        private bool isFlipped;                                     // TODO: match color on late join match

        private void Awake()
        {
            spriteRenderer = GetComponent<SpriteRenderer>();

            NetworkServer.RegisterHandler(MsgType.Flip,  OnFlip);
            NetworkServer.RegisterHandler(MsgType.Shift, OnShift);
            DebugStreamer.AddMessage(gameObject);
        }

        private static void OnFlip(NetworkMessage netMsg)
        {
            var msg = netMsg.ReadMessage<NetIdMessage>();
            var card = NetworkingUtilities.FindLocalComponent<Card>(msg.netId);

            card.RpcFlip();
        }

        private static void OnShift(NetworkMessage netMsg)
        {
            var msg = netMsg.ReadMessage<FloatNetIdMessage>();
            var card = NetworkingUtilities.FindLocalComponent<Card>(msg.netId);

            card.RpcShift(msg.value);
        }

        private void OnMouseOver()
        {
            if (Input.GetButtonDown("Vertical"))
            {
                var msg = new NetIdMessage(netId);
                NetworkManager.singleton.client.Send(MsgType.Flip, msg);
            }
            if (Input.GetButtonDown("Horizontal"))
            {
                var msg = new FloatNetIdMessage(netId, 0.25f);
                NetworkManager.singleton.client.Send(MsgType.Shift, msg);
            }
        }
    }
}