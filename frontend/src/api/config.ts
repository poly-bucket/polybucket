export interface ApiConfig {
  baseUrl: string;
  timeout: number;
  withCredentials: boolean;
}

export const getApiConfig = (): ApiConfig => {
  const env = import.meta.env.MODE;
  
  switch (env) {
    case 'development':
      return {
        baseUrl: import.meta.env.VITE_API_URL || 'http://localhost:11666',
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