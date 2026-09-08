// PWA push notification helpers called from Blazor via IJSRuntime.
// iOS only exposes Notification / PushManager when the site runs as an installed
// home-screen app, which is why `status()` reports `needsInstall`.

(function () {
    const SW_URL = '/service-worker.js';

    // Mirrors DA_Common.Notifications.NotificationTopic.All — keep in sync when adding topics.
    const ALL_TOPICS = [
        'posts',
        'turn-resolved',
        'gm-question',
        'battle-turn',
        'chat',
        'baron-letter',
    ];

    function isStandalone() {
        return window.matchMedia('(display-mode: standalone)').matches
            || window.navigator.standalone === true;
    }

    function isIos() {
        return /iphone|ipad|ipod/i.test(window.navigator.userAgent)
            || (window.navigator.platform === 'MacIntel' && navigator.maxTouchPoints > 1);
    }

    function isSupported() {
        return 'serviceWorker' in navigator
            && 'PushManager' in window
            && 'Notification' in window;
    }

    function urlBase64ToUint8Array(base64String) {
        const padding = '='.repeat((4 - (base64String.length % 4)) % 4);
        const base64 = (base64String + padding).replace(/-/g, '+').replace(/_/g, '/');
        const raw = window.atob(base64);
        const output = new Uint8Array(raw.length);
        for (let i = 0; i < raw.length; ++i) {
            output[i] = raw.charCodeAt(i);
        }
        return output;
    }

    async function getRegistration() {
        const existing = await navigator.serviceWorker.getRegistration(SW_URL);
        if (existing) {
            return existing;
        }
        return navigator.serviceWorker.register(SW_URL);
    }

    async function currentSubscription() {
        if (!isSupported()) {
            return null;
        }
        const registration = await navigator.serviceWorker.getRegistration(SW_URL);
        if (!registration) {
            return null;
        }
        return registration.pushManager.getSubscription();
    }

    async function postJson(url, body) {
        const response = await fetch(url, {
            method: 'POST',
            credentials: 'same-origin',
            headers: { 'Content-Type': 'application/json' },
            body: JSON.stringify(body),
        });
        return { ok: response.ok, status: response.status, body: response.ok ? await response.json() : null };
    }

    async function getJson(url) {
        const response = await fetch(url, { credentials: 'same-origin' });
        return { ok: response.ok, status: response.status, body: response.ok ? await response.json() : null };
    }

    function normalizeTopics(topics) {
        if (!Array.isArray(topics)) {
            return ALL_TOPICS.slice();
        }
        const wanted = new Set(topics.map((t) => String(t).toLowerCase()));
        return ALL_TOPICS.filter((t) => wanted.has(t));
    }

    async function uploadSubscription(subscription, topics) {
        const json = subscription.toJSON();
        const payload = {
            endpoint: json.endpoint,
            p256dh: json.keys.p256dh,
            auth: json.keys.auth,
        };
        if (Array.isArray(topics)) {
            payload.topics = normalizeTopics(topics);
        }
        return postJson('/api/push/subscribe', payload);
    }

    /// <summary>
    /// Browser may still hold a PushSubscription after the server row was wiped (DB reset,
    /// different account, etc.). Re-upload quietly so topic GETs stop 404-ing.
    /// </summary>
    async function ensureServerKnows(subscription) {
        const topicsUrl = `/api/push/topics?endpoint=${encodeURIComponent(subscription.endpoint)}`;
        const existing = await getJson(topicsUrl);
        if (existing.ok) {
            return { known: true, topics: normalizeTopics(existing.body.topics) };
        }

        const uploaded = await uploadSubscription(subscription, null);
        if (!uploaded.ok) {
            return { known: false, topics: ALL_TOPICS.slice() };
        }

        const again = await getJson(topicsUrl);
        return {
            known: again.ok,
            topics: again.ok ? normalizeTopics(again.body.topics) : ALL_TOPICS.slice(),
        };
    }

    window.dagonitePush = {
        allTopics: function () {
            return ALL_TOPICS.slice();
        },

        status: async function () {
            const supported = isSupported();
            const standalone = isStandalone();
            const result = {
                supported: supported,
                standalone: standalone,
                // On iOS push exists only after "Add to Home Screen".
                needsInstall: isIos() && !standalone,
                permission: supported ? Notification.permission : 'unsupported',
                subscribed: false,
                configured: false,
                topics: ALL_TOPICS.slice(),
                serverKnown: false,
            };

            try {
                const response = await fetch('/api/push/public-key');
                const payload = await response.json();
                result.configured = !!payload.configured;
            } catch {
                result.configured = false;
            }

            if (supported) {
                try {
                    const subscription = await currentSubscription();
                    result.subscribed = !!subscription;
                    if (subscription) {
                        const synced = await ensureServerKnows(subscription);
                        result.serverKnown = synced.known;
                        result.topics = synced.topics;
                    }
                } catch {
                    result.subscribed = false;
                }
            }

            return result;
        },

        enable: async function (topics) {
            if (!isSupported()) {
                return {
                    ok: false,
                    reason: isIos() && !isStandalone() ? 'needs-install' : 'unsupported',
                };
            }

            const keyResponse = await fetch('/api/push/public-key');
            const { configured, publicKey } = await keyResponse.json();
            if (!configured || !publicKey) {
                return { ok: false, reason: 'not-configured' };
            }

            // Must be called from a user gesture; iOS gives no second chance if denied.
            const permission = await Notification.requestPermission();
            if (permission !== 'granted') {
                return { ok: false, reason: permission === 'denied' ? 'denied' : 'dismissed' };
            }

            const registration = await getRegistration();
            await navigator.serviceWorker.ready;

            let subscription = await registration.pushManager.getSubscription();
            if (!subscription) {
                subscription = await registration.pushManager.subscribe({
                    userVisibleOnly: true,
                    applicationServerKey: urlBase64ToUint8Array(publicKey),
                });
            }

            const chosen = normalizeTopics(topics);
            const saved = await uploadSubscription(subscription, chosen);
            if (!saved.ok) {
                return { ok: false, reason: 'subscribe-failed' };
            }

            return { ok: true, reason: 'subscribed', devices: saved.body.devices, topics: chosen };
        },

        disable: async function () {
            const subscription = await currentSubscription();
            if (!subscription) {
                return { ok: true, reason: 'not-subscribed' };
            }

            const endpoint = subscription.endpoint;
            await subscription.unsubscribe();
            await postJson('/api/push/unsubscribe', { endpoint: endpoint });
            return { ok: true, reason: 'unsubscribed' };
        },

        saveTopics: async function (topics) {
            const subscription = await currentSubscription();
            if (!subscription) {
                return { ok: false, reason: 'not-subscribed' };
            }

            const chosen = normalizeTopics(topics);
            // Re-upload first when the server forgot this endpoint, then set preferences.
            const synced = await ensureServerKnows(subscription);
            if (!synced.known) {
                return { ok: false, reason: 'not-found' };
            }

            const saved = await postJson('/api/push/topics', {
                endpoint: subscription.endpoint,
                topics: chosen,
            });
            if (!saved.ok) {
                return { ok: false, reason: 'not-found' };
            }

            return { ok: true, topics: normalizeTopics(saved.body.topics) };
        },

        sendTest: async function () {
            const result = await postJson('/api/push/test', {});
            if (!result.ok) {
                return { ok: false, sent: 0 };
            }
            return { ok: result.body.sent > 0, sent: result.body.sent };
        },
    };
})();
