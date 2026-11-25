import { describe, it, expect, vi } from "vitest";
import { render, screen } from "@testing-library/react";
import PreferencesPage from "./page";

const mockUpdateSettings = vi.fn().mockResolvedValue(true);

vi.mock("@/contexts/UserSettingsContext", () => ({
  useUserSettings: () => ({
    settings: {
      language: "en",
      theme: "dark",
      timeZone: "UTC",
      measurementSystem: "metric",
      autoRotateModels: true,
    },
    updateSettings: mockUpdateSettings,
  }),
}));

vi.mock("@/lib/api/clientFactory", () => ({
  ApiClientFactory: {
    getApiClient: () => ({
      getPrinters_GetPrinters: vi.fn().mockResolvedValue({ printers: [] }),
    }),
  },
}));

describe("PreferencesPage", () => {
  it("renders preference sections", () => {
    render(<PreferencesPage />);
    expect(screen.getByText("Regional")).toBeInTheDocument();
    expect(screen.getByText("Auto-rotate models")).toBeInTheDocument();
  });
});