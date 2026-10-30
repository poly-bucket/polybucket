import { describe, it, expect, vi, beforeEach } from "vitest";
import { render, screen, waitFor } from "@/test/test-utils";
import userEvent from "@testing-library/user-event";
import { ProfileHeader } from "./profile-header";
import type { UserProfileData } from "@/lib/services/userProfileService";

const mockFollowUser = vi.fn();
const mockUnfollowUser = vi.fn();
const mockPush = vi.fn();

vi.mock("@/lib/services/userFollowService", () => ({
  followUser: (...args: unknown[]) => mockFollowUser(...args),
  unfollowUser: (...args: unknown[]) => mockUnfollowUser(...args),
}));

vi.mock("next/navigation", () => ({
  useRouter: () => ({ push: mockPush }),
}));

vi.mock("@/contexts/AuthContext", async (importOriginal) => {
  const actual = await importOriginal<typeof import("@/contexts/AuthContext")>();
  return {
    ...actual,
    useAuth: () => ({ isAuthenticated: true, user: { id: "viewer-1" } }),
  };
});

function baseProfile(overrides: Partial<UserProfileData> = {}): UserProfileData {
  return {
    id: "target-1",
    username: "maker",
    isProfilePublic: true,
    isFollowedByCurrentUser: false,
    websiteUrl: "",
    twitterUrl: "",
    instagramUrl: "",
    youtubeUrl: "",
    ...overrides,
  } as UserProfileData;
}

describe("ProfileHeader", () => {
  beforeEach(() => {
    mockFollowUser.mockReset();
    mockUnfollowUser.mockReset();
    mockPush.mockReset();
    mockFollowUser.mockResolvedValue(undefined);
    mockUnfollowUser.mockResolvedValue(undefined);
  });

  it("renders safe external website link", () => {
    render(
      <ProfileHeader
        profile={baseProfile({ websiteUrl: "https://example.com" })}
        isOwnProfile={false}
      />
    );
    const link = screen.getByRole("link", { name: "Website" });
    expect(link.getAttribute("href")).toMatch(/^https:\/\/example\.com\/?$/);
  });

  it("does not render javascript links", () => {
    render(
      <ProfileHeader
        profile={baseProfile({ websiteUrl: "javascript:alert(1)" })}
        isOwnProfile={false}
      />
    );
    expect(screen.queryByRole("link", { name: "Website" })).not.toBeInTheDocument();
  });

  it("shows Follow and switches to Following after a successful follow", async () => {
    const user = userEvent.setup();
    const onFollowChange = vi.fn();
    render(
      <ProfileHeader
        profile={baseProfile()}
        isOwnProfile={false}
        onFollowChange={onFollowChange}
      />
    );

    await user.click(screen.getByRole("button", { name: "Follow" }));

    await waitFor(() => expect(mockFollowUser).toHaveBeenCalledWith("target-1"));
    expect(await screen.findByRole("button", { name: "Following" })).toBeInTheDocument();
    expect(onFollowChange).toHaveBeenCalled();
  });

  it("does not render the followers chart component", () => {
    render(<ProfileHeader profile={baseProfile()} isOwnProfile={false} />);
    expect(screen.queryByText("Followers over time")).not.toBeInTheDocument();
  });
});
