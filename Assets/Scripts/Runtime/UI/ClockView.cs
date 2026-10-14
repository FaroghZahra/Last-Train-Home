using LastTrainHome.Domain;
using TMPro;
using UnityEngine;

namespace LastTrainHome.UI
{
    [DisallowMultipleComponent]
    public sealed class ClockView : MonoBehaviour
    {
        [SerializeField] private TMP_Text _text;

        /// <summary>While planning: when the route as planned would end.</summary>
        public void ShowPlannedEnd(ClockTime routeEnd, ClockTime closing) =>
            _text.SetText($"Route ends {routeEnd}   ·   line closes {closing}");

        /// <summary>While the train runs: the current time.</summary>
        public void ShowCurrentTime(ClockTime now, ClockTime closing) =>
            _text.SetText($"Now {now}   ·   line closes {closing}");

        private void Awake() => RequiredReferences.Check(this, (_text, nameof(_text)));
    }
}
