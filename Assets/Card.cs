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
        public class MyMsgType
        {
            public static short Flip   = MsgTypeIncrementer.Next;
            public static short Shift = MsgTypeIncrementer.Next;
        }

        public class MyNetMessage : MessageBase
        {
            public NetworkInstanceId Id;
        }

        public class ShiftMessage : MyNetMessage
        {
            public float Amount;
        }

        private Color FRONT_COLOR = Color.white;
        private Color BACK_COLOR = Color.red;
        private Color SHIFT_COLOR = Color.blue;

        private SpriteRenderer spriteRenderer;
        private bool isFlipped;                                     // TODO: match color on late join match

        private void Awake()
        {
            spriteRenderer = GetComponent<SpriteRenderer>();

            NetworkServer.RegisterHandler(MyMsgType.Flip, OnFlip);
            NetworkServer.RegisterHandler(MyMsgType.Shift, OnShift);
        }

        public void OnFlip(NetworkMessage netMsg) // can it be private?
        {
            var msg = netMsg.ReadMessage<MyNetMessage>();
            var go = ClientScene.FindLocalObject(msg.Id);
            go.GetComponent<Card>().RpcFlip();

            DebugStreamer.AddMessage("Flipped");
        }

        public void OnShift(NetworkMessage netMsg)
        {
            var msg = netMsg.ReadMessage<ShiftMessage>();
            var go = ClientScene.FindLocalObject(msg.Id);
            go.GetComponent<Card>().RpcShift(msg.Amount);

            DebugStreamer.AddMessage("Shifted: " + msg.Amount);
        }

        private void OnMouseOver()
        {
            if (Input.GetButtonDown("Vertical"))
            {
                var msg = new MyNetMessage();
                msg.Id = netId;
                NetworkManager.singleton.client.Send(MyMsgType.Flip, msg);
                // TODO: make "ShiftMessage : NetworkedMessage : MessageBase"
                //       NetworkedMessage contains NetworkIdentity or NetworkInstanceID
                //       Figure out how to use NetId or NetInstID to find a gameobject.
                //       Use that to have static OnFlip method which finds the correct game object
                //          to call RpcFlip() on.
            }
            if (Input.GetButtonDown("Horizontal"))
            {
                var x = 0.25f;
                var msg = new ShiftMessage();
                msg.Id = netId;
                msg.Amount = x;
                NetworkManager.singleton.client.Send(MyMsgType.Shift, msg);
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