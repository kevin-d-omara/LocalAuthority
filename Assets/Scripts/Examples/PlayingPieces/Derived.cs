using Examples.Debug;
using LocalAuthority;
using UnityEngine.Networking;

namespace Examples.PlayingPieces
{
    /// <summary>
    /// This class exists to demonstrate that <see cref="MessageBasedCallback"/> attributes work properly with inheritance.
    /// </summary>
    public class Derived : Base
    {
        // ClientSidePrediction is able to be overriden for derived class.
        [MessageRpc(ClientSidePrediction = false)]
        public override void FlipOver()
        {
            DebugStreamer.AddMessage("Derived: FlipOver");
        }

        // Don't actually need [MessageRpc()] attribute on child class.
        public override void Scale(float percent)
        {
            DebugStreamer.AddMessage("Derived: Scale " + percent + " percent.");
        }



        // These are temporary, for learning about how to accomodate duplicate scripts on the same game object.
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
