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
    public sealed class NightCardView : ScreenView, INightCardView
    {
        [SerializeField] private TMP_Text _kicker;
        [SerializeField] private TMP_Text _day;
        [SerializeField, Tooltip("Portrait and line of the passenger whose big night it is.")] private GameObject _featured;
        [SerializeField] private PortraitView _featuredPortrait;
        [SerializeField] private TMP_Text _featuredLine;
        [SerializeField] private TMP_Text _lines;
        [SerializeField, Tooltip("Inactive; cloned for each day.")] private WeekTileView _dayTemplate;
        [SerializeField] private TMP_Text _footnote;
        [SerializeField] private Button _continue;
        [SerializeField] private TMP_Text _continueLabel;
        [SerializeField] private PassengerArt _art;
        [SerializeField] private UiStyle _style;

        private TemplateList<WeekTileView> _days;

        public event Action ContinueClicked;

        public void Show(NightCardModel model)
        {
            Reveal(0.5f);
            _kicker.SetText(model.Kicker);
            _day.SetText(model.Day);
            _featured.SetActive(model.Featured != null);
            if (model.Featured != null)
            {
                _featuredPortrait.Show(model.Featured, _art);
                _featuredLine.SetText(model.FeaturedLine);
            }

            _lines.SetText(RichText.Paragraphs(model.Lines, model.Emphasis, _style.Cream, emphasisOnItsOwnLine: true));
            _footnote.SetText(model.Footnote);
            _footnote.gameObject.SetActive(!string.IsNullOrEmpty(model.Footnote));
            _continueLabel.SetText(model.ContinueLabel);

            _days ??= new TemplateList<WeekTileView>(_dayTemplate);
            IReadOnlyList<WeekTileView> days = _days.Show(model.Week.Count);
            for (int i = 0; i < model.Week.Count; i++)
            {
                days[i].Show(model.Week[i], _style);
            }
        }

        public void Hide() => Conceal();

        private void OnEnable() => _continue.onClick.AddListener(OnContinue);

        private void OnDisable() => _continue.onClick.RemoveListener(OnContinue);

        private void OnContinue() => ContinueClicked?.Invoke();
    }
}
