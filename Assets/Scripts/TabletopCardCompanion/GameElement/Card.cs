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
                // Positive rotation is counter-clockwise when looking at the screen.
                var direction = Input.GetAxis("Rotate") > 0 ? -1 : 1;
                var degrees = DEGREES_PER_ACTION * direction;

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
            isShowingFront = !isShowingFront;
            isChangingSides = true;
            DebugStreamer.AddMessage("FlipOver");
        }

        [MessageRpc(ClientSidePrediction = true)]
        private void RpcRotate(int degrees)
        {
            targetRotateAngle += degrees;
            isRotating = true;
            DebugStreamer.AddMessage("Rotate");
        }

        [MessageRpc(ClientSidePrediction = true)]
        private void RpcScale(float percent)
        {
            DebugStreamer.AddMessage("Scale");
        }


        // Update Model and View -----------------------------------------------

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
                var deltaAngle = newAngle - abstractEuler.y;
                transform.RotateAround(transform.position, transform.up, deltaAngle);
                abstractEuler.y = newAngle;

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
                if (dtLerp <= 0f || dtLerp >= 1f)
                {
                    isChangingSides = false;
                    dtLerp = isShowingFront ? 1f : 0f;
                }
            }

            // Rotate with a fixed velocity (degrees per second).
            if (isRotating)
            {
                var currentAngle = abstractEuler.z;
                var newAngle = Mathf.MoveTowards(currentAngle, targetRotateAngle, ROTATE_SPEED * Time.deltaTime);
                var deltaAngle = newAngle - currentAngle;
                abstractEuler.z = newAngle;

                var distToTarget = Mathf.Abs(targetRotateAngle - currentAngle);
                var distToNew    = Mathf.Abs(newAngle - currentAngle);
                if (distToNew > distToTarget)
                {
                    deltaAngle = targetRotateAngle - currentAngle;
                    isRotating = false;
                    abstractEuler.z = targetRotateAngle;
                }
                transform.RotateAround(transform.position, Vector3.forward, deltaAngle);
            }
        }

        // Model ---------------------------------------------------------------

        // For tracking rotations along each axis independent of each other.
        private Vector3 abstractEuler;

        // Two-Sided Sprite / Flip Over
        [SerializeField] private Sprite frontSide;
        [SerializeField] private Sprite backSide;

        [SyncVar]
        [SerializeField] private bool isShowingFront = true;

        private bool isChangingSides;
        private float dtLerp;
        private const float FRONT_ANGLE = 0f;
        private const float BACK_ANGLE = -180f;
        private const float LERP_TIME = .75f;

        // Rotation
        [SyncVar]
        private float targetRotateAngle;

        private bool isRotating;
        private const float ROTATE_SPEED = 120; // degrees per second
        private const int DEGREES_PER_ACTION = 45;


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

        public override void OnStartServer()
        {
            base.OnStartServer();

            targetRotateAngle = transform.eulerAngles.z;
        }

        public override void OnStartClient()
        {
            base.OnStartClient();

            // Show correct side, without lerping to it.
            if (isShowingFront)
            {
                spriteRenderer.sprite = frontSide;
                spriteRenderer.flipX = false;
                dtLerp = 1f;
            }
            else
            {
                spriteRenderer.sprite = backSide;
                spriteRenderer.flipX = true;
                dtLerp = 0f;
            }
            boxCollider.size = spriteRenderer.sprite.bounds.size;

            // Show correct rotation, without lerping to it.
            abstractEuler.x = transform.eulerAngles.x;
            abstractEuler.y = isShowingFront ? FRONT_ANGLE : BACK_ANGLE;
            abstractEuler.z = targetRotateAngle;
            transform.RotateAround(transform.position, transform.up, abstractEuler.y);
            transform.RotateAround(transform.position, Vector3.forward, abstractEuler.z);
        }


        // Serialization -------------------------------------------------------

        // This is an optimization to minimize network bandwidth. SyncVars are only being used to make sure late-joining
        // clients are up-to-date. These OnSerialize/OnDeserialize methods *only* read/write data for the initial state.

//        /// <summary>
//        /// Only send SyncVars when a new client joins or the object is first created.
//        /// </summary>
//        public override bool OnSerialize(NetworkWriter writer, bool initialState)
//        {
//            if (initialState)
//            {
//                // SyncVars
//                writer.Write(isShowingFront);
//                writer.Write(targetRotateAngle);
//                return true;
//            }
//
//            return false;
//        }
//
//        /// <summary>
//        /// Only overwrite SyncVars when a new client joins or the object is first created.
//        /// </summary>
//        public override void OnDeserialize(NetworkReader reader, bool initialState)
//        {
//            if (initialState)
//            {
//                // SyncVars
//                isShowingFront = reader.ReadBoolean();
//                targetRotateAngle = reader.ReadSingle();
//            }
//        }


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