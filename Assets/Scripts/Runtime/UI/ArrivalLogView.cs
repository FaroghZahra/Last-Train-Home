using System.Text;
using LastTrainHome.Data;
using LastTrainHome.Domain;
using TMPro;
using UnityEngine;

namespace LastTrainHome.UI
{
    /// <summary>A running account of the journey: who got off where, and what that meant for them.</summary>
    [DisallowMultipleComponent]
    public sealed class ArrivalLogView : MonoBehaviour
    {
        private const string StoryIndent = "      ";

        [SerializeField] private TMP_Text _text;
        [SerializeField] private OutcomeStyle _style;

        private readonly StringBuilder _log = new StringBuilder();

        public void Clear()
        {
            _log.Clear();
            _text.SetText(string.Empty);
        }

        public void AddArrival(PassengerDefinition passenger, PassengerOutcome outcome, string stationName)
        {
            string timing = outcome.Status == ArrivalStatus.OnTime ? "on time" : $"{outcome.MinutesLate} min late";
            AddLine(_style.Colorize($"{outcome.Arrival}  <b>{passenger.Name}</b> reached {stationName}, {timing}.", outcome.Status));
            AddStory(passenger.GetOutcomeLine(outcome.Status));
        }

        public void AddNotReached(PassengerDefinition passenger, string destinationName)
        {
            AddLine(_style.Colorize($"<b>{passenger.Name}</b> never reached {destinationName}.", ArrivalStatus.NotReached));
            AddStory(passenger.GetOutcomeLine(ArrivalStatus.NotReached));
        }

        public void AddEndOfLine(string stationName, ClockTime time) => AddLine($"End of the line at {stationName}, {time}.");

        private void Awake()
        {
            if (RequiredReferences.Check(this, (_text, nameof(_text)), (_style, nameof(_style))))
            {
                Clear();
            }
        }

        private void AddStory(string line)
        {
            string story = _style.FormatStory(line);
            if (story.Length > 0)
            {
                AddLine(StoryIndent + story);
            }
        }

        private void AddLine(string line)
        {
            if (_log.Length > 0)
            {
                _log.Append('\n');
            }

            _log.Append(line);
            _text.SetText(_log);
        }
    }
}
