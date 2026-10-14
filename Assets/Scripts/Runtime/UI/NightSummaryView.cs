using System;
using System.Collections.Generic;
using System.Linq;
using LastTrainHome.Data;
using LastTrainHome.Domain;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace LastTrainHome.UI
{
    /// <summary>
    /// The end-of-night card. Put this component on an object that stays active; it shows and hides the card.
    /// </summary>
    [DisallowMultipleComponent]
    public sealed class NightSummaryView : MonoBehaviour
    {
        [SerializeField] private RectTransform _card;
        [SerializeField, Tooltip("Top-anchored text inside the card.")] private TMP_Text _body;
        [SerializeField] private Button _planAgainButton;
        [SerializeField, Tooltip("Shown only when another night follows.")] private Button _nextNightButton;
        [SerializeField] private OutcomeStyle _style;
        [SerializeField, Min(0f), Tooltip("Space below the text for the buttons.")] private float _bottomPadding = 140f;

        public event Action PlanAgainRequested;
        public event Action NextNightRequested;

        /// <param name="outcomes">One per passenger, in the same order.</param>
        public void Show(IReadOnlyList<PassengerDefinition> passengers, IReadOnlyList<PassengerOutcome> outcomes, bool hasNextNight)
        {
            if (passengers.Count != outcomes.Count)
            {
                throw new ArgumentException("Every passenger needs exactly one outcome.", nameof(outcomes));
            }

            int onTime = outcomes.Count(outcome => outcome.Status == ArrivalStatus.OnTime);
            IEnumerable<string> entries = passengers.Select((passenger, i) => Describe(passenger, outcomes[i]));
            _body.SetText($"{onTime} of {passengers.Count} arrived on time.\n\n{string.Join("\n\n", entries)}");

            FitCardToText();
            _nextNightButton.gameObject.SetActive(hasNextNight);
            _card.gameObject.SetActive(true);
        }

        public void Hide() => _card.gameObject.SetActive(false);

        private string Describe(PassengerDefinition passenger, PassengerOutcome outcome)
        {
            string line = $"<b>{passenger.Name}</b>  ·  {_style.FormatStatus(outcome)}";
            string story = _style.FormatStory(passenger.GetOutcomeLine(outcome.Status));
            return story.Length > 0 ? $"{line}\n{story}" : line;
        }

        private void FitCardToText()
        {
            RectTransform bodyRect = _body.rectTransform;
            float textHeight = _body.GetPreferredValues(_body.text, bodyRect.rect.width, 0f).y;
            bodyRect.sizeDelta = new Vector2(bodyRect.sizeDelta.x, textHeight);

            float textTop = -bodyRect.anchoredPosition.y;
            _card.sizeDelta = new Vector2(_card.sizeDelta.x, textTop + textHeight + _bottomPadding);
        }

        private void Awake()
        {
            RequiredReferences.Check(this,
                (_card, nameof(_card)), (_body, nameof(_body)),
                (_planAgainButton, nameof(_planAgainButton)), (_nextNightButton, nameof(_nextNightButton)),
                (_style, nameof(_style)));
        }

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

        private void OnPlanAgainClicked() => PlanAgainRequested?.Invoke();

        private void OnNextNightClicked() => NextNightRequested?.Invoke();
    }
}
