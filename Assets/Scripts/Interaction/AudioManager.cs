using UnityEngine;

namespace CardsVR.Interaction
{
    public class AudioManager : Singleton<AudioManager>
    {
        private AudioSource _audioSource;
        public AudioClip cardSwipe;

        private bool _initialized = false;

        private void Start()
        {
            _audioSource = GetComponent<AudioSource>();
            _initialized = true;
        }

        public void PlayCardSwipe()
        {
            if (!_initialized)
                return;

            _audioSource.PlayOneShot(cardSwipe);
        }
    }
}
