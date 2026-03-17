import { describe, it, expect } from "vitest";
import { render, screen } from "@/test/test-utils";
import { PanelLayout } from "./panel-layout";
import { LayoutDashboard, Users } from "lucide-react";

const mockNavItems = [
  { id: "dashboard", label: "Dashboard", group: "Overview", icon: LayoutDashboard, path: "/admin/dashboard" },
  { id: "users", label: "Users", group: "Management", icon: Users, path: "/admin/users" },
];

describe("PanelLayout", () => {
  it("renders title and groups nav items", () => {
    render(
      <PanelLayout title="Admin Panel" navItems={mockNavItems}>
        <div>Main content</div>
      </PanelLayout>
    );

    expect(screen.getByRole("heading", { name: "Admin Panel" })).toBeInTheDocument();
    expect(screen.getByText("Dashboard")).toBeInTheDocument();
    expect(screen.getByText("Users")).toBeInTheDocument();
    expect(screen.getByText("Main content")).toBeInTheDocument();
  });

  it("renders grouped section labels", () => {
    render(
      <PanelLayout title="Admin Panel" navItems={mockNavItems}>
        <div>Content</div>
      </PanelLayout>
    );

    expect(screen.getByText("Overview")).toBeInTheDocument();
    expect(screen.getByText("Management")).toBeInTheDocument();
  });

  it("renders description when provided", () => {
    render(
      <PanelLayout
        title="Admin Panel"
        description="Manage the system"
        navItems={mockNavItems}
      >
        <div>Content</div>
      </PanelLayout>
    );

    expect(screen.getByText("Manage the system")).toBeInTheDocument();
  });
});
