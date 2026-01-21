export interface ApiConfig {
  baseUrl: string;
  timeout: number;
  withCredentials: boolean;
}

export const getApiConfig = (): ApiConfig => {
  const env = import.meta.env.MODE;
  
  // In development, use empty string to leverage Vite proxy
  // This allows requests to go through the proxy configured in vite.config.ts
  // which handles CORS and routing automatically
  const getDevelopmentBaseUrl = (): string => {
    const viteApiUrl = import.meta.env.VITE_API_URL;
    // If VITE_API_URL is explicitly set, use it (for Docker/production-like setups)
    // Otherwise, use empty string to use Vite proxy
    if (viteApiUrl && viteApiUrl !== '') {
      return viteApiUrl;
    }
    // Use empty string to leverage Vite proxy in development
    return '';
  };
  
  switch (env) {
    case 'development':
      return {
        baseUrl: getDevelopmentBaseUrl(),
        timeout: 30000,
        withCredentials: true
      };
    case 'production':
      return {
        baseUrl: import.meta.env.VITE_API_URL || 'https://api.polybucket.com',
        timeout: 30000,
        withCredentials: true
      };
    case 'staging':
      return {
        baseUrl: import.meta.env.VITE_API_URL || 'https://staging-api.polybucket.com',
        timeout: 30000,
        withCredentials: true
      };
    default:
      return {
        baseUrl: import.meta.env.VITE_API_URL || 'http://localhost:11666',
        timeout: 30000,
        withCredentials: true
      };
  }
};

export const API_CONFIG = getApiConfig(); 