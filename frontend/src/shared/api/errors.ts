import axios from "axios";
import type { ProblemDetails } from "./problemDetails";

export function getErrorMessage(err: unknown): string {
  if (!axios.isAxiosError(err)) return "Unexpected error";

  const data = err.response?.data as ProblemDetails | undefined;

  if (data?.errors) {
    const firstKey = Object.keys(data.errors)[0];
    const firstMsg = firstKey ? data.errors[firstKey]?.[0] : undefined;
    return firstMsg ?? data.title ?? "Validation error";
  }

  if (data?.detail) return data.detail;
  if (data?.title) return data.title;

  if (typeof err.response?.data === "string") return err.response.data;
  return `Request failed (${err.response?.status ?? "unknown"})`;
}
