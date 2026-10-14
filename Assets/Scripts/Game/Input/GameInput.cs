using System;
using UnityEngine.InputSystem;

namespace LastTrainHome.Game.Input
{
    /// <summary>Reads <see cref="GameControls"/> (generated from GameControls.inputactions) and reports intents.</summary>
    public sealed class GameInput : IGameInput, IDisposable
    {
        private readonly GameControls _controls = new GameControls();

        public GameInput()
        {
            _controls.Night.Undo.performed += OnUndo;
            _controls.Night.Enable();
        }

        public event Action UndoRequested;

        public void Dispose()
        {
            _controls.Night.Undo.performed -= OnUndo;
            _controls.Night.Disable();
            _controls.Dispose();
        }

        private void OnUndo(InputAction.CallbackContext context) => UndoRequested?.Invoke();
    }
}
