import type { PluginDefinition, PluginContributions } from "./types";

const plugins: PluginDefinition[] = [];
let frozen = false;

export function loadPlugins(defs: PluginDefinition[]): void {
  if (frozen) throw new Error("Plugins already frozen");
  plugins.push(...defs);
}

export function freezePlugins(): void {
  frozen = true;
}

export function getPlugins(): readonly PluginDefinition[] {
  return plugins;
}

function collectContributions<K extends keyof PluginContributions>(
  key: K,
): NonNullable<PluginContributions[K]> {
  return plugins.flatMap(
    (p) => (p.contributions[key] ?? []) as unknown
  ) as NonNullable<PluginContributions[K]>;
}

export function getNavItems() {
  return collectContributions("navItems");
}
export function getUserMenuItems() {
  return collectContributions("userMenuItems");
}
export function getAdminNavItems() {
  return collectContributions("adminNavItems");
}
export function getModerationNavItems() {
  return collectContributions("moderationNavItems");
}
export function getSettingsNavItems() {
  return collectContributions("settingsNavItems");
}
export function getLayoutProviders() {
  return collectContributions("layoutProviders");
}
export function getDashboardSections() {
  return collectContributions("dashboardSections");
}
export function getModelDetailTabs() {
  return collectContributions("modelDetailTabs");
}
export function getModelSidebarCards() {
  return collectContributions("modelSidebarCards");
}
export function getCollectionDetailTabs() {
  return collectContributions("collectionDetailTabs");
}
export function getProfileTabs() {
  return collectContributions("profileTabs");
}
export function getPluginRoutes() {
  return collectContributions("routes");
}
