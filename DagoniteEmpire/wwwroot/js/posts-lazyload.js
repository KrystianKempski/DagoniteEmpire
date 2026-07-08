(function () {
    // Chat-style lazy loading for the chapter thread:
    // - IntersectionObserver on a top sentinel triggers loading older posts.
    // - A scroll listener toggles the "Back to latest" button based on
    //   how far the user is from the bottom of the page.
    // The page scrolls the window (posts live in normal document flow).

    function scroller() {
        return document.scrollingElement || document.documentElement;
    }

    window.PostsLazyLoad = {
        _dotNetRef: null,
        _observer: null,
        _scrollHandler: null,
        _ticking: false,
        _lastShowBackToEnd: null,
        _threshold: 800,

        init: function (dotNetRef, sentinelId, dockId, threshold) {
            this.dispose();

            this._dotNetRef = dotNetRef;
            this._threshold = typeof threshold === 'number' ? threshold : 800;
            this._dockId = dockId;

            var sentinel = document.getElementById(sentinelId);
            if (sentinel && 'IntersectionObserver' in window) {
                var self = this;
                this._observer = new IntersectionObserver(function (entries) {
                    for (var i = 0; i < entries.length; i++) {
                        if (entries[i].isIntersecting) {
                            // C# guards against concurrent / redundant loads.
                            self._dotNetRef.invokeMethodAsync('LoadOlderPostsAsync');
                            break;
                        }
                    }
                }, { root: null, rootMargin: '300px 0px 0px 0px', threshold: 0 });

                this._observer.observe(sentinel);
            }

            var self2 = this;
            this._scrollHandler = function () {
                if (self2._ticking) {
                    return;
                }
                self2._ticking = true;
                window.requestAnimationFrame(function () {
                    self2._ticking = false;
                    self2._evaluateBackToEnd();
                });
            };

            window.addEventListener('scroll', this._scrollHandler, { passive: true });
            window.addEventListener('resize', this._scrollHandler, { passive: true });
            // Initial evaluation.
            this._evaluateBackToEnd();
        },

        _evaluateBackToEnd: function () {
            if (!this._dotNetRef) {
                return;
            }

            var el = scroller();
            var distanceFromBottom = el.scrollHeight - el.scrollTop - el.clientHeight;
            var show = distanceFromBottom > this._threshold;

            if (show !== this._lastShowBackToEnd) {
                this._lastShowBackToEnd = show;
                this._dotNetRef.invokeMethodAsync('SetShowBackToEnd', show);
            }
        },

        getScrollHeight: function () {
            return scroller().scrollHeight;
        },

        // After prepending older posts, keep the viewport anchored on the
        // same content by shifting the scroll position by the height delta.
        restoreScroll: function (prevHeight) {
            var el = scroller();
            var delta = el.scrollHeight - prevHeight;
            if (delta !== 0) {
                window.scrollBy(0, delta);
            }
        },

        // Scroll the whole window to the very bottom. We deliberately do NOT
        // use scrollIntoView on the editor dock: it is position:sticky and thus
        // always in view, so scrollIntoView would be a no-op.
        scrollToBottom: function (smooth) {
            var el = scroller();
            window.scrollTo({ top: el.scrollHeight, behavior: smooth ? 'smooth' : 'auto' });
        },

        // Used on initial load: repeatedly snap to the bottom for a short window
        // so late layout shifts (images, Quill-rendered HTML) don't leave the
        // view above the last post. Also re-snaps when pending images finish.
        scrollToBottomSettled: function () {
            var attempts = 0;
            var maxAttempts = 10;

            function snap() {
                var el = scroller();
                window.scrollTo({ top: el.scrollHeight, behavior: 'auto' });
                attempts++;
                if (attempts < maxAttempts) {
                    setTimeout(snap, 100);
                }
            }
            snap();

            var images = document.querySelectorAll('img');
            for (var i = 0; i < images.length; i++) {
                var img = images[i];
                if (!img.complete) {
                    img.addEventListener('load', function () {
                        window.scrollTo({ top: scroller().scrollHeight, behavior: 'auto' });
                    }, { once: true });
                }
            }
        },

        dispose: function () {
            if (this._observer) {
                this._observer.disconnect();
                this._observer = null;
            }
            if (this._scrollHandler) {
                window.removeEventListener('scroll', this._scrollHandler);
                window.removeEventListener('resize', this._scrollHandler);
                this._scrollHandler = null;
            }
            this._dotNetRef = null;
            this._ticking = false;
            this._lastShowBackToEnd = null;
        }
    };
})();
