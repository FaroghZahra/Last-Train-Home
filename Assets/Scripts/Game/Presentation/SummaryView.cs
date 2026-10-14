using System;
using LastTrainHome.Game.Flow.Views;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace LastTrainHome.Game.Presentation
{
    /// <summary>
    /// The end-of-night card. Put this component on an object that stays active; it shows and hides the card.
    /// </summary>
    [DisallowMultipleComponent]
    public sealed class SummaryView : MonoBehaviour, ISummaryView
    {
        [SerializeField] private RectTransform _card;
        [SerializeField, Tooltip("Top-anchored text inside the card.")] private TMP_Text _body;
        [SerializeField] private Button _planAgainButton;
        [SerializeField] private Button _nextNightButton;
        [SerializeField, Min(0f), Tooltip("Space below the text for the buttons.")] private float _bottomPadding = 140f;

        public event Action PlanAgainClicked;

        public event Action NextNightClicked;

        public void Show(string body, bool canContinue)
        {
            _body.SetText(body);
            FitCardToText();
            _nextNightButton.gameObject.SetActive(canContinue);
            _card.gameObject.SetActive(true);
        }

        public void Hide() => _card.gameObject.SetActive(false);

        private void OnEnable()
        {
            _planAgainButton.onClick.AddListener(OnPlanAgainClicked);
            _nextNightButton.onClick.AddListener(OnNextNightClicked);
        }

        private void OnDisable()
        {
            _planAgainButton.onClick.RemoveListener(OnPlanAgainClicked);
            _nextNightButton.onClick.RemoveListener(OnNextNightClicked);
        }

        private void FitCardToText()
        {
            RectTransform bodyRect = _body.rectTransform;
            float textHeight = _body.GetPreferredValues(_body.text, bodyRect.rect.width, 0f).y;
            bodyRect.sizeDelta = new Vector2(bodyRect.sizeDelta.x, textHeight);

            float textTop = -bodyRect.anchoredPosition.y;
            _card.sizeDelta = new Vector2(_card.sizeDelta.x, textTop + textHeight + _bottomPadding);
        }

        private void OnPlanAgainClicked() => PlanAgainClicked?.Invoke();

        private void OnNextNightClicked() => NextNightClicked?.Invoke();
    }
}
