using LastTrainHome.Game.Config;
using LastTrainHome.Game.Flow.Views;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace LastTrainHome.Game.Presentation
{
    /// <summary>One hook in the cab: a keepsake's picture (or its name, until it has one), or an empty hook.</summary>
    [DisallowMultipleComponent]
    public sealed class KeepsakeSlotView : MonoBehaviour
    {
        [SerializeField] private Image _back;
        [SerializeField] private Image _picture;
        [SerializeField] private TMP_Text _label;

        public void Show(KeepsakeSlot slot, KeepsakeArt art, UiStyle style)
        {
            bool isEmpty = slot.KeepsakeId == null;
            Sprite sprite = isEmpty ? null : art.Get(slot.KeepsakeId);
            _picture.sprite = sprite;
            _picture.gameObject.SetActive(sprite != null);
            _back.color = isEmpty ? new Color(style.Disabled.r, style.Disabled.g, style.Disabled.b, 0.35f)
                : slot.IsNew ? style.Coach
                : style.PaperLight;
            _label.color = isEmpty ? style.InkSoft : style.Ink;
            _label.fontStyle = slot.IsNew ? FontStyles.Bold : FontStyles.Normal;
            _label.SetText(slot.Label);
        }
    }
}
