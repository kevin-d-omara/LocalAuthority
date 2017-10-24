using NonPlayerClientAuthority;
using TabletopCardCompanion.Utils;
using UnityEngine;
using UnityEngine.Networking;

namespace TabletopCardCompanion
{
    public class CardController : NetworkBehaviour
    {
        public override void OnStartClient()
        {
            base.OnStartClient();
        }

        private void Update()
        {
            if (Input.GetButtonDown("Cancel"))
            {
                var netTransform = GetComponent<NetworkTransform>();
                var identity = GetComponent<NetworkIdentity>();
                CommandAuthorizer.Instance.CmdReleaseOwnership(identity);
            }

            if (Input.GetButtonDown("Submit"))
            {
                var identity = GetComponent<NetworkIdentity>();
                // TODO: take ownership from others
                CommandAuthorizer.Instance.CmdRequestOwnership(identity);
            }

            var speed = 0.2f;
            var dx = Input.GetAxis("Horizontal") * speed;
            var dy = Input.GetAxis("Vertical") * speed;

            var deltaPos = new Vector3(dx, dy, 0f);
            var msg = new Vector3NetIdMessage(netId, deltaPos);
//            NetworkManager.singleton.client.Send(MsgType.Move, msg);

            transform.position += deltaPos;

            var netTrans = GetComponent<NetworkTransform>();
            DebugStreamer.AddMessage("pos: " + netTrans.targetSyncPosition + "; vel: " + netTrans.targetSyncVelocity);
        }

        private static void OnMoved(NetworkMessage netMsg)
        {
            var msg = netMsg.ReadMessage<Vector3NetIdMessage>();
            var model = NetworkingUtilities.FindLocalComponent<CardModel>(msg.netId);

            var delta = msg.value;
            model.transform.position += delta;
        }

        private void OnMouseDown()
        {
            CommandAuthorizer.Instance.CmdRequestOwnership(GetComponent<NetworkIdentity>());
        }

        private void OnMouseUp()
        {
//            CommandAuthorizer.Instance.CmdReleaseOwnership(GetComponent<NetworkIdentity>());
        }

        private void OnMouseDrag()
        {
            var cam = FindObjectOfType<Camera>();
            var mousePosition = cam.ScreenToWorldPoint(Input.mousePosition);
            mousePosition.z = 0f;

            var delta = mousePosition - transform.position;

            transform.position += delta;
            DebugStreamer.AddMessage(delta);
        }


        private void OnMouseOver()
        {
            if (Input.GetButtonDown("Jump"))
            {
                var msg = new NetIdMessage(netId);
                NetworkManager.singleton.client.Send(MsgType.ToggleColor, msg);
            }
            if (Input.GetButtonDown("Rotate"))
            {
                var direction = Input.GetAxis("Rotate") > 0 ? 1 : -1;
                var msg = new IntNetIdMessage(netId, 60 * direction);
                NetworkManager.singleton.client.Send(MsgType.Rotate, msg);
            }
            if (Input.GetButtonDown("Scale"))
            {
                var percent = Input.GetAxis("Scale") > 0 ? 1f : -1f;
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
            NetworkServer.RegisterHandler(MsgType.Move, OnMoved);
        }

        private static class MsgType
        {
            public static readonly short ToggleColor = MsgTypeUid.Next;
            public static readonly short Rotate = MsgTypeUid.Next;
            public static readonly short Scale = MsgTypeUid.Next;
            public static readonly short Move = MsgTypeUid.Next;
        }
    }
}