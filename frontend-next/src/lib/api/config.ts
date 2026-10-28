export interface ApiConfig {
  baseUrl: string;
  timeout: number;
  withCredentials: boolean;
}

export const getApiConfig = (): ApiConfig => {
  const baseUrl =
    typeof window !== "undefined"
      ? "/api/polybucket"
      : process.env.POLYBUCKET_INTERNAL_BASE_URL?.replace(/\/$/, "") ||
        `${(process.env.POLYBUCKET_API_URL || process.env.NEXT_PUBLIC_API_URL || "http://localhost:11666").replace(/\/$/, "")}/api`;

  return {
    baseUrl,
    timeout: 30000,
    withCredentials: true,
  };
};
