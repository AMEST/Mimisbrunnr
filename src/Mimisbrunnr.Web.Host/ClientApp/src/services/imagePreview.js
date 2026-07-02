import { openPreview } from './previewApi'

export function setupImagePreview(container) {
  let containerEl = container
  if (typeof container === 'string') {
    containerEl = document.querySelector(container)
  }
  if (!containerEl) return
  if (containerEl.dataset.previewInitialized) return
  containerEl.dataset.previewInitialized = 'true'

  containerEl.addEventListener('click', function (e) {
    var img = e.target.closest('img')
    if (!img) return
    if (img.closest('a')) return

    var src = img.src
    if (!src) return

    var name = img.getAttribute('alt') || ''
    if (!name) {
      name = decodeURIComponent(src.split('/').pop() || '')
    }

    openPreview('image', src, name)
  })
}
