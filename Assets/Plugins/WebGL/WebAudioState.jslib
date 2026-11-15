// Reports whether the browser has let Unity's audio context run, which needs a click, tap or key press in the page.
mergeInto(LibraryManager.library, {
  WebAudioIsRunning__deps: ['$WEBAudio'],
  WebAudioIsRunning: function () {
    return WEBAudio.audioContext && WEBAudio.audioContext.state === 'running' ? 1 : 0;
  }
});
