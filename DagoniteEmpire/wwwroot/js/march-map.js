/** Actual eastern-march.jpg size (fallback until naturalWidth is available). */
const DEFAULT_MAP_WIDTH = 8192;
const DEFAULT_MAP_HEIGHT = 6690;

/**
 * Zoom toward cursor; pan via scroll when zoomed in.
 * Scale 1 = largest contain-fit of the map in the viewport (correct aspect, no stretch).
 * Cannot zoom out below 1.
 */
/**
 * @param {number|null|undefined} focusNx - SVG viewBox X (0..1000) to center on load
 * @param {number|null|undefined} focusNy - SVG viewBox Y (0..1000) to center on load
 * @param {number|null|undefined} focusScale - initial zoom (e.g. 4); min 1, max 8
 */
export function attachWheelZoom(viewport, media, focusNx, focusNy, focusScale) {
    if (!viewport || !media)
        return;

    const stage = media.parentElement;
    const scrollpad = stage?.parentElement;
    if (!stage || !scrollpad)
        return;

    const img = media.querySelector('img');

    const initialFocus =
        typeof focusScale === 'number' && focusScale > 1 &&
        typeof focusNx === 'number' && typeof focusNy === 'number'
            ? { nx: focusNx, ny: focusNy, scale: focusScale }
            : null;

    let scale = initialFocus ? Math.min(8, Math.max(1, initialFocus.scale)) : 1;
    const min = 1;
    const max = 8;
    const step = 0.12;

    const imageSize = () => {
        if (img && img.naturalWidth > 0 && img.naturalHeight > 0)
            return { w: img.naturalWidth, h: img.naturalHeight };
        return { w: DEFAULT_MAP_WIDTH, h: DEFAULT_MAP_HEIGHT };
    };

    /** Display size at scale 1 — contain within viewport, preserving map aspect. */
    const fitAtScale1 = () => {
        const vw = Math.max(1, viewport.clientWidth);
        const vh = Math.max(1, viewport.clientHeight);
        const { w: iw, h: ih } = imageSize();
        const mapAspect = iw / ih;
        const viewAspect = vw / vh;
        if (viewAspect >= mapAspect)
            return { w: vh * mapAspect, h: vh };
        return { w: vw, h: vw / mapAspect };
    };

    const applyLayout = () => {
        const fit = fitAtScale1();
        const stageW = fit.w * scale;
        const stageH = fit.h * scale;
        const padW = Math.max(viewport.clientWidth, stageW);
        const padH = Math.max(viewport.clientHeight, stageH);

        scrollpad.style.width = `${padW}px`;
        scrollpad.style.height = `${padH}px`;
        stage.style.width = `${stageW}px`;
        stage.style.height = `${stageH}px`;
    };

    const centerScrollIfNeeded = () => {
        const padW = scrollpad.offsetWidth;
        const padH = scrollpad.offsetHeight;
        const vw = viewport.clientWidth;
        const vh = viewport.clientHeight;
        viewport.scrollLeft = Math.max(0, (padW - vw) / 2);
        viewport.scrollTop = Math.max(0, (padH - vh) / 2);
    };

    const scrollToViewBoxPoint = (nx, ny) => {
        const fit = fitAtScale1();
        const stageW = fit.w * scale;
        const stageH = fit.h * scale;
        const padW = Math.max(viewport.clientWidth, stageW);
        const padH = Math.max(viewport.clientHeight, stageH);
        const pointX = (nx / 1000) * stageW;
        const pointY = (ny / 1000) * stageH;
        const vw = viewport.clientWidth;
        const vh = viewport.clientHeight;
        viewport.scrollLeft = Math.min(Math.max(0, pointX - vw / 2), Math.max(0, padW - vw));
        viewport.scrollTop = Math.min(Math.max(0, pointY - vh / 2), Math.max(0, padH - vh));
    };

    const applyInitialView = () => {
        if (initialFocus) {
            scale = Math.min(max, Math.max(min, initialFocus.scale));
            applyLayout();
            scrollToViewBoxPoint(initialFocus.nx, initialFocus.ny);
        } else {
            applyLayout();
            centerScrollIfNeeded();
        }
    };

    const clampScroll = () => {
        const maxScrollLeft = Math.max(0, scrollpad.offsetWidth - viewport.clientWidth);
        const maxScrollTop = Math.max(0, scrollpad.offsetHeight - viewport.clientHeight);
        viewport.scrollLeft = Math.min(Math.max(0, viewport.scrollLeft), maxScrollLeft);
        viewport.scrollTop = Math.min(Math.max(0, viewport.scrollTop), maxScrollTop);
    };

    const onWheel = (e) => {
        e.preventDefault();

        const rect = viewport.getBoundingClientRect();
        const pointerX = e.clientX - rect.left + viewport.scrollLeft;
        const pointerY = e.clientY - rect.top + viewport.scrollTop;

        const prev = scale;
        scale = e.deltaY < 0
            ? Math.min(max, +(scale + step).toFixed(2))
            : Math.max(min, +(scale - step).toFixed(2));

        if (scale === prev)
            return;

        applyLayout();

        const ratio = scale / prev;
        viewport.scrollLeft = pointerX * ratio - (e.clientX - rect.left);
        viewport.scrollTop = pointerY * ratio - (e.clientY - rect.top);
        clampScroll();
    };

    const onResize = () => {
        const wasMin = scale <= min;
        applyLayout();
        if (wasMin)
            centerScrollIfNeeded();
        else
            clampScroll();
    };

    const onImageReady = () => {
        applyInitialView();
    };

    viewport.addEventListener('wheel', onWheel, { passive: false });
    if (img) {
        if (img.complete && img.naturalWidth > 0)
            onImageReady();
        else
            img.addEventListener('load', onImageReady, { once: true });
    }

    const ro = typeof ResizeObserver !== 'undefined'
        ? new ResizeObserver(onResize)
        : null;
    ro?.observe(viewport);

    applyInitialView();

    // --- drag to pan ---
    const panThreshold = 4;
    let panActive = false;
    let panDidMove = false;
    let panStartX = 0;
    let panStartY = 0;
    let panScrollLeft = 0;
    let panScrollTop = 0;

    const isPanTarget = (target) => {
        if (!(target instanceof Element))
            return true;
        if (target.closest('.march-map-marker'))
            return false;
        if (target.closest('.march-map-inspector'))
            return false;
        return target.closest('.march-map-viewport') === viewport;
    };

    const onPanMouseDown = (e) => {
        if (e.button !== 0)
            return;
        if (!isPanTarget(e.target))
            return;

        panActive = true;
        panDidMove = false;
        panStartX = e.clientX;
        panStartY = e.clientY;
        panScrollLeft = viewport.scrollLeft;
        panScrollTop = viewport.scrollTop;
        viewport.classList.add('march-map-viewport--grabbing');
    };

    const onPanMouseMove = (e) => {
        if (!panActive)
            return;

        const dx = e.clientX - panStartX;
        const dy = e.clientY - panStartY;
        if (!panDidMove && Math.hypot(dx, dy) < panThreshold)
            return;

        panDidMove = true;
        viewport.dataset.marchMapDidPan = '1';
        viewport.scrollLeft = panScrollLeft - dx;
        viewport.scrollTop = panScrollTop - dy;
        clampScroll();
    };

    const endPan = () => {
        if (!panActive)
            return;
        panActive = false;
        viewport.classList.remove('march-map-viewport--grabbing');
    };

    const onPanMouseUp = () => endPan();

    viewport.addEventListener('mousedown', onPanMouseDown);
    window.addEventListener('mousemove', onPanMouseMove);
    window.addEventListener('mouseup', onPanMouseUp);

    return () => {
        viewport.removeEventListener('wheel', onWheel);
        viewport.removeEventListener('mousedown', onPanMouseDown);
        window.removeEventListener('mousemove', onPanMouseMove);
        window.removeEventListener('mouseup', onPanMouseUp);
        ro?.disconnect();
        if (img)
            img.removeEventListener('load', onImageReady);
    };
}

/** True once if the user just dragged the map (clears the flag). */
export function consumeDragPan(viewport) {
    if (!viewport)
        return false;
    const did = viewport.dataset.marchMapDidPan === '1';
    delete viewport.dataset.marchMapDidPan;
    return did;
}

/** Map pointer to SVG viewBox coords (respects preserveAspectRatio). */
export function getMapCoords(svg, clientX, clientY) {
    const ctm = svg.getScreenCTM?.();
    if (ctm && typeof svg.createSVGPoint === 'function') {
        const pt = svg.createSVGPoint();
        pt.x = clientX;
        pt.y = clientY;
        const local = pt.matrixTransform(ctm.inverse());
        return {
            x: Math.max(0, Math.min(1000, local.x)),
            y: Math.max(0, Math.min(1000, local.y)),
        };
    }

    const r = svg.getBoundingClientRect();
    if (r.width <= 0 || r.height <= 0)
        return { x: 500, y: 500 };
    return {
        x: ((clientX - r.left) / r.width) * 1000,
        y: ((clientY - r.top) / r.height) * 1000,
    };
}
