using System;
using LastTrainHome.Game.Flow.Views;

namespace LastTrainHome.Game.Tests.Fakes
{
    internal sealed class FakeSummary : ISummaryView
    {
        public bool IsShown;
        public bool CanContinue;
        public string Body;

        public event Action PlanAgainClicked;

        public event Action NextNightClicked;

        public void Show(string body, bool canContinue)
        {
            IsShown = true;
            Body = body;
            CanContinue = canContinue;
        }

        public void Hide() => IsShown = false;

        public void ClickPlanAgain() => PlanAgainClicked?.Invoke();

        public void ClickNextNight() => NextNightClicked?.Invoke();
    }
}
