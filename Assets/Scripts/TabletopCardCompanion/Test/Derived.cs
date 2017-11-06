using LocalAuthority.Message;
using TabletopCardCompanion.Debug;
using UnityEngine.Networking;

namespace TabletopCardCompanion
{
    public class Derived : Base
    {
        // Note: Attribute unecessary, but not harmful.
        //       Derived attribute overrides base attribute.
        [MessageRpc(ClientSidePrediction = false)]
        public override void FlipOver()
        {
            DebugStreamer.AddMessage("Derived: FlipOver");
        }

        [MessageRpc(ClientSidePrediction = true)]
        public override void Scale(float percent)
        {
            DebugStreamer.AddMessage("Derived: Scale " + percent + " percent.");
        }

        [Command]
        private void CmdDoIt()
        {
            DebugStreamer.AddMessage("In Command: " + this + " " + GetHashCode());
        }

        private void Start()
        {
            // Unity manages to call [Command] on the exact script instance.
            // I have two of these scripts attached to the same game object, and both trigger with a different hashcode.
            CmdDoIt();
        }
    }
}
