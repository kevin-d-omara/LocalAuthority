using LocalAuthority;
using LocalAuthority.Components;
using UnityEngine;
using UnityEngine.Networking;

namespace Examples.Components
{
    /// <summary>
    /// This component represents a double-sided sprite.
    /// It could be used for playing cards, boardgame tiles, etc.
    /// <para>
    /// Having a BoxCollider2D attached to this game object is optional.
    /// </para>
    /// </summary>
    [RequireComponent(typeof(SpriteRenderer))]
    public class TwoSidedSprite : LocalAuthorityBehaviour
    {

        // Commands ------------------------------------------------------------

        [MessageRpc(ClientSidePrediction = true)]
        public void RpcFlipOver()
        {
            isShowingFront = !isShowingFront;
            isChangingSides = true;
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
                elapsedTime += (isShowingFront ? Time.deltaTime : -Time.deltaTime) * 1f / LerpDuration;
                var newAngle = Mathf.LerpAngle(BACK_ANGLE, FRONT_ANGLE, elapsedTime);
                var deltaAngle = newAngle - currentAngle;
                transform.RotateAround(transform.position, transform.up, deltaAngle);
                currentAngle = newAngle;

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
                if (elapsedTime <= 0f || elapsedTime >= 1f)
                {
                    isChangingSides = false;
                    elapsedTime = isShowingFront ? 1f : 0f;
                }
            }
        }


        // Model ---------------------------------------------------------------

        [SerializeField] private Sprite frontSide;
        [SerializeField] private Sprite backSide;

        /// <summary>
        /// True if the front side is showing *or* being lerped to.
        /// </summary>
        [SyncVar]
        [SerializeField]
        private bool isShowingFront = true;

        /// <summary>
        /// The duration in seconds to flip from one side to the other.
        /// </summary>
        [Tooltip("The duration in seconds to flip from one side to the other.")]
        [Range(0.01f, 2f)]
        public float LerpDuration = 0.75f;

        private bool isChangingSides;
        private float elapsedTime;
        private const float FRONT_ANGLE = 0f;
        private const float BACK_ANGLE = -180f;

        /// <summary>
        /// Measures rotation along the flipping axis, independent of the object's actual orientation.
        /// </summary>
        private float currentAngle;


        // Initialization ------------------------------------------------------

        private BoxCollider2D boxCollider;
        private SpriteRenderer spriteRenderer;

        protected override void Awake()
        {
            base.Awake();
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
                elapsedTime = 1f;
            }
            else
            {
                spriteRenderer.sprite = backSide;
                spriteRenderer.flipX = true;
                elapsedTime = 0f;
            }

            // Match box collider width/height to sprite.
            if (boxCollider != null)
            {
                boxCollider.size = spriteRenderer.sprite.bounds.size;
            }

            // Show correct rotation, without lerping to it.
            currentAngle = isShowingFront ? FRONT_ANGLE : BACK_ANGLE;
            transform.RotateAround(transform.position, transform.up, currentAngle);
        }


        // Serialization -------------------------------------------------------

        // SyncVars are only sent to each client once, when they join the game.
        public override bool OnSerialize(NetworkWriter writer, bool initialState)
        {
            if (initialState)
            {
                // SyncVars
                writer.Write(isShowingFront);
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

            // Display correct sprite.
            spriteRenderer.sprite = isShowingFront ? frontSide : backSide;

            // Match box collider width/height to sprite.
            if (boxCollider != null)
            {
                boxCollider.size = spriteRenderer.sprite.bounds.size;
            }
        }
    }
}
