using LocalAuthority;
using LocalAuthority.Components;
using UnityEngine;
using UnityEngine.Networking;

namespace TabletopCardCompanion.Components
{
    /// <summary>
    /// This component enables the game object's size to be increased or decreased by fixed amounts.
    /// </summary>
    public class Scale : LocalAuthorityBehaviour
    {

        // Commands ------------------------------------------------------------

        [MessageRpc(ClientSidePrediction = true)]
        public void RpcScale(bool increaseSize)
        {
            var increment = increaseSize ? ScaleIncrement : -ScaleIncrement;
            var newScale = localScale * (1f + increment);

            localScale = newScale;
            transform.localScale = newScale;
        }


        // Model ---------------------------------------------------------------

        /// <summary>
        /// Percent to scale up or down each time.
        /// </summary>
        [Range(0f, 1f)]
        public float ScaleIncrement = 0.1f;

        /// <summary>
        /// This is identical to <c>transform.localScale</c>, except as a SyncVar.
        /// </summary>
        [SyncVar]
        private Vector3 localScale;


        // Initialization ------------------------------------------------------

        public override void OnStartServer()
        {
            base.OnStartServer();
            localScale = transform.localScale;
        }

        public override void OnStartClient()
        {
            base.OnStartClient();
            transform.localScale = localScale;
        }


        // Serialization -------------------------------------------------------

        // SyncVars are only sent to each client once, when they join the game.
        public override bool OnSerialize(NetworkWriter writer, bool initialState)
        {
            if (initialState)
            {
                // SyncVars
                writer.Write(localScale);
                return true;
            }

            return false;
        }

        // SyncVars are only read once, when the client joins the game.
        public override void OnDeserialize(NetworkReader reader, bool initialState)
        {
            if (initialState)
            {
                // SyncVars
                localScale = reader.ReadVector3();
            }
        }
    }
}
