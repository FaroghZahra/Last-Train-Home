using System.Collections;
using LastTrainHome.Game.Flow.Views;
using TMPro;
using UnityEngine;

namespace LastTrainHome.Game.Presentation
{
    /// <summary>A short message that explains why something didn't work, then clears itself.</summary>
    [DisallowMultipleComponent]
    public sealed class HintView : MonoBehaviour, IHintView
    {
        [SerializeField] private TMP_Text _text;
        [SerializeField, Min(0.1f)] private float _visibleSeconds = 2.5f;

        private Coroutine _hide;

        public void Show(string message)
        {
            StopHiding();
            _text.SetText(message);
            _hide = StartCoroutine(HideAfterDelay());
        }

        public void Clear()
        {
            StopHiding();
            _text.SetText(string.Empty);
        }

        private void Awake() => _text.SetText(string.Empty);

        private void OnDisable() => StopHiding();

        private IEnumerator HideAfterDelay()
        {
            yield return new WaitForSeconds(_visibleSeconds);
            _text.SetText(string.Empty);
            _hide = null;
        }

        private void StopHiding()
        {
            if (_hide == null)
            {
                return;
            }

            StopCoroutine(_hide);
            _hide = null;
        }
    }
}
