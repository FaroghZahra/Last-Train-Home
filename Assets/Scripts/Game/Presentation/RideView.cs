using System;
using System.Collections;
using System.Collections.Generic;
using LastTrainHome.Game.Config;
using LastTrainHome.Game.Flow.Views;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace LastTrainHome.Game.Presentation
{
    /// <summary>
    /// The ride: the map dimmed behind a clock pill, and a card for each arrival. Late and missed arrivals get the same
    /// card in muted colours, fading in more slowly.
    /// </summary>
    [DisallowMultipleComponent]
    public sealed class RideView : ScreenView, IRideView
    {
        [SerializeField] private TMP_Text _now;
        [SerializeField] private TMP_Text _station;
        [SerializeField] private TMP_Text _progress;
        [SerializeField] private CanvasGroup _card;
        [SerializeField] private Image _cardBack;
        [SerializeField, Tooltip("Amber outline, on for tonight's promise.")] private GameObject _cardRing;
        [SerializeField] private PortraitView _portrait;
        [SerializeField, Tooltip("Inactive; cloned for each tag.")] private TagView _tagTemplate;
        [SerializeField] private TMP_Text _title;
        [SerializeField] private TMP_Text _line;
        [SerializeField] private GameObject _keepsakeRow;
        [SerializeField] private TMP_Text _keepsake;
        [SerializeField] private TMP_Text _aside;
        [SerializeField] private GameObject _aboard;
        [SerializeField] private TMP_Text _aboardLabel;
        [SerializeField, Tooltip("Inactive; cloned for each passenger still aboard.")] private PortraitView _aboardTemplate;
        [SerializeField] private TMP_Text _countdown;
        [SerializeField] private Button _continue;
        [SerializeField] private Image _continueBack;
        [SerializeField] private TMP_Text _continueLabel;
        [SerializeField, Min(0f)] private float _cardFadeSeconds = 0.3f;
        [SerializeField, Min(0f)] private float _mutedCardFadeSeconds = 0.9f;
        [SerializeField] private PassengerArt _art;
        [SerializeField] private UiStyle _style;

        private TemplateList<TagView> _tags;
        private TemplateList<PortraitView> _stillAboard;
        private Coroutine _fade;

        public event Action ContinueClicked;

        public void Show()
        {
            Reveal();
            HideArrival();
            _now.SetText(string.Empty);
            _station.SetText(string.Empty);
            _progress.SetText(string.Empty);
        }

        public void ShowProgress(string now, string station, string progress)
        {
            _now.SetText(now);
            _station.SetText(station);
            _progress.SetText(progress);
        }

        public void ShowArrival(ArrivalModel model)
        {
            _card.gameObject.SetActive(true);
            _continue.gameObject.SetActive(true);
            _cardBack.color = model.IsMuted ? _style.MutedPaper : _style.Paper;
            _cardRing.SetActive(model.IsPromised && !model.IsMuted);
            _portrait.Show(model.Name, _art, model.IsMuted);
            _title.SetText(model.Title);
            _line.SetText(model.Line);
            _keepsakeRow.SetActive(!string.IsNullOrEmpty(model.Keepsake));
            _keepsake.SetText(model.Keepsake);
            _aside.gameObject.SetActive(!string.IsNullOrEmpty(model.Aside));
            _aside.SetText(model.Aside);

            _tags ??= new TemplateList<TagView>(_tagTemplate);
            IReadOnlyList<TagView> tags = _tags.Show(model.Tags.Count);
            for (int i = 0; i < model.Tags.Count; i++)
            {
                tags[i].Show(model.Tags[i], _style);
            }

            _aboard.SetActive(model.StillAboard.Count > 0);
            _aboardLabel.SetText(model.StillAboardLabel);
            _stillAboard ??= new TemplateList<PortraitView>(_aboardTemplate);
            IReadOnlyList<PortraitView> aboard = _stillAboard.Show(model.StillAboard.Count);
            for (int i = 0; i < model.StillAboard.Count; i++)
            {
                aboard[i].Show(model.StillAboard[i], _art);
            }

            _continueLabel.SetText(model.ContinueLabel);
            _continueBack.color = model.IsMuted ? _style.Disabled : _style.Amber;

            if (_fade != null)
            {
                StopCoroutine(_fade);
            }

            _fade = StartCoroutine(FadeIn(model.IsMuted ? _mutedCardFadeSeconds : _cardFadeSeconds));
        }

        public void ShowCountdown(string text)
        {
            _countdown.gameObject.SetActive(!string.IsNullOrEmpty(text));
            _countdown.SetText(text);
        }

        public void HideArrival()
        {
            _card.gameObject.SetActive(false);
            _aboard.SetActive(false);
            _continue.gameObject.SetActive(false);
            _countdown.gameObject.SetActive(false);
        }

        public void Hide() => Conceal();

        private void OnEnable() => _continue.onClick.AddListener(OnContinue);

        private void OnDisable()
        {
            _continue.onClick.RemoveListener(OnContinue);
            _fade = null;
        }

        private IEnumerator FadeIn(float seconds)
        {
            for (float elapsed = 0f; elapsed < seconds; elapsed += Time.deltaTime)
            {
                _card.alpha = elapsed / seconds;
                yield return null;
            }

            _card.alpha = 1f;
            _fade = null;
        }

        private void OnContinue() => ContinueClicked?.Invoke();
    }
}
