using UnityEngine;
using UnityEngine.Networking;

namespace TabletopCardCompanion.GameElement
{
    public class CardModel : NetworkBehaviour
    {
        // Data ----------------------------------------------------------------
        public bool IsToggled { get; set; }

        public Vector3 LocalScale { get; set; }

        public Color ToggleColor { get; }= Color.yellow;


        // Hooks (Update View) -------------------------------------------------
        public void HookIsToggled(bool newState)
        {
            IsToggled = newState;
            view.ApplyIsToggled();
        }

        public void HookLocalScale(Vector3 newScale)
        {
            LocalScale = newScale;
            view.ApplyLocalScale();
            SetDirtyBit(1);
        }


        // Initialization ------------------------------------------------------
        private CardView view;

        private void Awake()
        {
            view = GetComponent<CardView>();
        }

        public override void OnStartServer()
        {
            base.OnStartServer();

            LocalScale = transform.localScale;
        }

        public override void OnStartClient()
        {
            base.OnStartClient();

            view.ApplyIsToggled();
            view.ApplyLocalScale();
        }


        // Serialization -------------------------------------------------------

        /// <summary>
        /// Only send SyncVars when a new client joins or the object is first created.
        /// </summary>
        public override bool OnSerialize(NetworkWriter writer, bool initialState)
        {
            if (initialState)
            {
                // SyncVars
                writer.Write(IsToggled);
                writer.Write(LocalScale);
                return true;
            }

            return false;
        }

        /// <summary>
        /// Only overwrite SyncVars when a new client joins or the object is first created.
        /// </summary>
        public override void OnDeserialize(NetworkReader reader, bool initialState)
        {
            if (initialState)
            {
                // SyncVars
                IsToggled = reader.ReadBoolean();
                LocalScale = reader.ReadVector3();
            }
        }
    }
}