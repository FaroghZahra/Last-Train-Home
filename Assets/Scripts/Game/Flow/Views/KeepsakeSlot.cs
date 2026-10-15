namespace LastTrainHome.Game.Flow.Views
{
    /// <summary>One hook in the cab: a keepsake, or an empty hook.</summary>
    public sealed class KeepsakeSlot
    {
        /// <summary>Null for an empty hook.</summary>
        public string KeepsakeId { get; set; }

        public string Label { get; set; }

        /// <summary>Left tonight.</summary>
        public bool IsNew { get; set; }
    }
}
