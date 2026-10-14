using LastTrainHome.Game.Flow.Views;
using TMPro;
using UnityEngine;

namespace LastTrainHome.Game.Presentation
{
    [DisallowMultipleComponent]
    public sealed class ClockView : MonoBehaviour, IClockView
    {
        [SerializeField] private TMP_Text _text;

        public void Show(string text) => _text.SetText(text);
    }
}
