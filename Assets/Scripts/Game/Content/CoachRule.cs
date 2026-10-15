namespace LastTrainHome.Game.Content
{
    /// <summary>One planning coach line and when it shows. A level's rules are tried in order; the first match wins.</summary>
    public sealed class CoachRule
    {
        /// <summary>Tokens a coach line may use.</summary>
        public static readonly string[] Tokens = { "late", "notReached", "waiting", "nextDeadline", "promised" };

        public CoachRule(CoachCondition condition, int count, string station, string text)
        {
            Condition = condition;
            Count = count;
            Station = station;
            Text = text ?? string.Empty;
        }

        public CoachCondition Condition { get; }

        public int Count { get; }

        public string Station { get; }

        public string Text { get; }
    }
}
