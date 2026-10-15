using System;
using System.Collections.Generic;
using LastTrainHome.Game.Flow.Views;

namespace LastTrainHome.Game.Tests.Fakes
{
    internal sealed class FakeRide : IRideView
    {
        public readonly List<ArrivalModel> Arrivals = new List<ArrivalModel>();

        public event Action ContinueClicked;

        public bool IsVisible { get; private set; }

        public bool IsShowingArrival { get; private set; }

        public string Progress { get; private set; }

        public void Show() => IsVisible = true;

        public void ShowProgress(string now, string station, string progress) => Progress = $"{now} {station} {progress}";

        public void ShowArrival(ArrivalModel model)
        {
            Arrivals.Add(model);
            IsShowingArrival = true;
        }

        public void ShowCountdown(string text)
        {
        }

        public void HideArrival() => IsShowingArrival = false;

        public void Hide() => IsVisible = false;

        public void Continue() => ContinueClicked?.Invoke();
    }
}
