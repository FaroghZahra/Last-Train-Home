using System;
using System.Collections.Generic;
using LastTrainHome.Game.Config;
using LastTrainHome.Game.Flow.Views;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace LastTrainHome.Game.Presentation
{
    [DisallowMultipleComponent]
    public sealed class PlatformView : ScreenView, IPlatformView
    {
        [SerializeField] private TMP_Text _kicker;
        [SerializeField] private TMP_Text _headline;
        [SerializeField] private GameObject _coachBox;
        [SerializeField] private TMP_Text _coach;
        [SerializeField, Tooltip("Inactive; cloned for each passenger.")] private PlatformCardView _cardTemplate;
        [SerializeField] private TMP_Text _footer;
        [SerializeField] private Button _continue;
        [SerializeField] private Image _continueBack;
        [SerializeField] private TMP_Text _continueLabel;
        [SerializeField] private PassengerArt _art;
        [SerializeField] private UiStyle _style;

        private readonly HashSet<PlatformCardView> _listening = new HashSet<PlatformCardView>();
        private TemplateList<PlatformCardView> _cards;

        public event Action<string> AskClicked;

        public event Action<string> PromiseClicked;

        public event Action ContinueClicked;

        public void Show(PlatformModel model)
        {
            if (!gameObject.activeSelf)
            {
                Reveal();
            }

            _kicker.SetText(model.Kicker);
            _headline.SetText(model.Headline);
            _coachBox.SetActive(!string.IsNullOrEmpty(model.Coach));
            _coach.SetText(model.Coach);
            _footer.SetText(model.Footer);
            _continueLabel.SetText(model.ContinueLabel);
            _continue.interactable = model.CanContinue;
            _continueBack.color = model.CanContinue ? _style.Amber : _style.Disabled;

            _cards ??= new TemplateList<PlatformCardView>(_cardTemplate);
            IReadOnlyList<PlatformCardView> cards = _cards.Show(model.Cards.Count);
            for (int i = 0; i < model.Cards.Count; i++)
            {
                if (_listening.Add(cards[i]))
                {
                    cards[i].AskClicked += OnAsk;
                    cards[i].PromiseClicked += OnPromise;
                }

                cards[i].Show(model.Cards[i], _art, _style);
            }
        }

        public void Hide() => Conceal();

        private void OnEnable() => _continue.onClick.AddListener(OnContinue);

        private void OnDisable() => _continue.onClick.RemoveListener(OnContinue);

        private void OnAsk(string passengerName) => AskClicked?.Invoke(passengerName);

        private void OnPromise(string passengerName) => PromiseClicked?.Invoke(passengerName);

        private void OnContinue() => ContinueClicked?.Invoke();
    }
}
