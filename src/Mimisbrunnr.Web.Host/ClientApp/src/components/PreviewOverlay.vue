<template>
  <div v-if="active" class="preview-overlay" @click.self="onOverlayClick">
    <button class="preview-close" @click="close" type="button">
      <b-icon-x :width="48" :height="48" />
    </button>
    <div class="preview-body" @wheel.prevent="onZoom" @pointerdown.prevent="startDrag">
      <img v-if="type === 'image'" :src="src" class="preview-image" :style="imageStyle" @error="onError" />
    </div>
    <div v-if="caption" class="preview-caption">{{ caption }}</div>
  </div>
</template>

<script>
import { registerPreview, unregisterPreview } from '@/services/previewApi'
import { BIconX } from 'bootstrap-vue'
export default {
  name: 'PreviewOverlay',
  components: {
    BIconX,
  },
  data() {
    return {
      active: false,
      type: '',
      src: '',
      caption: '',
      scale: 1,
      offsetX: 0,
      offsetY: 0,
      dragging: false,
      dragStartX: 0,
      dragStartY: 0,
      dragOffsetX: 0,
      dragOffsetY: 0,
      suppressClick: false,
    }
  },
  computed: {
    imageStyle() {
      return {
        transform: 'translate(' + this.offsetX + 'px,' + this.offsetY + 'px) scale(' + this.scale + ')',
        transition: this.dragging ? 'none' : '',
        cursor: this.scale > 1 ? (this.dragging ? 'grabbing' : 'grab') : 'zoom-in',
      }
    }
  },
  mounted() {
    registerPreview(this)
    window.addEventListener('keydown', this.onKeyDown)
  },
  beforeDestroy() {
    unregisterPreview()
    window.removeEventListener('keydown', this.onKeyDown)
    this.endDrag()
  },
  methods: {
    open(type, src, caption) {
      this.type = type
      this.src = src
      this.caption = caption || ''
      this.scale = 1
      this.offsetX = 0
      this.offsetY = 0
      this.active = true
      document.body.style.overflow = 'hidden'
    },
    close() {
      this.endDrag()
      this.active = false
      this.type = ''
      this.src = ''
      this.caption = ''
      this.scale = 1
      this.offsetX = 0
      this.offsetY = 0
      document.body.style.overflow = ''
    },
    onKeyDown(e) {
      if (e.key === 'Escape' && this.active) {
        this.close()
      }
    },
    onZoom(e) {
      const rect = e.currentTarget.getBoundingClientRect()
      const px = e.clientX - rect.left - rect.width / 2
      const py = e.clientY - rect.top - rect.height / 2
      const oldScale = this.scale
      const nextScale = Math.min(5, Math.max(0.25, oldScale - e.deltaY * 0.001))
      const imageX = (px - this.offsetX) / oldScale
      const imageY = (py - this.offsetY) / oldScale
      this.offsetX = px - imageX * nextScale
      this.offsetY = py - imageY * nextScale
      this.scale = nextScale
      if (this.scale <= 1) {
        this.offsetX = 0
        this.offsetY = 0
      }
    },
    startDrag(e) {
      if (this.scale <= 1 || e.button !== 0) return
      this.endDrag()
      this.dragging = true
      this.suppressClick = false
      this.dragStartX = e.clientX
      this.dragStartY = e.clientY
      this.dragOffsetX = this.offsetX
      this.dragOffsetY = this.offsetY
      window.addEventListener('pointermove', this.onDrag)
      window.addEventListener('pointerup', this.endDrag)
      window.addEventListener('pointercancel', this.endDrag)
    },
    onDrag(e) {
      if (!this.dragging) return
      const dx = e.clientX - this.dragStartX
      const dy = e.clientY - this.dragStartY
      if (Math.abs(dx) > 3 || Math.abs(dy) > 3) this.suppressClick = true
      this.offsetX = this.dragOffsetX + dx
      this.offsetY = this.dragOffsetY + dy
    },
    endDrag() {
      if (!this.dragging) return
      this.dragging = false
      window.removeEventListener('pointermove', this.onDrag)
      window.removeEventListener('pointerup', this.endDrag)
      window.removeEventListener('pointercancel', this.endDrag)
      if (this.suppressClick) setTimeout(() => { this.suppressClick = false }, 0)
    },
    onOverlayClick() {
      if (this.suppressClick) {
        this.suppressClick = false
        return
      }
      this.close()
    },
    onError() {
      this.src = ''
      this.close()
    }
  }
}
</script>

<style>
.preview-overlay {
  position: fixed;
  top: 0;
  left: 0;
  width: 100%;
  height: 100%;
  background: rgba(0, 0, 0, 0.7);
  z-index: 9999;
}

.preview-close {
  position: absolute;
  top: 16px;
  right: 16px;
  background: none;
  border: none;
  color: #fff;
  cursor: pointer;
  padding: 4px;
  z-index: 1;
  opacity: 0.8;
  line-height: 1;
}

.preview-close:hover {
  opacity: 1;
}

.preview-body {
  display: flex;
  align-items: center;
  justify-content: center;
  width: 100%;
  height: 100%;
  touch-action: none;
}

.preview-image {
  max-width: 90vw;
  max-height: 90vh;
  object-fit: contain;
  border-radius: 4px;
  user-select: none;
  -webkit-user-drag: none;
  cursor: zoom-in;
  transition: transform 0.15s ease;
}

.preview-caption {
  position: absolute;
  bottom: 2em;
  left: 0;
  right: 0;
  color: #fff;
  text-align: center;
  font-size: 14px;
  padding: 0 16px;
  word-break: break-all;
}
</style>
