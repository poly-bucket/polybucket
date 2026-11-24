import { describe, it, expect, vi, beforeEach } from "vitest";
import { render, screen } from "@/test/test-utils";
import { ProfilePageContent } from "./profile-page-content";

vi.mock("next/navigation", () => ({
  useParams: () => ({ idOrUsername: "maker" }),
  useSearchParams: () => new URLSearchParams("tab=models"),
  useRouter: () => ({ replace: vi.fn() }),
  notFound: () => {
    throw new Error("notFound");
  },
}));

const mockUseAuth = vi.fn(() => ({ user: { id: "other" }, isAdmin: false }));

vi.mock("@/contexts/AuthContext", async (importOriginal) => {
  const actual = await importOriginal<typeof import("@/contexts/AuthContext")>();
  return {
    ...actual,
    useAuth: () => mockUseAuth(),
  };
});

const mockFetchUserProfile = vi.fn();

vi.mock("@/lib/services/userProfileService", () => ({
  fetchUserProfile: (...args: unknown[]) => mockFetchUserProfile(...args),
  enrichOwnProfileFromMe: vi.fn((p: unknown) => Promise.resolve(p)),
}));

vi.mock("@/lib/plugins", () => ({
  useProfileTabs: () => [{ id: "models", label: "Models", icon: () => null, component: () => null }],
  PluginBoundary: ({ children }: { children: React.ReactNode }) => children,
}));

vi.mock("./profile-header", () => ({
  ProfileHeader: () => <div data-testid="profile-header" />,
}));

describe("ProfilePageContent", () => {
  beforeEach(() => {
    vi.clearAllMocks();
    mockUseAuth.mockReturnValue({ user: { id: "other" }, isAdmin: false });
    mockFetchUserProfile.mockResolvedValue({
      id: "maker-id",
      username: "maker",
      isProfilePublic: true,
      showStatistics: true,
      createdAt: "2026-01-01T00:00:00Z",
    });
  });

  it("renders profile header when profile loads", async () => {
    render(<ProfilePageContent />);
    await screen.findByTestId("profile-header");
  });

  it("shows private wall for another user's private profile stub", async () => {
    mockFetchUserProfile.mockResolvedValue({
      id: "private-id",
      username: "private",
      isProfilePublic: false,
      message: "This profile is private",
    });
    render(<ProfilePageContent />);
    await screen.findByText("Profile is Private");
    expect(screen.queryByTestId("profile-header")).not.toBeInTheDocument();
  });

  it("renders profile header when owner views their private profile", async () => {
    mockUseAuth.mockReturnValue({ user: { id: "maker-id" }, isAdmin: false });
    mockFetchUserProfile.mockResolvedValue({
      id: "maker-id",
      username: "maker",
      isProfilePublic: false,
      createdAt: "2026-01-01T00:00:00Z",
    });
    render(<ProfilePageContent />);
    await screen.findByTestId("profile-header");
    await screen.findByText(/Your profile is private/);
  });
});
