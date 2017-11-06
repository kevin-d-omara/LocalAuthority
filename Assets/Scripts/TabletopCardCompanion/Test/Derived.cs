﻿using LocalAuthority.Message;
using TabletopCardCompanion.Debug;
using UnityEngine.Networking;

namespace TabletopCardCompanion
{
    public class Derived : Base
    {
        // Note: Attribute unecessary, but not harmful.
        //       Derived attribute overrides base attribute.
        [MessageRpc(ClientSidePrediction = true)]
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
            DebugStreamer.AddMessage("In Command: " + this + " " + this.GetHashCode());
        }

        private void Start()
        {
            //            var obj = Utility.FindLocalComponent<Base>(netId);
            //            DebugStreamer.AddMessage(obj);
            CmdDoIt();
            var x = new NetworkHash128();
        }
    }
}