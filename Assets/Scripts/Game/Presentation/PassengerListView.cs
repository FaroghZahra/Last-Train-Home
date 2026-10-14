using System.Collections.Generic;
using LastTrainHome.Game.Flow.Views;
using TMPro;
using UnityEngine;

namespace LastTrainHome.Game.Presentation
{
    /// <summary>
    /// Tonight's passengers, one text row each, cloned from an inactive template under a layout group that does not
    /// control child height (rows size themselves to their text).
    /// </summary>
    [DisallowMultipleComponent]
    public sealed class PassengerListView : MonoBehaviour, IPassengerListView
    {
        [SerializeField] private TMP_Text _rowTemplate;

        private readonly List<TMP_Text> _rows = new List<TMP_Text>();

        public void ShowRows(IReadOnlyList<string> rows)
        {
            SetRowCount(rows.Count);
            for (int i = 0; i < rows.Count; i++)
            {
                TMP_Text row = _rows[i];
                row.SetText(rows[i]);

                RectTransform rect = row.rectTransform;
                float height = row.GetPreferredValues(row.text, rect.rect.width, 0f).y;
                rect.sizeDelta = new Vector2(rect.sizeDelta.x, height);
            }
        }

        private void Awake() => _rowTemplate.gameObject.SetActive(false);

        private void SetRowCount(int count)
        {
            while (_rows.Count > count)
            {
                Destroy(_rows[_rows.Count - 1].gameObject);
                _rows.RemoveAt(_rows.Count - 1);
            }

            while (_rows.Count < count)
            {
                TMP_Text row = Instantiate(_rowTemplate, _rowTemplate.transform.parent);
                row.gameObject.SetActive(true);
                _rows.Add(row);
            }
        }
    }
}
