import { HTMLAttributes, ReactNode } from "react";
import s from "./Card.module.css";

interface CardProps extends HTMLAttributes<HTMLDivElement> {
  children: ReactNode;
  title?: string;
}

export function Card({ children, title, className, ...props }: CardProps) {
  return (
    <div className={`${s.card} ${className || ""}`} {...props}>
      {title && <div className={s.cardTitle}>{title}</div>}
      {children}
    </div>
  );
}
