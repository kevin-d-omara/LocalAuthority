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
            FlipOverTo(!isShowingFront);
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

        private void FlipOverTo(bool toFrontSide)
        {
            isShowingFront = toFrontSide;
            isChangingSides = true;
        }

        private void Update()
        {
            // Flipping over the card looks like:
            // Back                    Front
            //   |<----------|---------->|
            // -180°       -90°          0°
            // 0 sec                   1 sec
            if (isChangingSides)
            {
                // Rotate.
                dtLerp += (isShowingFront ? Time.deltaTime : -Time.deltaTime) * 1f / LERP_TIME;
                var newAngle = Mathf.LerpAngle(BACK_ANGLE, FRONT_ANGLE, dtLerp);
                transform.eulerAngles = new Vector3(0, newAngle, 0);

                // When 90 degrees is crossed, change the sprite.
                if (isShowingFront && newAngle >= -90f)
                {
                    spriteRenderer.sprite = frontSide;
                    spriteRenderer.flipX = false;
                    boxCollider.size = spriteRenderer.sprite.bounds.size;
                }
                else if (!isShowingFront && newAngle <= -90f)
                {
                    spriteRenderer.sprite = backSide;
                    spriteRenderer.flipX = true;
                    boxCollider.size = spriteRenderer.sprite.bounds.size;
                }

                // Stop lerping.
                if (dtLerp <= BACK_TIME || dtLerp >= FRONT_TIME)
                {
                    isChangingSides = false;
                    dtLerp = isShowingFront ? FRONT_TIME : BACK_TIME;
                }
            }
        }

        // Model ---------------------------------------------------------------

        [SerializeField] private Sprite frontSide;
        [SerializeField] public Sprite backSide;

        [SyncVar]
        [SerializeField] public bool isShowingFront = true;

        private bool isChangingSides;
        private float dtLerp;
        private const float FRONT_ANGLE = 0f;
        private const float BACK_ANGLE = -180f;
        private const float FRONT_TIME = 1f;
        private const float BACK_TIME = 0f;
        private const float LERP_TIME = .75f;


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

            // Show correct side, without lerping to it.
            if (isShowingFront)
            {
                spriteRenderer.sprite = frontSide;
                spriteRenderer.flipX = false;
                transform.eulerAngles = new Vector3(0f, FRONT_ANGLE, 0f);
                dtLerp = FRONT_TIME;
            }
            else
            {
                spriteRenderer.sprite = backSide;
                spriteRenderer.flipX = true;
                transform.eulerAngles = new Vector3(0f, BACK_ANGLE, 0f);
                dtLerp = BACK_TIME;
            }
            boxCollider.size = spriteRenderer.sprite.bounds.size;
        }


        // Editor --------------------------------------------------------------

        private void OnValidate()
        {
            if (frontSide == null || backSide == null)
                return;

            // Get components.
            spriteRenderer = GetComponent<SpriteRenderer>();
            boxCollider = GetComponent<BoxCollider2D>();

            // Display correct sprite.
            // Note: simplified from runtime logic. Not flipping x-dir or rotating.
            spriteRenderer.sprite = isShowingFront ? frontSide : backSide;

            // Match box collider width/height to sprite.
            boxCollider.size = spriteRenderer.sprite.bounds.size;
        }
    }
}