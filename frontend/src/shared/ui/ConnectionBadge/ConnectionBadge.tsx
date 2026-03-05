import s from "./ConnectionBadge.module.css";

export function ConnectionBadge({
  status = "disconnected",
}: {
  status?: "disconnected" | "connecting" | "connected" | "reconnecting";
}) {
  const label =
    status === "connected"
      ? "Connected"
      : status === "connecting"
        ? "Connecting"
        : status === "reconnecting"
          ? "Reconnecting"
          : "Offline";

  const statusClass =
    status === "connected"
      ? s.connected
      : status === "connecting" || status === "reconnecting"
        ? s.connecting
        : s.disconnected;

  const tooltip =
    status === "connected"
      ? "Real-time updates enabled. Auto-reconnect active."
      : status === "connecting"
        ? "Connecting to real-time server..."
        : status === "reconnecting"
          ? "Reconnecting... Auto-retry enabled."
          : "Real-time offline. Still works via polling.";

  return (
    <div className={`${s.badge} ${statusClass}`} title={tooltip}>
      <span className={s.dot}></span>
      {label}
    </div>
  );
}
