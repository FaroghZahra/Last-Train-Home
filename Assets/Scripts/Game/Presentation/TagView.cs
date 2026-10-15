using LastTrainHome.Game.Config;
using LastTrainHome.Game.Flow.Views;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace LastTrainHome.Game.Presentation
{
    /// <summary>A small rounded tag, coloured by what it says, e.g. "On time · 00:10".</summary>
    [DisallowMultipleComponent]
    public sealed class TagView : MonoBehaviour
    {
        [SerializeField] private Image _back;
        [SerializeField] private TMP_Text _text;

        public void Show(TagModel tag, UiStyle style) => Show(tag.Text, style.GetTag(tag.Kind));

        public void Show(string text, (Color Back, Color Text) colors)
        {
            _back.color = colors.Back;
            _text.color = colors.Text;
            _text.SetText(text);
        }
    }
}
