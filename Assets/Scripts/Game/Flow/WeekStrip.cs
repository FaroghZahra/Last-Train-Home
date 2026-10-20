using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using LastTrainHome.Domain.Passengers;
using LastTrainHome.Domain.Session;
using LastTrainHome.Game.Content;
using LastTrainHome.Game.Flow.Views;

namespace LastTrainHome.Game.Flow
{
    /// <summary>
    /// The week as the player knows it: past nights by how many got home, tonight, and the big nights ahead that
    /// someone has told them about. A big night is known once its passenger answers a question, or when it arrives.
    /// </summary>
    public static class WeekStrip
    {
        public static IReadOnlyList<WeekDayTile> Days(NightContext night)
        {
            UiText ui = night.Ui;
            var tiles = new List<WeekDayTile>();
            for (int i = 0; i < night.NightCount; i++)
            {
                string day = night.ShortDay(i);
                int dayIndex = i;
                string[] knownNames = night.Week.BigNightsOn(i)
                    .Where(member => dayIndex <= night.NightIndex || night.Campaign.IsBigNightRevealed(member.Name))
                    .Select(member => member.Name)
                    .ToArray();
                bool known = knownNames.Length > 0;

                if (i < night.NightIndex)
                {
                    tiles.Add(new WeekDayTile { Day = day, Text = HomeCount(night, i), Kind = WeekDayKind.Past });
                    continue;
                }

                string text = known ? string.Join(ui.Get(UiKey.ListAnd), knownNames) : ui.Get(i == night.NightIndex ? UiKey.WeekTonight : UiKey.WeekUnknown);
                if (i == night.NightCount - 1)
                {
                    text = known ? text + ui.Get(UiKey.ListDot) + ui.Get(UiKey.WeekLastTrain) : ui.Get(UiKey.WeekLastTrain);
                }

                if (i == night.NightIndex)
                {
                    tiles.Add(new WeekDayTile
                    {
                        Day = known ? ui.Format(UiKey.WeekTonightDay, ("day", day)) : day,
                        Text = text,
                        Kind = WeekDayKind.Tonight,
                    });
                }
                else
                {
                    tiles.Add(new WeekDayTile { Day = day, Text = text, Kind = known ? WeekDayKind.Known : WeekDayKind.Unknown });
                }
            }

            return tiles;
        }

        /// <summary>One pip per night for a passenger's planning card.</summary>
        public static IReadOnlyList<PipKind> Pips(NightContext night, string passengerName)
        {
            CastMember member = night.Week.FindCastMember(passengerName);
            bool bigNightKnown = member != null
                                 && (member.BigNight <= night.NightIndex || night.Campaign.IsBigNightRevealed(passengerName));
            var pips = new List<PipKind>();
            for (int i = 0; i < night.NightCount; i++)
            {
                bool isBig = bigNightKnown && member.BigNight == i;
                if (i < night.NightIndex)
                {
                    pips.Add(PastPip(night, i, passengerName));
                }
                else if (i == night.NightIndex)
                {
                    pips.Add(isBig ? PipKind.TonightBig : PipKind.Tonight);
                }
                else
                {
                    pips.Add(isBig ? PipKind.AheadBig : PipKind.Ahead);
                }
            }

            return pips;
        }

        private static string HomeCount(NightContext night, int nightIndex)
        {
            if (!night.Campaign.TryGetResult(night.Content.Levels[nightIndex].Night.Id, out NightResult result))
            {
                return string.Empty;
            }

            return night.Ui.Format(UiKey.WeekHome,
                ("onTime", result.OnTimeCount.ToString(CultureInfo.InvariantCulture)),
                ("total", result.Outcomes.Count.ToString(CultureInfo.InvariantCulture)));
        }

        private static PipKind PastPip(NightContext night, int nightIndex, string passengerName)
        {
            if (!night.Campaign.TryGetResult(night.Content.Levels[nightIndex].Night.Id, out NightResult result)
                || !result.TryGetStatus(passengerName, out ArrivalStatus status))
            {
                return PipKind.Absent;
            }

            return status switch
            {
                ArrivalStatus.OnTime => PipKind.OnTime,
                ArrivalStatus.Late => PipKind.Late,
                _ => PipKind.Missed,
            };
        }
    }
}
