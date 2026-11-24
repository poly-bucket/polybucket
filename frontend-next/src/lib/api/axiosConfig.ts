import axios, { isAxiosError, type AxiosError, type InternalAxiosRequestConfig } from "axios";
import {
  clearSession,
  fetchSessionFromBff,
  getOrCreateRefreshPromise,
  getStoredUser,
} from "@/lib/auth/authSession";
import {
  PRIVATE_SITE_MARKER_HEADER,
  PRIVATE_SITE_MARKER_VALUE,
  shouldRedirectToLogin,
} from "@/lib/auth/privateSiteRedirect";
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
  if (requestConfig.data instanceof FormData && requestConfig.headers) {
    const h = requestConfig.headers;
    h.delete("Content-Type");
  }
  return requestConfig;
});

type RequestWithTokenRetry = InternalAxiosRequestConfig & { _tokenRetry?: boolean };

function readResponseMarker(headers: unknown): boolean {
  if (!headers || typeof headers !== "object") {
    return false;
  }
  const record = headers as Record<string, unknown>;
  const raw =
    record[PRIVATE_SITE_MARKER_HEADER] ??
    record[PRIVATE_SITE_MARKER_HEADER.toLowerCase()] ??
    record["X-Site-Access"];
  return typeof raw === "string" && raw.toLowerCase() === PRIVATE_SITE_MARKER_VALUE;
}

function redirectToLoginIfPrivateSite(error: AxiosError): void {
  if (typeof window === "undefined") {
    return;
  }
  const target = shouldRedirectToLogin(
    error.response?.status,
    error.response?.headers as Record<string, unknown> | undefined,
    window.location.pathname
  );
  if (target) {
    window.location.assign(target);
  }
}

async function tryRecoverSessionAndRetry(
  original: RequestWithTokenRetry | undefined
): Promise<unknown> {
  if (!original || original._tokenRetry) {
    return null;
  }

  if (!getStoredUser()) {
    await fetchSessionFromBff();
  }

  const refreshed = await getOrCreateRefreshPromise();
  if (!refreshed && !getStoredUser()) {
    return null;
  }

  original._tokenRetry = true;
  return axiosInstance.request(original);
}

axiosInstance.interceptors.response.use(
  (r) => r,
  async (error: unknown) => {
    if (!isAxiosError(error) || error.response?.status !== 401) {
      return Promise.reject(error);
    }

    const original = error.config as RequestWithTokenRetry | undefined;
    const privateSiteBlock = readResponseMarker(error.response?.headers);

    const retried = await tryRecoverSessionAndRetry(original);
    if (retried !== null) {
      return retried;
    }

    if (privateSiteBlock) {
      await clearSession();
      redirectToLoginIfPrivateSite(error);
      return Promise.reject(error);
    }

    if (!original) {
      await clearSession();
      return Promise.reject(error);
    }
    if (original._tokenRetry) {
      await clearSession();
      return Promise.reject(error);
    }
    if (!getStoredUser()) {
      await clearSession();
      return Promise.reject(error);
    }
    const refreshed = await getOrCreateRefreshPromise();
    if (!refreshed) {
      await clearSession();
      return Promise.reject(error);
    }
    original._tokenRetry = true;
    return axiosInstance.request(original);
  }
);

export default axiosInstance;
