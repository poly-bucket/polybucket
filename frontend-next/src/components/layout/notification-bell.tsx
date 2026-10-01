"use client";

import { useCallback, useEffect, useId, useRef, useState } from "react";
import { createPortal } from "react-dom";
import Link from "next/link";
import { Bell, CheckCheck } from "lucide-react";
import {
  formatNotificationTime,
  notificationsService,
  type NotificationItem,
} from "@/lib/services/notificationsService";
import { ListRowSkeleton } from "@/components/ui/skeletons";

export const NOTIFICATION_POLL_INTERVAL_MS = 60_000;
const DROPDOWN_PAGE_SIZE = 10;

function badgeLabel(count: number): string {
  return count > 9 ? "9+" : String(count);
}

export function NotificationBell() {
  const panelId = useId();
  const anchorRef = useRef<HTMLButtonElement>(null);
  const panelRef = useRef<HTMLDivElement>(null);
  const [open, setOpen] = useState(false);
  const [unreadCount, setUnreadCount] = useState(0);
  const [items, setItems] = useState<NotificationItem[] | null>(null);
  const [loadError, setLoadError] = useState(false);
  const [position, setPosition] = useState({ top: 0, right: 16 });

  const refreshCount = useCallback(async () => {
    try {
      setUnreadCount(await notificationsService.getUnreadCount());
    } catch {
      return;
    }
  }, []);

  const loadItems = useCallback(async () => {
    setLoadError(false);
    try {
      const page = await notificationsService.getNotifications(1, DROPDOWN_PAGE_SIZE);
      setItems(page.items);
      setUnreadCount(page.unreadCount);
    } catch {
      setLoadError(true);
    }
  }, []);

  useEffect(() => {
    refreshCount();
    const interval = window.setInterval(() => {
      if (document.visibilityState !== "hidden") {
        refreshCount();
      }
    }, NOTIFICATION_POLL_INTERVAL_MS);
    const handleFocus = () => refreshCount();
    window.addEventListener("focus", handleFocus);
    return () => {
      window.clearInterval(interval);
      window.removeEventListener("focus", handleFocus);
    };
  }, [refreshCount]);

  useEffect(() => {
    if (!open) {
      return;
    }
    const handlePointer = (event: MouseEvent) => {
      const target = event.target as Node;
      if (!panelRef.current?.contains(target) && !anchorRef.current?.contains(target)) {
        setOpen(false);
      }
    };
    const handleKey = (event: KeyboardEvent) => {
      if (event.key === "Escape") {
        setOpen(false);
        anchorRef.current?.focus();
      }
    };
    document.addEventListener("mousedown", handlePointer);
    document.addEventListener("keydown", handleKey);
    return () => {
      document.removeEventListener("mousedown", handlePointer);
      document.removeEventListener("keydown", handleKey);
    };
  }, [open]);

  const toggle = () => {
    if (open) {
      setOpen(false);
      return;
    }
    const rect = anchorRef.current?.getBoundingClientRect();
    if (rect) {
      setPosition({ top: rect.bottom + 8, right: Math.max(8, window.innerWidth - rect.right) });
    }
    setOpen(true);
    loadItems();
  };

  const markRead = async (notification: NotificationItem) => {
    if (notification.isRead) {
      return;
    }
    setItems((prev) => prev?.map((n) => (n.id === notification.id ? { ...n, isRead: true } : n)) ?? prev);
    setUnreadCount((count) => Math.max(0, count - 1));
    try {
      await notificationsService.markRead(notification.id);
    } catch {
      refreshCount();
    }
  };

  const markAllRead = async () => {
    const previous = items;
    setItems((prev) => prev?.map((n) => ({ ...n, isRead: true })) ?? prev);
    setUnreadCount(0);
    try {
      await notificationsService.markAllRead();
    } catch {
      setItems(previous);
      refreshCount();
    }
  };

  const handleItemClick = (notification: NotificationItem) => {
    markRead(notification);
    if (notification.actionUrl) {
      setOpen(false);
    }
  };

  const label = unreadCount > 0 ? `Notifications (${unreadCount} unread)` : "Notifications";

  return (
    <>
      <button
        ref={anchorRef}
        type="button"
        aria-label={label}
        aria-expanded={open}
        aria-controls={panelId}
        onClick={toggle}
        className="relative flex h-7 w-7 items-center justify-center rounded-md text-white/70 transition-colors hover:bg-white/10 hover:text-white"
      >
        <Bell className="h-4 w-4" strokeWidth={2} />
        {unreadCount > 0 && (
          <span
            aria-hidden="true"
            className="absolute -right-0.5 -top-0.5 flex h-4 min-w-4 items-center justify-center rounded-full bg-red-500 px-1 text-[10px] font-semibold leading-none text-white"
          >
            {badgeLabel(unreadCount)}
          </span>
        )}
      </button>

      {open &&
        createPortal(
          <div
            ref={panelRef}
            id={panelId}
            role="dialog"
            aria-label="Notifications"
            style={{ top: position.top, right: position.right }}
            className="fixed z-[60] w-80 max-w-[calc(100vw-1rem)] rounded-lg glass-bg border-white/20 text-white"
          >
            <div className="flex items-center justify-between border-b border-white/10 px-3 py-2">
              <h2 className="text-sm font-semibold text-white">Notifications</h2>
              <button
                type="button"
                onClick={markAllRead}
                disabled={unreadCount === 0}
                className="flex items-center gap-1 rounded px-1.5 py-0.5 text-xs text-white/70 hover:bg-white/10 hover:text-white disabled:opacity-40"
              >
                <CheckCheck className="h-3.5 w-3.5" />
                Mark all read
              </button>
            </div>

            {loadError ? (
              <div role="alert" className="space-y-2 p-3 text-sm text-red-300">
                <p>Notifications could not be loaded.</p>
                <button type="button" onClick={loadItems} className="underline hover:text-red-200">
                  Retry
                </button>
              </div>
            ) : items === null ? (
              <ListRowSkeleton count={3} showIcon={false} className="py-1" />
            ) : items.length === 0 ? (
              <p className="p-3 text-sm text-white/60">You&apos;re all caught up.</p>
            ) : (
              <ul className="max-h-96 divide-y divide-white/5 overflow-y-auto">
                {items.map((notification) => {
                  const body = (
                    <>
                      <span className="flex items-start justify-between gap-2">
                        <span className={`text-sm ${notification.isRead ? "text-white/70" : "font-medium text-white"}`}>
                          {notification.title}
                        </span>
                        {!notification.isRead && (
                          <span className="mt-1.5 h-2 w-2 shrink-0 rounded-full bg-sky-400">
                            <span className="sr-only">Unread</span>
                          </span>
                        )}
                      </span>
                      <span className="mt-0.5 line-clamp-2 block text-xs text-white/60">{notification.message}</span>
                      <span className="mt-1 block text-[11px] text-white/40">
                        {formatNotificationTime(notification.createdAt)}
                      </span>
                    </>
                  );
                  const className = "block w-full px-3 py-2 text-left hover:bg-white/10";
                  return (
                    <li key={notification.id}>
                      {notification.actionUrl ? (
                        <Link href={notification.actionUrl} className={className} onClick={() => handleItemClick(notification)}>
                          {body}
                        </Link>
                      ) : (
                        <button type="button" className={className} onClick={() => handleItemClick(notification)}>
                          {body}
                        </button>
                      )}
                    </li>
                  );
                })}
              </ul>
            )}

            <div className="border-t border-white/10 px-3 py-2 text-right">
              <Link
                href="/settings/notifications"
                onClick={() => setOpen(false)}
                className="text-xs text-white/60 hover:text-white"
              >
                Notification settings
              </Link>
            </div>
          </div>,
          document.body
        )}
    </>
  );
}
