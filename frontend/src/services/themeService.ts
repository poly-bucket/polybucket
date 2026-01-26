import { ApiClientFactory } from '../api/clientFactory';
import {
  ThemeDto,
  CreateThemeRequest,
  ThemeResponse,
  ThemeListResponse
} from '../api/client';

const api = () => ApiClientFactory.getApiClient();

interface ThemeColors {
  primary: string;
  primaryLight: string;
  primaryDark: string;
  secondary: string;
  secondaryLight: string;
  secondaryDark: string;
  accent: string;
  accentLight: string;
  accentDark: string;
  bgPrimary: string;
  bgSecondary: string;
  bgTertiary: string;
}

class ThemeService {
  async getThemes(): Promise<ThemeListResponse> {
    try {
      return await api().getThemes_GetThemes();
    } catch (error) {
      console.error('Failed to fetch themes:', error);
      throw error;
    }
  }

  async getActiveTheme(): Promise<ThemeDto | null> {
    try {
      const themesResponse = await this.getThemes();
      return themesResponse.activeTheme || null;
    } catch (error) {
      console.error('Failed to fetch active theme:', error);
      return null;
    }
  }

  async createTheme(request: CreateThemeRequest): Promise<ThemeResponse> {
    try {
      return await api().createTheme_CreateTheme(request);
    } catch (error) {
      console.error('Failed to create theme:', error);
      throw error;
    }
  }

  async setActiveTheme(themeId: number): Promise<boolean> {
    try {
      const response = await api().setActiveTheme_SetActiveTheme(themeId);
      return (response as any).success || false;
    } catch (error) {
      console.error('Failed to set active theme:', error);
      return false;
    }
  }

  convertToFrontendFormat(themeColors: any): ThemeColors {
    return {
      primary: themeColors.primary || '#6366f1',
      primaryLight: themeColors.primaryLight || '#818cf8',
      primaryDark: themeColors.primaryDark || '#4f46e5',
      secondary: themeColors.secondary || '#8b5cf6',
      secondaryLight: themeColors.secondaryLight || '#a78bfa',
      secondaryDark: themeColors.secondaryDark || '#7c3aed',
      accent: themeColors.accent || '#06b6d4',
      accentLight: themeColors.accentLight || '#22d3ee',
      accentDark: themeColors.accentDark || '#0891b2',
      bgPrimary: themeColors.backgroundPrimary || '#0f0f23',
      bgSecondary: themeColors.backgroundSecondary || '#1a1a2e',
      bgTertiary: themeColors.backgroundTertiary || '#16213e'
    };
  }
}

export const themeService = new ThemeService();
export default themeService;
