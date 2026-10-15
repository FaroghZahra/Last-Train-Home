using System.Collections;
using UnityEngine;

namespace LastTrainHome.Game.Presentation
{
    /// <summary>
    /// A full screen of the interface: shown by switching its GameObject on and fading it in, hidden by switching it
    /// off. Put the screen's view on its root, with a CanvasGroup.
    /// </summary>
    [RequireComponent(typeof(CanvasGroup))]
    public abstract class ScreenView : MonoBehaviour
    {
        [SerializeField, Min(0f)] private float _fadeInSeconds = 0.25f;

        private CanvasGroup _group;
        private Coroutine _fade;

        protected void Reveal(float fadeSeconds = -1f)
        {
            gameObject.SetActive(true);
            Group.alpha = 0f;
            if (_fade != null)
            {
                StopCoroutine(_fade);
            }

            _fade = StartCoroutine(FadeIn(fadeSeconds < 0f ? _fadeInSeconds : fadeSeconds));
        }

        protected void Conceal()
        {
            if (_fade != null)
            {
                StopCoroutine(_fade);
                _fade = null;
            }

            gameObject.SetActive(false);
        }

        private CanvasGroup Group => _group != null ? _group : _group = GetComponent<CanvasGroup>();

        private IEnumerator FadeIn(float seconds)
        {
            for (float elapsed = 0f; elapsed < seconds; elapsed += Time.unscaledDeltaTime)
            {
                Group.alpha = elapsed / seconds;
                yield return null;
            }

            Group.alpha = 1f;
            _fade = null;
        }
    }
}
