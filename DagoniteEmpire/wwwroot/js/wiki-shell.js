let deniedHandler = null;
let activeDotNetRef = null;

export function registerDeniedListener(dotNetRef) {
  unregisterDeniedListener();
  activeDotNetRef = dotNetRef;
  deniedHandler = (event) => {
    if (event.origin !== window.location.origin) {
      return;
    }
    if (event.data?.type === "dagonite-wiki-denied") {
      dotNetRef.invokeMethodAsync("OnWikiAccessDenied");
    }
  };
  window.addEventListener("message", deniedHandler);
}

export function unregisterDeniedListener() {
  if (deniedHandler) {
    window.removeEventListener("message", deniedHandler);
    deniedHandler = null;
    activeDotNetRef = null;
  }
}
