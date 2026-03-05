import { InputHTMLAttributes } from "react";
import s from "./Input.module.css";

interface InputProps extends InputHTMLAttributes<HTMLInputElement> {}

export function Input({ className, ...props }: InputProps) {
  return <input className={`${s.input} ${className || ""}`} {...props} />;
}
