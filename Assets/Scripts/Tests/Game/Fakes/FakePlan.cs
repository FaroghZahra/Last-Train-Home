using System;
using System.Collections.Generic;
using LastTrainHome.Game.Flow.Views;

namespace LastTrainHome.Game.Tests.Fakes
{
    internal sealed class FakePlan : FakeScreen<PlanModel>, IPlanView
    {
        public readonly List<string> Refusals = new List<string>();

        public event Action UndoClicked;

        public event Action ClearClicked;

        public event Action DepartClicked;

        public void ShowRefusal(string message) => Refusals.Add(message);

        public void Undo() => UndoClicked?.Invoke();

        public void Clear() => ClearClicked?.Invoke();

        public void Depart() => DepartClicked?.Invoke();
    }
}
