// Blazor Server does not use offline/PWA caching. This file replaces the legacy
// WASM PWA worker so existing registrations can update and remove themselves.
self.addEventListener('install', () => self.skipWaiting());

self.addEventListener('activate', (event) => {
    event.waitUntil((async () => {
        const keys = await caches.keys();
        await Promise.all(
            keys
                .filter((key) => key.startsWith('offline-cache-'))
                .map((key) => caches.delete(key)));
        await self.registration.unregister();
    })());
});
