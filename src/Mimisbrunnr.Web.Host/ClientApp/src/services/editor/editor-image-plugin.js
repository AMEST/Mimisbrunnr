import Vue from "vue";
import { BIconArrowsAngleExpand } from "bootstrap-vue";
import "./editor-float-menu.css";

export const DEFAULT_IMAGE_SIZE_PRESETS = {
  small: { width: "25%", height: "" },
  medium: { width: "50%", height: "" },
  large: { width: "100%", height: "" },
  original: { width: "", height: "" },
};

const PRESET_ICON_SIZES = {
  small: 12,
  medium: 16,
  large: 20,
};

const IMAGE_MARKUP_REGEX = /!\[[^\]]*\]\([^)]*\)/;
const IMAGE_MARKUP_FULL_REGEX = /^!\[([^\]]*)\]\(([^)]*)\)$/;
const IMAGE_SIZE_REGEX = /\s+=([0-9%]*)x([0-9%]*)\s*$/;
const IMAGE_SIZE_VALUE_REGEX = /^[0-9]+%?$/;

function isValidSizeValue(value) {
  const normalized = String(value == null ? "" : value).trim();
  return normalized === "" || IMAGE_SIZE_VALUE_REGEX.test(normalized);
}

export function findImageMarkupAt(cm, pos) {
  const line = cm.getLine(pos.line);
  const regex = new RegExp(IMAGE_MARKUP_REGEX.source, "g");
  let match;
  while ((match = regex.exec(line)) !== null) {
    if (pos.ch >= match.index && pos.ch <= match.index + match[0].length) {
      return {
        markup: match[0],
        start: match.index,
        end: match.index + match[0].length,
      };
    }
  }
  return null;
}

export function parseImageMarkup(markup) {
  const match = IMAGE_MARKUP_FULL_REGEX.exec(markup);
  if (!match) return null;

  const inner = match[2];
  const sizeMatch = IMAGE_SIZE_REGEX.exec(inner);
  let width = "";
  let height = "";
  let base = inner;
  if (sizeMatch) {
    width = sizeMatch[1];
    height = sizeMatch[2];
    base = inner.slice(0, sizeMatch.index);
  }
  return { alt: match[1], base: base.trimEnd(), width, height };
}

export function buildImageMarkup(parsed, width, height) {
  const size = width || height ? ` =${width}x${height}` : "";
  return `![${parsed.alt}](${parsed.base}${size})`;
}

export function createImageSizeMenu(options = {}) {
  const t = options.t || ((key) => key);
  const presets = options.presets || DEFAULT_IMAGE_SIZE_PRESETS;

  let currentButtons = null;
  let currentCm = null;
  let iconVms = [];

  function onDocumentMouseDown(event) {
    if (!currentButtons) return;
    const target = event.target;
    if (currentButtons.contains(target)) return;
    if (currentCm && currentCm.getWrapperElement().contains(target)) return;
    hide();
  }

  function onDocumentKeyDown(event) {
    if (event.key === "Escape") hide();
  }

  document.addEventListener("mousedown", onDocumentMouseDown);
  document.addEventListener("keydown", onDocumentKeyDown);

  function createIcon(size) {
    const vm = new Vue({
      render: (h) => h(BIconArrowsAngleExpand),
    }).$mount();
    const el = vm.$el;
    el.setAttribute("width", size);
    el.setAttribute("height", size);
    el.style.width = `${size}px`;
    el.style.height = `${size}px`;
    return { el, vm };
  }

  function hide() {
    if (iconVms.length > 0) {
      for (const vm of iconVms) vm.$destroy();
      iconVms = [];
    }
    if (currentButtons) {
      document.body.removeChild(currentButtons);
      currentButtons = null;
    }
  }

  function setSize(cm, pos, width, height) {
    const image = findImageMarkupAt(cm, pos);
    if (!image) {
      hide();
      return;
    }
    const parsed = parseImageMarkup(image.markup);
    if (!parsed) {
      hide();
      return;
    }
    cm.replaceRange(
      buildImageMarkup(parsed, width, height),
      { line: pos.line, ch: image.start },
      { line: pos.line, ch: image.end }
    );
    hide();
  }

  function show(cm, pos, image) {
    hide();
    const parsed = parseImageMarkup(image.markup);
    if (!parsed) return;

    const container = document.createElement("div");
    container.className = "image-buttons";

    const widthInput = document.createElement("input");
    widthInput.type = "text";
    widthInput.className = "image-size-input";
    widthInput.placeholder = t("pageEditor.imageMenu.width");
    widthInput.value = parsed.width;

    const separator = document.createElement("span");
    separator.className = "image-size-separator";
    separator.innerText = "×";

    const heightInput = document.createElement("input");
    heightInput.type = "text";
    heightInput.className = "image-size-input";
    heightInput.placeholder = t("pageEditor.imageMenu.height");
    heightInput.value = parsed.height;

    const applyBtn = document.createElement("button");
    applyBtn.className = "image-size-apply";
    applyBtn.innerText = t("pageEditor.imageMenu.apply");
    applyBtn.title = t("pageEditor.imageMenu.apply");

    const applySize = () => {
      const width = widthInput.value.trim();
      const height = heightInput.value.trim();
      const validWidth = isValidSizeValue(width);
      const validHeight = isValidSizeValue(height);
      widthInput.classList.toggle("is-invalid", !validWidth);
      heightInput.classList.toggle("is-invalid", !validHeight);
      if (!validWidth || !validHeight) return;
      setSize(cm, pos, width, height);
    };
    applyBtn.onclick = applySize;

    const applyOnEnter = (e) => {
      if (e.key === "Enter") {
        e.preventDefault();
        applySize();
      }
    };
    widthInput.addEventListener("keydown", applyOnEnter);
    heightInput.addEventListener("keydown", applyOnEnter);

    container.appendChild(widthInput);
    container.appendChild(separator);
    container.appendChild(heightInput);
    container.appendChild(applyBtn);

    for (const key of ["small", "medium", "large"]) {
      const preset = presets[key];
      const label = t(`pageEditor.imageMenu.${key}`);
      const btn = document.createElement("button");
      btn.title = label;
      btn.setAttribute("aria-label", label);
      const icon = createIcon(PRESET_ICON_SIZES[key]);
      btn.appendChild(icon.el);
      iconVms.push(icon.vm);
      btn.onclick = () => setSize(cm, pos, preset.width, preset.height);
      container.appendChild(btn);
    }

    const originalBtn = document.createElement("button");
    originalBtn.innerText = t("pageEditor.imageMenu.original");
    originalBtn.title = t("pageEditor.imageMenu.original");
    originalBtn.onclick = () =>
      setSize(cm, pos, presets.original.width, presets.original.height);
    container.appendChild(originalBtn);

    const coords = cm.charCoords(pos);
    container.style.left = `${coords.left}px`;
    container.style.top = `${coords.bottom}px`;

    document.body.appendChild(container);
    currentButtons = container;
    currentCm = cm;
  }

  function handleHover(cm, event) {
    const pos = cm.coordsChar({ left: event.clientX, top: event.clientY });
    const image = findImageMarkupAt(cm, pos);
    if (image) {
      show(cm, pos, image);
    } else {
      hide();
    }
  }

  function destroy() {
    hide();
    currentCm = null;
    document.removeEventListener("mousedown", onDocumentMouseDown);
    document.removeEventListener("keydown", onDocumentKeyDown);
  }

  return { handleHover, hide, destroy };
}
