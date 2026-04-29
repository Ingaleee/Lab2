import type { InputHTMLAttributes } from "react";

import s from "./Input.module.css";

export type InputProps = InputHTMLAttributes<HTMLInputElement>;

export function Input({ className, ...props }: InputProps) {
  return <input className={`${s.input} ${className || ""}`} {...props} />;
}
