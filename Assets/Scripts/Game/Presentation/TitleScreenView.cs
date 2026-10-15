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
    public sealed class TitleScreenView : ScreenView, ITitleScreenView
    {
        [SerializeField] private TMP_Text _title;
        [SerializeField] private TMP_Text _intro;
        [SerializeField] private TMP_Text _footnote;
        [SerializeField] private Button _begin;
        [SerializeField] private TMP_Text _beginLabel;
        [SerializeField, Tooltip("Inactive; cloned for each of the cast.")] private PortraitView _portraitTemplate;
        [SerializeField] private PassengerArt _art;
        [SerializeField] private UiStyle _style;

        private TemplateList<PortraitView> _portraits;

        public event Action BeginClicked;

        public void Show(TitleScreenModel model)
        {
            Reveal(0.6f);
            _title.SetText(model.Title);
            var paragraphs = new List<string>(model.Intro);
            _intro.SetText(RichText.Paragraphs(paragraphs, model.Emphasis, _style.Cream, emphasisOnItsOwnLine: false)
                .Replace("\n", "\n\n"));
            _footnote.SetText(model.Footnote);
            _beginLabel.SetText(model.BeginLabel);

            _portraits ??= new TemplateList<PortraitView>(_portraitTemplate);
            IReadOnlyList<PortraitView> portraits = _portraits.Show(model.Cast.Count);
            for (int i = 0; i < model.Cast.Count; i++)
            {
                portraits[i].Show(model.Cast[i], _art);
            }
        }

        public void Hide() => Conceal();

        private void OnEnable() => _begin.onClick.AddListener(OnBegin);

        private void OnDisable() => _begin.onClick.RemoveListener(OnBegin);

        private void OnBegin() => BeginClicked?.Invoke();
    }
}
