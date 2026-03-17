import { describe, it, expect, vi, beforeEach } from "vitest";
import { render, screen, waitFor } from "@/test/test-utils";
import AdminLayout from "./layout";

const mockReplace = vi.fn();

vi.mock("next/navigation", () => ({
  useRouter: () => ({ replace: mockReplace }),
  usePathname: () => "/admin/dashboard",
}));

vi.mock("@/lib/plugins", () => ({
  useAdminNavItems: () => [
    { id: "dashboard", label: "Dashboard", group: "Overview", icon: () => null, path: "/admin/dashboard" },
  ],
}));

describe("AdminLayout", () => {
  beforeEach(() => {
    mockReplace.mockClear();
  });

  it("renders loading skeleton when isLoading", () => {
    render(
      <AdminLayout>
        <div>Child</div>
      </AdminLayout>,
      { mockAuth: { user: null, isLoading: true } }
    );

    const skeletons = document.querySelectorAll('[class*="animate"]');
    expect(skeletons.length).toBeGreaterThan(0);
  });

  it("redirects unauthenticated user to login", async () => {
    render(
      <AdminLayout>
        <div>Child</div>
      </AdminLayout>,
      { mockAuth: { user: null, isLoading: false } }
    );

    await waitFor(() => {
      expect(mockReplace).toHaveBeenCalledWith("/login?redirect=/admin");
    });
  });

  it("redirects non-admin user to dashboard", async () => {
    const userWithUserRole = {
      id: "u1",
      email: "u@ex.com",
      username: "user",
      accessToken: "t",
      roles: ["User"],
    };

    render(
      <AdminLayout>
        <div>Child</div>
      </AdminLayout>,
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
      <AdminLayout>
        <div>Child content</div>
      </AdminLayout>,
      { mockAuth: { user: adminUser, isLoading: false } }
    );

    await waitFor(() => {
      expect(screen.getByRole("heading", { name: "Admin Control Panel" })).toBeInTheDocument();
    });
    expect(screen.getByText("Child content")).toBeInTheDocument();
  });
});
