import type React from "react";
import type { LucideIcon } from "lucide-react";
import type { Model } from "@/lib/api/client";
import type { UserProfileData } from "@/lib/services/userProfileService";
import type { Collection } from "@/lib/services/collectionsService";

/**
 * Root plugin definition. Register via loadPlugins() before the app mounts.
 * @public
 */
export interface PluginDefinition {
  /** Unique plugin identifier (e.g. "my-plugin") */
  id: string;
  /** Display name */
  name: string;
  /** Semantic version */
  version?: string;
  /** Called when the app initializes; receives auth context */
  onInit?: (context: PluginContext) => void | Promise<void>;
  /** Called when the app unmounts or plugin is unloaded */
  onDestroy?: () => void;
  /** All contributions this plugin makes */
  contributions: PluginContributions;
}

/** Context passed to onInit. */
export interface PluginContext {
  isAuthenticated: boolean;
  userId?: string;
  roles?: string[];
}

/** All contribution types a plugin can provide. */
export interface PluginContributions {
  navItems?: NavItemContribution[];
  userMenuItems?: UserMenuItemContribution[];
  adminNavItems?: AdminNavItemContribution[];
  moderationNavItems?: ModerationNavItemContribution[];
  settingsNavItems?: SettingsNavItemContribution[];
  layoutProviders?: LayoutProviderContribution[];
  dashboardSections?: DashboardSectionContribution[];
  modelDetailTabs?: ModelDetailTabContribution[];
  modelSidebarCards?: ModelSidebarCardContribution[];
  collectionDetailTabs?: CollectionDetailTabContribution[];
  profileTabs?: ProfileTabContribution[];
  routes?: PluginRouteContribution[];
}

/** Wraps the app layout; order controls render order. */
export interface LayoutProviderContribution {
  id: string;
  order: number;
  component: React.ComponentType<{ children: React.ReactNode }>;
}

/** Main navigation link (Dashboard, Upload, etc.). */
export interface NavItemContribution {
  id: string;
  label: string;
  href: string;
  icon: LucideIcon;
  order: number;
  position: "left" | "right";
  requiresAuth?: boolean;
  requiredRoles?: string[];
}

/** User dropdown menu item. Logout uses id "logout" and no href. */
export interface UserMenuItemContribution {
  id: string;
  label: string;
  href?: string;
  icon: React.ComponentType<{ className?: string }>;
  order: number;
  onClick?: () => void;
  variant?: "default" | "danger";
  requiresAuth?: boolean;
  requiredRoles?: string[];
}

/** Shared base for panel sidebar links (admin, moderation). group determines heading. */
export interface PanelNavItemContribution {
  id: string;
  label: string;
  group: string;
  icon: LucideIcon;
  path: string;
  badge?: string;
  requiredRoles?: string[];
}

/** Admin panel sidebar link. */
export interface AdminNavItemContribution extends PanelNavItemContribution {}

/** Moderation panel sidebar link. */
export interface ModerationNavItemContribution extends PanelNavItemContribution {}

/** Settings page sidebar link. group determines heading. */
export interface SettingsNavItemContribution {
  id: string;
  label: string;
  group: string;
  icon: LucideIcon;
  path: string;
  badge?: string;
  requiredRoles?: string[];
}

/** Dashboard section card. Rendered below the dashboard title. */
export interface DashboardSectionContribution {
  id: string;
  order: number;
  component: React.ComponentType;
  requiresAuth?: boolean;
}

/** Tab in model detail main area. Rendered below carousel when present. */
export interface ModelDetailTabContribution {
  id: string;
  label: string;
  icon: LucideIcon;
  order: number;
  component: React.ComponentType<{ model: Model }>;
  requiresAuth?: boolean;
  requiredRoles?: string[];
}

/** Card in model detail sidebar. Rendered after the default cards. */
export interface ModelSidebarCardContribution {
  id: string;
  order: number;
  component: React.ComponentType<{ model: Model; isOwner: boolean }>;
  requiresAuth?: boolean;
  requiredRoles?: string[];
}

/** Tab in collection detail main area. Rendered below the header. */
export interface CollectionDetailTabContribution {
  id: string;
  label: string;
  icon: LucideIcon;
  order: number;
  component: React.ComponentType<{ collection: Collection }>;
  requiresAuth?: boolean;
  requiredRoles?: string[];
}

/** Tab on user profile page (Models, Collections, etc.). */
export interface ProfileTabContribution {
  id: string;
  label: string;
  icon: LucideIcon;
  order: number;
  badge?: (profile: UserProfileData) => string;
  component: React.ComponentType<{
    userId: string;
    username: string;
    profile: UserProfileData;
  }>;
  requiresAuth?: boolean;
  requiredRoles?: string[];
}

/** Full-page route under /ext/{path}. Path is relative (e.g. "analytics" or "reports/dashboard"). */
export interface PluginRouteContribution {
  path: string;
  component: React.ComponentType;
  protected?: boolean;
}
