using System;
using UnityEngine;
using UnityEngine.Networking;

namespace TabletopCardCompanion.GameElement
{
    public class CardModel : NetworkBehaviour
    {
        [SyncVar(hook = nameof(HookIsToggled))]
        [NonSerialized]
        public bool IsToggled;

        [SyncVar(hook = nameof(HookLocalScale))]
        [NonSerialized]
        public Vector3 LocalScale;

        [NonSerialized]
        public readonly Color ToggleColor = Color.yellow;

        private void HookIsToggled(bool newState)
        {
            IsToggled = newState;
            view.ApplyIsToggled();
        }

        private void HookLocalScale(Vector3 newScale)
        {
            LocalScale = newScale;
            view.ApplyLocalScale();
        }

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
    }
}