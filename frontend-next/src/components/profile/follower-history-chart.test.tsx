import { describe, it, expect, vi, beforeEach } from "vitest";
import { render, screen, waitFor } from "@/test/test-utils";
import { FollowerHistoryChart } from "./follower-history-chart";

const mockFetchFollowerHistory = vi.fn();

vi.mock("@/lib/services/userFollowService", () => ({
  fetchFollowerHistory: (...args: unknown[]) => mockFetchFollowerHistory(...args),
}));

describe("FollowerHistoryChart", () => {
  beforeEach(() => {
    mockFetchFollowerHistory.mockReset();
    mockFetchFollowerHistory.mockResolvedValue({
      days: 30,
      points: [{ date: "2026-01-01T00:00:00Z", followerCount: 2 }],
    });
  });

  it("loads follower history for the profile owner", async () => {
    render(<FollowerHistoryChart userId="owner-1" />);

    expect(screen.getByText("Followers over time")).toBeInTheDocument();
    await waitFor(() =>
      expect(mockFetchFollowerHistory).toHaveBeenCalledWith("owner-1", 30)
    );
  });
});
