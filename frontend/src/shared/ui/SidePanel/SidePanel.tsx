import { useEffect } from "react";
import { X } from "lucide-react";
import type { ReactNode } from "react";
import s from "./SidePanel.module.css";

export function SidePanel({
  isOpen,
  onClose,
  title,
  children,
}: {
  isOpen: boolean;
  onClose: () => void;
  title: string;
  children: ReactNode;
}) {
  useEffect(() => {
    if (isOpen) {
      document.body.style.overflow = "hidden";
    } else {
      document.body.style.overflow = "";
    }
    return () => {
      document.body.style.overflow = "";
    };
  }, [isOpen]);

  if (!isOpen) return null;

  return (
    <>
      <div className={s.overlay} onClick={onClose} />
      <div className={s.panel}>
        <div className={s.header}>
          <h2 className={s.title}>{title}</h2>
          <button className={s.closeButton} onClick={onClose} aria-label="Close">
            <X size={20} />
          </button>
        </div>
        <div className={s.content}>{children}</div>
      </div>
    </>
  );
}
