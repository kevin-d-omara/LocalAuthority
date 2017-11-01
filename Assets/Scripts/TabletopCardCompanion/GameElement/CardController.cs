﻿using System;
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

                SendCommand<NetIdMessage>((short) MsgType.ToggleColor);
            }

            if (Input.GetButtonDown(AxisName.Rotate))
            {
                var direction = Input.GetAxis("Rotate") > 0 ? 1 : -1;
                var degrees = 60 * direction;

                Rotate(degrees);

                SendCommand<IntNetIdMessage>((short)MsgType.Rotate, degrees);
            }

            if (Input.GetButtonDown(AxisName.Scale))
            {
                var direction = Input.GetAxis("Scale") > 0 ? 1 : -1;
                var percent = 0.1f * direction;

                Scale(percent);

                SendCommand<FloatNetIdMessage>((short)MsgType.Scale, percent);
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
            var msg = netMsg.ReadMessage<NetIdMessage>();
            var obj = FindLocalComponent<CardController>(msg.netId);
            Action action = () => obj.ToggleColor();
            obj.RunNetworkAction(action, netMsg, msg, ignoreSender: true);
        }

        private static void CmdRotate(NetworkMessage netMsg)
        {
            var msg = netMsg.ReadMessage<IntNetIdMessage>();
            var obj = FindLocalComponent<CardController>(msg.netId);
            Action action = () => obj.Rotate(msg.value);
            obj.RunNetworkAction(action, netMsg, msg, ignoreSender: true);
        }

        private static void CmdScale(NetworkMessage netMsg)
        {
            var msg = netMsg.ReadMessage<FloatNetIdMessage>();
            var obj = FindLocalComponent<CardController>(msg.netId);
            Action action = () => obj.Scale(msg.value);
            obj.RunNetworkAction(action, netMsg, msg, ignoreSender: true);
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
            RegisterCallback((short)MsgType.ToggleColor, CmdToggleColor, registerClient: true);
            RegisterCallback((short)MsgType.Rotate, CmdRotate, registerClient: true);
            RegisterCallback((short)MsgType.Scale, CmdScale, registerClient: true);
        }
    }
}