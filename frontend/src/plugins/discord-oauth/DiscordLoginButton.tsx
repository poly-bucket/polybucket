import React, { useState } from 'react';
import { useAuth } from '../../context/AuthContext';
import { usePlugin } from '../PluginAPI';

interface DiscordLoginButtonProps {
  onSuccess?: (userInfo: any) => void;
  onError?: (error: string) => void;
  className?: string;
  children?: React.ReactNode;
}

const DiscordLoginButton: React.FC<DiscordLoginButtonProps> = ({
  onSuccess,
  onError,
  className = '',
  children
}) => {
  const { login } = useAuth();
  const plugin = usePlugin('discord-oauth-plugin');
  const [isLoading, setIsLoading] = useState(false);

  const handleDiscordLogin = async () => {
    try {
      setIsLoading(true);

      // Generate state parameter for security
      const state = Math.random().toString(36).substring(2, 15);
      
      // Store state in session storage
      sessionStorage.setItem('discord_oauth_state', state);

      // Get authorization URL from backend
      const response = await fetch('/api/plugins/oauth/authorize/discord', {
        method: 'GET',
        headers: {
          'Content-Type': 'application/json'
        },
        body: JSON.stringify({
          redirectUri: `${window.location.origin}/auth/discord/callback`,
          state: state
        })
      });

      if (!response.ok) {
        throw new Error('Failed to get authorization URL');
      }

      const data = await response.json();
      
      // Redirect to Discord authorization
      window.location.href = data.authorizationUrl;
    } catch (error) {
      console.error('Discord login error:', error);
      onError?.(error instanceof Error ? error.message : 'Discord login failed');
    } finally {
      setIsLoading(false);
    }
  };

  const handleDiscordCallback = async (code: string, state: string) => {
    try {
      setIsLoading(true);

      // Verify state parameter
      const storedState = sessionStorage.getItem('discord_oauth_state');
      if (state !== storedState) {
        throw new Error('Invalid state parameter');
      }

      // Exchange code for tokens
      const response = await fetch('/api/plugins/oauth/callback/discord', {
        method: 'POST',
        headers: {
          'Content-Type': 'application/json'
        },
        body: JSON.stringify({
          code: code,
          redirectUri: `${window.location.origin}/auth/discord/callback`,
          state: state
        })
      });

      if (!response.ok) {
        throw new Error('Failed to exchange authorization code');
      }

      const authResult = await response.json();

      if (!authResult.success) {
        throw new Error(authResult.errorDescription || 'Authorization failed');
      }

      // Get user information
      const userInfoResponse = await fetch(`/api/plugins/oauth/userinfo/discord?accessToken=${authResult.accessToken}`);
      
      if (!userInfoResponse.ok) {
        throw new Error('Failed to get user information');
      }

      const userInfo = await userInfoResponse.json();

      // Log in the user
      await login({
        provider: 'discord',
        accessToken: authResult.accessToken,
        refreshToken: authResult.refreshToken,
        userInfo: userInfo
      });

      // Clean up
      sessionStorage.removeItem('discord_oauth_state');

      onSuccess?.(userInfo);
    } catch (error) {
      console.error('Discord callback error:', error);
      onError?.(error instanceof Error ? error.message : 'Discord login failed');
    } finally {
      setIsLoading(false);
    }
  };

  // Handle OAuth callback if we're on the callback page
  React.useEffect(() => {
    const urlParams = new URLSearchParams(window.location.search);
    const code = urlParams.get('code');
    const state = urlParams.get('state');
    const error = urlParams.get('error');

    if (error) {
      onError?.(error);
      return;
    }

    if (code && state) {
      handleDiscordCallback(code, state);
    }
  }, [onError]);

  return (
    <button
      onClick={handleDiscordLogin}
      disabled={isLoading}
      className={`inline-flex items-center px-4 py-2 border border-transparent text-sm font-medium rounded-md text-white bg-indigo-600 hover:bg-indigo-700 focus:outline-none focus:ring-2 focus:ring-offset-2 focus:ring-indigo-500 disabled:opacity-50 disabled:cursor-not-allowed ${className}`}
    >
      {isLoading ? (
        <>
          <div className="animate-spin rounded-full h-4 w-4 border-b-2 border-white mr-2"></div>
          Connecting...
        </>
      ) : (
        <>
          <svg className="w-4 h-4 mr-2" viewBox="0 0 24 24" fill="currentColor">
            <path d="M20.317 4.37a19.791 19.791 0 0 0-4.885-1.515.074.074 0 0 0-.079.037c-.21.375-.444.864-.608 1.25a18.27 18.27 0 0 0-5.487 0 12.64 12.64 0 0 0-.617-1.25.077.077 0 0 0-.079-.037A19.736 19.736 0 0 0 3.677 4.37a.07.07 0 0 0-.032.027C.533 9.046-.32 13.58.099 18.057a.082.082 0 0 0 .031.057 19.9 19.9 0 0 0 5.993 3.03.078.078 0 0 0 .084-.028 14.09 14.09 0 0 0 1.226-1.994.076.076 0 0 0-.041-.106 13.107 13.107 0 0 1-1.872-.892.077.077 0 0 1-.008-.128 10.2 10.2 0 0 0 .372-.292.074.074 0 0 1 .077-.01c3.928 1.793 8.18 1.793 12.062 0a.074.074 0 0 1 .078.01c.12.098.246.198.373.292a.077.077 0 0 1-.006.127 12.299 12.299 0 0 1-1.873.892.077.077 0 0 0-.041.107c.36.698.772 1.362 1.225 1.993a.076.076 0 0 0 .084.028 19.839 19.839 0 0 0 6.002-3.03.077.077 0 0 0 .032-.054c.5-5.177-.838-9.674-3.549-13.66a.061.061 0 0 0-.031-.03zM8.02 15.33c-1.183 0-2.157-1.085-2.157-2.419 0-1.333.956-2.419 2.157-2.419 1.21 0 2.176 1.096 2.157 2.42 0 1.333-.956 2.418-2.157 2.418zm7.975 0c-1.183 0-2.157-1.085-2.157-2.419 0-1.333.955-2.419 2.157-2.419 1.21 0 2.176 1.096 2.157 2.42 0 1.333-.946 2.418-2.157 2.418z"/>
          </svg>
          {children || 'Login with Discord'}
        </>
      )}
    </button>
  );
};

export default DiscordLoginButton;
