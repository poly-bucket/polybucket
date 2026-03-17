import { describe, it, expect, vi, beforeEach } from "vitest";
import { render, screen, waitFor } from "@/test/test-utils";
import ModerationAuditLogsPage from "./page";

const mockGetModerationAuditLogs = vi.fn();

vi.mock("@/lib/services/moderationService", () => ({
  getModerationAuditLogs: (...args: unknown[]) =>
    mockGetModerationAuditLogs(...args),
}));

describe("ModerationAuditLogsPage", () => {
  beforeEach(() => {
    mockGetModerationAuditLogs.mockReset();
    mockGetModerationAuditLogs.mockResolvedValue({
      logs: [],
      totalCount: 0,
      totalPages: 1,
    });
  });

  it("renders Audit Logs heading", async () => {
    render(<ModerationAuditLogsPage />);

    await waitFor(() => {
      expect(screen.getByText("Moderation Audit Logs")).toBeInTheDocument();
    });
  });

  it("calls getModerationAuditLogs on mount", async () => {
    render(<ModerationAuditLogsPage />);

    await waitFor(() => {
      expect(mockGetModerationAuditLogs).toHaveBeenCalled();
    });
  });
});
