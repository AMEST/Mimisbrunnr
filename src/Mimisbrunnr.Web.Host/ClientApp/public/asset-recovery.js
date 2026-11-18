/* eslint-env browser */
// Loaded before the bundles, so a missing entry script can recover as well.
(function () {
  if (window.mimisbrunnrAssetRecovery) return
  var recovery = {
    onError: null,
    reload: function (url) {
      var destination = new URL(url, window.location.href).href
      try {
        var key = 'mimisbrunnr:asset-reload'
        var last = JSON.parse(window.sessionStorage.getItem(key) || 'null')
        // A persistent failure or inconsistent replicas must not create an
        // endless reload loop, including before Vue starts.
        if (last && last.url === destination && Date.now() - last.time < 60000) return false
        window.sessionStorage.setItem(key, JSON.stringify({ url: destination, time: Date.now() }))
      } catch {
        return false
      }
      window.location.replace(destination)
      return true
    }
  }
  window.mimisbrunnrAssetRecovery = recovery
  window.addEventListener('error', function (event) {
    var element = event.target
    if (!element || !['SCRIPT', 'LINK'].includes(element.tagName)) return
    var url = new URL(element.src || element.href, window.location.href)
    if (url.origin !== window.location.origin || !/\.[a-f0-9]{8,}\.(js|css)$/i.test(url.pathname)) return
    if (recovery.onError) recovery.onError()
    else recovery.reload(window.location.href)
  }, true)
})()
