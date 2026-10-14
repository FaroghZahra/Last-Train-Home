using LastTrainHome.Game.Flow.Views;

namespace LastTrainHome.Game.Tests.Fakes
{
    internal sealed class FakeClock : IClockView
    {
        public string Text;

        public void Show(string text) => Text = text;
    }
}
