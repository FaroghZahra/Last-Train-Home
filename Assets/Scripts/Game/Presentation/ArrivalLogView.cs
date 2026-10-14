using System.Text;
using LastTrainHome.Game.Flow.Views;
using TMPro;
using UnityEngine;

namespace LastTrainHome.Game.Presentation
{
    /// <summary>A running account of the journey, one entry per event.</summary>
    [DisallowMultipleComponent]
    public sealed class ArrivalLogView : MonoBehaviour, IArrivalLogView
    {
        [SerializeField] private TMP_Text _text;

        private readonly StringBuilder _log = new StringBuilder();

        public void Clear()
        {
            _log.Clear();
            _text.SetText(string.Empty);
        }

        public void AddEntry(string entry)
        {
            if (_log.Length > 0)
            {
                _log.Append('\n');
            }

            _log.Append(entry);
            _text.SetText(_log);
        }

        private void Awake() => Clear();
    }
}
