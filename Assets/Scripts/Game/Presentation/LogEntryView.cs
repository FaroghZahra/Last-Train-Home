using LastTrainHome.Game.Config;
using LastTrainHome.Game.Flow.Views;
using TMPro;
using UnityEngine;

namespace LastTrainHome.Game.Presentation
{
    /// <summary>One passenger's handwritten line in the logbook.</summary>
    [DisallowMultipleComponent]
    public sealed class LogEntryView : MonoBehaviour
    {
        [SerializeField] private PortraitView _portrait;
        [SerializeField] private TMP_Text _name;
        [SerializeField] private TagView _tag;
        [SerializeField] private TMP_Text _line;

        public void Show(LogEntryModel entry, PassengerArt art, UiStyle style)
        {
            _portrait.Show(entry.Name, art, entry.Tag.Kind != TagKind.OnTime);
            _name.SetText(entry.Name);
            _tag.Show(entry.Tag, style);
            _line.SetText(entry.Line);
        }
    }
}
