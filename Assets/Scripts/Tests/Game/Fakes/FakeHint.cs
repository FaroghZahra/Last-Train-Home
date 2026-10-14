using LastTrainHome.Game.Flow.Views;

namespace LastTrainHome.Game.Tests.Fakes
{
    internal sealed class FakeHint : IHintView
    {
        public string Text = string.Empty;

        public void Show(string message) => Text = message;

        public void Clear() => Text = string.Empty;
    }
}
