using System;
using System.Collections;
using System.Collections.Generic;
using LastTrainHome.Game.Config;
using LastTrainHome.Game.Flow.Views;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace LastTrainHome.Game.Presentation
{
    /// <summary>
    /// The planning screen around the map: passenger cards on the left, the clock and coach line at the top, and the
    /// promise, route and buttons on the right. Leaves the middle clear so the map's stations can be clicked.
    /// </summary>
    [DisallowMultipleComponent]
    public sealed class PlanView : ScreenView, IPlanView
    {
        [SerializeField] private TMP_Text _night;
        [SerializeField] private TMP_Text _clock;
        [SerializeField] private GameObject _coachBox;
        [SerializeField] private TMP_Text _coach;
        [SerializeField, Tooltip("Inactive; cloned for each passenger.")] private PlanCardView _cardTemplate;
        [SerializeField] private GameObject _promiseBox;
        [SerializeField] private Image _promiseBack;
        [SerializeField] private TMP_Text _promiseHeader;
        [SerializeField] private TMP_Text _promiseLine;
        [SerializeField] private TMP_Text _promiseStatus;
        [SerializeField] private TMP_Text _routeHeader;
        [SerializeField, Tooltip("Inactive; cloned for each stop.")] private StopRowView _stopTemplate;
        [SerializeField] private Button _undo;
        [SerializeField] private TMP_Text _undoLabel;
        [SerializeField] private Button _clear;
        [SerializeField] private TMP_Text _clearLabel;
        [SerializeField] private Button _depart;
        [SerializeField] private Image _departBack;
        [SerializeField] private TMP_Text _departLabel;
        [SerializeField] private TMP_Text _departDetail;
        [SerializeField] private GameObject _refusal;
        [SerializeField] private TMP_Text _refusalText;
        [SerializeField, Min(0.1f)] private float _refusalSeconds = 2.5f;
        [SerializeField] private PassengerArt _art;
        [SerializeField] private UiStyle _style;

        private TemplateList<PlanCardView> _cards;
        private TemplateList<StopRowView> _stops;
        private Coroutine _hideRefusal;

        public event Action UndoClicked;

        public event Action ClearClicked;

        public event Action DepartClicked;

        public void Show(PlanModel model)
        {
            if (!gameObject.activeSelf)
            {
                Reveal();
                _refusal.SetActive(false);
            }

            _night.SetText(model.NightLabel);
            _clock.SetText(model.ClockLabel);
            _coachBox.SetActive(!string.IsNullOrEmpty(model.Coach));
            _coach.SetText(model.Coach);

            _cards ??= new TemplateList<PlanCardView>(_cardTemplate);
            IReadOnlyList<PlanCardView> cards = _cards.Show(model.Cards.Count);
            for (int i = 0; i < model.Cards.Count; i++)
            {
                cards[i].Show(model.Cards[i], _art, _style);
            }

            _promiseBox.SetActive(model.HasPromise);
            _promiseBack.color = model.PromiseKept ? _style.Coach : _style.GetTag(TagKind.PromiseBroken).Back;
            _promiseHeader.SetText(model.PromiseHeader);
            _promiseLine.SetText(model.PromiseLine);
            _promiseStatus.SetText(model.PromiseStatus);
            _promiseStatus.color = model.PromiseKept ? _style.GetTag(TagKind.OnTime).Text : _style.GetTag(TagKind.PromiseBroken).Text;

            _routeHeader.SetText(model.RouteHeader);
            _stops ??= new TemplateList<StopRowView>(_stopTemplate);
            IReadOnlyList<StopRowView> stops = _stops.Show(model.Stops.Count);
            for (int i = 0; i < model.Stops.Count; i++)
            {
                stops[i].Show(model.Stops[i]);
            }

            _undoLabel.SetText(model.UndoLabel);
            _clearLabel.SetText(model.ClearLabel);
            _undo.interactable = model.CanEdit;
            _clear.interactable = model.CanEdit;
            _departLabel.SetText(model.DepartLabel);
            _departDetail.SetText(model.DepartDetail);
            _depart.interactable = model.CanDepart;
            _departBack.color = model.CanDepart ? _style.Green : _style.Disabled;
            _departLabel.color = model.CanDepart ? Color.white : _style.InkSoft;
            _departDetail.color = model.CanDepart ? Color.white : _style.InkSoft;
        }

        public void ShowRefusal(string message)
        {
            _refusalText.SetText(message);
            _refusal.SetActive(true);
            if (_hideRefusal != null)
            {
                StopCoroutine(_hideRefusal);
            }

            _hideRefusal = StartCoroutine(HideRefusalLater());
        }

        public void Hide() => Conceal();

        private void OnEnable()
        {
            _undo.onClick.AddListener(OnUndo);
            _clear.onClick.AddListener(OnClear);
            _depart.onClick.AddListener(OnDepart);
        }

        private void OnDisable()
        {
            _undo.onClick.RemoveListener(OnUndo);
            _clear.onClick.RemoveListener(OnClear);
            _depart.onClick.RemoveListener(OnDepart);
            _hideRefusal = null;
        }

        private IEnumerator HideRefusalLater()
        {
            yield return new WaitForSeconds(_refusalSeconds);
            _refusal.SetActive(false);
            _hideRefusal = null;
        }

        private void OnUndo() => UndoClicked?.Invoke();

        private void OnClear() => ClearClicked?.Invoke();

        private void OnDepart() => DepartClicked?.Invoke();
    }
}
