using System;
using LocalAuthority.Components;
using UnityEngine.Networking;

namespace LocalAuthority.Message
{
    /// <summary>
    /// This is an attribute that can be put on methods of a <see cref="LocalAuthorityBehaviour"/> class to allow them
    /// to be invoked on the server by sending a message from a client.
    /// <para>
    /// [MessageCommand] functions may be invoked from any LocalAuthorityBehaviour, even those not attached to the
    /// player GameObject. Invoke a [MessageCommand] by using <see cref="LocalAuthorityBehaviour.SendCommand"/>.
    /// </para>
    /// <para>
    /// These functions must be static and match the <see cref="NetworkMessageDelegate"/> signature. I.e.
    /// <code>static void MyMessageCommand(NetworkMessageDelegate netMsg)</code>
    /// </para>
    /// </summary>
    [AttributeUsage(AttributeTargets.Method)]
    class MessageCommand : Attribute
    {
        /// <summary>
        /// A number unique to this callback. <see cref="MsgType"/> and <see cref="UnityEngine.Networking.MsgType"/>
        /// </summary>
        public short MsgType { get; set; }

        public MessageCommand(short msgType)
        {
            MsgType = msgType;
        }
    }
}
