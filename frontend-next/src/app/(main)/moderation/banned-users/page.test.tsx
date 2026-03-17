import { describe, it, expect, vi, beforeEach } from "vitest";
import { render, screen, waitFor } from "@/test/test-utils";
import ModerationBannedUsersPage from "./page";

const mockGetBannedUsers = vi.fn();

vi.mock("@/lib/services/moderationService", () => ({
  getBannedUsers: (...args: unknown[]) => mockGetBannedUsers(...args),
}));

describe("ModerationBannedUsersPage", () => {
  beforeEach(() => {
    mockGetBannedUsers.mockReset();
    mockGetBannedUsers.mockResolvedValue({
      users: [],
      totalCount: 0,
      totalPages: 1,
    });
  });

  it("renders Banned Users heading", async () => {
    render(<ModerationBannedUsersPage />);

    await waitFor(() => {
      expect(screen.getByText("Banned Users Management")).toBeInTheDocument();
    });
  });

  it("calls getBannedUsers on mount", async () => {
    render(<ModerationBannedUsersPage />);

    await waitFor(() => {
      expect(mockGetBannedUsers).toHaveBeenCalled();
    });
  });
});
