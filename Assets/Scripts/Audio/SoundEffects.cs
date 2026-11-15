using System.Collections;
using GameStates;
using GridSystem.GridItems;
using UnityEngine;

namespace GameAudio
{
    /// <summary>
    /// Plays the game's sound effects: a rising trickle for pellets, a chime for bonus pickups, pause and resume tones, and jingles for the level start and the win.
    /// </summary>
    public class SoundEffects : MonoBehaviour
    {
        [SerializeField] private AudioSource effectsSource;
        [SerializeField] private AudioSource jingleSource;
        [SerializeField] private AudioClip[] pelletClips;
        [SerializeField] private AudioClip bonusClip;
        [SerializeField] private AudioClip pauseClip;
        [SerializeField] private AudioClip resumeClip;
        [SerializeField] private AudioClip startClip;
        [SerializeField] private AudioClip winClip;
        [SerializeField] [Min(0f)] private float startJingleWindowSeconds = 4f;
        private int nextPellet;
        private bool gameWon;

        /// <summary>
        /// Plays the level-start jingle as soon as it can be heard.
        /// </summary>
        public void Initialise()
        {
            StartCoroutine(PlayStartJingleWhenAudible());
        }

        /// <summary>
        /// Plays the pause tone when the game pauses and the resume tone when it carries on.
        /// </summary>
        public void PlayPauseChange(bool paused)
        {
            PlayEffect(paused ? pauseClip : resumeClip);
        }

        /// <summary>
        /// Listens for eaten pellets, collected pickups and the win.
        /// </summary>
        private void OnEnable()
        {
            Pellet.Eaten += OnPelletEaten;
            BonusItem.Collected += OnBonusCollected;
            ScoreManager.OnGameWon += OnGameWon;
        }

        /// <summary>
        /// Stops listening for eaten pellets, collected pickups and the win.
        /// </summary>
        private void OnDisable()
        {
            Pellet.Eaten -= OnPelletEaten;
            BonusItem.Collected -= OnBonusCollected;
            ScoreManager.OnGameWon -= OnGameWon;
        }

        /// <summary>
        /// Plays the next note of the pellet trickle, except for the last pellet, whose win jingle has already started.
        /// </summary>
        private void OnPelletEaten(Vector3 position)
        {
            if (gameWon || pelletClips == null || pelletClips.Length == 0) return;
            PlayEffect(pelletClips[nextPellet]);
            nextPellet = (nextPellet + 1) % pelletClips.Length;
        }

        /// <summary>
        /// Plays the bonus chime.
        /// </summary>
        private void OnBonusCollected(Vector3 position, int points)
        {
            PlayEffect(bonusClip);
        }

        /// <summary>
        /// Cuts any playing effects and jingle and plays the win fanfare.
        /// </summary>
        private void OnGameWon()
        {
            gameWon = true;
            if (effectsSource != null) effectsSource.Stop();
            PlayJingle(winClip);
        }

        /// <summary>
        /// Waits for a browser to allow audio, then plays the start jingle; skips it if that takes longer than the window, when it would interrupt play rather than open it.
        /// </summary>
        private IEnumerator PlayStartJingleWhenAudible()
        {
            var deadline = Time.unscaledTime + startJingleWindowSeconds;
            while (!WebAudioState.IsRunning)
            {
                if (Time.unscaledTime > deadline) yield break;
                yield return null;
            }
            if (!gameWon) PlayJingle(startClip);
        }

        /// <summary>
        /// Plays a short effect, overlapping any already playing, once audio can be heard.
        /// </summary>
        private void PlayEffect(AudioClip clip)
        {
            if (effectsSource != null && clip != null && WebAudioState.IsRunning) effectsSource.PlayOneShot(clip);
        }

        /// <summary>
        /// Replaces the playing jingle with another, once audio can be heard.
        /// </summary>
        private void PlayJingle(AudioClip clip)
        {
            if (jingleSource == null || clip == null || !WebAudioState.IsRunning) return;
            jingleSource.clip = clip;
            jingleSource.Play();
        }
    }
}
