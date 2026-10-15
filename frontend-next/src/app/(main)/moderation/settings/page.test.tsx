import { describe, it, expect, vi } from "vitest";
import { render, screen } from "@testing-library/react";
import ModerationSettingsPage from "./page";

vi.mock("@/components/moderation/moderation-settings-section", () => ({
  ModerationSettingsSection: () => (
    <div data-testid="moderation-settings-section">Moderation settings</div>
  ),
}));

describe("ModerationSettingsPage", () => {
  it("renders moderation settings section", () => {
    render(<ModerationSettingsPage />);
    expect(screen.getByTestId("moderation-settings-section")).toBeInTheDocument();
    expect(screen.getByRole("heading", { name: /moderation settings/i })).toBeInTheDocument();
  });
});
