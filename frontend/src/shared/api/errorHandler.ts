import axios from "axios";
import type { ProblemDetails } from "./problemDetails";

export function parseApiError(error: unknown): string {
  if (axios.isAxiosError<ProblemDetails>(error)) {
    const problemDetails = error.response?.data;

    if (problemDetails?.errors) {
      const validationErrors = Object.entries(problemDetails.errors)
        .map(([field, messages]) => `${field}: ${messages.join(", ")}`)
        .join("\n");
      return validationErrors;
    }

    if (problemDetails?.detail) {
      return problemDetails.detail;
    }

    if (problemDetails?.title) {
      return problemDetails.title;
    }

    if (error.response?.status === 404) {
      return "Resource not found";
    }

    if (error.response?.status === 409) {
      return "Conflict: Invalid status transition or concurrent modification";
    }

    if (error.response?.status) {
      return `Request failed with status ${error.response.status}`;
    }
  }

  if (error instanceof Error) {
    return error.message;
  }

  return "An unexpected error occurred";
}
