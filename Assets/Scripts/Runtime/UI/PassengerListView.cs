using System;
using System.Collections.Generic;
using LastTrainHome.Data;
using LastTrainHome.Domain;
using TMPro;
using UnityEngine;

namespace LastTrainHome.UI
{
    /// <summary>
    /// Tonight's passengers: who they are, where they need to be, what they said, and how the planned route
    /// treats them. One row per passenger, cloned from an inactive template under a layout group that does not
    /// control child height (rows size themselves to their text).
    /// </summary>
    [DisallowMultipleComponent]
    public sealed class PassengerListView : MonoBehaviour
    {
        [SerializeField] private TMP_Text _rowTemplate;
        [SerializeField] private OutcomeStyle _style;

        private readonly List<TMP_Text> _rows = new List<TMP_Text>();
        private readonly List<string> _rowHeadings = new List<string>();

        /// <param name="stationName">Resolves a station id to the name the player sees.</param>
        public void Build(IReadOnlyList<PassengerDefinition> passengers, Func<string, string> stationName)
        {
            ClearRows();
            foreach (PassengerDefinition passenger in passengers)
            {
                TMP_Text row = Instantiate(_rowTemplate, _rowTemplate.transform.parent);
                row.name = passenger.Name;
                row.gameObject.SetActive(true);
                _rows.Add(row);

                string heading =
                    $"<b>{passenger.Name}</b>, to {stationName(passenger.DestinationStationId)} by {passenger.Deadline}";
                string motivation = _style.FormatStory(Quote(passenger.Motivation));
                _rowHeadings.Add(motivation.Length > 0 ? $"{heading}\n{motivation}" : heading);
            }
        }

        /// <param name="outcomes">One per passenger, in the order given to <see cref="Build"/>.</param>
        public void ShowOutcomes(IReadOnlyList<PassengerOutcome> outcomes)
        {
            if (outcomes.Count != _rows.Count)
            {
                throw new ArgumentException($"Expected {_rows.Count} outcomes, got {outcomes.Count}.", nameof(outcomes));
            }

            for (int i = 0; i < _rows.Count; i++)
            {
                TMP_Text row = _rows[i];
                row.SetText($"{_rowHeadings[i]}\n{_style.FormatStatus(outcomes[i])}");

                RectTransform rect = row.rectTransform;
                float height = row.GetPreferredValues(row.text, rect.rect.width, 0f).y;
                rect.sizeDelta = new Vector2(rect.sizeDelta.x, height);
            }
        }

        private static string Quote(string text) => string.IsNullOrWhiteSpace(text) ? string.Empty : $"“{text}”";

        private void Awake()
        {
            if (RequiredReferences.Check(this, (_rowTemplate, nameof(_rowTemplate)), (_style, nameof(_style))))
            {
                _rowTemplate.gameObject.SetActive(false);
            }
        }

        private void ClearRows()
        {
            foreach (TMP_Text row in _rows)
            {
                Destroy(row.gameObject);
            }

            _rows.Clear();
            _rowHeadings.Clear();
        }
    }
}
