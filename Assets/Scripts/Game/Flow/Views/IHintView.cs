namespace LastTrainHome.Game.Flow.Views
{
    public interface IHintView
    {
        /// <summary>Shows a short message that clears itself after a moment.</summary>
        void Show(string message);

        void Clear();
    }
}
