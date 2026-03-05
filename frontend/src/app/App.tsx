import { RouterProvider } from "react-router-dom";
import { router } from "./providers/router";

export function App() {
  return <RouterProvider router={router} />;
}
