let currentPreview = null

export function registerPreview(instance) {
  currentPreview = instance
}

export function unregisterPreview() {
  currentPreview = null
}

export function openPreview(type, src, caption) {
  if (currentPreview) {
    currentPreview.open(type, src, caption)
  }
}

export function closePreview() {
  if (currentPreview) {
    currentPreview.close()
  }
}
