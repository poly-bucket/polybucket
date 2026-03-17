import { ApiClientFactory } from "@/lib/api/clientFactory";
import {
  EnableTwoFactorAuthCommand,
  InitializeTwoFactorAuthCommand,
  DisableTwoFactorAuthCommand,
} from "@/lib/api/client";

const api = () => ApiClientFactory.getApiClient();

export interface TwoFactorAuthStatus {
  isEnabled: boolean;
  isInitialized: boolean;
  enabledAt?: string;
  lastUsedAt?: string;
  remainingBackupCodes: number;
}

export interface InitializeTwoFactorAuthResponse {
  qrCodeUrl: string;
  secretKey: string;
}

export interface EnableTwoFactorAuthResponse {
  success: boolean;
  message: string;
  backupCodes?: string[];
}

export interface DisableTwoFactorAuthResponse {
  success: boolean;
  message: string;
}

class TwoFactorAuthService {
  async getStatus(): Promise<TwoFactorAuthStatus> {
    const response = await api().getTwoFactorAuthStatus_GetStatus();
    return response as unknown as TwoFactorAuthStatus;
  }

  async initialize(): Promise<InitializeTwoFactorAuthResponse> {
    const command = new InitializeTwoFactorAuthCommand({});
    const response = await api().initializeTwoFactorAuth_Initialize(command);
    return response as unknown as InitializeTwoFactorAuthResponse;
  }

  async enable(token: string): Promise<EnableTwoFactorAuthResponse> {
    const command = new EnableTwoFactorAuthCommand({ token });
    const response = await api().enableTwoFactorAuth_Enable(command);
    return response as unknown as EnableTwoFactorAuthResponse;
  }

  async disable(): Promise<DisableTwoFactorAuthResponse> {
    const command = new DisableTwoFactorAuthCommand({});
    const response = await api().disableTwoFactorAuth_Disable(command);
    return response as unknown as DisableTwoFactorAuthResponse;
  }
}

export const twoFactorAuthService = new TwoFactorAuthService();
