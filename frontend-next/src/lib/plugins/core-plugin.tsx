"use client";

import React from "react";
import { useSearchParams, useRouter } from "next/navigation";
import {
  Home,
  Plus,
  User,
  Settings,
  Box,
  LogOut,
  Bell,
  Lock,
  FolderOpen,
  Upload,
  LayoutGrid,
  AlertTriangle,
  Shield,
  LayoutDashboard,
  Users,
  ShieldCheck,
  Flag,
  UserMinus,
  Clock,
  Globe,
  Palette,
  Puzzle,
} from "lucide-react";
import { Card, CardContent, CardHeader, CardTitle } from "@/components/primitives/card";
import { useAuth } from "@/contexts/AuthContext";
import { RecentModelsSection } from "@/components/dashboard/recent-models-section";
import { ProfileModelsTab } from "@/components/profile/profile-models-tab";
import { ProfileCollectionsTab } from "@/components/profile/profile-collections-tab";
import { AddToCollectionCard } from "@/components/collections/add-to-collection-card";
import { CollectionModelsTab } from "@/components/collections/collection-models-tab";
import type { UserProfileData } from "@/lib/services/userProfileService";
import type { PluginDefinition } from "./types";

function DashboardWelcomeSection(): React.ReactElement {
  const { user, isAuthenticated } = useAuth();
  return (
    <Card variant="glass" className="border-white/20">
      <CardHeader>
        <CardTitle className="text-white">
          {isAuthenticated
            ? `Welcome back, ${user?.username ?? "User"}`
            : "Welcome to Polybucket"}
        </CardTitle>
      </CardHeader>
      <CardContent>
        <p className="text-white/80">
          {isAuthenticated
            ? "Your dashboard is ready. More features will be added here soon."
            : "Sign in to access your models, collections, and more."}
        </p>
      </CardContent>
    </Card>
  );
}

function ProfileModelsTabWrapper({
  username,
}: {
  userId: string;
  username: string;
  profile: UserProfileData;
}): React.ReactElement {
  const searchParams = useSearchParams();
  const router = useRouter();
  const page = Math.max(1, parseInt(searchParams.get("page") ?? "1", 10) || 1);
  const setPage = React.useCallback(
    (p: number) => {
      const next = new URLSearchParams(searchParams.toString());
      next.set("page", String(p));
      router.replace(`?${next.toString()}`, { scroll: false });
    },
    [searchParams, router]
  );
  return (
    <ProfileModelsTab
      username={username}
      profileUsername={username}
      page={page}
      onPageChange={setPage}
    />
  );
}

function ProfileCollectionsTabWrapper({
  userId,
}: {
  userId: string;
  username: string;
  profile: UserProfileData;
}): React.ReactElement {
  const searchParams = useSearchParams();
  const router = useRouter();
  const page = Math.max(1, parseInt(searchParams.get("page") ?? "1", 10) || 1);
  const q = searchParams.get("q") ?? "";
  const setPage = React.useCallback(
    (p: number) => {
      const next = new URLSearchParams(searchParams.toString());
      next.set("page", String(p));
      router.replace(`?${next.toString()}`, { scroll: false });
    },
    [searchParams, router]
  );
  const setQ = React.useCallback(
    (s: string) => {
      const next = new URLSearchParams(searchParams.toString());
      next.set("q", s);
      next.set("page", "1");
      router.replace(`?${next.toString()}`, { scroll: false });
    },
    [searchParams, router]
  );
  return (
    <ProfileCollectionsTab
      userId={userId}
      page={page}
      q={q}
      onPageChange={setPage}
      onSearchChange={setQ}
    />
  );
}

export const corePlugin: PluginDefinition = {
  id: "core",
  name: "Core",
  contributions: {
    navItems: [
      {
        id: "dashboard",
        label: "Dashboard",
        href: "/dashboard",
        icon: Home,
        order: 0,
        position: "left",
      },
      {
        id: "upload-model",
        label: "Upload Model",
        href: "/models/upload",
        icon: Plus,
        order: 10,
        position: "right",
        requiresAuth: true,
      },
    ],
    userMenuItems: [
      {
        id: "profile",
        label: "My Profile",
        href: "/profile",
        icon: User,
        order: 0,
        requiresAuth: true,
      },
      {
        id: "settings",
        label: "Settings",
        href: "/settings",
        icon: Settings,
        order: 10,
        requiresAuth: true,
      },
      {
        id: "my-models",
        label: "My Models",
        href: "/my-models",
        icon: Box,
        order: 20,
        requiresAuth: true,
      },
      {
        id: "my-collections",
        label: "My Collections",
        href: "/collections/mine",
        icon: FolderOpen,
        order: 25,
        requiresAuth: true,
      },
      {
        id: "moderation",
        label: "Moderation",
        href: "/moderation",
        icon: Shield,
        order: 40,
        requiresAuth: true,
        requiredRoles: ["Admin", "Moderator"],
      },
      {
        id: "admin",
        label: "Admin Panel",
        href: "/admin",
        icon: ShieldCheck,
        order: 60,
        requiresAuth: true,
        requiredRoles: ["Admin"],
      },
      {
        id: "logout",
        label: "Logout",
        icon: LogOut,
        order: 100,
        variant: "danger",
        requiresAuth: true,
      },
    ],
    settingsNavItems: [
      { id: "profile", label: "Profile", group: "Account", icon: User, path: "/settings/profile" },
      { id: "privacy", label: "Privacy", group: "Account", icon: Shield, path: "/settings/privacy" },
      { id: "notifications", label: "Notifications", group: "Account", icon: Bell, path: "/settings/notifications" },
      { id: "security", label: "Password & 2FA", group: "Security", icon: Lock, path: "/settings/security" },
      { id: "collections", label: "Collections", group: "Content", icon: FolderOpen, path: "/settings/collections" },
      { id: "uploads", label: "Model Uploads", group: "Content", icon: Upload, path: "/settings/uploads" },
      { id: "layout-preferences", label: "Layout Preferences", group: "Display", icon: LayoutGrid, path: "/settings/layout-preferences" },
      { id: "danger-zone", label: "Danger Zone", group: "Danger Zone", icon: AlertTriangle, path: "/settings/danger-zone" },
    ],
    adminNavItems: [
      { id: "dashboard", label: "Dashboard", group: "Overview", icon: LayoutDashboard, path: "/admin/dashboard", requiredRoles: ["Admin"] },
      { id: "users", label: "Users", group: "Management", icon: Users, path: "/admin/users", requiredRoles: ["Admin"] },
      { id: "roles", label: "Roles & Permissions", group: "Management", icon: Shield, path: "/admin/roles", requiredRoles: ["Admin"] },
      { id: "models", label: "Models", group: "Content", icon: Box, path: "/admin/models", requiredRoles: ["Admin"] },
      { id: "system", label: "System", group: "Configuration", icon: Settings, path: "/admin/system", requiredRoles: ["Admin"] },
      { id: "auth", label: "Authentication", group: "Configuration", icon: Lock, path: "/admin/auth", requiredRoles: ["Admin"] },
      { id: "federation", label: "Federation", group: "Configuration", icon: Globe, path: "/admin/federation", requiredRoles: ["Admin"] },
      { id: "theme", label: "Theme", group: "Appearance", icon: Palette, path: "/admin/theme", requiredRoles: ["Admin"] },
      { id: "plugins", label: "Plugins", group: "Extensions", icon: Puzzle, path: "/admin/plugins", requiredRoles: ["Admin"] },
    ],
    moderationNavItems: [
      { id: "reports", label: "Reports", group: "Content", icon: Flag, path: "/moderation/reports", requiredRoles: ["Admin", "Moderator"] },
      { id: "banned-users", label: "Banned Users", group: "Content", icon: UserMinus, path: "/moderation/banned-users", requiredRoles: ["Admin", "Moderator"] },
      { id: "audit-logs", label: "Audit Logs", group: "Content", icon: Clock, path: "/moderation/audit-logs", requiredRoles: ["Admin", "Moderator"] },
    ],
    dashboardSections: [
      { id: "welcome", order: 0, component: DashboardWelcomeSection },
      { id: "recent-models", order: 10, component: RecentModelsSection, requiresAuth: true },
    ],
    collectionDetailTabs: [
      {
        id: "models",
        label: "Models",
        icon: LayoutGrid,
        order: 0,
        component: CollectionModelsTab,
      },
    ],
    modelSidebarCards: [
      {
        id: "add-to-collection",
        order: 10,
        component: AddToCollectionCard,
        requiresAuth: true,
      },
    ],
    profileTabs: [
      {
        id: "models",
        label: "Models",
        icon: Box,
        order: 0,
        badge: (p) => String(p.totalModels ?? 0),
        component: ProfileModelsTabWrapper,
      },
      {
        id: "collections",
        label: "Collections",
        icon: FolderOpen,
        order: 10,
        badge: (p) => String(p.totalCollections ?? 0),
        component: ProfileCollectionsTabWrapper,
      },
    ],
  },
};
