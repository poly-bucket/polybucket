const getApiUrl = (): string => {
  const apiUrl = process.env.NEXT_PUBLIC_API_URL || 'http://localhost:10120';
  
  if (typeof window === 'undefined') {
    return apiUrl;
  }
  
  if (apiUrl.includes('marketplace-api:') || apiUrl.includes('localhost:10280')) {
    return 'http://localhost:10120';
  }
  
  return apiUrl;
};

export const API_BASE_URL = getApiUrl();

