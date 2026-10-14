using System;
using LastTrainHome.Game.Flow.Views;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace LastTrainHome.Game.Presentation
{
    /// <summary>The player's buttons. Reports what was pressed; the presenter decides what is allowed.</summary>
    [DisallowMultipleComponent]
    public sealed class ControlsView : MonoBehaviour, IControlsView
    {
        [SerializeField] private Button _undoButton;
        [SerializeField] private Button _clearButton;
        [SerializeField] private Button _departButton;
        [SerializeField, Tooltip("Takes the Depart button's place while the train runs.")] private Button _fastForwardButton;
        [SerializeField] private TMP_Text _fastForwardLabel;
        [SerializeField] private string _fastForwardText = "Fast forward";
        [SerializeField] private string _normalSpeedText = "Normal speed";

        public event Action UndoClicked;

        public event Action ClearClicked;

        public event Action DepartClicked;

        public event Action FastForwardClicked;

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

        private void OnEnable()
        {
            _undoButton.onClick.AddListener(OnUndoClicked);
            _clearButton.onClick.AddListener(OnClearClicked);
            _departButton.onClick.AddListener(OnDepartClicked);
            _fastForwardButton.onClick.AddListener(OnFastForwardClicked);
        }

        private void OnDisable()
        {
            _undoButton.onClick.RemoveListener(OnUndoClicked);
            _clearButton.onClick.RemoveListener(OnClearClicked);
            _departButton.onClick.RemoveListener(OnDepartClicked);
            _fastForwardButton.onClick.RemoveListener(OnFastForwardClicked);
        }

        private void OnUndoClicked() => UndoClicked?.Invoke();

        private void OnClearClicked() => ClearClicked?.Invoke();

        private void OnDepartClicked() => DepartClicked?.Invoke();

        private void OnFastForwardClicked() => FastForwardClicked?.Invoke();
    }
}
