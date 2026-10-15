using System;
using System.Collections.Generic;
using LastTrainHome.Game.Flow.Views;

namespace LastTrainHome.Game.Tests.Fakes
{
    /// <summary>Remembers what a screen last showed and lets a test press its buttons.</summary>
    internal abstract class FakeScreen<TModel>
    {
        public TModel Shown { get; private set; }

        public bool IsVisible { get; private set; }

        public void Show(TModel model)
        {
            Shown = model;
            IsVisible = true;
        }

        public void Hide() => IsVisible = false;
    }
}
