// Push-only service worker. It deliberately has no `fetch` handler and no caching:
// the legacy Blazor WASM worker served stale index.html and killed the Blazor Server
// SignalR circuit. Without a fetch handler the browser bypasses this worker for all
// network traffic, so only push delivery goes through it.

const NOTIFICATION_ICON = '/icons/dragon-192.png';

self.addEventListener('install', () => self.skipWaiting());

self.addEventListener('activate', (event) => {
    event.waitUntil((async () => {
        // Drop caches left behind by the old WASM PWA worker.
        if ('caches' in self) {
            const keys = await caches.keys();
            await Promise.all(
                keys
                    .filter((key) => key.startsWith('offline-cache-'))
                    .map((key) => caches.delete(key)));
        }
        await self.clients.claim();
    })());
});

self.addEventListener('push', (event) => {
    let payload = {};
    try {
        payload = event.data ? event.data.json() : {};
    } catch {
        payload = { body: event.data ? event.data.text() : '' };
    }

    const title = payload.title || 'Dagonite Empire';
    const options = {
        body: payload.body || '',
        icon: NOTIFICATION_ICON,
        badge: NOTIFICATION_ICON,
        tag: payload.tag || undefined,
        renotify: !!payload.tag,
        data: { url: payload.url || '/' },
    };

    event.waitUntil(self.registration.showNotification(title, options));
});

self.addEventListener('notificationclick', (event) => {
    event.notification.close();
    const target = new URL(
        (event.notification.data && event.notification.data.url) || '/',
        self.location.origin).href;

    event.waitUntil((async () => {
        const clientList = await self.clients.matchAll({ type: 'window', includeUncontrolled: true });
        for (const client of clientList) {
            if (client.url === target) {
                await client.focus();
                return;
            }
        }
        // Reuse an open window when possible so the SignalR circuit survives.
        if (clientList.length > 0 && 'navigate' in clientList[0]) {
            const client = await clientList[0].focus();
            await client.navigate(target);
            return;
        }
        await self.clients.openWindow(target);
    })());
});

// Push services rotate endpoints; re-register so the user keeps receiving notifications.
self.addEventListener('pushsubscriptionchange', (event) => {
    event.waitUntil((async () => {
        try {
            const response = await fetch('/api/push/public-key');
            const { configured, publicKey } = await response.json();
            if (!configured || !publicKey) {
                return;
            }

            const subscription = await self.registration.pushManager.subscribe({
                userVisibleOnly: true,
                applicationServerKey: publicKey,
            });

            const json = subscription.toJSON();
            await fetch('/api/push/subscribe', {
                method: 'POST',
                credentials: 'same-origin',
                headers: { 'Content-Type': 'application/json' },
                body: JSON.stringify({
                    endpoint: json.endpoint,
                    p256dh: json.keys.p256dh,
                    auth: json.keys.auth,
                }),
            });
        } catch {
            // Nothing to do — the user re-enables notifications from settings.
        }
    })());
});
