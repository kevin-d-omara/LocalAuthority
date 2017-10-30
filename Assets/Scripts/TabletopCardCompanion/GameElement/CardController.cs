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
            ownership.RequestOwnership();
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
                networkPosition.ReleaseOwnership();
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
                SendCommand((short) MsgType.ToggleColor, msg);
            }

            if (Input.GetButtonDown(AxisName.Rotate))
            {
                var direction = Input.GetAxis("Rotate") > 0 ? 1 : -1;
                var degrees = 60 * direction;

                Rotate(degrees);

                var msg = NewMessage<IntCommandRecordMessage>();    // varargs constructor??
                msg.value = degrees;
                SendCommand((short) MsgType.Rotate, msg);
            }

            if (Input.GetButtonDown(AxisName.Scale))
            {
                var direction = Input.GetAxis("Scale") > 0 ? 1 : -1;
                var percent = 0.1f * direction;

                Scale(percent);

                var msg = NewMessage<FloatCommandRecordMessage>();
                msg.value = percent;
                SendCommand((short) MsgType.Scale, msg);
            }
        }


        // API (Controls) ------------------------------------------------------
        private void ToggleColor()
        {
            model.HookIsToggled(!model.IsToggled);
        }

        private void Rotate(int degrees)
        {
            model.transform.Rotate(Vector3.forward, degrees);
        }

        private void Scale(float percent)
        {
            var newScale = model.LocalScale * (1f + percent);
            model.HookLocalScale(newScale);
        }

        // Message RPCs (Update Model) -----------------------------------------
        [ClientRpc]
        private void RpcToggleColor(NetworkInstanceId requesterNetId)
        {
            if (requesterNetId == PlayerInfo.LocalPlayer.netId) return;

            ToggleColor();
        }

        [ClientRpc]
        private void RpcRotate(NetworkInstanceId requesterNetId, int degrees)
        {
            if (requesterNetId == PlayerInfo.LocalPlayer.netId) return;

            Rotate(degrees);
        }

        [ClientRpc]
        private void RpcScale(NetworkInstanceId requesterNetId, float percent)
        {
            if (requesterNetId == PlayerInfo.LocalPlayer.netId) return;

            Scale(percent);
        }

        // Message Commands ----------------------------------------------------
        private static void MsgCmdToggleColor(NetworkMessage netMsg)
        {
            var msg = netMsg.ReadMessage<CommandRecordMessage>();
            var controller = NetworkingUtilities.FindLocalComponent<CardController>(msg.netId);

            controller.RpcToggleColor(msg.cmdRecord.NetId);
        }

        private static void MsgCmdRotate(NetworkMessage netMsg)
        {
            var msg = netMsg.ReadMessage<IntCommandRecordMessage>();
            var controller = NetworkingUtilities.FindLocalComponent<CardController>(msg.netId);

            controller.RpcRotate(msg.cmdRecord.NetId, msg.value);
        }

        private static void MsgCmdScale(NetworkMessage netMsg)
        {
            var msg = netMsg.ReadMessage<FloatCommandRecordMessage>();
            var controller = NetworkingUtilities.FindLocalComponent<CardController>(msg.netId);

            controller.RpcScale(msg.cmdRecord.NetId, msg.value);
        }

        private static void MsgCmdRotate2(IntCommandRecordMessage msg, CardController controller)
        {
            controller.RpcRotate(msg.cmdRecord.NetId, msg.value);
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
            RegisterCallback((short)MsgType.ToggleColor, MsgCmdToggleColor);
            RegisterCallback((short)MsgType.Rotate, MsgCmdRotate);
            RegisterCallback((short)MsgType.Scale, MsgCmdScale);
        }
    }
}