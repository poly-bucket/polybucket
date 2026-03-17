import { describe, it, expect, vi, beforeEach } from "vitest";
import { render, screen, waitFor } from "@/test/test-utils";
import AdminSystemPage from "./page";

const mockGetEmailSettings = vi.fn();
const mockGetTokenSettings = vi.fn();
const mockGetSetupStatus = vi.fn();

vi.mock("@/lib/services/adminService", () => ({
  getEmailSettings: () => mockGetEmailSettings(),
  updateEmailSettings: vi.fn(),
  testEmailConfiguration: vi.fn(),
  getTokenSettings: () => mockGetTokenSettings(),
  updateTokenSettings: vi.fn(),
  getSetupStatus: () => mockGetSetupStatus(),
}));

describe("AdminSystemPage", () => {
  beforeEach(() => {
    mockGetEmailSettings.mockReset();
    mockGetTokenSettings.mockReset();
    mockGetSetupStatus.mockReset();
    mockGetEmailSettings.mockResolvedValue({
      enabled: false,
      smtpServer: "",
      smtpPort: 587,
      fromAddress: "",
      fromName: "",
    });
    mockGetTokenSettings.mockResolvedValue({
      accessTokenExpiryHours: 24,
      refreshTokenExpiryDays: 7,
      enableRefreshTokens: true,
    });
    mockGetSetupStatus.mockResolvedValue({
      isFirstTimeSetup: false,
      completedSteps: 5,
      totalSteps: 5,
    });
  });

  it("renders System Settings heading", async () => {
    render(<AdminSystemPage />);

    await waitFor(() => {
      expect(screen.getByText("System Settings")).toBeInTheDocument();
    });
  });

  it("renders Setup Status section", async () => {
    render(<AdminSystemPage />);

    await waitFor(() => {
      expect(screen.getByText("Setup Status")).toBeInTheDocument();
    });
  });

  it("renders Email Configuration section", async () => {
    render(<AdminSystemPage />);

    await waitFor(() => {
      expect(screen.getByText("Email Configuration")).toBeInTheDocument();
    });
  });

  it("fetches data on mount", async () => {
    render(<AdminSystemPage />);

    await waitFor(() => {
      expect(mockGetEmailSettings).toHaveBeenCalled();
      expect(mockGetTokenSettings).toHaveBeenCalled();
      expect(mockGetSetupStatus).toHaveBeenCalled();
    });
  });
});
