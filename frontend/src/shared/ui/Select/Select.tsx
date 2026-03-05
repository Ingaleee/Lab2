import { SelectHTMLAttributes, ReactNode } from "react";
import s from "./Select.module.css";

interface SelectProps extends SelectHTMLAttributes<HTMLSelectElement> {
  children: ReactNode;
}

export function Select({ children, className, ...props }: SelectProps) {
  return (
    <select className={`${s.select} ${className || ""}`} {...props}>
      {children}
    </select>
  );
}
