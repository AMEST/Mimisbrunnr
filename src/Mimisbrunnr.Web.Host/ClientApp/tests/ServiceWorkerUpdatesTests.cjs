const assert = require('node:assert/strict')
const { test } = require('node:test')
const fs = require('node:fs')
const path = require('node:path')
const vm = require('node:vm')
const Vue = require('vue')
const Router = require('vue-router')
Vue.use(Router)
const root = path.resolve(__dirname, '..')
const recoverySource = fs.readFileSync(path.join(root, 'public/asset-recovery.js'), 'utf8')
const registerSource = fs.readFileSync(path.join(root, 'src/registerServiceWorker.js'), 'utf8')
  .replace(/^import .*$/gm, '').replace('export default function', 'function')

async function harness({ controlled = true, editor = false, guard, storage = new Map(), serviceWorker = true } = {}) {
  let lazyCalls = 0
  const chunkError = Object.assign(new Error('Loading chunk profile failed'), { name: 'ChunkLoadError' })
  const router = new Router({ mode: 'abstract', routes: [
    { path: '/', component: { render() {} } },
    { path: '/space/KEY/123/edit', name: 'pageEdit', component: { render() {} } },
    { path: '/profile/person', component: () => { lazyCalls++; return Promise.reject(chunkError) } },
    { path: '/broken', component: () => Promise.reject(new Error('Application error')) }
  ] })
  await router.push(editor ? '/space/KEY/123/edit' : '/')
  if (guard) router.beforeEach(guard)
  const events = { window: {}, document: {}, worker: {} }
  function listen(target, type, callback) {
    if (!events[target][type]) events[target][type] = []
    events[target][type].push(callback)
  }
  const replaced = [], assigned = [], intervals = []
  const document = { visibilityState: 'visible', addEventListener: (type, cb) => listen('document', type, cb) }
  const navigator = { onLine: true }
  if (serviceWorker) navigator.serviceWorker = {
    controller: controlled ? {} : null, addEventListener: (type, cb) => listen('worker', type, cb)
  }
  const window = {
    location: { href: 'https://wiki.example' + router.currentRoute.path, origin: 'https://wiki.example',
      assign: url => assigned.push(url), replace: url => replaced.push(url) },
    sessionStorage: { getItem: key => storage.get(key), setItem: (key, value) => storage.set(key, value) },
    addEventListener: (type, cb) => listen('window', type, cb),
    setInterval: cb => intervals.push(cb)
  }
  let hooks
  const context = vm.createContext({ window, document, navigator, URL, Date, console: { error() {}, warn() {} },
    process: { env: { NODE_ENV: 'production', BASE_URL: '/', VUE_APP_BUILD_ID: 'A' } },
    register: (_, value) => { hooks = value }
  })
  vm.runInContext(recoverySource, context)
  vm.runInContext(registerSource, context)
  context.router = router
  vm.runInContext('registerServiceWorker(router)', context)
  return { router, navigator, window, document, replaced, assigned, hooks, intervals, context,
    lazyCalls: () => lazyCalls,
    emit: (target, type, event = {}) => (events[target][type] || []).forEach(callback => callback(event)) }
}

test('Should_ReloadOnLaterUpdates_WhenTabWasOpenedBeforeFirstWorkerInstallation', async () => {
  const h = await harness({ controlled: false })
  h.navigator.serviceWorker.controller = { version: 'A' }
  h.emit('worker', 'controllerchange')
  assert.equal(h.replaced.length, 0)
  h.navigator.serviceWorker.controller = { version: 'B' }
  h.emit('worker', 'controllerchange')
  h.emit('worker', 'controllerchange')
  assert.deepEqual(h.replaced, ['https://wiki.example/'])
})

test('Should_NavigateBeforeResolvingChunks_WhenLeavingEditorWithPendingUpdate', async () => {
  const h = await harness({ editor: true })
  h.emit('worker', 'controllerchange')
  assert.equal(h.replaced.length, 0)
  await assert.rejects(h.router.push('/profile/person?tab=about#details'))
  assert.deepEqual(h.assigned, ['/profile/person?tab=about#details'])
  assert.equal(h.lazyCalls(), 0)
})

test('Should_PreserveEdits_WhenAnEarlierLeaveGuardCancelsNavigation', async () => {
  const h = await harness({ editor: true, guard: (_, __, next) => next(false) })
  h.emit('worker', 'controllerchange')
  await assert.rejects(h.router.push('/profile/person'))
  assert.equal(h.assigned.length, 0)
  assert.equal(h.replaced.length, 0)
})

test('Should_RecoverTargetUrl_WhenLazyRouteChunkIsMissing', async () => {
  const h = await harness()
  await assert.rejects(h.router.push('/profile/person?tab=about#details'), { name: 'ChunkLoadError' })
  assert.deepEqual(h.replaced, ['https://wiki.example/profile/person?tab=about#details'])
})

test('Should_DeferRecovery_WhenBackgroundAssetFailsInsideEditor', async () => {
  const h = await harness({ editor: true })
  h.emit('window', 'error', { target: { tagName: 'SCRIPT', src: 'https://wiki.example/js/modal.12345678.js' } })
  assert.equal(h.replaced.length, 0)
  await assert.rejects(h.router.push('/profile/person'))
  assert.deepEqual(h.assigned, ['/profile/person'])
  assert.equal(h.lazyCalls(), 0)
})

test('Should_NotReload_WhenRouteFailsForAnUnrelatedReason', async () => {
  const h = await harness()
  await assert.rejects(h.router.push('/broken'))
  assert.equal(h.replaced.length, 0)
})

test('Should_ReportBuildId_WhenWorkerChecksWhichCachesAreInUse', async () => {
  const h = await harness()
  let version
  h.emit('worker', 'message', { data: { type: 'MIMISBRUNNR_BUILD' }, ports: [{ postMessage: data => { version = data.buildId } }] })
  assert.equal(version, 'A')
})

test('Should_CheckForUpdates_WhenVisibleTabReturnsOnlineOrTimerFires', async () => {
  const h = await harness()
  let checks = 0
  h.hooks.registered({ update: async () => { checks++ } })
  await new Promise(resolve => setImmediate(resolve))
  h.document.visibilityState = 'hidden'
  h.intervals[0]()
  assert.equal(checks, 1)
  h.document.visibilityState = 'visible'
  h.emit('document', 'visibilitychange')
  await new Promise(resolve => setImmediate(resolve))
  h.emit('window', 'online')
  await new Promise(resolve => setImmediate(resolve))
  h.intervals[0]()
  assert.equal(checks, 4)
  assert.equal(h.hooks.registrationOptions.updateViaCache, 'none')
})

test('Should_RecoverMissingEntryScriptOnlyOnce_WhenFailurePersistsAcrossReloads', async () => {
  const h = await harness({ serviceWorker: false })
  const event = { target: { tagName: 'SCRIPT', src: 'https://wiki.example/js/app.12345678.js' } }
  delete h.window.mimisbrunnrAssetRecovery
  vm.runInContext(recoverySource, h.context)
  h.emit('window', 'error', event)
  assert.equal(h.replaced.length, 1)
  delete h.window.mimisbrunnrAssetRecovery
  vm.runInContext(recoverySource, h.context)
  h.emit('window', 'error', event)
  assert.equal(h.replaced.length, 1)
})
