import { 
  GetThemesClient, 
  CreateThemeClient, 
  SetActiveThemeClient,
  ThemeDto, 
  CreateThemeRequest, 
  ThemeResponse, 
  ThemeListResponse 
} from './api.client';
import store from '../store';

// Helper function to get auth token from Redux store
const getAuthToken = (): string | null => {
  try {
    const state = store.getState();
    return state.auth.user?.accessToken || null;
  } catch (error) {
    console.warn('Failed to get auth token from store:', error);
    return null;
  }
};

// Custom HTTP client that includes authentication headers
class AuthenticatedHttpClient {
  private baseUrl: string;

  constructor(baseUrl: string) {
    this.baseUrl = baseUrl;
  }

  async fetch(url: string, init?: RequestInit): Promise<Response> {
    try {
      const token = getAuthToken();
      
      const headers = new Headers(init?.headers);
      if (token) {
        headers.set('Authorization', `Bearer ${token}`);
      }
      
      const config: RequestInit = {
        ...init,
        headers
      };

      // The API clients already construct full URLs, so we don't need to add the base URL
      console.log('AuthenticatedHttpClient: Making request to:', url);
      return fetch(url, config);
    } catch (error) {
      console.error('AuthenticatedHttpClient: Error in fetch:', error);
      throw error;
    }
  }
}

// Define the theme colors interface for better type safety
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
  private getThemesClient: GetThemesClient;
  private createThemeClient: CreateThemeClient;
  private setActiveThemeClient: SetActiveThemeClient;
  private isInitialized: boolean = false;

  constructor() {
    try {
      const baseUrl = import.meta.env.VITE_API_URL || 'http://localhost:11666';
      const httpClient = new AuthenticatedHttpClient(baseUrl);
      
      this.getThemesClient = new GetThemesClient(baseUrl, httpClient);
      this.createThemeClient = new CreateThemeClient(baseUrl, httpClient);
      this.setActiveThemeClient = new SetActiveThemeClient(baseUrl, httpClient);
      
      this.isInitialized = true;
      console.log('ThemeService: Successfully initialized');
    } catch (error) {
      console.error('ThemeService: Failed to initialize:', error);
      throw error;
    }
  }

  /**
   * Get all themes and the currently active theme
   */
  async getThemes(): Promise<ThemeListResponse> {
    try {
      if (!this.isInitialized) {
        throw new Error('ThemeService is not initialized');
      }
      
      console.log('ThemeService: Getting themes...');
      const response = await this.getThemesClient.getThemes();
      console.log('ThemeService: Themes response:', response);
      return response;
    } catch (error) {
      console.error('Failed to fetch themes:', error);
      throw error;
    }
  }

  /**
   * Get the currently active theme (public endpoint)
   */
  async getActiveTheme(): Promise<ThemeDto | null> {
    try {
      // For now, we'll get the active theme from the themes list
      // since there's no direct endpoint for just the active theme
      const themesResponse = await this.getThemes();
      return themesResponse.activeTheme || null;
    } catch (error) {
      console.error('Failed to fetch active theme:', error);
      return null;
    }
  }

  /**
   * Create a new theme
   */
  async createTheme(request: CreateThemeRequest): Promise<ThemeResponse> {
    try {
      const response = await this.createThemeClient.createTheme(request);
      return response;
    } catch (error) {
      console.error('Failed to create theme:', error);
      throw error;
    }
  }

  /**
   * Set a theme as active
   */
  async setActiveTheme(themeId: number): Promise<boolean> {
    try {
      const response = await this.setActiveThemeClient.setActiveTheme(themeId);
      return response.success || false;
    } catch (error) {
      console.error('Failed to set active theme:', error);
      return false;
    }
  }

  /**
   * Convert backend theme colors to frontend format
   */
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
