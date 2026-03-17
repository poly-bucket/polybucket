"use client";

import { useMemo } from "react";
import { useAuth } from "@/contexts/AuthContext";
import {
  getNavItems,
  getUserMenuItems,
  getAdminNavItems,
  getModerationNavItems,
  getSettingsNavItems,
  getDashboardSections,
  getModelDetailTabs,
  getModelSidebarCards,
  getCollectionDetailTabs,
  getProfileTabs,
  getPluginRoutes,
} from "./registry";
import type {
  NavItemContribution,
  UserMenuItemContribution,
  AdminNavItemContribution,
  ModerationNavItemContribution,
  SettingsNavItemContribution,
  DashboardSectionContribution,
  ModelDetailTabContribution,
  ModelSidebarCardContribution,
  CollectionDetailTabContribution,
  ProfileTabContribution,
  PluginRouteContribution,
} from "./types";

function passesAuthFilter<T extends { requiresAuth?: boolean; requiredRoles?: string[] }>(
  item: T,
  isAuthenticated: boolean,
  roles: string[] = []
): boolean {
  if (item.requiresAuth && !isAuthenticated) return false;
  if (
    item.requiredRoles?.length &&
    !item.requiredRoles.some((r) => roles.includes(r))
  ) {
    return false;
  }
  return true;
}

export function useNavItems(): NavItemContribution[] {
  const { isAuthenticated, user } = useAuth();
  const roles = user?.roles ?? [];

  return useMemo(() => {
    return getNavItems()
      .filter((item) => passesAuthFilter(item, isAuthenticated, roles))
      .sort((a, b) => a.order - b.order);
  }, [isAuthenticated, roles]);
}

export function useUserMenuItems(): UserMenuItemContribution[] {
  const { isAuthenticated, user } = useAuth();
  const roles = user?.roles ?? [];

  return useMemo(() => {
    return getUserMenuItems()
      .filter((item) => passesAuthFilter(item, isAuthenticated, roles))
      .sort((a, b) => a.order - b.order);
  }, [isAuthenticated, roles]);
}

export function useAdminNavItems(): AdminNavItemContribution[] {
  const { isAuthenticated, user } = useAuth();
  const roles = user?.roles ?? [];

  return useMemo(() => {
    return getAdminNavItems().filter((item) =>
      passesAuthFilter(item, isAuthenticated, roles)
    );
  }, [isAuthenticated, roles]);
}

export function useModerationNavItems(): ModerationNavItemContribution[] {
  const { isAuthenticated, user } = useAuth();
  const roles = user?.roles ?? [];

  return useMemo(() => {
    return getModerationNavItems().filter((item) =>
      passesAuthFilter(item, isAuthenticated, roles)
    );
  }, [isAuthenticated, roles]);
}

export function useSettingsNavItems(): SettingsNavItemContribution[] {
  const { isAuthenticated, user } = useAuth();
  const roles = user?.roles ?? [];

  return useMemo(() => {
    return getSettingsNavItems().filter((item) =>
      passesAuthFilter(item, isAuthenticated, roles)
    );
  }, [isAuthenticated, roles]);
}

export function getGroupedNavItems<T extends { group: string }>(
  items: T[]
): { group: string; items: T[] }[] {
  const groupMap = new Map<string, T[]>();
  const groupOrder: string[] = [];
  for (const item of items) {
    if (!groupMap.has(item.group)) {
      groupOrder.push(item.group);
    }
    const existing = groupMap.get(item.group) ?? [];
    existing.push(item);
    groupMap.set(item.group, existing);
  }
  return groupOrder.map((group) => ({
    group,
    items: groupMap.get(group) ?? [],
  }));
}

export function getSettingsGroups(
  items: SettingsNavItemContribution[]
): { group: string; items: SettingsNavItemContribution[] }[] {
  return getGroupedNavItems(items);
}

export function useDashboardSections(): DashboardSectionContribution[] {
  const { isAuthenticated, user } = useAuth();
  const roles = user?.roles ?? [];

  return useMemo(() => {
    return getDashboardSections()
      .filter((item) => passesAuthFilter(item, isAuthenticated, roles))
      .sort((a, b) => a.order - b.order);
  }, [isAuthenticated, roles]);
}

export function useModelDetailTabs(): ModelDetailTabContribution[] {
  const { isAuthenticated, user } = useAuth();
  const roles = user?.roles ?? [];

  return useMemo(() => {
    return getModelDetailTabs()
      .filter((item) => passesAuthFilter(item, isAuthenticated, roles))
      .sort((a, b) => a.order - b.order);
  }, [isAuthenticated, roles]);
}

export function useModelSidebarCards(): ModelSidebarCardContribution[] {
  const { isAuthenticated, user } = useAuth();
  const roles = user?.roles ?? [];

  return useMemo(() => {
    return getModelSidebarCards()
      .filter((item) => passesAuthFilter(item, isAuthenticated, roles))
      .sort((a, b) => a.order - b.order);
  }, [isAuthenticated, roles]);
}

export function useCollectionDetailTabs(): CollectionDetailTabContribution[] {
  const { isAuthenticated, user } = useAuth();
  const roles = user?.roles ?? [];

  return useMemo(() => {
    return getCollectionDetailTabs()
      .filter((item) => passesAuthFilter(item, isAuthenticated, roles))
      .sort((a, b) => a.order - b.order);
  }, [isAuthenticated, roles]);
}

export function useProfileTabs(): ProfileTabContribution[] {
  const { isAuthenticated, user } = useAuth();
  const roles = user?.roles ?? [];

  return useMemo(() => {
    return getProfileTabs()
      .filter((item) => passesAuthFilter(item, isAuthenticated, roles))
      .sort((a, b) => a.order - b.order);
  }, [isAuthenticated, roles]);
}

export function usePluginRoutes(): PluginRouteContribution[] {
  const { isAuthenticated, user } = useAuth();
  const roles = user?.roles ?? [];

  return useMemo(() => {
    return getPluginRoutes()
      .filter((item) => {
        if (item.protected !== false && !isAuthenticated) return false;
        return true;
      })
      .sort((a, b) => a.path.localeCompare(b.path));
  }, [isAuthenticated, roles]);
}
