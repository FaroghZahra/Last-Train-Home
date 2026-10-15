using System;
using LastTrainHome.Game.Flow.Views;
using UnityEngine;

namespace LastTrainHome.Game.Config
{
    /// <summary>
    /// The interface's palette: night blues behind, paper cards in front, amber for what to press, and one colour pair
    /// per outcome so on time, late and not reached look the same on every screen.
    /// </summary>
    [CreateAssetMenu(fileName = "UiStyle", menuName = "Last Train Home/UI Style")]
    public sealed class UiStyle : ScriptableObject
    {
        [Header("Ground")]
        [SerializeField] private Color _night = Hex(0x17202E);
        [SerializeField] private Color _paper = Hex(0xF2E8D3);
        [SerializeField] private Color _paperLight = Hex(0xFBF6EA);
        [SerializeField, Tooltip("A late or missed arrival card.")] private Color _mutedPaper = Hex(0xE9E1D2);
        [SerializeField] private Color _ink = Hex(0x2E241B);
        [SerializeField] private Color _inkSoft = Hex(0x5A4A3B);
        [SerializeField] private Color _cream = Hex(0xF2E8D3);
        [SerializeField] private Color _creamSoft = Hex(0xD9CDB4);

        [Header("Accents")]
        [SerializeField] private Color _amber = Hex(0xE9B44C);
        [SerializeField] private Color _green = Hex(0x3F6B45);
        [SerializeField] private Color _brick = Hex(0xA8432F);
        [SerializeField] private Color _disabled = Hex(0xCFC3A8);
        [SerializeField] private Color _coach = Hex(0xFFF6DE);

        [Header("Tags: background, then text")]
        [SerializeField] private Color _onTimeBack = Hex(0xDDEBD8);
        [SerializeField] private Color _onTimeText = Hex(0x22401F);
        [SerializeField] private Color _lateBack = Hex(0xF6E2C0);
        [SerializeField] private Color _lateText = Hex(0x6B4410);
        [SerializeField] private Color _notReachedBack = Hex(0xE3DCCF);
        [SerializeField] private Color _notReachedText = Hex(0x4A4038);
        [SerializeField] private Color _neutralBack = Hex(0xEFE3C8);
        [SerializeField] private Color _neutralText = Hex(0x4A3B2E);
        [SerializeField] private Color _promiseBack = Hex(0xFFF0CC);
        [SerializeField] private Color _promiseText = Hex(0x6B4410);
        [SerializeField] private Color _brokenBack = Hex(0xF8E3DC);
        [SerializeField] private Color _brokenText = Hex(0x8E2B1C);

        [Header("Week")]
        [SerializeField] private Color _pipAhead = Hex(0xCDBE9E);
        [SerializeField] private Color _weekLine = Hex(0x4D5A70);
        [SerializeField] private Color _weekPast = Hex(0x23392A);
        [SerializeField] private Color _weekPastText = Hex(0xCFE5C9);

        public Color Night => _night;

        public Color Paper => _paper;

        public Color PaperLight => _paperLight;

        public Color MutedPaper => _mutedPaper;

        public Color Ink => _ink;

        public Color InkSoft => _inkSoft;

        public Color Cream => _cream;

        public Color CreamSoft => _creamSoft;

        public Color Amber => _amber;

        public Color Green => _green;

        public Color Brick => _brick;

        public Color Disabled => _disabled;

        public Color Coach => _coach;

        public Color PipAhead => _pipAhead;

        public Color WeekLine => _weekLine;

        public Color WeekPast => _weekPast;

        public Color WeekPastText => _weekPastText;

        public (Color Back, Color Text) GetTag(TagKind kind) => kind switch
        {
            TagKind.OnTime => (_onTimeBack, _onTimeText),
            TagKind.Late => (_lateBack, _lateText),
            TagKind.NotReached => (_notReachedBack, _notReachedText),
            TagKind.Neutral => (_neutralBack, _neutralText),
            TagKind.Promise => (_promiseBack, _promiseText),
            TagKind.PromiseBroken => (_brokenBack, _brokenText),
            _ => throw new ArgumentOutOfRangeException(nameof(kind), kind, null),
        };

        public (Color Back, Color Text) GetTag(OutcomeKind outcome) => outcome switch
        {
            OutcomeKind.OnTime => GetTag(TagKind.OnTime),
            OutcomeKind.Late => GetTag(TagKind.Late),
            OutcomeKind.NotReached => GetTag(TagKind.NotReached),
            _ => throw new ArgumentOutOfRangeException(nameof(outcome), outcome, null),
        };

        public Color GetPip(PipKind pip) => pip switch
        {
            PipKind.OnTime => _green,
            PipKind.Late => Hex(0xB9822A),
            PipKind.Missed => _notReachedText,
            PipKind.Absent => _pipAhead,
            PipKind.Tonight => _ink,
            PipKind.TonightBig => _amber,
            PipKind.Ahead => _pipAhead,
            PipKind.AheadBig => _amber,
            _ => throw new ArgumentOutOfRangeException(nameof(pip), pip, null),
        };

        private static Color Hex(int rgb) =>
            new Color(((rgb >> 16) & 0xFF) / 255f, ((rgb >> 8) & 0xFF) / 255f, (rgb & 0xFF) / 255f);
    }
}
