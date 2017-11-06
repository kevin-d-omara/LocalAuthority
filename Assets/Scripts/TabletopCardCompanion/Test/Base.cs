using LocalAuthority.Components;
using LocalAuthority.Message;
using TabletopCardCompanion.Debug;
using UnityEngine;

namespace TabletopCardCompanion
{
    public class Base : LocalAuthorityBehaviour
    {
        [MessageRpc((short)MsgType.BaseRotate, ClientSidePrediction = true)]
        public void Rotate(int degrees)
        {
            DebugStreamer.AddMessage("Base: Rotate " + degrees + " degrees.");
        }

        [MessageRpc((short) MsgType.BaseFlip, ClientSidePrediction = true)]
        public virtual void FlipOver()
        {
            DebugStreamer.AddMessage("Base: FlipOver");
        }

        [MessageRpc((short)MsgType.BaseScale, ClientSidePrediction = true)]
        public virtual void Scale(float percent)
        {
            DebugStreamer.AddMessage("Base: Scale " + percent + " percent.");
        }

        private void OnMouseOver()
        {
            if (Input.GetButtonDown(AxisName.ToggleColor))
            {
                InvokeRpc(nameof(FlipOver));
            }

            if (Input.GetButtonDown(AxisName.Rotate))
            {
                var direction = Input.GetAxis("Rotate") > 0 ? 1 : -1;
                var degrees = 60 * direction;

                InvokeRpc(nameof(Rotate), degrees);
            }

            if (Input.GetButtonDown(AxisName.Scale))
            {
                var direction = Input.GetAxis("Scale") > 0 ? 1 : -1;
                var percent = 0.1f * direction;

                InvokeRpc(nameof(Scale), percent);
            }
        }
    }
}
