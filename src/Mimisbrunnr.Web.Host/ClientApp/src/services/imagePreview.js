import { openPreview } from './previewApi'

export function setupImagePreview(container) {
  let containerEl = container
  if (typeof container === 'string') {
    containerEl = document.querySelector(container)
  }
  if (!containerEl) return
  if (containerEl.dataset.previewInitialized) return
  containerEl.dataset.previewInitialized = 'true'

  var images = containerEl.querySelectorAll('img')

  for (var i = 0; i < images.length; i++) {
    var img = images[i]
    if (img.closest('a')) continue

    img.style.cursor = 'pointer'
    img.addEventListener('click', function () {
      var src = this.src
      if (!src) return

      var name = this.getAttribute('alt') || ''
      if (!name) {
        name = decodeURIComponent(src.split('/').pop() || '')
      }

      openPreview('image', src, name)
    })
  }
}
