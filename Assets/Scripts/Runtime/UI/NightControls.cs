using System;
using TMPro;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;

namespace LastTrainHome.UI
{
    /// <summary>
    /// The player's buttons and shortcuts. Reports what was pressed; the controller decides what is allowed.
    /// </summary>
    [DisallowMultipleComponent]
    public sealed class NightControls : MonoBehaviour
    {
        [SerializeField] private Button _undoButton;
        [SerializeField] private Button _clearButton;
        [SerializeField] private Button _departButton;
        [SerializeField, Tooltip("Takes the Depart button's place while the train runs.")] private Button _fastForwardButton;
        [SerializeField] private TMP_Text _fastForwardLabel;
        [SerializeField] private string _fastForwardText = "Fast forward";
        [SerializeField] private string _normalSpeedText = "Normal speed";
        [SerializeField] private InputAction _undoShortcut = new InputAction("Undo", InputActionType.Button, "<Mouse>/rightButton");

        public event Action UndoRequested;
        public event Action ClearRequested;
        public event Action DepartRequested;
        public event Action FastForwardToggled;

        public void ShowPlanning(bool hasStops)
        {
            _undoButton.interactable = hasStops;
            _clearButton.interactable = hasStops;
            _departButton.gameObject.SetActive(true);
            _departButton.interactable = hasStops;
            _fastForwardButton.gameObject.SetActive(false);
        }

        public void ShowTravelling(bool isFastForward)
        {
            _undoButton.interactable = false;
            _clearButton.interactable = false;
            _departButton.gameObject.SetActive(false);
            _fastForwardButton.gameObject.SetActive(true);
            _fastForwardLabel.SetText(isFastForward ? _normalSpeedText : _fastForwardText);
        }

        public void ShowFinished()
        {
            _undoButton.interactable = false;
            _clearButton.interactable = false;
            _departButton.gameObject.SetActive(false);
            _fastForwardButton.gameObject.SetActive(false);
        }

        private void Awake()
        {
            RequiredReferences.Check(this,
                (_undoButton, nameof(_undoButton)), (_clearButton, nameof(_clearButton)),
                (_departButton, nameof(_departButton)), (_fastForwardButton, nameof(_fastForwardButton)),
                (_fastForwardLabel, nameof(_fastForwardLabel)));
        }

        private void OnEnable()
        {
            _undoButton.onClick.AddListener(OnUndo);
            _clearButton.onClick.AddListener(OnClear);
            _departButton.onClick.AddListener(OnDepart);
            _fastForwardButton.onClick.AddListener(OnFastForward);
            _undoShortcut.performed += OnUndoShortcut;
            _undoShortcut.Enable();
        }

        private void OnDisable()
        {
            _undoButton.onClick.RemoveListener(OnUndo);
            _clearButton.onClick.RemoveListener(OnClear);
            _departButton.onClick.RemoveListener(OnDepart);
            _fastForwardButton.onClick.RemoveListener(OnFastForward);
            _undoShortcut.performed -= OnUndoShortcut;
            _undoShortcut.Disable();
        }

        private void OnUndo() => UndoRequested?.Invoke();

        private void OnClear() => ClearRequested?.Invoke();

        private void OnDepart() => DepartRequested?.Invoke();

        private void OnFastForward() => FastForwardToggled?.Invoke();

        private void OnUndoShortcut(InputAction.CallbackContext context) => UndoRequested?.Invoke();
    }
}
