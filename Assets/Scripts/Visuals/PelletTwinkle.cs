using System.Collections;
using System.Collections.Generic;
using GridSystem.GridItems;
using UnityEngine;
using UnityEngine.UI;

namespace Visuals
{
    /// <summary>
    /// Makes a sparse, random handful of pellets twinkle each tick, using one driver instead of an animator per pellet.
    /// </summary>
    public class PelletTwinkle : MonoBehaviour
    {
        private const int PickAttempts = 4;
        private static readonly System.Predicate<Image> IsGone = image => image == null;
        [SerializeField] private Sprite twinkleFrame;
        [SerializeField] [Min(0.05f)] private float tickSeconds = 0.25f;
        [SerializeField] [Min(1)] private int oneIn = 12;
        private readonly List<Image> pellets = new();
        private readonly Dictionary<Image, Sprite> restSprites = new();
        private readonly List<Image> lit = new();
        private readonly List<Image> previouslyLit = new();
        private Coroutine routine;

        /// <summary>
        /// Collects the pellets' Images, remembers each one's own sprite to return to, and starts twinkling them.
        /// </summary>
        public void Initialise(IEnumerable<Pellet> source)
        {
            if (routine != null) StopCoroutine(routine);
            RestLitPellets();
            previouslyLit.Clear();
            pellets.Clear();
            restSprites.Clear();
            foreach (var pellet in source)
            {
                if (!pellet.TryGetComponent(out Image image)) continue;
                pellets.Add(image);
                restSprites[image] = image.sprite;
            }
            routine = StartCoroutine(Twinkle());
        }

        /// <summary>
        /// Each tick, returns last tick's pellets to rest and lights about one in N of the uneaten ones.
        /// </summary>
        private IEnumerator Twinkle()
        {
            var wait = new WaitForSeconds(tickSeconds);
            while (true)
            {
                RestLitPellets();
                pellets.RemoveAll(IsGone);
                var count = pellets.Count == 0 ? 0 : Mathf.Max(1, pellets.Count / oneIn);
                for (var i = 0; i < count; i++)
                {
                    var image = PickPellet();
                    image.sprite = twinkleFrame;
                    lit.Add(image);
                }
                yield return wait;
            }
        }

        /// <summary>
        /// Puts every pellet lit last tick back on its own sprite and remembers them so they are not picked again straight away.
        /// </summary>
        private void RestLitPellets()
        {
            previouslyLit.Clear();
            foreach (var image in lit)
            {
                if (image == null || !restSprites.TryGetValue(image, out var restSprite)) continue;
                image.sprite = restSprite;
                previouslyLit.Add(image);
            }
            lit.Clear();
        }

        /// <summary>
        /// Picks a random pellet, preferring one that is neither lit now nor was lit last tick.
        /// </summary>
        private Image PickPellet()
        {
            var image = pellets[Random.Range(0, pellets.Count)];
            for (var attempt = 1; attempt < PickAttempts && (lit.Contains(image) || previouslyLit.Contains(image)); attempt++)
            {
                image = pellets[Random.Range(0, pellets.Count)];
            }
            return image;
        }
    }
}
