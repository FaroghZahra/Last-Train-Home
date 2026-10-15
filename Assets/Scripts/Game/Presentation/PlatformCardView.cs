using System;
using LastTrainHome.Game.Config;
using LastTrainHome.Game.Flow.Views;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace LastTrainHome.Game.Presentation
{
    /// <summary>One passenger on the platform: who they are, what they say, and Ask and Promise buttons.</summary>
    [DisallowMultipleComponent]
    public sealed class PlatformCardView : MonoBehaviour
    {
        [SerializeField, Tooltip("Amber glow behind the card, on while promised.")] private GameObject _promiseRing;
        [SerializeField] private PortraitView _portrait;
        [SerializeField] private TMP_Text _name;
        [SerializeField] private TMP_Text _subtitle;
        [SerializeField] private TMP_Text _line;
        [SerializeField] private GameObject _askedBox;
        [SerializeField] private TMP_Text _askedHeader;
        [SerializeField] private TMP_Text _askedLine;
        [SerializeField] private Button _ask;
        [SerializeField] private Image _askBack;
        [SerializeField] private TMP_Text _askLabel;
        [SerializeField] private Button _promise;
        [SerializeField] private Image _promiseBack;
        [SerializeField] private TMP_Text _promiseLabel;

        private string _passenger;

        /// <summary>Raised with the passenger's name.</summary>
        public event Action<string> AskClicked;

        /// <summary>Raised with the passenger's name.</summary>
        public event Action<string> PromiseClicked;

        public void Show(PlatformCardModel model, PassengerArt art, UiStyle style)
        {
            _passenger = model.Name;
            _promiseRing.SetActive(model.IsPromised);
            _portrait.Show(model.Name, art);
            _name.SetText(model.Name);
            _subtitle.SetText(model.Subtitle);
            _line.SetText(model.PlatformLine);
            _askedBox.SetActive(model.IsAsked);
            _askedHeader.SetText(model.AskedHeader);
            _askedLine.SetText(model.AskedLine);

            _ask.interactable = model.CanAsk;
            _askBack.color = model.CanAsk ? style.PaperLight : style.Disabled;
            _askLabel.color = model.CanAsk ? style.Ink : style.InkSoft;
            _askLabel.SetText(model.AskLabel);

            _promiseBack.color = model.IsPromised ? style.Green : style.PaperLight;
            _promiseLabel.color = model.IsPromised ? Color.white : style.Green;
            _promiseLabel.SetText(model.PromiseLabel);
        }

        private void OnEnable()
        {
            _ask.onClick.AddListener(OnAsk);
            _promise.onClick.AddListener(OnPromise);
        }

        private void OnDisable()
        {
            _ask.onClick.RemoveListener(OnAsk);
            _promise.onClick.RemoveListener(OnPromise);
        }

        private void OnAsk() => AskClicked?.Invoke(_passenger);

        private void OnPromise() => PromiseClicked?.Invoke(_passenger);
    }
}
