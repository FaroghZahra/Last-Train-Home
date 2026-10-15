using System;

namespace LastTrainHome.Game.Flow
{
    /// <summary>One screen of the game, run by <see cref="GameFlow"/> until it reports it's done.</summary>
    public interface IFlowStep : IDisposable
    {
        event Action Finished;

        /// <summary>Shows the screen. Call once.</summary>
        void Start();

        void Tick(float deltaSeconds);
    }
}
