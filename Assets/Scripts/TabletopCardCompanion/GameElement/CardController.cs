﻿using LocalAuthority;
using LocalAuthority.Components;
using UnityEngine;

namespace TabletopCardCompanion.GameElement
{
    [RequireComponent(typeof(Ownership))]
    [RequireComponent(typeof(NetworkPosition))]
    public class CardController : LocalAuthorityBehaviour
    {
        // Input Handler (Command Source) --------------------------------------
        private void OnMouseOver()
        {
            if (Input.GetButtonDown(AxisName.ToggleColor))
            {
                SendCallback(nameof(RpcToggleColor));
            }

            if (Input.GetButtonDown(AxisName.Rotate))
            {
                var direction = Input.GetAxis("Rotate") > 0 ? 1 : -1;
                var degrees = 60 * direction;

                SendCallback(nameof(RpcRotate), degrees);
            }

            if (Input.GetButtonDown(AxisName.Scale))
            {
                var direction = Input.GetAxis("Scale") > 0 ? 1 : -1;
                var percent = 0.1f * direction;

                SendCallback(nameof(RpcScale), percent);
            }
        }

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


        // Commands ------------------------------------------------------------

        [MessageRpc(ClientSidePrediction = true)]
        private void RpcToggleColor()
        {
            model.HookIsToggled(!model.IsToggled);
        }

        [MessageRpc(ClientSidePrediction = true)]
        private void RpcRotate(int degrees)
        {
            model.HookRotation(degrees);
        }

        [MessageRpc(ClientSidePrediction = true)]
        private void RpcScale(float percent)
        {
            var newScale = model.LocalScale * (1f + percent);
            model.HookLocalScale(newScale);
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
    }
}