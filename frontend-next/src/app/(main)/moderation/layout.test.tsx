import { describe, it, expect, vi, beforeEach } from "vitest";
import { render, screen, waitFor } from "@/test/test-utils";
import ModerationLayout from "./layout";

const mockReplace = vi.fn();

vi.mock("next/navigation", () => ({
  useRouter: () => ({ replace: mockReplace }),
  usePathname: () => "/moderation/reports",
}));

vi.mock("@/lib/plugins", () => ({
  useModerationNavItems: () => [
    {
      id: "reports",
      label: "Reports",
      group: "Content",
      icon: () => null,
      path: "/moderation/reports",
    },
  ],
}));

describe("ModerationLayout", () => {
  beforeEach(() => {
    mockReplace.mockClear();
  });

  it("renders loading skeleton when isLoading", () => {
    render(
      <ModerationLayout>
        <div>Child</div>
      </ModerationLayout>,
      { mockAuth: { user: null, isLoading: true } }
    );

    const skeletons = document.querySelectorAll('[class*="animate"]');
    expect(skeletons.length).toBeGreaterThan(0);
  });

  it("redirects unauthenticated user to login", async () => {
    render(
      <ModerationLayout>
        <div>Child</div>
      </ModerationLayout>,
      { mockAuth: { user: null, isLoading: false } }
    );

    await waitFor(() => {
      expect(mockReplace).toHaveBeenCalledWith("/login?redirect=/moderation");
    });
  });

  it("redirects non-moderator user to dashboard", async () => {
    const userWithUserRole = {
      id: "u1",
      email: "u@ex.com",
      username: "user",
      accessToken: "t",
      roles: ["User"],
    };

    render(
      <ModerationLayout>
        <div>Child</div>
      </ModerationLayout>,
      { mockAuth: { user: userWithUserRole, isLoading: false } }
    );

    await waitFor(() => {
      expect(mockReplace).toHaveBeenCalledWith("/dashboard");
    });
  });

  it("renders PanelLayout for admin user", async () => {
    const adminUser = {
      id: "a1",
      email: "admin@ex.com",
      username: "admin",
      accessToken: "t",
      roles: ["Admin"],
    };

    render(
      <ModerationLayout>
        <div>Child content</div>
      </ModerationLayout>,
      { mockAuth: { user: adminUser, isLoading: false } }
    );

    await waitFor(() => {
      expect(screen.getByRole("heading", { name: "Moderation Control Panel" })).toBeInTheDocument();
    });
    expect(screen.getByText("Child content")).toBeInTheDocument();
  });

  it("renders PanelLayout for moderator user", async () => {
    const moderatorUser = {
      id: "m1",
      email: "mod@ex.com",
      username: "moderator",
      accessToken: "t",
      roles: ["Moderator"],
    };

    render(
      <ModerationLayout>
        <div>Child content</div>
      </ModerationLayout>,
      { mockAuth: { user: moderatorUser, isLoading: false } }
    );

    await waitFor(() => {
      expect(screen.getByRole("heading", { name: "Moderation Control Panel" })).toBeInTheDocument();
    });
    expect(screen.getByText("Child content")).toBeInTheDocument();
  });
});
