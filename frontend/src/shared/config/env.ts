export const env = {
  apiBaseUrl: import.meta.env.VITE_API_BASE_URL as string,
  signalrHubUrl: import.meta.env.VITE_SIGNALR_HUB_URL as string,
};

if (!env.apiBaseUrl) {
  throw new Error("VITE_API_BASE_URL is not set");
}
if (!env.signalrHubUrl) {
  throw new Error("VITE_SIGNALR_HUB_URL is not set");
}
