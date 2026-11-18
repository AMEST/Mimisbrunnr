const assert = require('node:assert/strict')
const { test, before } = require('node:test')
const fs = require('node:fs')
const os = require('node:os')
const path = require('node:path')
const vm = require('node:vm')
const webpack = require('webpack')
const { InjectManifest } = require('workbox-webpack-plugin')

const origin = 'https://wiki.example'
const scope = origin + '/'
const appRoot = path.resolve(__dirname, '..')
const assets = version => [`/js/app.${version.repeat(8)}.js`, `/js/profile.${version.repeat(8)}.js`, `/css/app.${version.repeat(8)}.css`]
const name = build => `mimisbrunnr-assets-${build}-${scope}`
let sources

async function compile(build, urls) {
  const destination = fs.mkdtempSync(path.join(os.tmpdir(), 'mimisbrunnr-sw-test-'))
  const compiler = webpack({
    mode: 'production', context: appRoot, entry: {}, devtool: false,
    output: { path: destination },
    plugins: [
      new webpack.DefinePlugin({ 'process.env.VUE_APP_BUILD_ID': JSON.stringify(build) }),
      new InjectManifest({
        swSrc: path.join(appRoot, 'src/service-worker.js'), include: [],
        additionalManifestEntries: urls.map(url => ({ url, revision: null }))
      })
    ]
  })
  try {
    await new Promise((resolve, reject) => compiler.run((error, stats) => {
      if (error) reject(error)
      else if (stats.hasErrors()) reject(new Error(stats.toString({ all: false, errors: true })))
      else resolve()
    }))
    return fs.readFileSync(path.join(destination, 'service-worker.js'), 'utf8')
  } finally {
    await new Promise(resolve => compiler.close(resolve))
    fs.rmSync(destination, { recursive: true, force: true })
  }
}
before(async () => {
  sources = { A: await compile('A', assets('1')), B: await compile('B', assets('2')) }
})

class Cache {
  entries = new Map()
  key(input) { return typeof input === 'string' ? new URL(input, scope).href : input.url }
  async match(input, options = {}) {
    const key = this.key(input)
    for (const [url, response] of this.entries) {
      if (url === key || (options.ignoreSearch && url.split('?')[0] === key.split('?')[0])) return response.clone()
    }
  }
  async put(input, response) { this.entries.set(this.key(input), response.clone()) }
  async delete(input) { return this.entries.delete(this.key(input)) }
  async keys() { return [...this.entries.keys()].map(url => new Request(url)) }
}
class CacheStorage {
  entries = new Map()
  async open(key) {
    if (!this.entries.has(key)) this.entries.set(key, new Cache())
    return this.entries.get(key)
  }
  async keys() { return [...this.entries.keys()] }
  async delete(key) { return this.entries.delete(key) }
  async match(input, options = {}) {
    for (const [key, cache] of this.entries) {
      if (options.cacheName && options.cacheName !== key) continue
      const response = await cache.match(input, options)
      if (response) return response
    }
  }
}
class ExtendableEvent {
  promises = []
  constructor(type) { this.type = type }
  waitUntil(promise) { this.promises.push(promise) }
  async done() {
    let count = -1
    while (count !== this.promises.length) {
      count = this.promises.length
      await Promise.all(this.promises)
    }
  }
}
class FetchEvent extends ExtendableEvent {
  constructor(request) { super('fetch'); this.request = request }
  respondWith(promise) { this.response = Promise.resolve(promise) }
}
const client = buildId => ({ postMessage(_, ports) { ports[0].postMessage({ buildId }); ports[0].close() } })
function server(version) {
  return {
    version, calls: [], response: null,
    async fetch(request) {
      this.calls.push(request.url)
      if (this.response) return this.response(request)
      const url = new URL(request.url)
      if (assets(this.version).includes(url.pathname)) {
        return new Response('asset:' + url.pathname, { headers: { 'content-type': url.pathname.endsWith('.css') ? 'text/css' : 'text/javascript; charset=utf-8' } })
      }
      if (request.mode === 'navigate') return new Response('current HTML', { headers: { 'content-type': 'text/html' } })
      return new Response('not found', { status: 404 })
    }
  }
}
function worker(build, caches, network, clients = []) {
  const listeners = new Map()
  let skipped = false
  const context = vm.createContext({
    Request, Response, URL, Headers, MessageChannel, FetchEvent, ExtendableEvent,
    setTimeout, clearTimeout, console, caches,
    location: new URL(scope + 'service-worker.js'), registration: { scope },
    clients: { matchAll: async () => clients, claim: async () => {} },
    skipWaiting: async () => { skipped = true }, fetch: request => network.fetch(request),
    addEventListener(type, listener) {
      if (!listeners.has(type)) listeners.set(type, [])
      listeners.get(type).push(listener)
    }
  })
  context.self = context
  vm.runInContext(sources[build], context)
  return {
    get skipped() { return skipped },
    async lifecycle(type) {
      const event = new ExtendableEvent(type)
      for (const listener of listeners.get(type) || []) listener(event)
      await event.done()
    },
    async request(url, navigate = false) {
      const request = new Request(new URL(url, scope))
      if (navigate) Object.defineProperty(request, 'mode', { value: 'navigate' })
      const event = new FetchEvent(request)
      for (const listener of listeners.get('fetch') || []) listener(event)
      const intercepted = Boolean(event.response)
      const response = await (event.response || network.fetch(request))
      await event.done()
      return { response, intercepted }
    }
  }
}

test('Should_PreserveOldLazyChunks_WhenAnotherTabIsStillRunningPreviousBuild', async () => {
  const caches = new CacheStorage()
  const network = server('1')
  const old = worker('A', caches, network)
  await old.lifecycle('install')
  await old.lifecycle('activate')
  network.version = '2'
  const next = worker('B', caches, network, [client('A')])
  await next.lifecycle('install')
  await next.lifecycle('activate')
  network.calls = []
  const { response } = await next.request(assets('1')[1])
  assert.equal(response.status, 200)
  assert.match(await response.text(), /profile\.11111111/)
  assert.equal(network.calls.length, 0)
  assert.ok((await caches.keys()).includes(name('A')))
})

test('Should_RemoveUnusedVersions_WhenAllOpenTabsReportCurrentBuild', async () => {
  const caches = new CacheStorage()
  await caches.open(name('unused'))
  await caches.open('another-application-cache')
  const current = worker('B', caches, server('2'), [client('B')])
  await current.lifecycle('install')
  await current.lifecycle('activate')
  assert.deepEqual((await caches.keys()).sort(), [name('B'), 'another-application-cache'].sort())
})

test('Should_KeepLegacyCaches_WhenAnOldOrFrozenTabCannotReportItsBuild', async () => {
  const caches = new CacheStorage()
  const legacy = `mimisbrunnr-precache-v2-${scope}`
  await (await caches.open(legacy)).put(scope + assets('1')[1].slice(1), new Response('old chunk', { headers: { 'content-type': 'application/javascript' } }))
  const current = worker('B', caches, server('2'), [{ postMessage() {} }])
  await current.lifecycle('install')
  await current.lifecycle('activate')
  assert.ok((await caches.keys()).includes(legacy))
  assert.equal(await (await current.request(assets('1')[1])).response.text(), 'old chunk')
})

test('Should_AlwaysUseNetworkForHtml_WhenOldNavigationCacheExists', async () => {
  const caches = new CacheStorage()
  await (await caches.open('html-navigations')).put(scope, new Response('old HTML'))
  const current = worker('B', caches, server('2'))
  await current.lifecycle('install')
  await current.lifecycle('activate')
  const result = await current.request('/', true)
  assert.equal(result.intercepted, false)
  assert.equal(await result.response.text(), 'current HTML')
  assert.equal((await caches.keys()).includes('html-navigations'), false)
  assert.equal((await current.request('/api/quickstart')).intercepted, false)
})

for (const [description, response] of [
  ['HtmlFallback', () => new Response('<html>fallback</html>', { headers: { 'content-type': 'text/html' } })],
  ['MissingChunk', () => new Response('not found', { status: 404 })]
]) {
  test(`Should_NotActivateWorker_WhenPrecacheReceives${description}`, async () => {
    const caches = new CacheStorage()
    const network = server('2')
    network.response = response
    const current = worker('B', caches, network)
    await assert.rejects(current.lifecycle('install'))
    assert.equal(current.skipped, false)
  })
}

test('Should_RepairPoisonedLegacyAsset_WhenServerNowReturnsCorrectJavascript', async () => {
  const caches = new CacheStorage()
  const legacy = await caches.open(`mimisbrunnr-precache-v2-${scope}`)
  await legacy.put(scope + assets('1')[1].slice(1), new Response('<html>fallback</html>', { headers: { 'content-type': 'text/html' } }))
  const current = worker('B', caches, server('1'))
  const result = await current.request(assets('1')[1])
  assert.equal(result.response.headers.get('content-type'), 'text/javascript; charset=utf-8')
  assert.equal(await legacy.match(scope + assets('1')[1].slice(1)), undefined)
})
