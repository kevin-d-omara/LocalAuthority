using LocalAuthority;
using LocalAuthority.Components;
using UnityEngine;
using UnityEngine.Networking;

namespace Examples.PlayingPieces
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
    [RequireComponent(typeof(BoxCollider2D))]
    public class InstantCard : LocalAuthorityBehaviour
    {
        // Keyboard Button Actions ---------------------------------------------

        private void OnMouseOver()
        {
            if (Input.GetButtonDown(AxisName.ToggleColor))
            {
                SendCallback(nameof(RpcFlipOver));
            }

            if (Input.GetButtonDown(AxisName.Rotate))
            {
                // Positive rotation is counter-clockwise when looking at the screen.
                var direction = Input.GetAxis("Rotate") > 0 ? -1 : 1;
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


        // Callbacks -----------------------------------------------------------

        [MessageRpc(ClientSidePrediction = true)]
        private void RpcFlipOver()
        {
            isShowingFront = !isShowingFront;
            UpdateSpriteAndCollider();
        }

        private void UpdateSpriteAndCollider()
        {
            spriteRenderer.sprite = isShowingFront ? frontSide : backSide;
            boxCollider.size = spriteRenderer.sprite.bounds.size;
        }

        [MessageRpc(ClientSidePrediction = true)]
        private void RpcRotate(int degrees)
        {
            rotationDegrees += degrees;
            transform.rotation = Quaternion.Euler(0f, 0f, rotationDegrees);
        }

        [MessageRpc(ClientSidePrediction = true)]
        private void RpcScale(float percent)
        {
            var newScale = localScale * (1f + percent);
            localScale = newScale;
            transform.localScale = localScale;
        }


        // Data ----------------------------------------------------------------

        [SerializeField] private Sprite frontSide;
        [SerializeField] private Sprite backSide;

        [SyncVar]
        [SerializeField]
        private bool isShowingFront = true;

        [SyncVar]
        private Vector3 localScale;

        [SyncVar]
        private float rotationDegrees;


        // Initialization ------------------------------------------------------

        private Ownership ownership;
        private NetworkPosition networkPosition;
        private SpriteRenderer spriteRenderer;
        private BoxCollider2D boxCollider;

        protected override void Awake()
        {
            base.Awake();
            ownership = GetComponent<Ownership>();
            networkPosition = GetComponent<NetworkPosition>();
            spriteRenderer = GetComponent<SpriteRenderer>();
            boxCollider = GetComponent<BoxCollider2D>();
        }

        public override void OnStartServer()
        {
            base.OnStartServer();

            localScale = transform.localScale;
            rotationDegrees = transform.rotation.z;
        }

        public override void OnStartClient()
        {
            base.OnStartClient();

            // Apply SyncVar values to object view.
            transform.rotation = Quaternion.Euler(0f, 0f, rotationDegrees);
            transform.localScale = localScale;
            UpdateSpriteAndCollider();
        }


        // Serialization -------------------------------------------------------

        // SyncVars are only sent to each client once, when they join the game.
        public override bool OnSerialize(NetworkWriter writer, bool initialState)
        {
            if (initialState)
            {
                // SyncVars
                writer.Write(isShowingFront);
                writer.Write(localScale);
                writer.Write(rotationDegrees);
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
                isShowingFront = reader.ReadBoolean();
                localScale = reader.ReadVector3();
                rotationDegrees = reader.ReadSingle();
            }
        }


        // Editor --------------------------------------------------------------

        private void OnValidate()
        {
            if (frontSide == null || backSide == null)
                return;

            // Get components.
            spriteRenderer = GetComponent<SpriteRenderer>();
            boxCollider = GetComponent<BoxCollider2D>();

            UpdateSpriteAndCollider();
        }
    }
}
