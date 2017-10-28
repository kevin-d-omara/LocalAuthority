using LocalAuthority.Message;
using UnityEngine;
using UnityEngine.Networking;

namespace TabletopCardCompanion.GameElement
{
    [RequireComponent(typeof(Ownership))]
    [RequireComponent(typeof(NetworkPosition))]
    public class CardController : NetworkBehaviour
    {
        #region In-Progress

        private void OnMouseDown()
        {
            ownership.RequestOwnership();
        }

        private void OnMouseDrag()
        {
            if (ownership.IsOwnedByLocal || ownership.IsOwnedByNone)
            {
                MoveToMousePosition();
                // ^^ update NetworkPosition.TargetSyncPosition?
            }
        }

        private void OnMouseUp()
        {
            if (ownership.IsOwnedByLocal || ownership.IsOwnedByNone)
            {
                ownership.ReleaseOwnership();
                // broadcast final position
                // udpate targetsyncposition
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
                var msg = new NetIdMessage(netId);
                NetworkManager.singleton.client.Send((short)MsgType.ToggleColor, msg);
            }

            if (Input.GetButtonDown(AxisName.Rotate))
            {
                var direction = Input.GetAxis("Rotate") > 0 ? 1 : -1;
                var msg = new IntNetIdMessage(netId, 60 * direction);
                NetworkManager.singleton.client.Send((short)MsgType.Rotate, msg);
            }

            if (Input.GetButtonDown(AxisName.Scale))
            {
                var percent = Input.GetAxis("Scale") > 0 ? 1f : -1f;
                var msg = new FloatNetIdMessage(netId, 0.1f * percent);
                NetworkManager.singleton.client.Send((short)MsgType.Scale, msg);
            }
        }


        // Message Callbacks (Update Model) ------------------------------------
        private static void OnToggleColor(NetworkMessage netMsg)
        {
            var msg = netMsg.ReadMessage<NetIdMessage>();
            var model = NetworkingUtilities.FindLocalComponent<CardModel>(msg.netId);

            model.IsToggled = !model.IsToggled;
        }

        private static void OnRotate(NetworkMessage netMsg)
        {
            var msg = netMsg.ReadMessage<IntNetIdMessage>();
            var model = NetworkingUtilities.FindLocalComponent<CardModel>(msg.netId);

            var degrees = msg.value;
            model.transform.Rotate(Vector3.forward, degrees);
        }

        private static void OnScale(NetworkMessage netMsg)
        {
            var msg = netMsg.ReadMessage<FloatNetIdMessage>();
            var model = NetworkingUtilities.FindLocalComponent<CardModel>(msg.netId);

            var percent = msg.value;
            model.LocalScale *= 1f + percent;
        }


        // Initialization ------------------------------------------------------
        private Ownership ownership;
        private NetworkPosition networkPosition;

        private void Awake()
        {
            RegisterMessageCallbacks();
            ownership = GetComponent<Ownership>();
            networkPosition = GetComponent<NetworkPosition>();
        }

        private void RegisterMessageCallbacks()
        {
            NetworkServer.RegisterHandler((short)MsgType.ToggleColor, OnToggleColor);
            NetworkServer.RegisterHandler((short)MsgType.Rotate, OnRotate);
            NetworkServer.RegisterHandler((short)MsgType.Scale, OnScale);
        }

        private enum MsgType : short
        {
            ToggleColor = UnityEngine.Networking.MsgType.Highest + 1,
            Rotate,
            Scale,
        }
    }
}