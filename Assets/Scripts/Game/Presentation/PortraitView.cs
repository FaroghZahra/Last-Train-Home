using LastTrainHome.Game.Config;
using UnityEngine;
using UnityEngine.UI;

namespace LastTrainHome.Game.Presentation
{
    /// <summary>A passenger's portrait in a frame of their colour; muted for late and missed moments.</summary>
    [DisallowMultipleComponent]
    public sealed class PortraitView : MonoBehaviour
    {
        [SerializeField] private Image _frame;
        [SerializeField] private Image _portrait;
        [SerializeField] private Color _mutedTint = new Color(0.72f, 0.72f, 0.72f);

        public void Show(string passengerName, PassengerArt art, bool isMuted = false)
        {
            Color color = art.GetColor(passengerName);
            _frame.color = isMuted ? Color.Lerp(color, new Color(0.45f, 0.5f, 0.5f), 0.6f) : color;
            _portrait.sprite = art.GetPortrait(passengerName);
            _portrait.enabled = _portrait.sprite != null;
            _portrait.color = isMuted ? _mutedTint : Color.white;
        }
    }
}
