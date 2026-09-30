import { describe, it, expect, vi, beforeEach } from "vitest";
import { render, screen, waitFor, fireEvent } from "@/test/test-utils";
import EmailSetupStep from "./EmailSetupStep";

const mockGetEmailSettings = vi.fn();
const mockUpdateEmailSettings = vi.fn();

vi.mock("@/lib/services/emailService", () => ({
  getEmailSettings: () => mockGetEmailSettings(),
  updateEmailSettings: (...args: unknown[]) => mockUpdateEmailSettings(...args),
  testEmailConfiguration: vi.fn(),
  previewEmailTemplate: vi.fn(),
  getEmailApiErrorMessage: (_: unknown, fallback: string) => fallback,
}));

vi.mock("sonner", () => ({
  toast: { success: vi.fn(), error: vi.fn() },
}));

const logSettings = {
  enabled: true,
  transport: "Log",
  smtpHost: "",
  smtpPort: 587,
  smtpSecurity: "Auto",
  smtpUsername: "",
  hasPassword: false,
  allowInvalidCertificates: false,
  fromAddress: "noreply@example.com",
  fromName: "PolyBucket",
  publicBaseUrl: "https://models.example.com",
  requireEmailVerification: false,
  isConfigured: true,
  validationErrors: [],
  sources: {},
  managedByEnvironment: [],
};

describe("EmailSetupStep", () => {
  beforeEach(() => {
    mockGetEmailSettings.mockReset();
    mockUpdateEmailSettings.mockReset();
    mockGetEmailSettings.mockResolvedValue(logSettings);
    mockUpdateEmailSettings.mockResolvedValue(logSettings);
  });

  it("lets the admin skip email setup before saving", async () => {
    // Arrange
    const onComplete = vi.fn();
    render(<EmailSetupStep onComplete={onComplete} onBack={vi.fn()} />);
    await waitFor(() => expect(screen.getByLabelText("From address")).toBeInTheDocument());

    // Act
    fireEvent.click(screen.getByRole("button", { name: "Skip for now" }));

    // Assert
    expect(onComplete).toHaveBeenCalledWith({ emailConfigured: false });
    expect(screen.queryByRole("button", { name: "Continue" })).not.toBeInTheDocument();
  });

  it("shows Continue after saving and reports that email is configured", async () => {
    // Arrange
    const onComplete = vi.fn();
    render(<EmailSetupStep onComplete={onComplete} onBack={vi.fn()} />);
    await waitFor(() => expect(screen.getByLabelText("From address")).toBeInTheDocument());
    fireEvent.change(screen.getByLabelText("From name"), { target: { value: "Makers" } });
    fireEvent.click(screen.getByRole("button", { name: "Save changes" }));

    // Act
    const continueButton = await screen.findByRole("button", { name: "Continue" });
    fireEvent.click(continueButton);

    // Assert
    expect(onComplete).toHaveBeenCalledWith({ emailConfigured: true });
    expect(screen.queryByRole("button", { name: "Skip for now" })).not.toBeInTheDocument();
  });

  it("hides the template preview during setup", async () => {
    // Arrange
    render(<EmailSetupStep onComplete={vi.fn()} onBack={vi.fn()} />);

    // Act
    await waitFor(() => expect(screen.getByLabelText("From address")).toBeInTheDocument());

    // Assert
    expect(screen.queryByRole("button", { name: /preview template/i })).not.toBeInTheDocument();
  });
});
