"use client";

import {
  createContext,
  useContext,
  useEffect,
  useMemo,
  useState,
  type ReactNode,
} from "react";
import { fetchSiteAccessPolicy } from "@/lib/bff/fetchSiteAccessPolicy";
import { DEFAULT_SITE_NAME } from "@/lib/site/defaultSiteName";
const NAV_TITLE_LOADING_FALLBACK = "Polybucket";

type SiteBrandingContextValue = {
  siteName: string;
  isLoading: boolean;
};

const SiteBrandingContext = createContext<SiteBrandingContextValue | null>(null);

export function SiteBrandingProvider({ children }: { children: ReactNode }) {
  const [siteName, setSiteName] = useState(DEFAULT_SITE_NAME);
  const [isLoading, setIsLoading] = useState(true);

  useEffect(() => {
    let active = true;
    void fetchSiteAccessPolicy(window.location.origin).then((policy) => {
      if (!active) {
        return;
      }
      const name = policy.siteName?.trim() || DEFAULT_SITE_NAME;
      setSiteName(name);
      setIsLoading(false);
      document.title = `${name} | 3D models`;
    });
    return () => {
      active = false;
    };
  }, []);

  const value = useMemo(
    () => ({
      siteName: isLoading ? NAV_TITLE_LOADING_FALLBACK : siteName,
      isLoading,
    }),
    [siteName, isLoading]
  );

  return (
    <SiteBrandingContext.Provider value={value}>{children}</SiteBrandingContext.Provider>
  );
}

export function useSiteBranding(): SiteBrandingContextValue {
  const context = useContext(SiteBrandingContext);
  if (!context) {
    return { siteName: NAV_TITLE_LOADING_FALLBACK, isLoading: true };
  }
  return context;
}