using System;
using System.Collections;
using System.Collections.Generic;
using Abstractions;
using GridSystem.GridItems;
using UnityEngine;
using Visuals;

namespace PowerUps
{
    /// <summary>
    /// Runs power mode: eating a power pellet swaps the player's and enemies' looks for a few seconds, during which touching an enemy defeats it instead of ending the game. The looks flash back and forth just before it ends.
    /// </summary>
    public class PowerMode : Singleton<PowerMode>
    {
        [SerializeField] [Min(0.1f)] private float durationSeconds = 5f;
        [SerializeField] [Min(0f)] private float warningSeconds = 1f;
        [SerializeField] [Min(0.05f)] private float warningFlashSeconds = 0.15f;
        private readonly List<CharacterAnimator> characters = new();
        private WaitForSeconds steadyWait;
        private WaitForSeconds flashWait;
        private Coroutine routine;

        /// <summary>
        /// Raised when power mode starts, including when another power pellet restarts it.
        /// </summary>
        public static event Action Started;

        /// <summary>
        /// Raised when power mode runs out and the looks return to normal.
        /// </summary>
        public static event Action Ended;

        /// <summary>
        /// Whether power mode is on, so touching an enemy defeats it.
        /// </summary>
        public bool IsActive { get; private set; }

        /// <summary>
        /// Caches the characters whose looks swap and the timings, and starts listening for power pellets.
        /// </summary>
        public void Initialise(IEnumerable<CharacterAnimator> characterAnimators)
        {
            characters.Clear();
            characters.AddRange(characterAnimators);
            steadyWait = new WaitForSeconds(Mathf.Max(0f, durationSeconds - warningSeconds));
            flashWait = new WaitForSeconds(warningFlashSeconds);
            PowerPellet.Eaten += StartPower;
        }

        /// <summary>
        /// Stops listening for power pellets, so a reloaded level does not call into this destroyed component.
        /// </summary>
        private void OnDestroy()
        {
            PowerPellet.Eaten -= StartPower;
        }

        /// <summary>
        /// Starts power mode, or restarts its timer if it is already on.
        /// </summary>
        private void StartPower()
        {
            if (routine != null) StopCoroutine(routine);
            routine = StartCoroutine(RunPower());
            Started?.Invoke();
        }

        /// <summary>
        /// Swaps the looks, holds them, flashes them back and forth for the warning, then restores them and ends power mode.
        /// </summary>
        private IEnumerator RunPower()
        {
            IsActive = true;
            ShowPoweredLooks(true);
            yield return steadyWait;
            var flashes = Mathf.RoundToInt(warningSeconds / warningFlashSeconds);
            for (var i = 0; i < flashes; i++)
            {
                ShowPoweredLooks(i % 2 == 1);
                yield return flashWait;
            }
            ShowPoweredLooks(false);
            IsActive = false;
            routine = null;
            Ended?.Invoke();
        }

        /// <summary>
        /// Shows every character in its powered or normal look.
        /// </summary>
        private void ShowPoweredLooks(bool powered)
        {
            foreach (var character in characters) character.ShowPoweredLook(powered);
        }
    }
}
