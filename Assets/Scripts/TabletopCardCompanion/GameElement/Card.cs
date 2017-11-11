using LocalAuthority.Components;
using TabletopCardCompanion.Components;
using UnityEngine;

namespace TabletopCardCompanion.GameElement
{
    [RequireComponent(typeof(ClickAndDrag))]
    [RequireComponent(typeof(TwoSidedSprite))]
    [RequireComponent(typeof(Rotate))]
    [RequireComponent(typeof(Scale))]
    public class Card : LocalAuthorityBehaviour
    {
        // Press Button Actions ------------------------------------------------

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
    }
}