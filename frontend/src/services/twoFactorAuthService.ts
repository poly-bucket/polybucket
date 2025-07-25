import api from '../utils/axiosConfig';

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

export interface EnableTwoFactorAuthRequest {
  token: string;
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
  private baseUrl = '/auth/2fa';

  async getStatus(): Promise<TwoFactorAuthStatus> {
    const response = await api.get(`${this.baseUrl}/status`);
    return response.data;
  }

  async initialize(): Promise<InitializeTwoFactorAuthResponse> {
    const response = await api.post(`${this.baseUrl}/initialize`, {});
    return response.data;
  }

  async enable(token: string): Promise<EnableTwoFactorAuthResponse> {
    const response = await api.post(`${this.baseUrl}/enable`, { token });
    return response.data;
  }

  async disable(): Promise<DisableTwoFactorAuthResponse> {
    const response = await api.post(`${this.baseUrl}/disable`, {});
    return response.data;
  }
}

export const twoFactorAuthService = new TwoFactorAuthService(); 