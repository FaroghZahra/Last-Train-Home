using System;
using LastTrainHome.Game.Flow.Views;
using UnityEngine;

namespace LastTrainHome.Game.Presentation
{
    /// <summary>Plays one clip per <see cref="GameSound"/> through a single AudioSource. A sound with no clip is silent.</summary>
    [RequireComponent(typeof(AudioSource))]
    public sealed class AudioView : MonoBehaviour, IGameAudio
    {
        [SerializeField] private Clip[] _clips = Array.Empty<Clip>();

        private AudioSource _source;

        public void Play(GameSound sound)
        {
            foreach (Clip clip in _clips)
            {
                if (clip.Sound == sound && clip.AudioClip != null)
                {
                    _source.PlayOneShot(clip.AudioClip, clip.Volume);
                    return;
                }
            }
        }

        private void Awake() => _source = GetComponent<AudioSource>();

        [Serializable]
        private struct Clip
        {
            public GameSound Sound;
            public AudioClip AudioClip;

            [Range(0f, 1f)] public float Volume;
        }
    }
}
