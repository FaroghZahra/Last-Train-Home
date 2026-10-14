using System.Collections.Generic;
using LastTrainHome.Game.Flow.Views;

namespace LastTrainHome.Game.Tests.Fakes
{
    internal sealed class FakeArrivalLog : IArrivalLogView
    {
        public readonly List<string> Entries = new List<string>();

        public void Clear() => Entries.Clear();

        public void AddEntry(string entry) => Entries.Add(entry);
    }
}
