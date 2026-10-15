using System;
using System.Collections.Generic;
using LastTrainHome.Game.Flow.Views;

namespace LastTrainHome.Game.Tests.Fakes
{
    internal sealed class FakePlatform : FakeScreen<PlatformModel>, IPlatformView
    {
        public event Action<string> AskClicked;

        public event Action<string> PromiseClicked;

        public event Action ContinueClicked;

        public void Ask(string passengerName) => AskClicked?.Invoke(passengerName);

        public void Promise(string passengerName) => PromiseClicked?.Invoke(passengerName);

        public void Continue() => ContinueClicked?.Invoke();
    }
}
