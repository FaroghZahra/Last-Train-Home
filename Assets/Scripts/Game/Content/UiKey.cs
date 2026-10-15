using System.Collections.Generic;

namespace LastTrainHome.Game.Content
{
    /// <summary>
    /// Every piece of interface wording, keyed as in Data/Text/UiText.json, with the tokens each one may use.
    /// The file must have exactly these keys; <see cref="UiText"/> checks it when it loads.
    /// </summary>
    public static class UiKey
    {
        public const string TitleBegin = "title.begin";
        public const string ProgressLine = "progress.line";
        public const string EndingKicker = "ending.kicker";
        public const string EndingAgain = "ending.again";

        public const string WeekTonight = "week.tonight";
        public const string WeekUnknown = "week.unknown";
        public const string WeekLastTrain = "week.lastTrain";
        public const string WeekHome = "week.home";
        public const string WeekTonightDay = "week.tonightDay";
        public const string WeekKept = "week.kept";
        public const string WeekMissed = "week.missed";
        public const string NightCardContinue = "nightCard.continue";

        public const string PlatformAsk = "platform.ask";
        public const string PlatformAsked = "platform.asked";
        public const string PlatformAskUsed = "platform.askUsed";
        public const string PlatformPromise = "platform.promise";
        public const string PlatformPromised = "platform.promised";
        public const string PlatformYouAsked = "platform.youAsked";
        public const string PlatformYouAskedReveal = "platform.youAskedReveal";
        public const string PlatformSub = "platform.sub";
        public const string PlatformSubBigNight = "platform.subBigNight";
        public const string PlatformFooterNone = "platform.footerNone";
        public const string PlatformFooterPromised = "platform.footerPromised";
        public const string PlatformContinue = "platform.continue";

        public const string PlanNight = "plan.night";
        public const string PlanClock = "plan.clock";
        public const string PlanChipOnTime = "plan.chipOnTime";
        public const string PlanChipLate = "plan.chipLate";
        public const string PlanChipNotReached = "plan.chipNotReached";
        public const string PlanCard = "plan.card";
        public const string PlanPromiseHeader = "plan.promiseHeader";
        public const string PlanPromiseLine = "plan.promiseLine";
        public const string PlanPromiseKept = "plan.promiseKept";
        public const string PlanPromiseBroken = "plan.promiseBroken";
        public const string PlanRouteHeader = "plan.routeHeader";
        public const string PlanStopWith = "plan.stopWith";
        public const string PlanStopBoards = "plan.stopBoards";
        public const string TripPickup = "trip.pickup";
        public const string PlanUndo = "plan.undo";
        public const string PlanClear = "plan.clear";
        public const string PlanDepart = "plan.depart";
        public const string PlanDepartEmpty = "plan.departEmpty";
        public const string PlanDepartAll = "plan.departAll";
        public const string PlanDepartLate = "plan.departLate";
        public const string PlanDepartNotReached = "plan.departNotReached";
        public const string ListAnd = "list.and";
        public const string ListDot = "list.dot";
        public const string ListComma = "list.comma";

        public const string MapMinutes = "map.minutes";
        public const string MapClosedTrack = "map.closedTrack";
        public const string MapClosedStation = "map.closedStation";

        public const string RefuseAlready = "refuse.already";
        public const string RefuseNoTrack = "refuse.noTrack";
        public const string RefusePastClosing = "refuse.pastClosing";
        public const string RefuseUnknown = "refuse.unknown";
        public const string RefuseClosed = "refuse.closed";

        public const string RideNow = "ride.now";
        public const string RideArrival = "ride.arrival";
        public const string RideLastStop = "ride.lastStop";
        public const string RideEndOfLine = "ride.endOfLine";
        public const string RideTitle = "ride.title";
        public const string RideBoards = "ride.boards";
        public const string RideFound = "ride.found";
        public const string RideNoKeepsake = "ride.noKeepsake";
        public const string RideStillAboard = "ride.stillAboard";
        public const string RideMovesOn = "ride.movesOn";
        public const string RideContinue = "ride.continue";
        public const string RideToLogbook = "ride.toLogbook";
        public const string TagOnTime = "tag.onTime";
        public const string TagLate = "tag.late";
        public const string TagNotReached = "tag.notReached";
        public const string TagNeeded = "tag.needed";
        public const string TagNeverBoarded = "tag.neverBoarded";
        public const string TagNeededNotSaid = "tag.neededNotSaid";
        public const string TagJustInTime = "tag.justInTime";
        public const string TagPromiseKept = "tag.promiseKept";
        public const string TagPromiseBroken = "tag.promiseBroken";
        public const string TagBigNight = "tag.bigNight";

        public const string LogKicker = "log.kicker";
        public const string LogEveryoneHomeKept = "log.everyoneHomeKept";
        public const string LogEveryoneHome = "log.everyoneHome";
        public const string LogKeptWaited = "log.keptWaited";
        public const string LogBroken = "log.broken";
        public const string LogWaited = "log.waited";
        public const string LogCab = "log.cab";
        public const string LogEmptyHook = "log.emptyHook";
        public const string LogWeek = "log.week";
        public const string LogWeekTile = "log.weekTile";
        public const string LogWeekNext = "log.weekNext";
        public const string LogWeekNote = "log.weekNote";
        public const string LogTagPromiseKept = "log.tagPromiseKept";
        public const string LogTagPromiseBroken = "log.tagPromiseBroken";
        public const string LogTomorrow = "log.tomorrow";
        public const string LogCarried = "log.carried";
        public const string LogBestReached = "log.bestReached";
        public const string LogBestMissed = "log.bestMissed";
        public const string LogNext = "log.next";
        public const string LogFinish = "log.finish";

        /// <summary>Every key and the tokens it may use.</summary>
        public static readonly IReadOnlyDictionary<string, string[]> Tokens = new Dictionary<string, string[]>
        {
            [TitleBegin] = new[] { "day" },
            [ProgressLine] = new[] { "found", "total", "endings", "endingCount" },
            [EndingKicker] = None,
            [EndingAgain] = None,
            [WeekTonight] = None,
            [WeekUnknown] = None,
            [WeekLastTrain] = None,
            [WeekHome] = new[] { "onTime", "total" },
            [WeekTonightDay] = new[] { "day" },
            [WeekKept] = new[] { "name" },
            [WeekMissed] = new[] { "name" },
            [NightCardContinue] = None,
            [PlatformAsk] = None,
            [PlatformAsked] = None,
            [PlatformAskUsed] = None,
            [PlatformPromise] = None,
            [PlatformPromised] = None,
            [PlatformYouAsked] = None,
            [PlatformYouAskedReveal] = new[] { "day" },
            [PlatformSub] = new[] { "destination", "deadline" },
            [PlatformSubBigNight] = new[] { "destination", "deadline" },
            [PlatformFooterNone] = None,
            [PlatformFooterPromised] = new[] { "name" },
            [PlatformContinue] = None,
            [PlanNight] = new[] { "day", "night", "nights" },
            [PlanClock] = new[] { "end", "closing" },
            [PlanChipOnTime] = new[] { "time" },
            [PlanChipLate] = new[] { "time" },
            [PlanChipNotReached] = None,
            [PlanCard] = new[] { "destination", "deadline" },
            [PlanPromiseHeader] = None,
            [PlanPromiseLine] = new[] { "name", "destination", "deadline" },
            [PlanPromiseKept] = new[] { "time" },
            [PlanPromiseBroken] = None,
            [PlanRouteHeader] = None,
            [PlanStopWith] = new[] { "station", "names" },
            [PlanStopBoards] = new[] { "names" },
            [TripPickup] = new[] { "origin", "destination" },
            [PlanUndo] = None,
            [PlanClear] = None,
            [PlanDepart] = None,
            [PlanDepartEmpty] = None,
            [PlanDepartAll] = None,
            [PlanDepartLate] = new[] { "names" },
            [PlanDepartNotReached] = new[] { "names" },
            [ListAnd] = None,
            [ListDot] = None,
            [ListComma] = None,
            [MapMinutes] = new[] { "minutes" },
            [MapClosedTrack] = None,
            [MapClosedStation] = new[] { "station" },
            [RefuseAlready] = new[] { "station" },
            [RefuseNoTrack] = new[] { "from", "station" },
            [RefusePastClosing] = new[] { "station", "time", "closing" },
            [RefuseUnknown] = new[] { "station" },
            [RefuseClosed] = new[] { "station" },
            [RideNow] = new[] { "time" },
            [RideArrival] = new[] { "index", "count" },
            [RideLastStop] = None,
            [RideEndOfLine] = None,
            [RideTitle] = new[] { "name", "station" },
            [RideBoards] = new[] { "names" },
            [RideFound] = new[] { "keepsake" },
            [RideNoKeepsake] = None,
            [RideStillAboard] = None,
            [RideMovesOn] = new[] { "seconds" },
            [RideContinue] = None,
            [RideToLogbook] = None,
            [TagOnTime] = new[] { "time" },
            [TagLate] = new[] { "time" },
            [TagNotReached] = None,
            [TagNeeded] = new[] { "deadline" },
            [TagNeverBoarded] = None,
            [TagNeededNotSaid] = new[] { "deadline", "said" },
            [TagJustInTime] = None,
            [TagPromiseKept] = None,
            [TagPromiseBroken] = None,
            [TagBigNight] = None,
            [LogKicker] = new[] { "night" },
            [LogEveryoneHomeKept] = new[] { "promised" },
            [LogEveryoneHome] = None,
            [LogKeptWaited] = new[] { "promised", "waited" },
            [LogBroken] = new[] { "promised", "waited" },
            [LogWaited] = new[] { "waited" },
            [LogCab] = None,
            [LogEmptyHook] = None,
            [LogWeek] = None,
            [LogWeekTile] = new[] { "day", "name" },
            [LogWeekNext] = new[] { "day", "name" },
            [LogWeekNote] = new[] { "label", "note" },
            [LogTagPromiseKept] = new[] { "tag" },
            [LogTagPromiseBroken] = new[] { "tag" },
            [LogTomorrow] = None,
            [LogCarried] = new[] { "day" },
            [LogBestReached] = new[] { "onTime", "total" },
            [LogBestMissed] = new[] { "onTime", "total", "best" },
            [LogNext] = new[] { "day" },
            [LogFinish] = None,
        };

        private static string[] None => System.Array.Empty<string>();
    }
}
