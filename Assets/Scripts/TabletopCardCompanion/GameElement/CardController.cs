using System;
using LocalAuthority;
using LocalAuthority.Message;
using UnityEngine;
using UnityEngine.Networking;

namespace TabletopCardCompanion.GameElement
{
    [RequireComponent(typeof(Ownership))]
    [RequireComponent(typeof(NetworkPosition))]
    public class CardController : LocalAuthorityBehaviour
    {
        #region In-Progress

        private void OnMouseDown()
        {
            networkPosition.BeginMovement();
        }

        private void OnMouseDrag()
        {
            if (ownership.IsOwnedByLocal)
            {
                MoveToMousePosition();
            }
        }

        private void OnMouseUp()
        {
            if (ownership.IsOwnedByLocal)
            {
                networkPosition.EndMovement();
            }
        }

        private void MoveToMousePosition()
        {
            var cam = FindObjectOfType<Camera>();
            var mousePosition = cam.ScreenToWorldPoint(Input.mousePosition);
            mousePosition.z = 0f;

            var deltaPos = mousePosition - transform.position;
            transform.position += deltaPos;
        }

        #endregion In-Progress


        // Input Handler (Command Source) --------------------------------------
        private void OnMouseOver()
        {
            if (Input.GetButtonDown(AxisName.ToggleColor))
            {
                ToggleColor();

                var msg = NewMessage<CommandRecordMessage>();
                SendCommand((short)MsgType.ToggleColor, msg);
            }

            if (Input.GetButtonDown(AxisName.Rotate))
            {
                var direction = Input.GetAxis("Rotate") > 0 ? 1 : -1;
                var degrees = 60 * direction;

                Rotate(degrees);

                var msg = NewMessage<IntCommandRecordMessage>();    // varargs constructor??
                msg.value = degrees;
                SendCommand((short)MsgType.Rotate, msg);
            }

            if (Input.GetButtonDown(AxisName.Scale))
            {
                var direction = Input.GetAxis("Scale") > 0 ? 1 : -1;
                var percent = 0.1f * direction;

                Scale(percent);

                var msg = NewMessage<FloatCommandRecordMessage>();
                msg.value = percent;
                SendCommand((short)MsgType.Scale, msg);
            }
        }


        // API (Controls) ------------------------------------------------------
        private void ToggleColor()
        {
            model.HookIsToggled(!model.IsToggled);
        }

        private void Rotate(int degrees)
        {
            model.HookRotation(degrees);
        }

        private void Scale(float percent)
        {
            var newScale = model.LocalScale * (1f + percent);
            model.HookLocalScale(newScale);
        }

        // Commands ------------------------------------------------------------
        private static void CmdToggleColor(NetworkMessage netMsg)
        {
            var msg = netMsg.ReadMessage<CommandRecordMessage>();
            var obj = NetworkingUtilities.FindLocalComponent<CardController>(msg.netId);
            Action action = () => obj.ToggleColor();
            obj.RunNetworkAction(action, netMsg, msg);
        }

        private static void CmdRotate(NetworkMessage netMsg)
        {
            var msg = netMsg.ReadMessage<IntCommandRecordMessage>();
            var obj = NetworkingUtilities.FindLocalComponent<CardController>(msg.netId);
            Action action = () => obj.Rotate(msg.value);
            obj.RunNetworkAction(action, netMsg, msg);
        }

        private static void CmdScale(NetworkMessage netMsg)
        {
            var msg = netMsg.ReadMessage<FloatCommandRecordMessage>();
            var obj = NetworkingUtilities.FindLocalComponent<CardController>(msg.netId);
            Action action = () => obj.Scale(msg.value);
            obj.RunNetworkAction(action, netMsg, msg);
        }

        // Initialization ------------------------------------------------------
        private CardModel model;
        private Ownership ownership;
        private NetworkPosition networkPosition;

        protected override void Awake()
        {
            base.Awake();
            model = GetComponent<CardModel>();
            ownership = GetComponent<Ownership>();
            networkPosition = GetComponent<NetworkPosition>();
        }

        protected override void RegisterCallbacks()
        {
            NetworkManager.singleton.client.RegisterHandler((short)MsgType.ToggleColor, CmdToggleColor);
            NetworkManager.singleton.client.RegisterHandler((short)MsgType.Rotate, CmdRotate);
            NetworkManager.singleton.client.RegisterHandler((short)MsgType.Scale, CmdScale);

            RegisterCallback((short)MsgType.ToggleColor, CmdToggleColor);
            RegisterCallback((short)MsgType.Rotate, CmdRotate);
            RegisterCallback((short)MsgType.Scale, CmdScale);
        }
    }
}