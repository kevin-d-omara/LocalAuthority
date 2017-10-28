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