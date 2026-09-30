import { describe, it, expect, vi, beforeEach } from "vitest";
import { render, screen, waitFor, fireEvent } from "@/test/test-utils";
import { EmailOutboxSection } from "./email-outbox-section";

const mockGetEmailOutbox = vi.fn();
const mockRetryEmailMessage = vi.fn();

vi.mock("@/lib/services/emailService", () => ({
  getEmailOutbox: (...args: unknown[]) => mockGetEmailOutbox(...args),
  retryEmailMessage: (...args: unknown[]) => mockRetryEmailMessage(...args),
  getEmailApiErrorMessage: (_: unknown, fallback: string) => fallback,
}));

vi.mock("sonner", () => ({
  toast: { success: vi.fn(), error: vi.fn() },
}));

const now = new Date().toISOString();

function outboxPage() {
  return {
    items: [
      { id: "sent-1", template: "Welcome", recipient: "a@example.com", status: "Sent", attempts: 1, nextAttemptAt: now, createdAt: now, sentAt: now },
      { id: "dead-1", template: "PasswordReset", recipient: "b@example.com", status: "DeadLetter", attempts: 8, nextAttemptAt: now, createdAt: now, lastError: "550 mailbox unavailable" },
    ],
    totalCount: 2,
    page: 1,
    pageSize: 25,
    totalPages: 1,
    statusCounts: { Sent: 1, DeadLetter: 1 },
  };
}

describe("EmailOutboxSection", () => {
  beforeEach(() => {
    mockGetEmailOutbox.mockReset();
    mockRetryEmailMessage.mockReset();
    mockGetEmailOutbox.mockResolvedValue(outboxPage());
    mockRetryEmailMessage.mockResolvedValue(undefined);
  });

  it("lists messages and offers retry only for undeliverable ones", async () => {
    // Arrange
    render(<EmailOutboxSection />);

    // Act
    await waitFor(() => expect(screen.getByText("b@example.com")).toBeInTheDocument());

    // Assert
    expect(screen.getByText("a@example.com")).toBeInTheDocument();
    expect(screen.getByText("550 mailbox unavailable")).toBeInTheDocument();
    expect(screen.getAllByRole("button", { name: /retry now/i })).toHaveLength(1);
  });

  it("retries a dead-lettered message and reloads the list", async () => {
    // Arrange
    render(<EmailOutboxSection />);
    await waitFor(() => expect(screen.getByText("b@example.com")).toBeInTheDocument());

    // Act
    fireEvent.click(screen.getByRole("button", { name: /retry now/i }));

    // Assert
    await waitFor(() => expect(mockRetryEmailMessage).toHaveBeenCalledWith("dead-1"));
    await waitFor(() => expect(mockGetEmailOutbox).toHaveBeenCalledTimes(2));
  });

  it("requests the selected status when a filter is clicked", async () => {
    // Arrange
    render(<EmailOutboxSection />);
    await waitFor(() => expect(mockGetEmailOutbox).toHaveBeenCalledWith(undefined, 1, 25));

    // Act
    fireEvent.click(screen.getByRole("button", { name: /dead-lettered/i }));

    // Assert
    await waitFor(() => expect(mockGetEmailOutbox).toHaveBeenCalledWith("DeadLetter", 1, 25));
  });

  it("shows an empty state when no messages match", async () => {
    // Arrange
    mockGetEmailOutbox.mockResolvedValue({ ...outboxPage(), items: [], totalCount: 0, statusCounts: {} });

    // Act
    render(<EmailOutboxSection />);

    // Assert
    await waitFor(() => expect(screen.getByText("No emails match this filter.")).toBeInTheDocument());
  });
});
