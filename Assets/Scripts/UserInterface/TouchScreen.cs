#if UNITY_WEBGL && !UNITY_EDITOR
using System.Runtime.InteropServices;
#else
using UnityEngine;
#endif

namespace UserInterface
{
    /// <summary>
    /// Whether the device has a touch screen, asked of the browser in a web build because Unity reports touch support in every browser.
    /// </summary>
    public static class TouchScreen
    {
        #if UNITY_WEBGL && !UNITY_EDITOR
        [DllImport("__Internal")]
        private static extern int BrowserHasTouchScreen();

        /// <summary>
        /// True when the browser reports touch points, touch events or a coarse (finger) pointer.
        /// </summary>
        public static bool IsAvailable => BrowserHasTouchScreen() == 1;
        #else
        /// <summary>
        /// True on touch-capable platforms outside a browser.
        /// </summary>
        public static bool IsAvailable => Input.touchSupported || Application.isMobilePlatform;
        #endif
    }
}
