#if UNITY_WEBGL && !UNITY_EDITOR
using System.Runtime.InteropServices;
#endif

namespace GameAudio
{
    /// <summary>
    /// Whether audio can be heard yet: in a browser only once the page has had a click, tap or key press, elsewhere always.
    /// </summary>
    public static class WebAudioState
    {
        #if UNITY_WEBGL && !UNITY_EDITOR
        [DllImport("__Internal")]
        private static extern int WebAudioIsRunning();

        /// <summary>
        /// True once the browser runs the audio context; a sound started before then is queued by Unity and replayed from too far in, so it is lost.
        /// </summary>
        public static bool IsRunning => WebAudioIsRunning() == 1;
        #else
        /// <summary>
        /// Always true outside a browser.
        /// </summary>
        public static bool IsRunning => true;
        #endif
    }
}
