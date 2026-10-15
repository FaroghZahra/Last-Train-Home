using System.Collections.Generic;
using LastTrainHome.Game.Config;
using LastTrainHome.Game.Flow.Views;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace LastTrainHome.Game.Presentation
{
    /// <summary>
    /// One passenger while planning: status chip, their short quote, a bar from departure to closing with their
    /// deadline as a line and their arrival as a dot, and a pip for each night of the week.
    /// </summary>
    [DisallowMultipleComponent]
    public sealed class PlanCardView : MonoBehaviour
    {
        [SerializeField, Tooltip("Amber border, on for tonight's promise.")] private GameObject _promiseRing;
        [SerializeField] private PortraitView _portrait;
        [SerializeField] private TMP_Text _name;
        [SerializeField] private TagView _status;
        [SerializeField] private TMP_Text _quote;
        [SerializeField, Tooltip("Anchored along the bar at the deadline.")] private RectTransform _deadline;
        [SerializeField, Tooltip("Anchored along the bar at the arrival.")] private Image _arrival;
        [SerializeField] private TMP_Text _detail;
        [SerializeField, Tooltip("Inactive; cloned for each night.")] private Image _pipTemplate;

        private TemplateList<Image> _pips;

        public void Show(PlanCardModel model, PassengerArt art, UiStyle style)
        {
            _promiseRing.SetActive(model.IsPromised);
            _portrait.Show(model.Name, art);
            _name.SetText(model.Name);
            _status.Show(model.Status, style.GetTag(model.Outcome));
            _quote.SetText(model.Quote);
            _detail.SetText(model.Detail);

            PlaceAlongBar(_deadline, model.Deadline);
            _arrival.gameObject.SetActive(model.Arrival.HasValue);
            if (model.Arrival.HasValue)
            {
                PlaceAlongBar(_arrival.rectTransform, model.Arrival.Value);
                _arrival.color = model.Outcome == OutcomeKind.OnTime ? style.Green : style.GetPip(PipKind.Late);
            }

            _pips ??= new TemplateList<Image>(_pipTemplate);
            IReadOnlyList<Image> pips = _pips.Show(model.Week.Count);
            for (int i = 0; i < model.Week.Count; i++)
            {
                PipKind pip = model.Week[i];
                pips[i].color = style.GetPip(pip);
                bool isBig = pip == PipKind.TonightBig || pip == PipKind.AheadBig;
                pips[i].rectTransform.localRotation = Quaternion.Euler(0f, 0f, isBig ? 45f : 0f);
                pips[i].rectTransform.localScale = Vector3.one * (pip == PipKind.Tonight || pip == PipKind.TonightBig ? 1.25f : 1f);
            }
        }

        private static void PlaceAlongBar(RectTransform mark, float along)
        {
            float x = Mathf.Clamp01(along);
            mark.anchorMin = new Vector2(x, mark.anchorMin.y);
            mark.anchorMax = new Vector2(x, mark.anchorMax.y);
            mark.anchoredPosition = new Vector2(0f, mark.anchoredPosition.y);
        }
    }
}
