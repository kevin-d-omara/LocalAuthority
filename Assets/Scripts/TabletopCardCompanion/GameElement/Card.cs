using LocalAuthority;
using LocalAuthority.Components;
using TabletopCardCompanion.Debug;
using UnityEngine;
using UnityEngine.Networking;

namespace TabletopCardCompanion.GameElement
{
    [RequireComponent(typeof(Ownership))]
    [RequireComponent(typeof(NetworkPosition))]
    [RequireComponent(typeof(BoxCollider2D))]
    [RequireComponent(typeof(SpriteRenderer))]
    public class Card : LocalAuthorityBehaviour
    {
        // Press Button Actions ------------------------------------------------

        private void OnMouseOver()
        {
            if (Input.GetButtonDown(AxisName.FlipOver))
            {
                SendCallback(nameof(RpcFlipOver));
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


        // Mouse Drag Movement -------------------------------------------------

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
        private void RpcFlipOver()
        {
            OnFlipOver(!isShowingFront);
        }

        [MessageRpc(ClientSidePrediction = true)]
        private void RpcRotate(int degrees)
        {
            DebugStreamer.AddMessage("Rotate");
        }

        [MessageRpc(ClientSidePrediction = true)]
        private void RpcScale(float percent)
        {
            DebugStreamer.AddMessage("Scale");
        }


        // Update Model and View -----------------------------------------------

        private void OnFlipOver(bool newValue)
        {
            isShowingFront = newValue;
            spriteRenderer.sprite = isShowingFront ? frontSide : backSide;

            // Update box collider width/height.
            boxCollider.size = spriteRenderer.sprite.bounds.size;
        }

        // Model ---------------------------------------------------------------

        [SerializeField] private Sprite frontSide;
        [SerializeField] public Sprite backSide;

        [SyncVar]
        [SerializeField] public bool isShowingFront = true;


        // Initialization ------------------------------------------------------

        private Ownership ownership;
        private NetworkPosition networkPosition;
        private BoxCollider2D boxCollider;
        private SpriteRenderer spriteRenderer;

        protected override void Awake()
        {
            base.Awake();
            ownership = GetComponent<Ownership>();
            networkPosition = GetComponent<NetworkPosition>();
            boxCollider = GetComponent<BoxCollider2D>();
            spriteRenderer = GetComponent<SpriteRenderer>();
        }

        public override void OnStartClient()
        {
            base.OnStartClient();

            OnFlipOver(isShowingFront);
        }


        // Editor --------------------------------------------------------------

        private void OnValidate()
        {
            if (frontSide == null || backSide == null)
                return;

            // Display correct sprite.
            var renderer = GetComponent<SpriteRenderer>();
            renderer.sprite = isShowingFront ? frontSide : backSide;

            // Match box collider width/height to sprite.
            var collider = GetComponent<BoxCollider2D>();
            collider.size = renderer.sprite.bounds.size;
        }
    }
}