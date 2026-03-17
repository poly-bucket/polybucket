import { describe, it, expect, vi } from "vitest";
import userEvent from "@testing-library/user-event";
import { render, screen } from "@/test/test-utils";
import SecuritySettingsPage from "./page";
import { changePassword } from "@/lib/services/changePasswordService";

vi.mock("@/lib/services/changePasswordService", () => ({
  changePassword: vi.fn(),
}));

vi.mock("sonner", () => ({
  toast: {
    success: vi.fn(),
    error: vi.fn(),
  },
}));

vi.mock("@/components/settings/two-factor-auth", () => ({
  TwoFactorAuth: () => <div data-testid="two-factor-auth">2FA</div>,
}));

describe("SecuritySettingsPage", () => {
  const mockChangePassword = vi.mocked(changePassword);

  beforeEach(() => {
    mockChangePassword.mockResolvedValue({ success: true });
  });

  it("renders change password form", () => {
    render(<SecuritySettingsPage />);

    expect(screen.getByPlaceholderText(/enter current password/i)).toBeInTheDocument();
    expect(screen.getByPlaceholderText(/enter new password/i)).toBeInTheDocument();
    expect(screen.getByPlaceholderText(/confirm new password/i)).toBeInTheDocument();
    expect(screen.getByRole("button", { name: /change password/i })).toBeInTheDocument();
  });

  it("shows validation error when passwords do not match", async () => {
    const user = userEvent.setup();
    render(<SecuritySettingsPage />);

    await user.type(screen.getByPlaceholderText(/enter current password/i), "oldpass");
    await user.type(screen.getByPlaceholderText(/enter new password/i), "newpassword123");
    await user.type(screen.getByPlaceholderText(/confirm new password/i), "different");
    await user.click(screen.getByRole("button", { name: /change password/i }));

    expect(await screen.findByText(/passwords do not match/i)).toBeInTheDocument();
  });

  it("calls changePassword on valid submit", async () => {
    const user = userEvent.setup();
    render(<SecuritySettingsPage />);

    await user.type(screen.getByPlaceholderText(/enter current password/i), "oldpass");
    await user.type(screen.getByPlaceholderText(/enter new password/i), "newpassword123");
    await user.type(screen.getByPlaceholderText(/confirm new password/i), "newpassword123");
    await user.click(screen.getByRole("button", { name: /change password/i }));

    expect(mockChangePassword).toHaveBeenCalledWith(
      "oldpass",
      "newpassword123",
      "newpassword123"
    );
  });
});
