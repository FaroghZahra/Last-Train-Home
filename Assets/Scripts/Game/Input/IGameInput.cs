using System;

namespace LastTrainHome.Game.Input
{
    /// <summary>Player shortcuts, as intents. Buttons on screen raise the same intents through the views.</summary>
    public interface IGameInput
    {
        event Action UndoRequested;
    }
}
