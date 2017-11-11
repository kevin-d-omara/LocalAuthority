using LocalAuthority;
using LocalAuthority.Components;
using UnityEngine;
using UnityEngine.Networking;

namespace TabletopCardCompanion.Components
{
    /// <summary>
    /// This component enables a game object to be rotated smoothly in fixed increments.
    /// It could be used for "tapping" a card, changing a model's facing in a grid-based boardgame, etc.
    /// </summary>
    public class Rotate : LocalAuthorityBehaviour
    {

        // Commands ------------------------------------------------------------

        [MessageRpc(ClientSidePrediction = true)]
        public void RpcRotate(float degrees)
        {
            targetRotateAngle += degrees;
            isRotating = true;
        }


        // Update Model and View -----------------------------------------------

        private void Update()
        {
            // Rotate with a fixed velocity (degrees per second).
            if (isRotating)
            {
                var currentAngle = this.currentAngle;
                var newAngle = Mathf.MoveTowards(currentAngle, targetRotateAngle, RotateSpeed * Time.deltaTime);
                var deltaAngle = newAngle - currentAngle;
                this.currentAngle = newAngle;

                var distToTarget = Mathf.Abs(targetRotateAngle - currentAngle);
                var distToNew = Mathf.Abs(newAngle - currentAngle);
                if (distToNew > distToTarget)
                {
                    deltaAngle = targetRotateAngle - currentAngle;
                    isRotating = false;
                    this.currentAngle = targetRotateAngle;
                }
                transform.RotateAround(transform.position, Vector3.forward, deltaAngle);
            }
        }


        // Model ---------------------------------------------------------------

        [Range(0f, 180f)]
        public float DegreesPerAction = 45f;

        /// <summary>
        /// The speed of rotation in degrees per second.
        /// </summary>
        [Tooltip("The speed of rotation in degrees per second.")]
        [Range(0f, 720f)]
        public float RotateSpeed = 120f;

        /// <summary>
        /// The angle the object is rotating towards along the rotation axis.
        /// </summary>
        [SyncVar]
        private float targetRotateAngle;

        /// <summary>
        /// Measures rotation along the rotation axis, independent of the object's's actual orientation.
        /// </summary>
        private float currentAngle;

        private bool isRotating;


        // Initialization ------------------------------------------------------

        public override void OnStartServer()
        {
            base.OnStartServer();
            targetRotateAngle = transform.eulerAngles.z;
        }

        public override void OnStartClient()
        {
            base.OnStartClient();

            // Show correct rotation, without lerping to it.
            currentAngle = targetRotateAngle;
            transform.RotateAround(transform.position, Vector3.forward, currentAngle);
        }
    }
}
