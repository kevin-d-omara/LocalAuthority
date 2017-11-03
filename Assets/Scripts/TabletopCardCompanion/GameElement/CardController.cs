using LocalAuthority.Components;
using LocalAuthority.Message;
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
                ToggleColor();

                SendCommand<NetIdMessage>((short)MsgType.ToggleColor);
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

        [MessageRpc((short)MsgType.ToggleColor, Predicted = true)]
        private void CmdToggleColor()
        {
            ToggleColor();
        }

        [MessageRpc((short)MsgType.Rotate, Predicted = true)]
        private void CmdRotate(IntNetIdMessage msg)
        {
            Rotate(msg.value);
        }

        [MessageRpc((short)MsgType.Scale, Predicted = true)]
        private void CmdScale(FloatNetIdMessage msg)
        {
            Scale(msg.value);
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