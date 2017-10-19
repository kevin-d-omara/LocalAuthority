using UnityEngine;

namespace TabletopCardCompanion
{
    [RequireComponent(typeof(SpriteRenderer))]
    public class CardView : MonoBehaviour
    {
        public void ApplyIsToggled()
        {
            spriteRenderer.color = model.IsToggled ? model.ToggleColor : Color.white;
        }

        public void ApplyLocalScale()
        {
            transform.localScale = model.LocalScale;
        }


        private SpriteRenderer spriteRenderer;
        private CardModel model;

        private void Awake()
        {
            spriteRenderer = GetComponent<SpriteRenderer>();
            model = GetComponent<CardModel>();
        }
    }
}