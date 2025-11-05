'use client';

import React, { createContext, useContext, useEffect, useState, ReactNode } from 'react';

const getApiUrl = (): string => {
  if (typeof window === 'undefined') {
    return process.env.NEXT_PUBLIC_API_URL || 'http://localhost:10120';
  }
  
  const apiUrl = process.env.NEXT_PUBLIC_API_URL || 'http://localhost:10120';
  
  if (apiUrl.includes('marketplace-api:') || apiUrl.includes('localhost:10280')) {
    return 'http://localhost:10120';
  }
  
  return apiUrl;
};

// Types
export interface User {
  id: string;
  username: string;
  email: string;
  displayName?: string;
  avatarUrl?: string;
  bio?: string;
  location?: string;
  website?: string;
  company?: string;
  isVerified: boolean;
  createdAt: string;
  lastLoginAt?: string;
  lastActivityAt?: string;
  githubId?: number;
  githubUsername?: string;
  githubProfileUrl?: string;
  pluginCount: number;
  installationCount: number;
  reviewCount: number;
  status: string;
  primaryRole: string;
  roles: string[];
  permissions: string[];
  reputationScore: number;
  contributionScore: number;
}

export interface AuthState {
  user: User | null;
  isAuthenticated: boolean;
  isLoading: boolean;
  error: string | null;
}

export interface AuthContextType extends AuthState {
  login: () => Promise<void>;
  logout: () => Promise<void>;
  refreshToken: () => Promise<void>;
  hasPermission: (permission: string) => boolean;
  hasRole: (role: string) => boolean;
  isAdmin: () => boolean;
  isModerator: () => boolean;
  isDeveloper: () => boolean;
}

// Context
const AuthContext = createContext<AuthContextType | undefined>(undefined);

// Provider Props
interface AuthProviderProps {
  children: ReactNode;
}

// Auth Provider Component
export function AuthProvider({ children }: AuthProviderProps) {
  const [state, setState] = useState<AuthState>({
    user: null,
    isAuthenticated: false,
    isLoading: true,
    error: null,
  });

  // Initialize auth state from localStorage
  useEffect(() => {
    const initializeAuth = async () => {
      try {
        const token = localStorage.getItem('auth_token');
        if (token) {
          // Validate token and get user info
          await validateToken(token);
        }
      } catch (error) {
        console.error('Auth initialization error:', error);
        localStorage.removeItem('auth_token');
      } finally {
        setState(prev => ({ ...prev, isLoading: false }));
      }
    };

    initializeAuth();
  }, []);

  // Validate JWT token
  const validateToken = async (token: string) => {
    try {
      const apiUrl = getApiUrl();
      const response = await fetch(`${apiUrl}/api/auth/validate`, {
        method: 'GET',
        headers: {
          'Authorization': `Bearer ${token}`,
          'Content-Type': 'application/json',
        },
      });

      if (response.ok) {
        const userData = await response.json();
        setState({
          user: userData.user,
          isAuthenticated: true,
          isLoading: false,
          error: null,
        });
      } else {
        throw new Error('Token validation failed');
      }
    } catch (error) {
      throw error;
    }
  };

  // Login function
  const login = async () => {
    try {
      setState(prev => ({ ...prev, isLoading: true, error: null }));

      // Get GitHub OAuth configuration from environment variables
      const clientId = process.env.NEXT_PUBLIC_GITHUB_CLIENT_ID;
      const redirectUri = process.env.NEXT_PUBLIC_GITHUB_REDIRECT_URI || 'http://localhost:10110/auth/callback';

      if (!clientId) {
        throw new Error('GitHub Client ID is not configured');
      }

      // Generate a random state for CSRF protection
      const state = crypto.randomUUID();

      // Store state in sessionStorage for validation during callback
      sessionStorage.setItem('oauth_state', state);

      // Construct GitHub OAuth URL
      const authUrl = `https://github.com/login/oauth/authorize?` +
        `client_id=${encodeURIComponent(clientId)}&` +
        `redirect_uri=${encodeURIComponent(redirectUri)}&` +
        `scope=user:email,read:user&` +
        `state=${encodeURIComponent(state)}`;

      // Redirect to GitHub OAuth
      window.location.href = authUrl;
    } catch (error) {
      const errorMessage = error instanceof Error ? error.message : 'Login failed';
      setState(prev => ({
        ...prev,
        isLoading: false,
        error: errorMessage,
      }));
    }
  };

  // Logout function
  const logout = async () => {
    try {
      const token = localStorage.getItem('auth_token');
      
      const refreshTokenValue = localStorage.getItem('refresh_token');
      if (refreshTokenValue) {
        // Call logout endpoint
        const apiUrl = getApiUrl();
        await fetch(`${apiUrl}/api/auth/logout`, {
          method: 'POST',
          headers: {
            'Authorization': `Bearer ${token}`,
            'Content-Type': 'application/json',
          },
          body: JSON.stringify({ refreshToken: refreshTokenValue }),
        });
      }
    } catch (error) {
      console.error('Logout error:', error);
    } finally {
      // Clear local state regardless of API call success
      localStorage.removeItem('auth_token');
      localStorage.removeItem('refresh_token');
      setState({
        user: null,
        isAuthenticated: false,
        isLoading: false,
        error: null,
      });
    }
  };

  // Refresh token function
  const refreshToken = async () => {
    try {
      const refreshTokenValue = localStorage.getItem('refresh_token');
      if (!refreshTokenValue) {
        throw new Error('No refresh token available');
      }

      const apiUrl = getApiUrl();
      const response = await fetch(`${apiUrl}/api/auth/refresh`, {
        method: 'POST',
        headers: {
          'Content-Type': 'application/json',
        },
        body: JSON.stringify({ refreshToken: refreshTokenValue }),
      });

      if (response.ok) {
        const data = await response.json();
        localStorage.setItem('auth_token', data.token);
        if (data.refreshToken) {
          localStorage.setItem('refresh_token', data.refreshToken);
        }
        await validateToken(data.token);
      } else {
        throw new Error('Token refresh failed');
      }
    } catch (error) {
      console.error('Token refresh error:', error);
      await logout();
    }
  };

  // Permission checking functions
  const hasPermission = (permission: string): boolean => {
    return state.user?.permissions.includes(permission) ?? false;
  };

  const hasRole = (role: string): boolean => {
    return state.user?.roles.includes(role) ?? false;
  };

  const isAdmin = (): boolean => {
    return hasRole('SiteAdmin');
  };

  const isModerator = (): boolean => {
    return hasRole('Moderator') || isAdmin();
  };

  const isDeveloper = (): boolean => {
    return hasRole('Developer') || isModerator();
  };

  // Handle OAuth callback
  const handleOAuthCallback = async (code: string, state: string) => {
    try {
      setState(prev => ({ ...prev, isLoading: true, error: null }));

      // Validate state to prevent CSRF attacks
      const storedState = sessionStorage.getItem('oauth_state');
      if (!storedState || storedState !== state) {
        sessionStorage.removeItem('oauth_state');
        throw new Error('Invalid OAuth state. Please try again.');
      }

      // Remove state from sessionStorage after validation
      sessionStorage.removeItem('oauth_state');

      const apiUrl = getApiUrl();
      const response = await fetch(`${apiUrl}/api/auth/github/callback`, {
        method: 'POST',
        headers: {
          'Content-Type': 'application/json',
        },
        body: JSON.stringify({ code, state }),
      });

      if (!response.ok) {
        throw new Error('OAuth callback failed');
      }

      const data = await response.json();
      
      // Store tokens
      localStorage.setItem('auth_token', data.token);
      if (data.refreshToken) {
        localStorage.setItem('refresh_token', data.refreshToken);
      }
      
      // Update state
      setState({
        user: data.user,
        isAuthenticated: true,
        isLoading: false,
        error: null,
      });

      // Redirect to home or intended page
      const returnUrl = new URLSearchParams(window.location.search).get('returnUrl') || '/';
      window.location.href = returnUrl;
    } catch (error) {
      const errorMessage = error instanceof Error ? error.message : 'OAuth callback failed';
      setState(prev => ({
        ...prev,
        isLoading: false,
        error: errorMessage,
      }));
    }
  };

  // Check for OAuth callback on mount
  useEffect(() => {
    const urlParams = new URLSearchParams(window.location.search);
    const code = urlParams.get('code');
    const state = urlParams.get('state');

    if (code && state) {
      handleOAuthCallback(code, state);
    }
  }, []);

  const contextValue: AuthContextType = {
    ...state,
    login,
    logout,
    refreshToken,
    hasPermission,
    hasRole,
    isAdmin,
    isModerator,
    isDeveloper,
  };

  return (
    <AuthContext.Provider value={contextValue}>
      {children}
    </AuthContext.Provider>
  );
}

// Hook to use auth context
export function useAuth(): AuthContextType {
  const context = useContext(AuthContext);
  if (context === undefined) {
    throw new Error('useAuth must be used within an AuthProvider');
  }
  return context;
}

// Higher-order component for protected routes
export function withAuth<P extends object>(
  Component: React.ComponentType<P>,
  requiredPermission?: string,
  requiredRole?: string
) {
  return function AuthenticatedComponent(props: P) {
    const { isAuthenticated, isLoading, hasPermission, hasRole } = useAuth();

    if (isLoading) {
      return (
        <div className="min-h-screen flex items-center justify-center">
          <div className="animate-spin rounded-full h-32 w-32 border-b-2 border-primary"></div>
        </div>
      );
    }

    if (!isAuthenticated) {
      return (
        <div className="min-h-screen flex items-center justify-center">
          <div className="text-center">
            <h2 className="text-2xl font-bold mb-4">Authentication Required</h2>
            <p className="text-muted-foreground mb-4">Please sign in to access this page.</p>
            <button
              onClick={() => window.location.href = '/login'}
              className="px-4 py-2 bg-primary text-primary-foreground rounded-md hover:bg-primary/90"
            >
              Sign In
            </button>
          </div>
        </div>
      );
    }

    if (requiredPermission && !hasPermission(requiredPermission)) {
      return (
        <div className="min-h-screen flex items-center justify-center">
          <div className="text-center">
            <h2 className="text-2xl font-bold mb-4">Permission Denied</h2>
            <p className="text-muted-foreground mb-4">
              You don't have the required permission: {requiredPermission}
            </p>
          </div>
        </div>
      );
    }

    if (requiredRole && !hasRole(requiredRole)) {
      return (
        <div className="min-h-screen flex items-center justify-center">
          <div className="text-center">
            <h2 className="text-2xl font-bold mb-4">Access Denied</h2>
            <p className="text-muted-foreground mb-4">
              You don't have the required role: {requiredRole}
            </p>
          </div>
        </div>
      );
    }

    return <Component {...props} />;
  };
}
