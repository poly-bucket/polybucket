import { describe, it, expect, vi, beforeEach } from "vitest";
import { render, screen, waitFor } from "@/test/test-utils";
import { ProfileFollowListTab } from "./profile-follow-list-tab";

const mockFetchUserFollowers = vi.fn();

vi.mock("@/lib/services/userFollowService", () => ({
  fetchUserFollowers: (...args: unknown[]) => mockFetchUserFollowers(...args),
  fetchUserFollowing: vi.fn(),
}));

describe("ProfileFollowListTab", () => {
  beforeEach(() => {
    mockFetchUserFollowers.mockReset();
    mockFetchUserFollowers.mockResolvedValue({
      users: [{ id: "u2", username: "fan42", avatar: undefined, followedAt: "2026-01-01" }],
      totalCount: 1,
      page: 1,
      pageSize: 20,
      totalPages: 1,
    });
  });

  it("renders a profile link for each follower", async () => {
    render(
      <ProfileFollowListTab
        username="maker"
        mode="followers"
        page={1}
        onPageChange={() => {}}
      />
    );

    await waitFor(() => {
      const link = screen.getByRole("link", { name: /@fan42/ });
      expect(link.getAttribute("href")).toBe("/profile/fan42");
    });
  });
});
