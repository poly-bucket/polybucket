import { describe, it, expect, vi, beforeEach } from "vitest";
import { render, screen, waitFor, fireEvent } from "@/test/test-utils";
import { EmailSettingsSection, toEmailForm } from "./email-settings-section";
import type { EmailSettings } from "@/lib/services/emailService";

const mockGetEmailSettings = vi.fn();
const mockUpdateEmailSettings = vi.fn();
const mockTestEmailConfiguration = vi.fn();
const mockPreviewEmailTemplate = vi.fn();

vi.mock("@/lib/services/emailService", () => ({
  getEmailSettings: () => mockGetEmailSettings(),
  updateEmailSettings: (...args: unknown[]) => mockUpdateEmailSettings(...args),
  testEmailConfiguration: (...args: unknown[]) => mockTestEmailConfiguration(...args),
  previewEmailTemplate: (...args: unknown[]) => mockPreviewEmailTemplate(...args),
  getEmailApiErrorMessage: (_: unknown, fallback: string) => fallback,
}));

vi.mock("sonner", () => ({
  toast: { success: vi.fn(), error: vi.fn() },
}));

function smtpSettings(overrides: Partial<EmailSettings> = {}): EmailSettings {
  return {
    enabled: true,
    transport: "Smtp",
    smtpHost: "smtp.example.com",
    smtpPort: 587,
    smtpSecurity: "StartTls",
    smtpUsername: "mailer",
    hasPassword: true,
    allowInvalidCertificates: false,
    fromAddress: "noreply@example.com",
    fromName: "PolyBucket",
    publicBaseUrl: "https://models.example.com",
    requireEmailVerification: false,
    isConfigured: true,
    validationErrors: [],
    sources: {},
    managedByEnvironment: [],
    ...overrides,
  } as EmailSettings;
}

describe("EmailSettingsSection", () => {
  beforeEach(() => {
    mockGetEmailSettings.mockReset();
    mockUpdateEmailSettings.mockReset();
    mockTestEmailConfiguration.mockReset();
    mockPreviewEmailTemplate.mockReset();
  });

  it("disables fields managed by environment variables and labels them", async () => {
    // Arrange
    mockGetEmailSettings.mockResolvedValue(
      smtpSettings({ managedByEnvironment: ["smtpHost", "smtpPassword"] })
    );

    // Act
    render(<EmailSettingsSection />);

    // Assert
    await waitFor(() => {
      expect(screen.getByLabelText("SMTP host")).toBeDisabled();
    });
    expect(screen.getByLabelText("SMTP password")).toBeDisabled();
    expect(screen.getByLabelText("SMTP username")).not.toBeDisabled();
    expect(screen.getAllByText("Set by environment")).toHaveLength(2);
  });

  it("keeps the stored password when the password field is left blank on save", async () => {
    // Arrange
    const settings = smtpSettings();
    mockGetEmailSettings.mockResolvedValue(settings);
    mockUpdateEmailSettings.mockResolvedValue(settings);
    render(<EmailSettingsSection />);
    await waitFor(() => expect(screen.getByLabelText("SMTP host")).toBeInTheDocument());

    // Act
    fireEvent.change(screen.getByLabelText("SMTP host"), { target: { value: "smtp.new.example.com" } });
    fireEvent.click(screen.getByRole("button", { name: "Save changes" }));

    // Assert
    await waitFor(() => expect(mockUpdateEmailSettings).toHaveBeenCalledTimes(1));
    const payload = mockUpdateEmailSettings.mock.calls[0][0];
    expect(payload.smtpHost).toBe("smtp.new.example.com");
    expect(payload.smtpPassword).toBeUndefined();
    expect(payload.clearPassword).toBe(false);
  });

  it("disables the test button while there are unsaved changes", async () => {
    // Arrange
    mockGetEmailSettings.mockResolvedValue(smtpSettings());
    render(<EmailSettingsSection />);
    await waitFor(() => expect(screen.getByLabelText("SMTP host")).toBeInTheDocument());

    // Act
    fireEvent.change(screen.getByLabelText("From name"), { target: { value: "Makers" } });

    // Assert
    expect(screen.getByRole("button", { name: /send test email/i })).toBeDisabled();
    expect(screen.getByText("Save your changes before testing.")).toBeInTheDocument();
  });

  it("shows each diagnostic stage after running a test", async () => {
    // Arrange
    mockGetEmailSettings.mockResolvedValue(smtpSettings());
    mockTestEmailConfiguration.mockResolvedValue({
      success: false,
      message: "Authentication failed: 535",
      stages: [
        { stage: "Connect", success: true, message: "Connected", elapsedMilliseconds: 12 },
        { stage: "Auth", success: false, message: "Authentication failed: 535", elapsedMilliseconds: 8 },
      ],
    });
    render(<EmailSettingsSection defaultTestRecipient="admin@example.com" />);
    await waitFor(() => expect(screen.getByLabelText("SMTP host")).toBeInTheDocument());

    // Act
    fireEvent.click(screen.getByRole("button", { name: /send test email/i }));

    // Assert
    await waitFor(() => {
      expect(screen.getByLabelText("Email test results")).toBeInTheDocument();
    });
    expect(mockTestEmailConfiguration).toHaveBeenCalledWith("admin@example.com");
    expect(screen.getByText("Connect")).toBeInTheDocument();
    expect(screen.getByText("Auth")).toBeInTheDocument();
    expect(screen.getByLabelText("failed")).toBeInTheDocument();
  });

  it("shows an error when settings fail to load", async () => {
    // Arrange
    mockGetEmailSettings.mockRejectedValue(new Error("boom"));

    // Act
    render(<EmailSettingsSection />);

    // Assert
    await waitFor(() => {
      expect(screen.getByText("Failed to load email settings")).toBeInTheDocument();
    });
  });

  it("maps settings to a form without exposing the stored password", () => {
    // Arrange
    const settings = smtpSettings({ replyTo: undefined });

    // Act
    const form = toEmailForm(settings);

    // Assert
    expect(form.smtpPassword).toBe("");
    expect(form.clearPassword).toBe(false);
    expect(form.replyTo).toBe("");
    expect(form.smtpHost).toBe("smtp.example.com");
  });
});
