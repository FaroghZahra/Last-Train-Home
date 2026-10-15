namespace LastTrainHome.Game.Flow.Views
{
    /// <summary>The game's small sounds. Presenters say what happened; the view picks the clip.</summary>
    public interface IGameAudio
    {
        void Play(GameSound sound);
    }

    public enum GameSound
    {
        StopAdded,
        Refused,
        Depart,
        Boards,
        OnTime,

        /// <summary>On time with not a minute to spare.</summary>
        JustInTime,

        /// <summary>Late, or never reached.</summary>
        Missed,
    }

    /// <summary>Plays nothing; for tests, and when the scene has no audio.</summary>
    public sealed class SilentAudio : IGameAudio
    {
        public void Play(GameSound sound)
        {
        }
    }
}
