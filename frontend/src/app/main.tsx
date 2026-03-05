import React from "react";
import ReactDOM from "react-dom/client";
import { App } from "./App";
import { ReactQueryProvider } from "./providers/reactQuery";
import { RealtimeProvider } from "./providers/realtime";
import { CommandPaletteProvider } from "../shared/lib/commandPalette/CommandPaletteContext";
import { ErrorBoundary } from "../shared/lib/error/ErrorBoundary";
import "../shared/styles/global.css";

ReactDOM.createRoot(document.getElementById("root")!).render(
  <React.StrictMode>
    <ErrorBoundary>
      <ReactQueryProvider>
        <CommandPaletteProvider>
          <RealtimeProvider>
            <App />
          </RealtimeProvider>
        </CommandPaletteProvider>
      </ReactQueryProvider>
    </ErrorBoundary>
  </React.StrictMode>
);
