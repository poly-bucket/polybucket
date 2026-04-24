import axios, { isAxiosError, type InternalAxiosRequestConfig } from "axios";
import {
  clearSession,
  getOrCreateRefreshPromise,
  getStoredUser,
} from "@/lib/auth/authSession";
import { getApiConfig } from "./config";

const config = getApiConfig();

const axiosInstance = axios.create({
  baseURL: config.baseUrl,
  timeout: config.timeout,
  withCredentials: config.withCredentials,
  headers: {
    "Content-Type": "application/json",
  },
});

axiosInstance.interceptors.request.use((requestConfig: InternalAxiosRequestConfig) => {
  if (typeof window !== "undefined") {
    const parsed = getStoredUser();
    if (parsed?.accessToken?.trim()) {
      requestConfig.headers.Authorization = `Bearer ${parsed.accessToken}`;
    }
  }
  if (requestConfig.data instanceof FormData && requestConfig.headers) {
    const h = requestConfig.headers;
    h.delete("Content-Type");
  }
  return requestConfig;
});

type RequestWithTokenRetry = InternalAxiosRequestConfig & { _tokenRetry?: boolean };

axiosInstance.interceptors.response.use(
  (r) => r,
  async (error: unknown) => {
    if (!isAxiosError(error) || error.response?.status !== 401) {
      return Promise.reject(error);
    }
    const original = error.config as RequestWithTokenRetry | undefined;
    if (!original) {
      clearSession();
      return Promise.reject(error);
    }
    if (original._tokenRetry) {
      clearSession();
      return Promise.reject(error);
    }
    if (!getStoredUser()?.refreshToken?.trim()) {
      clearSession();
      return Promise.reject(error);
    }
    const refreshed = await getOrCreateRefreshPromise();
    if (!refreshed) {
      clearSession();
      return Promise.reject(error);
    }
    original._tokenRetry = true;
    if (!original.headers) {
      return Promise.reject(error);
    }
    original.headers.Authorization = `Bearer ${refreshed.accessToken}`;
    return axiosInstance.request(original);
  }
);

export default axiosInstance;
