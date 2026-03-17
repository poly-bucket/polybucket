import { ApiClientFactory } from "@/lib/api/clientFactory";
import { ChangePasswordCommand } from "@/lib/api/client";

export async function changePassword(
  currentPassword: string,
  newPassword: string,
  confirmPassword: string
): Promise<{ success: boolean; message?: string }> {
  try {
    const api = ApiClientFactory.getApiClient();
    const command = new ChangePasswordCommand({
      currentPassword,
      newPassword,
      confirmPassword,
    });
    const response = await api.changePassword_ChangePassword(command);
    return {
      success: response?.success ?? false,
      message: response?.message,
    };
  } catch (error: unknown) {
    const err = error as { response?: { data?: { message?: string } }; message?: string };
    const message =
      err.response?.data?.message ?? err.message ?? "Failed to change password";
    return { success: false, message };
  }
}
