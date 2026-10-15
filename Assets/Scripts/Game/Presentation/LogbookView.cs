using System;
using System.Collections.Generic;
using LastTrainHome.Game.Config;
using LastTrainHome.Game.Flow.Views;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace LastTrainHome.Game.Presentation
{
    /// <summary>
    /// The logbook, as an open book: tonight's entries and how the promise went on the left; the cab, the week ahead
    /// and what carries into tomorrow on the right.
    /// </summary>
    [DisallowMultipleComponent]
    public sealed class LogbookView : ScreenView, ILogbookView
    {
        [SerializeField] private TMP_Text _day;
        [SerializeField] private TMP_Text _kicker;
        [SerializeField, Tooltip("Inactive; cloned for each passenger.")] private LogEntryView _entryTemplate;
        [SerializeField] private TMP_Text _summary;
        [SerializeField] private TMP_Text _cabHeader;
        [SerializeField, Tooltip("Inactive; cloned for each hook.")] private KeepsakeSlotView _slotTemplate;
        [SerializeField] private TMP_Text _weekHeader;
        [SerializeField, Tooltip("Inactive; cloned for each big night ahead.")] private WeekAheadTileView _weekTemplate;
        [SerializeField] private GameObject _noteBox;
        [SerializeField] private TMP_Text _noteHeader;
        [SerializeField] private TMP_Text _note;
        [SerializeField] private TMP_Text _footer;
        [SerializeField] private Button _next;
        [SerializeField] private TMP_Text _nextLabel;
        [SerializeField] private PassengerArt _art;
        [SerializeField] private KeepsakeArt _keepsakeArt;
        [SerializeField] private UiStyle _style;

        private TemplateList<LogEntryView> _entries;
        private TemplateList<KeepsakeSlotView> _slots;
        private TemplateList<WeekAheadTileView> _week;

        public event Action NextClicked;

        public void Show(LogbookModel model)
        {
            Reveal(0.5f);
            _day.SetText(model.Day);
            _kicker.SetText(model.Kicker);
            _summary.SetText(model.Summary);
            _summary.color = model.SummaryIsGood ? _style.Green : _style.GetTag(TagKind.Late).Text;
            _cabHeader.SetText(model.CabHeader);
            _weekHeader.SetText(model.WeekHeader);
            _weekHeader.gameObject.SetActive(model.WeekAhead.Count > 0);
            _noteBox.SetActive(!string.IsNullOrEmpty(model.Note));
            _noteHeader.SetText(model.NoteHeader);
            _note.SetText(model.Note);
            _footer.SetText(model.Footer);
            _nextLabel.SetText(model.NextLabel);

            _entries ??= new TemplateList<LogEntryView>(_entryTemplate);
            IReadOnlyList<LogEntryView> entries = _entries.Show(model.Entries.Count);
            for (int i = 0; i < model.Entries.Count; i++)
            {
                entries[i].Show(model.Entries[i], _art, _style);
            }

            _slots ??= new TemplateList<KeepsakeSlotView>(_slotTemplate);
            IReadOnlyList<KeepsakeSlotView> slots = _slots.Show(model.Cab.Count);
            for (int i = 0; i < model.Cab.Count; i++)
            {
                slots[i].Show(model.Cab[i], _keepsakeArt, _style);
            }

            _week ??= new TemplateList<WeekAheadTileView>(_weekTemplate);
            IReadOnlyList<WeekAheadTileView> week = _week.Show(model.WeekAhead.Count);
            for (int i = 0; i < model.WeekAhead.Count; i++)
            {
                week[i].Show(model.WeekAhead[i], _art, _style);
            }
        }

        public void Hide() => Conceal();

        private void OnEnable() => _next.onClick.AddListener(OnNext);

        private void OnDisable() => _next.onClick.RemoveListener(OnNext);

        private void OnNext() => NextClicked?.Invoke();
    }
}
