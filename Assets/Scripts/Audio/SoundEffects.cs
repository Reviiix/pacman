using System.Collections;
using GameStates;
using GridSystem.GridItems;
using Movement;
using PowerUps;
using UnityEngine;

namespace GameAudio
{
    /// <summary>
    /// Plays the game's sound effects: a rising trickle for pellets, a chime for bonus pickups, a charge-up and a wind-down for power mode, pause and resume tones, and jingles for the level start, the win and game over.
    /// </summary>
    public class SoundEffects : MonoBehaviour
    {
        [SerializeField] private AudioSource effectsSource;
        [SerializeField] private AudioSource jingleSource;
        [SerializeField] private AudioClip[] pelletClips;
        [SerializeField] private AudioClip bonusClip;
        [SerializeField] private AudioClip powerOnClip;
        [SerializeField] private AudioClip powerOffClip;
        [SerializeField] private AudioClip pauseClip;
        [SerializeField] private AudioClip resumeClip;
        [SerializeField] private AudioClip startClip;
        [SerializeField] private AudioClip winClip;
        [SerializeField] private AudioClip loseClip;
        [SerializeField] [Min(0f)] private float startJingleWindowSeconds = 4f;
        private int nextPellet;
        private bool gameOver;

        /// <summary>
        /// Plays the level-start jingle as soon as it can be heard.
        /// </summary>
        public void Initialise()
        {
            StartCoroutine(PlayStartJingleWhenAudible());
        }

        /// <summary>
        /// Cuts any playing effects and jingle and plays the game-over sound.
        /// </summary>
        public void PlayLose()
        {
            EndWith(loseClip);
        }

        /// <summary>
        /// Plays the pause tone when the game pauses and the resume tone when it carries on.
        /// </summary>
        public void PlayPauseChange(bool paused)
        {
            PlayEffect(paused ? pauseClip : resumeClip);
        }

        /// <summary>
        /// Listens for eaten pellets, collected pickups, power mode starting and ending, the win and the player being caught.
        /// </summary>
        private void OnEnable()
        {
            Pellet.Eaten += OnPelletEaten;
            BonusItem.Collected += OnBonusCollected;
            PowerMode.Started += OnPowerStarted;
            PowerMode.Ended += OnPowerEnded;
            ScoreManager.OnGameWon += OnGameWon;
            EnemyContact.PlayerCaught += PlayLose;
        }

        /// <summary>
        /// Stops listening for eaten pellets, collected pickups, power mode starting and ending, the win and the player being caught.
        /// </summary>
        private void OnDisable()
        {
            Pellet.Eaten -= OnPelletEaten;
            BonusItem.Collected -= OnBonusCollected;
            PowerMode.Started -= OnPowerStarted;
            PowerMode.Ended -= OnPowerEnded;
            ScoreManager.OnGameWon -= OnGameWon;
            EnemyContact.PlayerCaught -= PlayLose;
        }

        /// <summary>
        /// Plays the next note of the pellet trickle, except for the last pellet, whose win jingle has already started.
        /// </summary>
        private void OnPelletEaten(Vector3 position)
        {
            if (gameOver || pelletClips == null || pelletClips.Length == 0) return;
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
        /// Plays the power-up charge when power mode starts or restarts, unless the game is over.
        /// </summary>
        private void OnPowerStarted()
        {
            if (!gameOver) PlayEffect(powerOnClip);
        }

        /// <summary>
        /// Plays the power-down when power mode runs out, unless the game is over.
        /// </summary>
        private void OnPowerEnded()
        {
            if (!gameOver) PlayEffect(powerOffClip);
        }

        /// <summary>
        /// Cuts any playing effects and jingle and plays the win fanfare.
        /// </summary>
        private void OnGameWon()
        {
            EndWith(winClip);
        }

        /// <summary>
        /// Ends the game's sound once (a second ending, such as two enemies catching the player together, is ignored): no more pellet notes or start jingle, any playing effects cut, and the closing jingle played.
        /// </summary>
        private void EndWith(AudioClip clip)
        {
            if (gameOver) return;
            gameOver = true;
            if (effectsSource != null) effectsSource.Stop();
            PlayJingle(clip);
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
            if (!gameOver) PlayJingle(startClip);
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
