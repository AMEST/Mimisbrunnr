/* eslint-disable no-console */
import { register } from 'register-service-worker'
import '../public/asset-recovery'

export default function registerServiceWorker(router) {
  if (process.env.NODE_ENV !== 'production') return

  const recovery = window.mimisbrunnrAssetRecovery
  const isEditor = route => route.name === 'pageEdit' || /\/edit\/?$/.test(route.path)
  let updatePending = false
  let navigatingTo = null
  let refreshing = false

  function recoverAsset() {
    // Navigation guards have already permitted departure if navigatingTo is
    // set. Background/editor modal failures must not discard ongoing edits.
    if (isEditor(router.currentRoute) && !navigatingTo) {
      updatePending = true
      return false
    }
    const url = navigatingTo ? router.resolve(navigatingTo).href : window.location.href
    return recovery.reload(url)
  }
  recovery.onError = recoverAsset

  router.beforeEach((to, from, next) => {
    if (updatePending && !isEditor(to)) {
      // Run before resolving async route components, whose old hashes may
      // already be unavailable. Save/cancel and leave guards have run first.
      next(false)
      window.location.assign(router.resolve(to).href)
      return
    }
    navigatingTo = to
    next()
  })
  router.afterEach(() => { navigatingTo = null })

  function isChunkError(error) {
    return error && (error.name === 'ChunkLoadError' || error.code === 'CSS_CHUNK_LOAD_FAILED' ||
      /Loading (?:CSS )?chunk .* failed|Failed to fetch dynamically imported module/i.test(error.message || ''))
  }
  router.onError(error => {
    if (isChunkError(error)) recoverAsset()
    else console.error(error)
    navigatingTo = null
  })
  window.addEventListener('unhandledrejection', event => {
    if (isChunkError(event.reason) && recoverAsset()) event.preventDefault()
  })

  if (!('serviceWorker' in navigator)) return
  let hadController = Boolean(navigator.serviceWorker.controller)
  navigator.serviceWorker.addEventListener('message', event => {
    if (event.data && event.data.type === 'MIMISBRUNNR_BUILD' && event.ports[0]) {
      event.ports[0].postMessage({ buildId: process.env.VUE_APP_BUILD_ID })
    }
  })
  navigator.serviceWorker.addEventListener('controllerchange', () => {
    const wasControlled = hadController
    hadController = Boolean(navigator.serviceWorker.controller)
    if (!wasControlled || !hadController || refreshing) return
    updatePending = true
    if (isEditor(router.currentRoute)) return
    refreshing = recovery.reload(window.location.href)
  })

  let registration
  let checking = false
  function checkForUpdate() {
    if (!registration || checking || document.visibilityState === 'hidden' || navigator.onLine === false) return
    checking = true
    registration.update().catch(error => console.warn('Service worker update check failed:', error))
      .finally(() => { checking = false })
  }
  document.addEventListener('visibilitychange', checkForUpdate)
  window.addEventListener('online', checkForUpdate)
  window.setInterval(checkForUpdate, 5 * 60 * 1000)

  register(`${process.env.BASE_URL}service-worker.js`, {
    registrationOptions: { updateViaCache: 'none' },
    registered(value) {
      registration = value
      checkForUpdate()
    },
    error(error) {
      console.error('Service worker registration failed:', error)
    }
  })
}
