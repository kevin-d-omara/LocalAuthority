using LocalAuthority;
using LocalAuthority.Components;
using UnityEngine;
using UnityEngine.Networking;

namespace TabletopCardCompanion.PlayingPieces
{
    /// <summary>
    /// Represents a playing card (Poker, Magic the Gathering, Zombicide, etc.).
    /// <para>
    /// Does not use smoothing. Animations and state changes are instant.
    /// </para>
    /// </summary>
    [RequireComponent(typeof(Ownership))]
    [RequireComponent(typeof(NetworkPosition))]
    [RequireComponent(typeof(SpriteRenderer))]
    public class InstantCard : LocalAuthorityBehaviour
    {
        // Keyboard Button Actions ---------------------------------------------

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


        // Click-and-Drag Movement ---------------------------------------------

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
            IsToggled = !IsToggled;
            spriteRenderer.color = IsToggled ? ToggleColor : Color.white;
        }

        [MessageRpc(ClientSidePrediction = true)]
        private void RpcRotate(int degrees)
        {
            RotationDegrees += degrees;
            transform.rotation = Quaternion.Euler(0f, 0f, RotationDegrees);
        }

        [MessageRpc(ClientSidePrediction = true)]
        private void RpcScale(float percent)
        {
            var newScale = LocalScale * (1f + percent);
            LocalScale = newScale;
            transform.localScale = LocalScale;
        }


        // Data ----------------------------------------------------------------

        // "[SyncVar]"
        public bool IsToggled { get; set; }

        // "[SyncVar]"
        public Vector3 LocalScale { get; set; }

        // "[SyncVar]"
        public float RotationDegrees { get; set; }

        public Color ToggleColor { get; } = Color.yellow;


        // Initialization ------------------------------------------------------

        private Ownership ownership;
        private NetworkPosition networkPosition;
        private SpriteRenderer spriteRenderer;

        protected override void Awake()
        {
            base.Awake();
            ownership = GetComponent<Ownership>();
            networkPosition = GetComponent<NetworkPosition>();
            spriteRenderer = GetComponent<SpriteRenderer>();
        }

        public override void OnStartServer()
        {
            base.OnStartServer();

            LocalScale = transform.localScale;
            RotationDegrees = transform.rotation.z;
        }

        public override void OnStartClient()
        {
            base.OnStartClient();

            // Apply SyncVar values to object view.
            spriteRenderer.color = IsToggled ? ToggleColor : Color.white;
            transform.rotation = Quaternion.Euler(0f, 0f, RotationDegrees);
            transform.localScale = LocalScale;
        }


        // Serialization -------------------------------------------------------

        // SyncVars are only sent to each client once, when they join the game.
        public override bool OnSerialize(NetworkWriter writer, bool initialState)
        {
            if (initialState)
            {
                // SyncVars
                writer.Write(IsToggled);
                writer.Write(LocalScale);
                writer.Write(RotationDegrees);
                return true;
            }

            return false;
        }

        // SyncVars are only read once, when the client joins the game.
        public override void OnDeserialize(NetworkReader reader, bool initialState)
        {
            if (initialState)
            {
                // SyncVars
                IsToggled = reader.ReadBoolean();
                LocalScale = reader.ReadVector3();
                RotationDegrees = reader.ReadSingle();
            }
        }
    }
}
