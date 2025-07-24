export interface AuthenticationSettings {
  loginMethod: 'email' | 'username' | 'both';
  allowEmailLogin: boolean;
  allowUsernameLogin: boolean;
  requireEmailVerification: boolean;
  maxFailedLoginAttempts: number;
  lockoutDurationMinutes: number;
  requireStrongPasswords: boolean;
  passwordMinLength: number;
}

class AuthSettingsService {
  private settings: AuthenticationSettings | null = null;
  private settingsPromise: Promise<AuthenticationSettings> | null = null;

  async getSettings(): Promise<AuthenticationSettings> {
    if (this.settings) {
      return this.settings;
    }

    if (this.settingsPromise) {
      return this.settingsPromise;
    }

    this.settingsPromise = this.loadSettings();
    this.settings = await this.settingsPromise;
    return this.settings;
  }

  private async loadSettings(): Promise<AuthenticationSettings> {
    try {
      const response = await fetch('/api/system-settings/auth', {
        headers: {
          'Content-Type': 'application/json',
        },
      });

      if (!response.ok) {
        // Return default settings if API is not available
        return this.getDefaultSettings();
      }

      const settings = await response.json();
      return settings;
    } catch (error) {
      console.warn('Failed to load authentication settings, using defaults:', error);
      return this.getDefaultSettings();
    }
  }

  private getDefaultSettings(): AuthenticationSettings {
    return {
      loginMethod: 'email',
      allowEmailLogin: true,
      allowUsernameLogin: false,
      requireEmailVerification: false,
      maxFailedLoginAttempts: 5,
      lockoutDurationMinutes: 15,
      requireStrongPasswords: true,
      passwordMinLength: 8
    };
  }

  async updateSettings(settings: AuthenticationSettings): Promise<boolean> {
    try {
      // For now, we'll skip the update functionality to avoid the auth header issue
      // This can be implemented later with proper auth handling
      console.warn('Update settings functionality temporarily disabled');
      return false;
    } catch (error) {
      console.error('Failed to update authentication settings:', error);
      return false;
    }
  }

  getLoginPlaceholder(): string {
    if (!this.settings) {
      return 'Email or Username';
    }

    switch (this.settings.loginMethod) {
      case 'email':
        return 'Email address';
      case 'username':
        return 'Username';
      case 'both':
        return 'Email or Username';
      default:
        return 'Email or Username';
    }
  }

  isEmailLoginEnabled(): boolean {
    return this.settings?.allowEmailLogin ?? true;
  }

  isUsernameLoginEnabled(): boolean {
    return this.settings?.allowUsernameLogin ?? false;
  }

  getLoginMethod(): 'email' | 'username' | 'both' {
    return this.settings?.loginMethod ?? 'email';
  }
}

export const authSettingsService = new AuthSettingsService();
export default authSettingsService; 