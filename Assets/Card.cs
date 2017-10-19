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
        public void RpcRotate(int degrees)
        {
            transform.Rotate(Vector3.forward, degrees);
        }

        [ClientRpc]
        public void RpcScale(float percent)
        {
            transform.localScale *= 1f + percent;
        }

        private static class MsgType
        {
            public static readonly short ToggleColor  = MsgTypeUid.Next;
            public static readonly short Rotate = MsgTypeUid.Next;
            public static readonly short Scale = MsgTypeUid.Next;
        }

        private void RegisterMessageCallbacks()
        {
            NetworkServer.RegisterHandler(MsgType.ToggleColor, OnToggleColor);
            NetworkServer.RegisterHandler(MsgType.Rotate, OnRotate);
            NetworkServer.RegisterHandler(MsgType.Scale, OnScale);
        }

        private static void OnToggleColor(NetworkMessage netMsg)
        {
            var msg = netMsg.ReadMessage<NetIdMessage>();
            var card = NetworkingUtilities.FindLocalComponent<Card>(msg.netId);

            card.isToggled = !card.isToggled; // Update Model.
        }

        private static void OnRotate(NetworkMessage netMsg)
        {
            var msg = netMsg.ReadMessage<IntNetIdMessage>();
            var card = NetworkingUtilities.FindLocalComponent<Card>(msg.netId);

            card.RpcRotate(msg.value);
        }

        private static void OnScale(NetworkMessage netMsg)
        {
            var msg = netMsg.ReadMessage<FloatNetIdMessage>();
            var card = NetworkingUtilities.FindLocalComponent<Card>(msg.netId);

            card.RpcScale(msg.value);
        }

        private void OnMouseOver()
        {
            if (Input.GetButtonDown("Jump"))
            {
                var msg = new NetIdMessage(netId);
                NetworkManager.singleton.client.Send(MsgType.ToggleColor, msg);
            }
            if (Input.GetButtonDown("Horizontal"))
            {
                var direction = Input.GetAxis("Horizontal") > 0 ? 1 : -1;
                var msg = new IntNetIdMessage(netId, 60 * direction);
                NetworkManager.singleton.client.Send(MsgType.Rotate, msg);
            }
            if (Input.GetButtonDown("Vertical"))
            {
                var percent = Input.GetAxis("Vertical") > 0 ? 1f : -1f;
                var msg = new FloatNetIdMessage(netId, 0.1f * percent);
                NetworkManager.singleton.client.Send(MsgType.Scale, msg);
            }
        }

        private Color TOGGLE_COLOR = Color.yellow;

        private SpriteRenderer spriteRenderer;

        [SyncVar(hook = nameof(HookOnToggleColor))]
        private bool isToggled;

        private void Awake()
        {
            spriteRenderer = GetComponent<SpriteRenderer>();
            RegisterMessageCallbacks();
        }

        /// <summary>
        /// Model is recieved over network, update the View.
        /// </summary>
        public override void OnStartClient()
        {
            base.OnStartClient();
            ApplyToggle(); // Update view.
        }

        /// <summary>
        /// Match the color to the current toggle state (i.e. update View with Model).
        /// </summary>
        private void ApplyToggle()
        {
            spriteRenderer.color = isToggled ? TOGGLE_COLOR : Color.white;
        }

        private void HookOnToggleColor(bool newState)
        {
            isToggled = newState;
            ApplyToggle();
        }
    }
}