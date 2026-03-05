import type { ButtonHTMLAttributes, ReactNode } from "react";
import s from "./Button.module.css";

interface ButtonProps extends ButtonHTMLAttributes<HTMLButtonElement> {
  children: ReactNode;
}

export function Button({ children, className, ...props }: ButtonProps) {
  return (
    <button className={`${s.button} ${className || ""}`} {...props}>
      {children}
    </button>
  );
}
