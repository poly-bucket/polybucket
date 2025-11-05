import { API_CONFIG } from '../api/config';
import { AxiosHttpClient } from '../api/axiosAdapter';
import {
  GetTwoFactorAuthStatusClient,
  InitializeTwoFactorAuthClient,
  EnableTwoFactorAuthClient,
  DisableTwoFactorAuthClient,
  EnableTwoFactorAuthCommand,
  InitializeTwoFactorAuthCommand
} from './api.client';

const sharedHttpClient = new AxiosHttpClient(API_CONFIG.baseUrl);

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
  async getStatus(): Promise<TwoFactorAuthStatus> {
    const client = new GetTwoFactorAuthStatusClient(API_CONFIG.baseUrl, sharedHttpClient);
    const response = await client.getTwoFactorAuthStatus();
    return response as any as TwoFactorAuthStatus;
  }

  async initialize(): Promise<InitializeTwoFactorAuthResponse> {
    const client = new InitializeTwoFactorAuthClient(API_CONFIG.baseUrl, sharedHttpClient);
    const command = new InitializeTwoFactorAuthCommand({});
    const response = await client.initialize(command);
    return response as any as InitializeTwoFactorAuthResponse;
  }

  async enable(token: string): Promise<EnableTwoFactorAuthResponse> {
    const client = new EnableTwoFactorAuthClient(API_CONFIG.baseUrl, sharedHttpClient);
    const command = new EnableTwoFactorAuthCommand({
      token: token
    });
    const response = await client.enable(command);
    return response as any as EnableTwoFactorAuthResponse;
  }

  async disable(): Promise<DisableTwoFactorAuthResponse> {
    const client = new DisableTwoFactorAuthClient(API_CONFIG.baseUrl, sharedHttpClient);
    const response = await client.disableTwoFactorAuth();
    return response as any as DisableTwoFactorAuthResponse;
  }
}

export const twoFactorAuthService = new TwoFactorAuthService();
