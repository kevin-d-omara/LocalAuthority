using NonPlayerClientAuthority;
using UnityEngine;
using UnityEngine.Networking;

namespace TabletopCardCompanion
{
    public class CardController : NetworkBehaviour
    {
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


        private void Awake()
        {
            RegisterMessageCallbacks();
        }

        private void RegisterMessageCallbacks()
        {
            NetworkServer.RegisterHandler(MsgType.ToggleColor, OnToggleColor);
            NetworkServer.RegisterHandler(MsgType.Rotate, OnRotate);
            NetworkServer.RegisterHandler(MsgType.Scale, OnScale);
        }

        private static class MsgType
        {
            public static readonly short ToggleColor = MsgTypeUid.Next;
            public static readonly short Rotate = MsgTypeUid.Next;
            public static readonly short Scale = MsgTypeUid.Next;
        }
    }
}