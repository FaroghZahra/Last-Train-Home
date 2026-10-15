using LastTrainHome.Game.Config;
using LastTrainHome.Game.Flow.Views;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace LastTrainHome.Game.Presentation
{
    /// <summary>One day on the night card's week strip.</summary>
    [DisallowMultipleComponent]
    public sealed class WeekTileView : MonoBehaviour
    {
        [SerializeField] private Image _back;
        [SerializeField] private TMP_Text _day;
        [SerializeField] private TMP_Text _text;

        public void Show(WeekDayTile tile, UiStyle style)
        {
            (Color back, Color text) = tile.Kind switch
            {
                WeekDayKind.Past => (style.WeekPast, style.WeekPastText),
                WeekDayKind.Tonight => (style.Amber, style.Night),
                WeekDayKind.Known => (Color.Lerp(style.Night, style.WeekLine, 0.45f), style.CreamSoft),
                _ => (new Color(style.WeekLine.r, style.WeekLine.g, style.WeekLine.b, 0.25f), style.CreamSoft * 0.8f),
            };

            _back.color = back;
            _day.color = text;
            _text.color = text;
            _day.SetText(tile.Day);
            _text.SetText(tile.Text);
        }
    }
}
