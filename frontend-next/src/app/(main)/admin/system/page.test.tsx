import { describe, it, expect, vi, beforeEach } from "vitest";
import { render, screen, waitFor } from "@/test/test-utils";
import AdminSystemPage from "./page";

const mockGetEmailSettings = vi.fn();
const mockGetEmailOutbox = vi.fn();
const mockGetTokenSettings = vi.fn();
const mockGetSetupStatus = vi.fn();

vi.mock("@/lib/services/adminService", () => ({
  getTokenSettings: () => mockGetTokenSettings(),
  updateTokenSettings: vi.fn(),
  getSetupStatus: () => mockGetSetupStatus(),
}));

vi.mock("@/lib/services/emailService", () => ({
  getEmailSettings: () => mockGetEmailSettings(),
  updateEmailSettings: vi.fn(),
  testEmailConfiguration: vi.fn(),
  previewEmailTemplate: vi.fn(),
  getEmailOutbox: () => mockGetEmailOutbox(),
  retryEmailMessage: vi.fn(),
  getEmailApiErrorMessage: (_: unknown, fallback: string) => fallback,
}));

describe("AdminSystemPage", () => {
  beforeEach(() => {
    mockGetEmailSettings.mockReset();
    mockGetEmailOutbox.mockReset();
    mockGetTokenSettings.mockReset();
    mockGetSetupStatus.mockReset();
    mockGetEmailSettings.mockResolvedValue({
      enabled: false,
      transport: "Disabled",
      smtpHost: "",
      smtpPort: 587,
      smtpSecurity: "Auto",
      smtpUsername: "",
      hasPassword: false,
      allowInvalidCertificates: false,
      fromAddress: "",
      fromName: "PolyBucket",
      publicBaseUrl: "",
      requireEmailVerification: false,
      isConfigured: false,
      validationErrors: [],
      sources: {},
      managedByEnvironment: [],
    });
    mockGetEmailOutbox.mockResolvedValue({
      items: [],
      totalCount: 0,
      page: 1,
      pageSize: 25,
      totalPages: 0,
      statusCounts: {},
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
    // Arrange
    render(<AdminSystemPage />);

    // Act
    // Assert
    await waitFor(() => {
      expect(screen.getByText("System Settings")).toBeInTheDocument();
    });
  });

  it("renders Setup Status section", async () => {
    // Arrange
    render(<AdminSystemPage />);

    // Act
    // Assert
    await waitFor(() => {
      expect(screen.getByText("Setup Status")).toBeInTheDocument();
    });
  });

  it("renders Email Configuration and Email Outbox sections", async () => {
    // Arrange
    render(<AdminSystemPage />);

    // Act
    // Assert
    await waitFor(() => {
      expect(screen.getByText("Email Configuration")).toBeInTheDocument();
      expect(screen.getByText("Email Outbox")).toBeInTheDocument();
    });
  });

  it("fetches data on mount", async () => {
    // Arrange
    render(<AdminSystemPage />);

    // Act
    // Assert
    await waitFor(() => {
      expect(mockGetEmailSettings).toHaveBeenCalled();
      expect(mockGetEmailOutbox).toHaveBeenCalled();
      expect(mockGetTokenSettings).toHaveBeenCalled();
      expect(mockGetSetupStatus).toHaveBeenCalled();
    });
  });
});
