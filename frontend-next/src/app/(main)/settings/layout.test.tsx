import { describe, it, expect, vi } from "vitest";
import { render, screen } from "@/test/test-utils";
import SettingsLayout from "./layout";

vi.mock("next/navigation", () => ({
  usePathname: () => "/settings/profile",
}));

vi.mock("@/lib/plugins", () => ({
  useSettingsNavItems: () => [
    {
      id: "profile",
      label: "Profile",
      group: "Account",
      icon: () => null,
      path: "/settings/profile",
    },
    {
      id: "security",
      label: "Password & 2FA",
      group: "Security",
      icon: () => null,
      path: "/settings/security",
    },
  ],
}));

describe("SettingsLayout", () => {
  it("renders PanelLayout with title, groups, nav links, and children", () => {
    render(
      <SettingsLayout>
        <div>Child content</div>
      </SettingsLayout>
    );

    expect(screen.getByRole("heading", { name: "Settings" })).toBeInTheDocument();
    expect(
      screen.getByText("Manage your account, security, and preferences")
    ).toBeInTheDocument();
    expect(screen.getByText("Account")).toBeInTheDocument();
    expect(screen.getByText("Security")).toBeInTheDocument();
    expect(screen.getByText("Profile")).toBeInTheDocument();
    expect(screen.getByText("Password & 2FA")).toBeInTheDocument();
    expect(screen.getByText("Child content")).toBeInTheDocument();
  });

  it("renders sidebar toggle control", () => {
    render(
      <SettingsLayout>
        <div>Child</div>
      </SettingsLayout>
    );

    const toggles = screen.getAllByRole("button", { name: "Toggle Sidebar" });
    expect(toggles.length).toBeGreaterThanOrEqual(1);
  });
});
