/**
 * Prefer page vertical scroll when the document can move.
 * Otherwise leave the default so the map viewport pans vertically.
 * Horizontal / Shift+wheel always pans the map sideways.
 */
export function attachPageScrollWheel(scrollEl) {
    if (!scrollEl) {
        return { dispose() {} };
    }

    const onWheel = (e) => {
        const absX = Math.abs(e.deltaX);
        const absY = Math.abs(e.deltaY);

        // Trackpad horizontal or Shift+wheel → pan map sideways.
        if (e.shiftKey || absX > absY) {
            if (e.shiftKey && absY > 0 && absX === 0) {
                e.preventDefault();
                scrollEl.scrollLeft += e.deltaY;
            }
            return;
        }

        const se = document.scrollingElement || document.documentElement;
        const pageCanScrollUp = e.deltaY < 0 && se.scrollTop > 0;
        const pageCanScrollDown =
            e.deltaY > 0 &&
            se.scrollTop + se.clientHeight < se.scrollHeight - 1;

        if (pageCanScrollUp || pageCanScrollDown) {
            e.preventDefault();
            se.scrollTop += e.deltaY;
        }
        // else: default — map viewport scrolls vertically
    };

    scrollEl.addEventListener("wheel", onWheel, { passive: false });
    return {
        dispose() {
            scrollEl.removeEventListener("wheel", onWheel);
        },
    };
}
