import "@/lib/plugins/config";
import { describe, it, expect } from "vitest";
import { render, screen } from "@/test/test-utils";
import { useAdminNavItems, useModerationNavItems, useUserMenuItems } from "./hooks";

function TestAdminNavItems() {
  const items = useAdminNavItems();
  return (
    <div>
      {items.map((i) => (
        <span key={i.id} data-testid={`admin-nav-${i.id}`}>
          {i.label}
        </span>
      ))}
    </div>
  );
}

function TestModerationNavItems() {
  const items = useModerationNavItems();
  return (
    <div>
      {items.map((i) => (
        <span key={i.id} data-testid={`moderation-nav-${i.id}`}>
          {i.label}
        </span>
      ))}
    </div>
  );
}

function TestUserMenuItems() {
  const items = useUserMenuItems();
  return (
    <div>
      {items.map((i) => (
        <span key={i.id} data-testid={`menu-${i.id}`}>
          {i.label}
        </span>
      ))}
    </div>
  );
}

describe("useAdminNavItems", () => {
  it("returns admin nav items for Admin role", () => {
    const adminUser = {
      id: "a1",
      email: "admin@ex.com",
      username: "admin",
      accessToken: "t",
      roles: ["Admin"],
    };

    render(<TestAdminNavItems />, {
      mockAuth: { user: adminUser, isLoading: false },
    });

    expect(screen.getByTestId("admin-nav-dashboard")).toBeInTheDocument();
    expect(screen.getByTestId("admin-nav-users")).toBeInTheDocument();
  });

  it("filters out admin nav items for User role", () => {
    const regularUser = {
      id: "u1",
      email: "user@ex.com",
      username: "user",
      accessToken: "t",
      roles: ["User"],
    };

    render(<TestAdminNavItems />, {
      mockAuth: { user: regularUser, isLoading: false },
    });

    expect(screen.queryByTestId("admin-nav-dashboard")).not.toBeInTheDocument();
  });
});

describe("useModerationNavItems", () => {
  it("returns moderation nav items for Admin role", () => {
    const adminUser = {
      id: "a1",
      email: "admin@ex.com",
      username: "admin",
      accessToken: "t",
      roles: ["Admin"],
    };

    render(<TestModerationNavItems />, {
      mockAuth: { user: adminUser, isLoading: false },
    });

    expect(screen.getByTestId("moderation-nav-reports")).toBeInTheDocument();
    expect(screen.getByTestId("moderation-nav-banned-users")).toBeInTheDocument();
  });

  it("returns moderation nav items for Moderator role", () => {
    const moderatorUser = {
      id: "m1",
      email: "mod@ex.com",
      username: "moderator",
      accessToken: "t",
      roles: ["Moderator"],
    };

    render(<TestModerationNavItems />, {
      mockAuth: { user: moderatorUser, isLoading: false },
    });

    expect(screen.getByTestId("moderation-nav-reports")).toBeInTheDocument();
  });

  it("returns empty for User role", () => {
    const regularUser = {
      id: "u1",
      email: "user@ex.com",
      username: "user",
      accessToken: "t",
      roles: ["User"],
    };

    render(<TestModerationNavItems />, {
      mockAuth: { user: regularUser, isLoading: false },
    });

    expect(screen.queryByTestId("moderation-nav-reports")).not.toBeInTheDocument();
  });
});

describe("useUserMenuItems", () => {
  it("shows Admin Panel item for Admin role", () => {
    const adminUser = {
      id: "a1",
      email: "admin@ex.com",
      username: "admin",
      accessToken: "t",
      roles: ["Admin"],
    };

    render(<TestUserMenuItems />, {
      mockAuth: { user: adminUser, isLoading: false },
    });

    expect(screen.getByTestId("menu-admin")).toBeInTheDocument();
    expect(screen.getByText("Admin Panel")).toBeInTheDocument();
  });

  it("hides Admin Panel item for User role", () => {
    const regularUser = {
      id: "u1",
      email: "user@ex.com",
      username: "user",
      accessToken: "t",
      roles: ["User"],
    };

    render(<TestUserMenuItems />, {
      mockAuth: { user: regularUser, isLoading: false },
    });

    expect(screen.queryByTestId("menu-admin")).not.toBeInTheDocument();
  });

  it("shows Moderation item for Moderator role", () => {
    const moderatorUser = {
      id: "m1",
      email: "mod@ex.com",
      username: "moderator",
      accessToken: "t",
      roles: ["Moderator"],
    };

    render(<TestUserMenuItems />, {
      mockAuth: { user: moderatorUser, isLoading: false },
    });

    expect(screen.getByTestId("menu-moderation")).toBeInTheDocument();
    expect(screen.getByText("Moderation")).toBeInTheDocument();
  });
});
