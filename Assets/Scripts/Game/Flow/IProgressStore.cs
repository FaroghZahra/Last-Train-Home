using System.Collections.Generic;

namespace LastTrainHome.Game.Flow
{
    /// <summary>What the player has found across every week they've played: keepsakes and endings.</summary>
    public interface IProgressStore
    {
        IReadOnlyCollection<string> FoundKeepsakes { get; }

        /// <summary>Endings seen, by how many big nights were kept.</summary>
        IReadOnlyCollection<int> EndingsSeen { get; }

        void AddKeepsake(string keepsakeId);

        void AddEnding(int kept);
    }

    /// <summary>Progress that lasts as long as the game is running; for tests, and when nothing can be saved.</summary>
    public sealed class MemoryProgressStore : IProgressStore
    {
        private readonly HashSet<string> _keepsakes = new HashSet<string>();
        private readonly HashSet<int> _endings = new HashSet<int>();

        public IReadOnlyCollection<string> FoundKeepsakes => _keepsakes;

        public IReadOnlyCollection<int> EndingsSeen => _endings;

        public void AddKeepsake(string keepsakeId) => _keepsakes.Add(keepsakeId);

        public void AddEnding(int kept) => _endings.Add(kept);
    }
}
