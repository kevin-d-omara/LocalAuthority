using LocalAuthority.Components;
using TabletopCardCompanion.Components;
using UnityEngine;

namespace TabletopCardCompanion.PlayingPieces
{
    /// <summary>
    /// Represents a playing card (Poker, Magic the Gathering, Zombicide, etc.).
    /// <para>
    /// Uses smoothing and interpolation to keep state changes looking nice.
    /// </para>
    /// </summary>
    [RequireComponent(typeof(ClickAndDrag))]
    [RequireComponent(typeof(TwoSidedSprite))]
    [RequireComponent(typeof(Rotate))]
    [RequireComponent(typeof(Scale))]
    public class SmoothCard : LocalAuthorityBehaviour
    {
        // Keyboard Button Actions ---------------------------------------------

        private void OnMouseOver()
        {
            if (Input.GetButtonDown(AxisName.FlipOver))
            {
                twoSidedSprite.SendCallback(nameof(TwoSidedSprite.RpcFlipOver));
            }

            if (Input.GetButtonDown(AxisName.Rotate))
            {
                // Positive rotation is counter-clockwise when looking at the screen.
                var direction = Input.GetAxis("Rotate") > 0 ? -1 : 1;
                var degrees = rotate.DegreesPerAction * direction;

                rotate.SendCallback(nameof(Rotate.RpcRotate), degrees);
            }

            if (Input.GetButtonDown(AxisName.Scale))
            {
                var increaseSize = Input.GetAxis("Scale") > 0;

                scale.SendCallback(nameof(Scale.RpcScale), increaseSize);
            }
        }


        // Initialization ------------------------------------------------------

        private TwoSidedSprite twoSidedSprite;
        private Rotate rotate;
        private Scale scale;

        protected override void Awake()
        {
            base.Awake();
            twoSidedSprite = GetComponent<TwoSidedSprite>();
            rotate = GetComponent<Rotate>();
            scale = GetComponent<Scale>();
        }
    }
}