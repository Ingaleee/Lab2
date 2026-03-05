import s from "./Skeleton.module.css";

export function Skeleton({ width, height, className }: { width?: string; height?: string; className?: string }) {
  return (
    <div
      className={`${s.skeleton} ${className || ""}`}
      style={{ width: width || "100%", height: height || "1em" }}
    />
  );
}
