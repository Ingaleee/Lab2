import { useRef, useEffect } from "react";
import { X } from "lucide-react";
import type { OrderStatus } from "../../entities/order/model/types";
import type { OrdersListFilter, OrdersSort } from "../../entities/order/model/selectors";
import s from "./OrdersListToolbar.module.css";

const statuses: (OrderStatus | "All")[] = ["All", "New", "InProgress", "Delivered", "Cancelled"];
const sorts: OrdersSort[] = ["Newest", "Oldest"];

export function OrdersListToolbar({
  value,
  onChange,
  searchInputRef,
}: {
  value: OrdersListFilter;
  onChange: (next: OrdersListFilter) => void;
  searchInputRef?: React.MutableRefObject<HTMLInputElement | null>;
}) {
  const internalRef = useRef<HTMLInputElement>(null);
  const inputRef = (searchInputRef || internalRef) as React.MutableRefObject<HTMLInputElement | null>;

  useEffect(() => {
    const handleKeyDown = (e: KeyboardEvent) => {
      if (e.key === "/" && !e.ctrlKey && !e.metaKey && !e.altKey) {
        const target = e.target as HTMLElement;
        if (target.tagName !== "INPUT" && target.tagName !== "TEXTAREA") {
          e.preventDefault();
          inputRef.current?.focus();
        }
      }
    };

    window.addEventListener("keydown", handleKeyDown);
    return () => window.removeEventListener("keydown", handleKeyDown);
  }, [inputRef]);
  const hasActiveFilters =
    value.search.trim().length > 0 || value.status !== "All" || value.sort !== "Newest";

  const clearFilter = (key: keyof OrdersListFilter) => {
    if (key === "search") {
      onChange({ ...value, search: "" });
    } else if (key === "status") {
      onChange({ ...value, status: "All" });
    } else if (key === "sort") {
      onChange({ ...value, sort: "Newest" });
    }
  };

  return (
    <div>
      <div className={s.toolbar}>
        <div className={s.search}>
          <svg
            className={s.searchIcon}
            fill="none"
            stroke="currentColor"
            viewBox="0 0 24 24"
            xmlns="http://www.w3.org/2000/svg"
          >
            <path
              strokeLinecap="round"
              strokeLinejoin="round"
              strokeWidth={2}
              d="M21 21l-6-6m2-5a7 7 0 11-14 0 7 7 0 0114 0z"
            />
          </svg>
          <input
            ref={inputRef}
            className={s.searchInput}
            type="text"
            placeholder="Search by order number or description… (Press /)"
            value={value.search}
            onChange={(e) => onChange({ ...value, search: e.target.value })}
          />
        </div>

        <select
          className={s.select}
          value={value.status}
          onChange={(e) => onChange({ ...value, status: e.target.value as OrderStatus | "All" })}
        >
          {statuses.map((s) => (
            <option key={s} value={s}>
              {s === "All" ? "All statuses" : s}
            </option>
          ))}
        </select>

        <select
          className={s.select}
          value={value.sort}
          onChange={(e) => onChange({ ...value, sort: e.target.value as OrdersSort })}
        >
          {sorts.map((s) => (
            <option key={s} value={s}>
              {s === "Newest" ? "Newest first" : "Oldest first"}
            </option>
          ))}
        </select>
      </div>

      {hasActiveFilters && (
        <div className={s.activeFilters}>
          {value.search.trim().length > 0 && (
            <div className={s.chip}>
              Search: "{value.search}"
              <button
                className={s.chipButton}
                onClick={() => clearFilter("search")}
                aria-label="Clear search"
              >
                <X size={14} />
              </button>
            </div>
          )}
          {value.status !== "All" && (
            <div className={s.chip}>
              Status: {value.status}
              <button
                className={s.chipButton}
                onClick={() => clearFilter("status")}
                aria-label="Clear status filter"
              >
                <X size={14} />
              </button>
            </div>
          )}
          {value.sort !== "Newest" && (
            <div className={s.chip}>
              Sort: {value.sort}
              <button
                className={s.chipButton}
                onClick={() => clearFilter("sort")}
                aria-label="Clear sort"
              >
                <X size={14} />
              </button>
            </div>
          )}
        </div>
      )}
    </div>
  );
}
