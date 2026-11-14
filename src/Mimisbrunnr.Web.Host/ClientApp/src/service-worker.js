/* eslint-env serviceworker */
import { PrecacheController, PrecacheRoute } from 'workbox-precaching'
import { registerRoute } from 'workbox-routing'

const buildId = process.env.VUE_APP_BUILD_ID
const cachePrefix = 'mimisbrunnr-assets-'
const cacheName = `${cachePrefix}${buildId}-${self.registration.scope}`
const manifest = self.__WB_MANIFEST

function isAsset(url) {
  return url.origin === self.location.origin && /\.[a-f0-9]{8,}\.(js|css)$/i.test(url.pathname)
}

function isValidAsset(request, response) {
  if (!response || response.status !== 200) return false
  const contentType = response.headers.get('content-type') || ''
  return new URL(request.url).pathname.endsWith('.css')
    ? /^text\/css(?:;|$)/i.test(contentType)
    : /^(?:text|application)\/(?:x-)?(?:java|ecma)script(?:;|$)/i.test(contentType)
}

function isAppCache(name) {
  return (name.startsWith(cachePrefix) || name.startsWith('mimisbrunnr-precache-')) &&
    name.endsWith(self.registration.scope)
}

const precache = new PrecacheController({
  cacheName,
  plugins: [{
    cacheWillUpdate: async ({ request, response }) => isValidAsset(request, response) ? response : null,
    cachedResponseWillBeUsed: async ({ request, cachedResponse }) =>
      isValidAsset(request, cachedResponse) ? cachedResponse : null
  }]
})
precache.addToCacheList(manifest)
registerRoute(new PrecacheRoute(precache))

// A new worker can control a page which is still running an older build (in
// particular, an editor). Serve its immutable chunks from that build's cache.
registerRoute(({ url }) => isAsset(url), async ({ request, event }) => {
  for (const name of (await caches.keys()).filter(isAppCache)) {
    const cache = await caches.open(name)
    const response = await cache.match(request)
    if (isValidAsset(request, response)) return response
    if (response) await cache.delete(request)
  }
  const response = await fetch(request)
  if (isValidAsset(request, response)) {
    const copy = response.clone()
    event.waitUntil(caches.open(cacheName).then(cache => cache.put(request, copy)).catch(() => {}))
  }
  return response
})

function getClientBuild(client) {
  return new Promise(resolve => {
    const channel = new MessageChannel()
    const finish = version => {
      clearTimeout(timeout)
      channel.port1.close()
      resolve(version)
    }
    const timeout = setTimeout(() => finish(null), 1000)
    channel.port1.onmessage = ({ data }) => finish(data && typeof data.buildId === 'string' ? data.buildId : null)
    try {
      client.postMessage({ type: 'MIMISBRUNNR_BUILD' }, [channel.port2])
    } catch {
      finish(null)
    }
  })
}

self.addEventListener('install', event => {
  // A missing chunk or an HTML fallback must fail installation, leaving the
  // previous worker and all of its resources intact.
  event.waitUntil(precache.install(event).then(() => self.skipWaiting()))
})

self.addEventListener('activate', event => {
  event.waitUntil((async () => {
    await caches.delete('html-navigations')
    const clients = await self.clients.matchAll({ type: 'window', includeUncontrolled: true })
    const builds = await Promise.all(clients.map(getClientBuild))
    await self.clients.claim()

    // Frozen tabs and pages from before this protocol may not reply. Retain
    // their caches until a later activation can establish what is in use.
    if (builds.some(build => build === null)) return
    const keep = new Set([cacheName, ...builds.map(build => `${cachePrefix}${build}-${self.registration.scope}`)])
    await Promise.all((await caches.keys())
      .filter(name => isAppCache(name) && !keep.has(name))
      .map(name => caches.delete(name)))
  })())
})
