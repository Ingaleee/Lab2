import { useEffect, useState, useMemo } from "react";
import { useNavigate } from "react-router-dom";
import { Plus, Package, Search, ArrowDown, ArrowUp } from "lucide-react";
import type { OrderStatus } from "../../../entities/order/model/types";
import { useOrdersQuery } from "../../../entities/order/model/queries";
import s from "./CommandPalette.module.css";
import type { ReactNode } from "react";

type Command = {
  id: string;
  label: string;
  keywords: string[];
  action: () => void;
  icon?: ReactNode;
  group: string;
};

export function CommandPalette({
  isOpen,
  onClose,
  onCreateOrder,
}: {
  isOpen: boolean;
  onClose: () => void;
  onCreateOrder: () => void;
}) {
  const [query, setQuery] = useState("");
  const [selectedIndex, setSelectedIndex] = useState(0);
  const navigate = useNavigate();
  const ordersQuery = useOrdersQuery();

  const commands = useMemo<Command[]>(() => {
    const cmds: Command[] = [
      {
        id: "create-order",
        label: "Create order",
        keywords: ["create", "new", "add"],
        action: () => {
          onCreateOrder();
          onClose();
        },
        icon: <Plus size={16} />,
        group: "Actions",
      },
    ];

    if (ordersQuery.data) {
      ordersQuery.data.forEach((order) => {
        cmds.push({
          id: `order-${order.id}`,
          label: `Go to ${order.orderNumber}`,
          keywords: [order.orderNumber.toLowerCase(), order.description.toLowerCase()],
          action: () => {
            navigate(`/orders/${order.id}`);
            onClose();
          },
          icon: <Package size={16} />,
          group: "Orders",
        });
      });

      const statuses: OrderStatus[] = ["New", "InProgress", "Delivered", "Cancelled"];
      statuses.forEach((status) => {
        cmds.push({
          id: `filter-${status}`,
          label: `Filter: ${status}`,
          keywords: ["filter", status.toLowerCase()],
          action: () => {
            navigate(`/orders?status=${status}`);
            onClose();
          },
          icon: <Search size={16} />,
          group: "Filters",
        });
      });

      cmds.push({
        id: "sort-newest",
        label: "Sort: Newest",
        keywords: ["sort", "newest", "recent"],
        action: () => {
          navigate("/orders?sort=Newest");
          onClose();
        },
        icon: <ArrowDown size={16} />,
        group: "Sort",
      });

      cmds.push({
        id: "sort-oldest",
        label: "Sort: Oldest",
        keywords: ["sort", "oldest"],
        action: () => {
          navigate("/orders?sort=Oldest");
          onClose();
        },
        icon: <ArrowUp size={16} />,
        group: "Sort",
      });
    }

    return cmds;
  }, [ordersQuery.data, navigate, onCreateOrder, onClose]);

  const filteredCommands = useMemo(() => {
    if (!query.trim()) return commands;

    const q = query.toLowerCase();
    return commands.filter(
      (cmd) =>
        cmd.label.toLowerCase().includes(q) ||
        cmd.keywords.some((kw) => kw.includes(q))
    );
  }, [commands, query]);

  const groupedCommands = useMemo(() => {
    const groups: Record<string, Command[]> = {};
    filteredCommands.forEach((cmd) => {
      if (!groups[cmd.group]) {
        groups[cmd.group] = [];
      }
      groups[cmd.group].push(cmd);
    });
    return groups;
  }, [filteredCommands]);

  useEffect(() => {
    if (isOpen) {
      setQuery("");
      setSelectedIndex(0);
    }
  }, [isOpen]);

  useEffect(() => {
    if (!isOpen) return;

    const handleKeyDown = (e: KeyboardEvent) => {
      if (e.key === "Escape") {
        onClose();
      } else if (e.key === "ArrowDown") {
        e.preventDefault();
        setSelectedIndex((prev) => Math.min(prev + 1, filteredCommands.length - 1));
      } else if (e.key === "ArrowUp") {
        e.preventDefault();
        setSelectedIndex((prev) => Math.max(prev - 1, 0));
      } else if (e.key === "Enter" && filteredCommands[selectedIndex]) {
        e.preventDefault();
        filteredCommands[selectedIndex].action();
      }
    };

    window.addEventListener("keydown", handleKeyDown);
    return () => window.removeEventListener("keydown", handleKeyDown);
  }, [isOpen, filteredCommands, selectedIndex, onClose]);

  if (!isOpen) return null;

  return (
    <>
      <div className={s.overlay} onClick={onClose} />
      <div className={s.palette}>
        <div className={s.inputWrapper}>
          <Search className={s.searchIcon} size={18} />
          <input
            className={s.input}
            type="text"
            placeholder="Type a command or search..."
            value={query}
            onChange={(e) => {
              setQuery(e.target.value);
              setSelectedIndex(0);
            }}
            autoFocus
          />
          <kbd className={s.kbd}>Esc</kbd>
        </div>

        <div className={s.results}>
          {Object.entries(groupedCommands).length === 0 ? (
            <div className={s.empty}>No results</div>
          ) : (
            Object.entries(groupedCommands).map(([group, cmds]) => (
              <div key={group}>
                <div className={s.groupLabel}>{group}</div>
                {cmds.map((cmd) => {
                  const globalIndex = filteredCommands.indexOf(cmd);
                  const isSelected = globalIndex === selectedIndex;
                  return (
                    <div
                      key={cmd.id}
                      className={`${s.command} ${isSelected ? s.selected : ""}`}
                      onClick={cmd.action}
                      onMouseEnter={() => setSelectedIndex(globalIndex)}
                    >
                      <span className={s.commandIcon}>{cmd.icon || null}</span>
                      <span className={s.commandLabel}>{cmd.label}</span>
                      {cmd.id.startsWith("order-") && (
                        <span className={s.commandHint}>Enter to open</span>
                      )}
                    </div>
                  );
                })}
              </div>
            ))
          )}
        </div>
      </div>
    </>
  );
}
