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
        private int nextPellet;
        private bool gameWon;

        /// <summary>
        /// Plays the level-start jingle.
        /// </summary>
        public void Initialise()
        {
            PlayJingle(startClip);
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
        /// Plays a short effect, overlapping any already playing.
        /// </summary>
        private void PlayEffect(AudioClip clip)
        {
            if (effectsSource != null && clip != null) effectsSource.PlayOneShot(clip);
        }

        /// <summary>
        /// Replaces the playing jingle with another.
        /// </summary>
        private void PlayJingle(AudioClip clip)
        {
            if (jingleSource == null || clip == null) return;
            jingleSource.clip = clip;
            jingleSource.Play();
        }
    }
}
