using LastTrainHome.Game.Config;
using LastTrainHome.Game.Flow.Views;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace LastTrainHome.Game.Presentation
{
    /// <summary>One big night still to come, tinted with its passenger's colour once the player knows whose it is.</summary>
    [DisallowMultipleComponent]
    public sealed class WeekAheadTileView : MonoBehaviour
    {
        [SerializeField] private Image _back;
        [SerializeField, Tooltip("Border in the passenger's colour, on for tomorrow.")] private Image _nextRing;
        [SerializeField] private TMP_Text _header;
        [SerializeField] private TMP_Text _text;

        public void Show(WeekAheadTile tile, PassengerArt art, UiStyle style)
        {
            Color color = tile.Passenger != null ? art.GetColor(tile.Passenger) : style.InkSoft;
            _back.color = Color.Lerp(style.PaperLight, color, tile.Passenger != null ? 0.18f : 0.06f);
            _nextRing.gameObject.SetActive(tile.IsNext);
            _nextRing.color = color;
            _header.color = Color.Lerp(color, style.Ink, 0.45f);
            _header.SetText(tile.Header);
            _text.color = style.Ink;
            _text.SetText(tile.Text);
        }
    }
}
