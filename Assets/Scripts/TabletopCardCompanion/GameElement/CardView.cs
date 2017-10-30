using UnityEngine;

namespace TabletopCardCompanion.GameElement
{
    [RequireComponent(typeof(SpriteRenderer))]
    public class CardView : MonoBehaviour
    {
        // Update View ---------------------------------------------------------
        public void ApplyIsToggled()
        {
            spriteRenderer.color = model.IsToggled ? model.ToggleColor : Color.white;
        }

        public void ApplyLocalScale()
        {
            transform.localScale = model.LocalScale;
        }

        public void ApplyRotation()
        {
            model.transform.rotation = Quaternion.Euler(0f, 0f, model.RotationDegrees);
        }


        // Initialization ------------------------------------------------------
        private SpriteRenderer spriteRenderer;
        private CardModel model;

        private void Awake()
        {
            spriteRenderer = GetComponent<SpriteRenderer>();
            model = GetComponent<CardModel>();
        }
    }
}