using LocalAuthority.Components;
using UnityEngine;

namespace TabletopCardCompanion.Components
{
    /// <summary>
    /// This component enables the game object to be moved by clicking on the object and dragging.
    /// Movement is allowed/restricted based on ownership.
    /// </summary>
    [RequireComponent(typeof(Ownership))]
    [RequireComponent(typeof(NetworkPosition))]
//    [RequireComponent(typeof(BoxCollider2D))]
    public class ClickAndDrag : MonoBehaviour
    {
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


        // Initialization ------------------------------------------------------

        private Ownership ownership;
        private NetworkPosition networkPosition;

        private void Awake()
        {
            ownership = GetComponent<Ownership>();
            networkPosition = GetComponent<NetworkPosition>();
        }
    }
}
