import { describe, it, expect, vi, beforeEach, afterEach } from "vitest";
import userEvent from "@testing-library/user-event";
import { act, render, screen, waitFor, within } from "@/test/test-utils";
import type { NotificationItem } from "@/lib/services/notificationsService";
import { NotificationBell, NOTIFICATION_POLL_INTERVAL_MS } from "./notification-bell";

const mockGetUnreadCount = vi.fn();
const mockGetNotifications = vi.fn();
const mockMarkRead = vi.fn();
const mockMarkAllRead = vi.fn();

vi.mock("@/lib/services/notificationsService", async (importOriginal) => {
  const actual = await importOriginal<typeof import("@/lib/services/notificationsService")>();
  return {
    ...actual,
    notificationsService: {
      getUnreadCount: (...args: unknown[]) => mockGetUnreadCount(...args),
      getNotifications: (...args: unknown[]) => mockGetNotifications(...args),
      markRead: (...args: unknown[]) => mockMarkRead(...args),
      markAllRead: (...args: unknown[]) => mockMarkAllRead(...args),
    },
  };
});

const signedInUser = { id: "user-1", email: "maker@example.com", username: "maker", accessToken: "token" };

function notification(id: string, title: string, overrides: Partial<NotificationItem> = {}): NotificationItem {
  return {
    id,
    type: "ModelApproved",
    priority: "Normal",
    title,
    message: `${title} message`,
    actionUrl: `/models/${id}`,
    isRead: false,
    createdAt: new Date().toISOString(),
    ...overrides,
  };
}

function page(items: NotificationItem[]) {
  return {
    items,
    totalCount: items.length,
    unreadCount: items.filter((n) => !n.isRead).length,
    page: 1,
    pageSize: 10,
    totalPages: 1,
  };
}

describe("NotificationBell", () => {
  beforeEach(() => {
    mockGetUnreadCount.mockReset().mockResolvedValue(2);
    mockGetNotifications.mockReset().mockResolvedValue(
      page([notification("n1", "Your model was approved"), notification("n2", "alice liked your model")])
    );
    mockMarkRead.mockReset().mockResolvedValue(undefined);
    mockMarkAllRead.mockReset().mockResolvedValue(2);
  });

  afterEach(() => {
    vi.useRealTimers();
  });

  it("shows the unread count from the server on the bell", async () => {
    // Arrange
    render(<NotificationBell />, { mockAuth: { user: signedInUser } });

    // Act
    const bell = await screen.findByRole("button", { name: "Notifications (2 unread)" });

    // Assert
    expect(bell).toHaveTextContent("2");
    expect(mockGetNotifications).not.toHaveBeenCalled();
  });

  it("caps the badge at 9+", async () => {
    // Arrange
    mockGetUnreadCount.mockResolvedValue(42);
    render(<NotificationBell />, { mockAuth: { user: signedInUser } });

    // Act
    const bell = await screen.findByRole("button", { name: "Notifications (42 unread)" });

    // Assert
    expect(bell).toHaveTextContent("9+");
  });

  it("polls the unread count periodically", async () => {
    // Arrange
    vi.useFakeTimers({ shouldAdvanceTime: true });
    render(<NotificationBell />, { mockAuth: { user: signedInUser } });
    await waitFor(() => expect(mockGetUnreadCount).toHaveBeenCalledTimes(1));
    mockGetUnreadCount.mockResolvedValue(5);

    // Act
    await act(async () => {
      await vi.advanceTimersByTimeAsync(NOTIFICATION_POLL_INTERVAL_MS);
    });

    // Assert
    expect(mockGetUnreadCount).toHaveBeenCalledTimes(2);
    expect(await screen.findByRole("button", { name: "Notifications (5 unread)" })).toBeInTheDocument();
  });

  it("opens a list of recent notifications linking to their targets", async () => {
    // Arrange
    const user = userEvent.setup();
    render(<NotificationBell />, { mockAuth: { user: signedInUser } });

    // Act
    await user.click(await screen.findByRole("button", { name: /notifications/i }));

    // Assert
    const panel = await screen.findByRole("dialog", { name: "Notifications" });
    const link = await within(panel).findByRole("link", { name: /your model was approved/i });
    expect(link).toHaveAttribute("href", "/models/n1");
    expect(mockGetNotifications).toHaveBeenCalledWith(1, 10);
  });

  it("marks a notification read when it is opened", async () => {
    // Arrange
    const user = userEvent.setup();
    render(<NotificationBell />, { mockAuth: { user: signedInUser } });
    await user.click(await screen.findByRole("button", { name: /notifications/i }));
    const panel = await screen.findByRole("dialog", { name: "Notifications" });

    // Act
    await user.click(await within(panel).findByRole("link", { name: /your model was approved/i }));

    // Assert
    expect(mockMarkRead).toHaveBeenCalledWith("n1");
    expect(await screen.findByRole("button", { name: "Notifications (1 unread)" })).toBeInTheDocument();
  });

  it("marks everything read", async () => {
    // Arrange
    const user = userEvent.setup();
    render(<NotificationBell />, { mockAuth: { user: signedInUser } });
    await user.click(await screen.findByRole("button", { name: /notifications/i }));
    const panel = await screen.findByRole("dialog", { name: "Notifications" });
    await within(panel).findByRole("link", { name: /your model was approved/i });

    // Act
    await user.click(within(panel).getByRole("button", { name: /mark all read/i }));

    // Assert
    expect(mockMarkAllRead).toHaveBeenCalledTimes(1);
    expect(await screen.findByRole("button", { name: "Notifications" })).toBeInTheDocument();
    expect(within(panel).queryByText("Unread")).not.toBeInTheDocument();
  });

  it("restores the unread state when marking all read fails", async () => {
    // Arrange
    mockMarkAllRead.mockRejectedValue(new Error("network"));
    const user = userEvent.setup();
    render(<NotificationBell />, { mockAuth: { user: signedInUser } });
    await user.click(await screen.findByRole("button", { name: /notifications/i }));
    const panel = await screen.findByRole("dialog", { name: "Notifications" });
    await within(panel).findByRole("link", { name: /your model was approved/i });

    // Act
    await user.click(within(panel).getByRole("button", { name: /mark all read/i }));

    // Assert
    await waitFor(() => expect(within(panel).getAllByText("Unread")).toHaveLength(2));
    expect(await screen.findByRole("button", { name: "Notifications (2 unread)" })).toBeInTheDocument();
  });

  it("shows an empty state when there are no notifications", async () => {
    // Arrange
    mockGetUnreadCount.mockResolvedValue(0);
    mockGetNotifications.mockResolvedValue(page([]));
    const user = userEvent.setup();
    render(<NotificationBell />, { mockAuth: { user: signedInUser } });

    // Act
    await user.click(screen.getByRole("button", { name: "Notifications" }));

    // Assert
    expect(await screen.findByText(/all caught up/i)).toBeInTheDocument();
    expect(screen.getByRole("button", { name: /mark all read/i })).toBeDisabled();
  });

  it("closes when Escape is pressed", async () => {
    // Arrange
    const user = userEvent.setup();
    render(<NotificationBell />, { mockAuth: { user: signedInUser } });
    await user.click(await screen.findByRole("button", { name: /notifications/i }));
    await screen.findByRole("dialog", { name: "Notifications" });

    // Act
    await user.keyboard("{Escape}");

    // Assert
    expect(screen.queryByRole("dialog", { name: "Notifications" })).not.toBeInTheDocument();
  });
});
