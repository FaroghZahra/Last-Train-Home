using System;
using System.Collections.Generic;
using LastTrainHome.Game.Flow.Views;

namespace LastTrainHome.Game.Tests.Fakes
{
    internal sealed class FakeLogbook : FakeScreen<LogbookModel>, ILogbookView
    {
        public event Action NextClicked;

        public void Next() => NextClicked?.Invoke();
    }
}
