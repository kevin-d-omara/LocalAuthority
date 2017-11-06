using System.Collections;
using System.Collections.Generic;
using LocalAuthority.Message;
using UnityEngine;
using UnityEngine.Networking;

namespace TabletopCardCompanion
{
    public class ShipControl : NetworkBehaviour
    {
        private static int kCmdCmdThrust = 42; // value??

        [Command]
        public void CmdThrust(float thrusting, int spin)
        {
            // do the thing
        }

        //  if (xyz) CmdThrust(thrust, spin) ==>
        //  if (xyz) CallCmdThrust(thrust, spin)
        public void CallCmdThrust(float thrusting, int spin)
        {
            UnityEngine.Debug.LogError("Call Command function CmdThrust");
            if (!NetworkClient.active)
            {
                UnityEngine.Debug.LogError("Command function CmdThrust called on server.");
                return;
            }

            if (isServer)
            {
                // we are ON the server, invoke directly
                CmdThrust(thrusting, spin);
                return;
            }

            // Pack up message:
            var args = new object[] {thrusting, spin};
            var msg = new VarArgsNetIdMessasge(netId, args);
            msg.msgType = UnityEngine.Networking.MsgType.Command; // change to my own pitstop message
            // msg.invokeTarget = (uint)ShipControl.kCmdCmdThrust // constant
            // msg.playerId = ??



//            NetworkWriter networkWriter = new NetworkWriter();
//            networkWriter.Write(0);
//
//            networkWriter.Write((ushort)MsgType.SYSTEM_COMMAND);
//            networkWriter.WritePackedUInt32((uint)ShipControl.kCmdCmdThrust);
//            networkWriter.WritePackedUInt32((uint)playerId);
//
//            networkWriter.Write(thrusting);
//            networkWriter.WritePackedUInt32((uint)spin);
//
//            base.SendCommandInternal(networkWriter);
        }

        // Method invoked "on the other side".
        protected static void InvokeCmdCmdThrust(NetworkBehaviour obj, NetworkReader reader)
        {
            if (!NetworkServer.active)
            {
                return;
            }

            ((ShipControl)obj).CmdThrust(reader.ReadSingle(), (int)reader.ReadPackedUInt32());
        }
    }
}
