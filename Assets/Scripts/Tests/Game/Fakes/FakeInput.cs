using System;
using LastTrainHome.Game.Input;

namespace LastTrainHome.Game.Tests.Fakes
{
    internal sealed class FakeInput : IGameInput
    {
        public event Action UndoRequested;

        public void PressUndo() => UndoRequested?.Invoke();
    }
}
