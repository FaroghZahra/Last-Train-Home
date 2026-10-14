using System;
using System.Collections.Generic;
using LastTrainHome.Game.Flow.Views;

namespace LastTrainHome.Game.Tests.Fakes
{
    internal sealed class FakePassengerList : IPassengerListView
    {
        public IReadOnlyList<string> Rows = Array.Empty<string>();

        public void ShowRows(IReadOnlyList<string> rows) => Rows = rows;
    }
}
