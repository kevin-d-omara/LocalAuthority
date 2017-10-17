using System;
using NonPlayerClientAuthority;
using TabletopCardCompanion.Utils;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.Networking.NetworkSystem;

namespace TabletopCardCompanion
{
    [RequireComponent(typeof(SpriteRenderer))]
    public class Card : NetworkBehaviour
    {
        public class MsgType
        {
            public static short Flip  = MsgTypeIncrementer.Next;
            public static short Shift = MsgTypeIncrementer.Next;
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
        }

        public void OnFlip(NetworkMessage netMsg) // can it be private?
        {
            var msg = netMsg.ReadMessage<NetIdMessage>();
            var card = NetUtils.FindLocalComponent<Card>(msg.netId);

            card.RpcFlip();

            DebugStreamer.AddMessage("Flipped");
        }

        public void OnShift(NetworkMessage netMsg)
        {
            var msg = netMsg.ReadMessage<FloatNetIdMessage>();
            var card = NetUtils.FindLocalComponent<Card>(msg.netId);

            card.RpcShift(0.25f);

            DebugStreamer.AddMessage("Shifted: " + msg.value);
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
    }
}