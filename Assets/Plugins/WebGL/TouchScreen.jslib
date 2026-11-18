// Reports whether the browser has a touch screen, which Unity's own Input.touchSupported claims in every browser.
mergeInto(LibraryManager.library, {
  BrowserHasTouchScreen: function () {
    var coarse = typeof window.matchMedia === 'function' && window.matchMedia('(pointer: coarse)').matches;
    return (navigator.maxTouchPoints > 0 || 'ontouchstart' in window || coarse) ? 1 : 0;
  }
});
