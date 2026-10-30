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

vi.mock("@/contexts/AuthContext", async (importOriginal) => {
  const actual = await importOriginal<typeof import("@/contexts/AuthContext")>();
  return {
    ...actual,
    useAuth: () => ({ user: { id: "other" } }),
  };
});

vi.mock("@/lib/services/userProfileService", () => ({
  fetchUserProfile: vi.fn().mockResolvedValue({
    id: "maker-id",
    username: "maker",
    isProfilePublic: true,
    showStatistics: true,
  }),
}));

vi.mock("@/lib/plugins", () => ({
  useProfileTabs: () => [{ id: "models", label: "Models", icon: () => null, component: () => null }],
  PluginBoundary: ({ children }: { children: React.ReactNode }) => children,
}));

vi.mock("./profile-header", () => ({
  ProfileHeader: () => <div data-testid="profile-header" />,
}));

vi.mock("./follower-history-chart", () => ({
  FollowerHistoryChart: () => <div data-testid="follower-chart" />,
}));

describe("ProfilePageContent", () => {
  beforeEach(() => {
    vi.clearAllMocks();
  });

  it("does not show the follower chart when viewing another profile", async () => {
    render(<ProfilePageContent />);
    await screen.findByTestId("profile-header");
    expect(screen.queryByTestId("follower-chart")).not.toBeInTheDocument();
  });
});
