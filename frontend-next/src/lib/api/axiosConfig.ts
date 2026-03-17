import axios, { type InternalAxiosRequestConfig } from "axios";
import { getApiConfig } from "./config";

const config = getApiConfig();

const AUTH_STORAGE_KEY = "polybucket-auth";

const axiosInstance = axios.create({
  baseURL: config.baseUrl,
  timeout: config.timeout,
  withCredentials: config.withCredentials,
  headers: {
    "Content-Type": "application/json",
  },
});

axiosInstance.interceptors.request.use((config: InternalAxiosRequestConfig) => {
  if (typeof window !== "undefined") {
    try {
      const stored = localStorage.getItem(AUTH_STORAGE_KEY);
      if (stored) {
        const parsed = JSON.parse(stored) as { accessToken?: string };
        if (parsed.accessToken?.trim()) {
          config.headers.Authorization = `Bearer ${parsed.accessToken}`;
        }
      }
    } catch {
      /* ignore parse errors */
    }
  }
  return config;
});

export default axiosInstance;
