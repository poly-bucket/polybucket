'use client';

import { useEffect, useState } from 'react';
import { useRouter, useSearchParams } from 'next/navigation';
import { Card, CardContent, CardHeader, CardTitle } from '@/components/ui/card';
import { Button } from '@/components/ui/button';
import { Loader2, CheckCircle, XCircle } from 'lucide-react';

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

export default function AuthCallbackPage() {
  const router = useRouter();
  const searchParams = useSearchParams();
  const [status, setStatus] = useState<'loading' | 'success' | 'error'>('loading');
  const [error, setError] = useState<string | null>(null);

  useEffect(() => {
    const handleCallback = async () => {
      try {
        const code = searchParams.get('code');
        const state = searchParams.get('state');
        const error = searchParams.get('error');

        if (error) {
          setError(`GitHub OAuth error: ${error}`);
          setStatus('error');
          return;
        }

        if (!code || !state) {
          setError('Missing OAuth parameters');
          setStatus('error');
          return;
        }

        // Validate state to prevent CSRF attacks
        const storedState = sessionStorage.getItem('oauth_state');
        if (!storedState || storedState !== state) {
          sessionStorage.removeItem('oauth_state');
          setError('Invalid OAuth state. Please try again.');
          setStatus('error');
          return;
        }

        // Remove state from sessionStorage after validation
        sessionStorage.removeItem('oauth_state');

        // Call backend OAuth callback
        const apiUrl = getApiUrl();
        const response = await fetch(`${apiUrl}/api/auth/github/callback`, {
          method: 'POST',
          headers: {
            'Content-Type': 'application/json',
          },
          body: JSON.stringify({ code, state }),
        });

        if (!response.ok) {
          const errorData = await response.json();
          throw new Error(errorData.error || 'OAuth callback failed');
        }

        const data = await response.json();
        
        // Store tokens
        localStorage.setItem('auth_token', data.token);
        if (data.refreshToken) {
          localStorage.setItem('refresh_token', data.refreshToken);
        }
        
        setStatus('success');
        
        // Redirect after a short delay
        setTimeout(() => {
          const returnUrl = searchParams.get('returnUrl') || '/';
          router.push(returnUrl);
        }, 2000);

      } catch (err) {
        console.error('OAuth callback error:', err);
        setError(err instanceof Error ? err.message : 'Authentication failed');
        setStatus('error');
      }
    };

    handleCallback();
  }, [searchParams, router]);

  const getStatusIcon = () => {
    switch (status) {
      case 'loading':
        return <Loader2 className="h-8 w-8 animate-spin text-blue-500" />;
      case 'success':
        return <CheckCircle className="h-8 w-8 text-green-500" />;
      case 'error':
        return <XCircle className="h-8 w-8 text-red-500" />;
    }
  };

  const getStatusMessage = () => {
    switch (status) {
      case 'loading':
        return 'Completing authentication...';
      case 'success':
        return 'Authentication successful! Redirecting...';
      case 'error':
        return error || 'Authentication failed';
    }
  };

  return (
    <div className="min-h-screen bg-background flex items-center justify-center p-4">
      <Card className="w-full max-w-md">
        <CardHeader className="text-center">
          <div className="flex justify-center mb-4">
            {getStatusIcon()}
          </div>
          <CardTitle className="text-xl">
            {status === 'loading' && 'Authenticating...'}
            {status === 'success' && 'Success!'}
            {status === 'error' && 'Authentication Failed'}
          </CardTitle>
        </CardHeader>
        <CardContent className="text-center">
          <p className="text-muted-foreground mb-4">
            {getStatusMessage()}
          </p>
          
          {status === 'error' && (
            <div className="space-y-2">
              <Button 
                onClick={() => router.push('/login')}
                className="w-full"
              >
                Try Again
              </Button>
              <Button 
                variant="outline"
                onClick={() => router.push('/')}
                className="w-full"
              >
                Go Home
              </Button>
            </div>
          )}
        </CardContent>
      </Card>
    </div>
  );
}










