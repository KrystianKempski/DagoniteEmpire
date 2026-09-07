// PWA push notification helpers called from Blazor via IJSRuntime.
// iOS only exposes Notification / PushManager when the site runs as an installed
// home-screen app, which is why `status()` reports `needsInstall`.

(function () {
    const SW_URL = '/service-worker.js';

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
        if (!response.ok) {
            throw new Error(`${url} responded ${response.status}`);
        }
        return response.json();
    }

    window.dagonitePush = {
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
                    result.subscribed = !!(await currentSubscription());
                } catch {
                    result.subscribed = false;
                }
            }

            return result;
        },

        enable: async function () {
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

            const json = subscription.toJSON();
            const saved = await postJson('/api/push/subscribe', {
                endpoint: json.endpoint,
                p256dh: json.keys.p256dh,
                auth: json.keys.auth,
            });

            return { ok: true, reason: 'subscribed', devices: saved.devices };
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

        sendTest: async function () {
            const result = await postJson('/api/push/test', {});
            return { ok: result.sent > 0, sent: result.sent };
        },
    };
})();
