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
        #region Controller

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

            // Update Model.
            card.isToggled = !card.isToggled;
        }

        private static void OnRotate(NetworkMessage netMsg)
        {
            var msg = netMsg.ReadMessage<IntNetIdMessage>();
            var card = NetworkingUtilities.FindLocalComponent<Card>(msg.netId);

            // Update Model.
            card.transform.Rotate(Vector3.forward, msg.value);
        }

        private static void OnScale(NetworkMessage netMsg)
        {
            var msg = netMsg.ReadMessage<FloatNetIdMessage>();
            var card = NetworkingUtilities.FindLocalComponent<Card>(msg.netId);

            // Update Model.
            card.localScale *= 1f + msg.value;
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

        #endregion

        #region Model

        // Components
        private SpriteRenderer spriteRenderer;

        // Data
        private Color TOGGLE_COLOR = Color.yellow;

        [SyncVar(hook = nameof(HookIsToggled))]
        private bool isToggled;

        [SyncVar(hook = nameof(HookLocalScale))]
        private Vector3 localScale;

        private void Awake()
        {
            // Components
            spriteRenderer = GetComponent<SpriteRenderer>();

            // Initialization
            RegisterMessageCallbacks();
            localScale = transform.localScale;
        }

        /// <summary>
        /// Model is updated, and then updates the View.
        /// </summary>
        private void HookIsToggled(bool newState)
        {
            isToggled = newState;
            ApplyIsToggled();
        }

        /// <summary>
        /// Model is updated, and then updates the View.
        /// </summary>
        private void HookLocalScale(Vector3 newScale)
        {
            localScale = newScale;
            ApplyLocalScale();
        }

        #endregion

        #region View

        /// <summary>
        /// Model is recieved over network, update the View.
        /// </summary>
        public override void OnStartClient()
        {
            base.OnStartClient();

            // Update view.
            ApplyIsToggled();
            ApplyLocalScale();
        }

        /// <summary>
        /// Match the color to the current toggle state (i.e. update View with Model).
        /// </summary>
        private void ApplyIsToggled()
        {
            spriteRenderer.color = isToggled ? TOGGLE_COLOR : Color.white;
        }

        /// <summary>
        /// Match the local scale to the current local scale (i.e. update View with Model).
        /// </summary>
        private void ApplyLocalScale()
        {
            transform.localScale = localScale;
        }

        #endregion
    }
}