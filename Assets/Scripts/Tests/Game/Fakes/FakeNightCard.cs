using System;
using System.Collections.Generic;
using LastTrainHome.Game.Flow.Views;

namespace LastTrainHome.Game.Tests.Fakes
{
    internal sealed class FakeNightCard : FakeScreen<NightCardModel>, INightCardView
    {
        public event Action ContinueClicked;

        public void Continue() => ContinueClicked?.Invoke();
    }
}
