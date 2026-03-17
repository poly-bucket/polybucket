export interface ApiConfig {
  baseUrl: string;
  timeout: number;
  withCredentials: boolean;
}

export const getApiConfig = (): ApiConfig => {
  const baseUrl = typeof window !== "undefined"
    ? (process.env.NEXT_PUBLIC_API_URL || "http://localhost:11666")
    : (process.env.NEXT_PUBLIC_API_URL || "http://localhost:11666");

  return {
    baseUrl,
    timeout: 30000,
    withCredentials: true,
  };
};
