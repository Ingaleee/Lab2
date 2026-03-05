import { useEffect, useState } from "react";
import { Link, useLocation } from "react-router-dom";
import type { ReactNode } from "react";
import { Github } from "lucide-react";
import { ConnectionBadge } from "../ConnectionBadge/ConnectionBadge";
import { useRealtimeStatus } from "../../lib/realtime/RealtimeContext";
import s from "./AppHeader.module.css";

export function AppHeader() {
  const loc = useLocation();
  const connectionStatus = useRealtimeStatus();
  const [isScrolled, setIsScrolled] = useState(false);

  useEffect(() => {
    const handleScroll = () => {
      setIsScrolled(window.scrollY > 10);
    };

    window.addEventListener("scroll", handleScroll);
    return () => window.removeEventListener("scroll", handleScroll);
  }, []);

  return (
    <header className={`${s.header} ${isScrolled ? s.scrolled : ""}`}>
      <div className={s.leftGroup}>
        <div className={s.logo}>Order Tracking</div>
        <nav className={s.nav}>
          <NavLink to="/orders" active={loc.pathname.startsWith("/orders")}>
            Orders
          </NavLink>
        </nav>
      </div>

      <div className={s.rightGroup}>
        <ConnectionBadge status={connectionStatus} />
        <a
          href="https://github.com/Ingaleee"
          target="_blank"
          rel="noopener noreferrer"
          className={s.docsLink}
          title="GitHub Repository"
        >
          <Github size={16} />
        </a>
      </div>
    </header>
  );
}

function NavLink({
  to,
  active,
  children,
}: {
  to: string;
  active: boolean;
  children: ReactNode;
}) {
  return (
    <Link to={to} className={`${s.navLink} ${active ? s.active : ""}`}>
      {children}
    </Link>
  );
}
