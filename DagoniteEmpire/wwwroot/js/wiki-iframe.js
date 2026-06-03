window.dagoniteWikiIframe = (function () {
  let dotNetHelper = null;
  let iframeEl = null;
  let messageHandler = null;
  let loadHandler = null;
  let clickHandler = null;
  let contentObserver = null;
  let pollTimer = null;
  let blockedPending = false;
  let lastGoodPath = "/wiki/index.html";
  let allowedSlugs = new Set();
  let indexLoaded = false;
  let bypassAccessChecks = false;

  function notifyBlocked() {
    if (!dotNetHelper || blockedPending) {
      return;
    }
    blockedPending = true;
    dotNetHelper.invokeMethodAsync("OnWikiBlocked", lastGoodPath).finally(function () {
      blockedPending = false;
    });
  }

  function normalizeSlug(path) {
    if (!path) {
      return "";
    }

    let slug = path;
    if (slug.startsWith("/wiki/")) {
      slug = slug.slice(6);
    } else if (slug.startsWith("/wiki")) {
      slug = slug.slice(5);
    }

    slug = slug.replace(/^\/+|\/+$/g, "");
    if (slug.endsWith(".html")) {
      slug = slug.slice(0, -5);
    }
    if (slug.endsWith("/index")) {
      slug = slug.slice(0, -6);
    }

    return slug;
  }

  function hrefToSlug(href, basePath) {
    try {
      const url = new URL(href, window.location.origin + basePath);
      const slug = normalizeSlug(url.pathname);
      if (!slug) {
        return "index";
      }
      return slug;
    } catch (_) {
      return "";
    }
  }

  function isBlockedPage(doc) {
    if (!doc?.body) {
      return false;
    }

    const slug = doc.body.dataset?.slug;
    const title = doc.title || "";
    if (slug === "404" || title.includes("Nie znaleziono")) {
      return true;
    }

    if (doc.body.dataset?.wikiAccessDenied != null || title.includes("Brak dostępu")) {
      return true;
    }

    const root = doc.querySelector("#quartz-root");
    if (root?.dataset?.frame === "minimal") {
      return true;
    }

    const articleText = doc.querySelector("#quartz-body article")?.textContent || "";
    if (articleText.includes("prywatna lub nie istnieje")) {
      return true;
    }

    const h1 = doc.querySelector("article h1")?.textContent?.trim();
    if (h1 === "404") {
      return true;
    }

    return false;
  }

  async function loadAllowedSlugs() {
    try {
      const res = await fetch("/wiki/static/contentIndex.json", {
        credentials: "include",
        headers: { Accept: "application/json" },
        cache: "no-store"
      });
      if (!res.ok) {
        indexLoaded = false;
        allowedSlugs = new Set();
        return;
      }

      const data = await res.json();
      allowedSlugs = new Set();
      for (const key of Object.keys(data)) {
        allowedSlugs.add(normalizeSlug(key));
        if (!key.endsWith("/")) {
          allowedSlugs.add(normalizeSlug(key + "/"));
        }
      }
      indexLoaded = true;
    } catch (_) {
      indexLoaded = false;
      allowedSlugs = new Set();
    }
  }

  function isSlugAllowedLocal(slug) {
    if (bypassAccessChecks) {
      return true;
    }
    if (!slug) {
      return null;
    }
    if (!indexLoaded) {
      return null;
    }
    return allowedSlugs.has(slug);
  }

  function preventNavigation(e) {
    e.preventDefault();
    e.stopImmediatePropagation();
  }

  async function isSlugAllowedRemote(slug) {
    const res = await fetch(
      "/api/wiki/access?slug=" + encodeURIComponent(slug),
      { credentials: "include", headers: { Accept: "application/json" }, cache: "no-store" }
    );
    if (!res.ok) {
      return false;
    }
    const data = await res.json();
    return data.allowed === true;
  }

  function inspectIframeDocument() {
    if (!iframeEl) {
      return;
    }

    try {
      const doc = iframeEl.contentDocument;
      const win = iframeEl.contentWindow;
      if (!doc || !win) {
        return;
      }

      if (!bypassAccessChecks && isBlockedPage(doc)) {
        notifyBlocked();
        return;
      }

      const path = win.location.pathname + win.location.search;
      if (path.startsWith("/wiki")) {
        lastGoodPath = path;
        if (dotNetHelper) {
          dotNetHelper.invokeMethodAsync("OnWikiIframeNavigated", path);
        }
      }
    } catch (_) {
      /* same-origin only */
    }
  }

  function attachClickInterceptor() {
    if (!iframeEl) {
      return;
    }

    try {
      const doc = iframeEl.contentDocument;
      if (!doc?.body) {
        return;
      }

      if (clickHandler) {
        doc.removeEventListener("click", clickHandler, true);
      }

      clickHandler = function (e) {
        if (bypassAccessChecks) {
          return;
        }

        const anchor = e.target?.closest?.("a[href]");
        if (!anchor) {
          return;
        }

        const href = anchor.getAttribute("href");
        if (!href || href.startsWith("#") || href.startsWith("mailto:") || href.startsWith("javascript:")) {
          return;
        }

        if (/^https?:\/\//i.test(href)) {
          try {
            const linkUrl = new URL(href);
            if (linkUrl.origin !== window.location.origin) {
              return;
            }
          } catch (_) {
            return;
          }
        }

        const win = iframeEl.contentWindow;
        const slug = hrefToSlug(href, win.location.pathname);
        if (!slug) {
          return;
        }

        const local = isSlugAllowedLocal(slug);
        // Let Quartz handle navigation when allowed or index unknown (hover popover + SPA).
        if (local !== false) {
          return;
        }

        preventNavigation(e);

        isSlugAllowedRemote(slug).then(function (allowed) {
          if (!allowed) {
            notifyBlocked();
            return;
          }
          const resolved = new URL(href, window.location.origin + win.location.pathname);
          const path = resolved.pathname + resolved.search;
          if (win.location.pathname + win.location.search !== path) {
            win.location.assign(path);
          }
        }).catch(function () {
          notifyBlocked();
        });
      };

      doc.addEventListener("click", clickHandler, true);
    } catch (_) {
      /* same-origin only */
    }
  }

  function attachContentObserver() {
    if (!iframeEl) {
      return;
    }

    try {
      const doc = iframeEl.contentDocument;
      if (!doc?.body) {
        return;
      }

      if (contentObserver) {
        contentObserver.disconnect();
      }

      contentObserver = new MutationObserver(function () {
        inspectIframeDocument();
      });
      contentObserver.observe(doc.body, {
        attributes: true,
        attributeFilter: ["data-slug", "data-wiki-access-denied", "data-frame"],
        childList: true,
        subtree: true
      });
    } catch (_) {
      /* same-origin only */
    }
  }

  function startPolling() {
    if (pollTimer) {
      clearInterval(pollTimer);
    }
    pollTimer = setInterval(inspectIframeDocument, 400);
  }

  async function onIframeLoad() {
    await loadAllowedSlugs();
    inspectIframeDocument();
    attachContentObserver();
    attachClickInterceptor();
    startPolling();
  }

  return {
    init: function (helper, iframeId, bypass) {
      dotNetHelper = helper;
      bypassAccessChecks = bypass === true;
      iframeEl = document.getElementById(iframeId);
      if (!iframeEl) {
        return;
      }

      if (!messageHandler) {
        messageHandler = function (e) {
          if (e.origin !== window.location.origin) {
            return;
          }
          const type = e.data?.type;
          if (type === "dagonite-wiki-denied" || type === "dagonite-wiki-blocked") {
            notifyBlocked();
          }
        };
        window.addEventListener("message", messageHandler);
      }

      if (!loadHandler) {
        loadHandler = function () {
          onIframeLoad();
        };
        iframeEl.addEventListener("load", loadHandler);
      }

      if (iframeEl.contentDocument?.readyState === "complete") {
        onIframeLoad();
      }
    },
    setBypassAccessChecks: function (bypass) {
      bypassAccessChecks = bypass === true;
    },
    reloadAllowedIndex: function () {
      return loadAllowedSlugs();
    },
    dispose: function () {
      if (pollTimer) {
        clearInterval(pollTimer);
        pollTimer = null;
      }
      if (contentObserver) {
        contentObserver.disconnect();
        contentObserver = null;
      }
      try {
        if (iframeEl?.contentDocument && clickHandler) {
          iframeEl.contentDocument.removeEventListener("click", clickHandler, true);
        }
      } catch (_) { }
      if (iframeEl && loadHandler) {
        iframeEl.removeEventListener("load", loadHandler);
      }
      if (messageHandler) {
        window.removeEventListener("message", messageHandler);
      }
      dotNetHelper = null;
      iframeEl = null;
      loadHandler = null;
      clickHandler = null;
      messageHandler = null;
      blockedPending = false;
      indexLoaded = false;
      allowedSlugs = new Set();
      bypassAccessChecks = false;
    }
  };
})();
