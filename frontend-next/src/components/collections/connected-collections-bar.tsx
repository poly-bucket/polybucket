"use client";

import { useEffect, useState, useCallback } from "react";
import { useAuth } from "@/contexts/AuthContext";
import { collectionsService, type Collection } from "@/lib/services/collectionsService";
import { mapCollectionToCardData, type CollectionCardData } from "./collection-card";
import { CollectionsBar, SIDEBAR_STORAGE_KEY } from "./collections-bar";
import { fetchSiteAccessPolicy } from "@/lib/bff/fetchSiteAccessPolicy";
import { canShowCollectionsNav } from "@/lib/auth/featureAccess";

export function ConnectedCollectionsBar() {
  const { user, isAuthenticated } = useAuth();
  const [navVisible, setNavVisible] = useState(true);
  const [collections, setCollections] = useState<CollectionCardData[]>([]);
  const [loading, setLoading] = useState(true);
  const [isCollapsed, setIsCollapsed] = useState(() => {
    if (typeof window === "undefined") return false;
    return localStorage.getItem(SIDEBAR_STORAGE_KEY) === "true";
  });

  const loadCollections = useCallback(async () => {
    if (!user) {
      setCollections([]);
      setLoading(false);
      return;
    }
    try {
      setLoading(true);
      const favorites = await collectionsService.getFavoriteCollections();
      setCollections(
        favorites.map((c: Collection) => mapCollectionToCardData(c))
      );
    } catch {
      setCollections([]);
    } finally {
      setLoading(false);
    }
  }, [user]);

  useEffect(() => {
    if (typeof window === "undefined") {
      return;
    }
    void fetchSiteAccessPolicy(window.location.origin).then((policy) => {
      setNavVisible(canShowCollectionsNav(policy));
    });
  }, []);

  useEffect(() => {
    if (isAuthenticated) {
      loadCollections();
    } else {
      setCollections([]);
      setLoading(false);
    }
  }, [isAuthenticated, loadCollections]);

  const handleToggle = useCallback(() => {
    const next = !isCollapsed;
    setIsCollapsed(next);
    localStorage.setItem(SIDEBAR_STORAGE_KEY, String(next));
  }, [isCollapsed]);

  if (!isAuthenticated || !navVisible) return null;

  return (
    <CollectionsBar
      collections={collections}
      isCollapsed={isCollapsed}
      onToggle={handleToggle}
      loading={loading}
    />
  );
}
