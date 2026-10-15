using System;
using System.Collections.Generic;
using LastTrainHome.Game.Flow.Views;

namespace LastTrainHome.Game.Tests.Fakes
{
    internal sealed class FakeTitleScreen : FakeScreen<TitleScreenModel>, ITitleScreenView
    {
        public event Action BeginClicked;

        public void Begin() => BeginClicked?.Invoke();
    }
}
