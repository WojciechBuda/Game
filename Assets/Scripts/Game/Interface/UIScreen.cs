using System.Collections.Generic;
using JellyDrop.Util;
using JellyDrop.Visuals;
using UnityEngine;
using UnityEngine.UI;

namespace JellyDrop.Interface
{
    /// <summary>Base class for every screen and popup. Each screen builds its own UI in code.</summary>
    public abstract class UIScreen : MonoBehaviour
    {
        protected App App { get; private set; }
        protected RectTransform Root { get; private set; }
        public bool IsModal { get; protected set; }
        public bool IsVisible => gameObject.activeSelf;

        private CanvasGroup _group;
        /// <summary>For modals: the panel that pops in.</summary>
        protected RectTransform PopTarget;

        public void Setup(App app)
        {
            App = app;
            Root = (RectTransform)transform;
            Root.Fill();
            _group = gameObject.AddComponent<CanvasGroup>();
            Build();
            gameObject.SetActive(false);
        }

        protected abstract void Build();

        /// <summary>Called whenever player data changes while the screen is visible.</summary>
        public virtual void Refresh() { }

        protected virtual void OnShow() { }
        protected virtual void OnHide() { }

        /// <summary>Android back button. Return true if handled.</summary>
        public virtual bool OnBack() => false;

        public void Show()
        {
            gameObject.SetActive(true);
            transform.SetAsLastSibling();
            _group.alpha = 0f;
            _group.interactable = true;
            _group.blocksRaycasts = true;
            OnShow();
            Refresh();
            var g = _group;
            Tweener.Run(0.18f, k => { if (g != null) g.alpha = k; }, Ease.OutQuad, owner: this);
            if (PopTarget != null) UIKit.Pop(PopTarget, 0.7f, 0.35f);
        }

        public void Hide()
        {
            if (!gameObject.activeSelf) return;
            OnHide();
            _group.interactable = false;
            _group.blocksRaycasts = false;
            var g = _group;
            var go = gameObject;
            Tweener.Run(0.14f, k => { if (g != null) g.alpha = 1f - k; }, Ease.OutQuad,
                () => { if (go != null && g != null && !g.blocksRaycasts) go.SetActive(false); }, owner: this);
        }

        /// <summary>Adds the dimmed backdrop + centred card used by popups. Returns the card.</summary>
        protected RectTransform BuildModalCard(Vector2 size, Color cardColor, Color? accent = null)
        {
            IsModal = true;
            var dim = UIKit.Image(Root, null, PixelCanvas.WithAlpha(ColorUtil.Night, 0.78f), true, "Dim");
            dim.rectTransform.Fill();
            var card = UIKit.NeonCard(Root, accent ?? ColorUtil.Pink, cardColor, 0.8f, true, "Card");
            card.rectTransform.At(UIKit.Center, Vector2.zero, size);
            PopTarget = card.rectTransform;
            return card.rectTransform;
        }
    }

    /// <summary>One base screen at a time plus a stack of modal popups on top.</summary>
    public sealed class ScreenStack
    {
        private UIScreen _current;
        private readonly List<UIScreen> _modals = new List<UIScreen>();

        public UIScreen Current => _current;
        public UIScreen TopModal => _modals.Count > 0 ? _modals[_modals.Count - 1] : null;
        public bool HasModal => _modals.Count > 0;

        public void Show(UIScreen screen)
        {
            CloseAllModals();
            if (_current == screen && screen.IsVisible) { screen.Refresh(); return; }
            _current?.Hide();
            _current = screen;
            screen.Show();
        }

        public void PushModal(UIScreen modal)
        {
            _modals.Remove(modal);
            _modals.Add(modal);
            modal.Show();
        }

        public void PopModal(UIScreen modal = null)
        {
            if (_modals.Count == 0) return;
            var m = modal ?? _modals[_modals.Count - 1];
            _modals.Remove(m);
            m.Hide();
        }

        /// <summary>Forgets all screens (used when the UI is rebuilt, e.g. after a language change).</summary>
        public void Reset()
        {
            _current = null;
            _modals.Clear();
        }

        public void CloseAllModals()
        {
            for (int i = _modals.Count - 1; i >= 0; i--) _modals[i].Hide();
            _modals.Clear();
        }

        public void RefreshAll()
        {
            if (_current != null && _current.IsVisible) _current.Refresh();
            foreach (var m in _modals) if (m.IsVisible) m.Refresh();
        }

        /// <summary>Routes the Android back button to the topmost screen.</summary>
        public bool HandleBack()
        {
            if (TopModal != null) return TopModal.OnBack();
            return _current != null && _current.OnBack();
        }
    }
}
