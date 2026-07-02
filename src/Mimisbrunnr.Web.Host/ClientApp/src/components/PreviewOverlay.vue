<template>
  <div v-if="active" class="preview-overlay" @click.self="close">
    <button class="preview-close" @click="close" type="button">
      <b-icon-x :width="48" :height="48" />
    </button>
    <div class="preview-body" @wheel.prevent="onZoom">
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
    }
  },
  computed: {
    imageStyle() {
      return { transform: 'scale(' + this.scale + ')' }
    }
  },
  mounted() {
    registerPreview(this)
    window.addEventListener('keydown', this.onKeyDown)
  },
  beforeDestroy() {
    unregisterPreview()
    window.removeEventListener('keydown', this.onKeyDown)
  },
  methods: {
    open(type, src, caption) {
      this.type = type
      this.src = src
      this.caption = caption || ''
      this.scale = 1
      this.active = true
      document.body.style.overflow = 'hidden'
    },
    close() {
      this.active = false
      this.type = ''
      this.src = ''
      this.caption = ''
      document.body.style.overflow = ''
    },
    onKeyDown(e) {
      if (e.key === 'Escape' && this.active) {
        this.close()
      }
    },
    onZoom(e) {
      if (e.deltaY > 0) {
        this.scale = Math.max(0.25, this.scale - e.deltaY * 0.001)
      } else {
        this.scale = Math.min(5, this.scale - e.deltaY * 0.001)
      }
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
}

.preview-image {
  max-width: 90vw;
  max-height: 90vh;
  object-fit: contain;
  border-radius: 4px;
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
