import type { ThemeColorsDto } from "@/lib/api/client";

const STORAGE_KEY = "polybucket-site-theme-colors";

export type SiteThemeColors = {
  primary: string;
  primaryLight: string;
  primaryDark: string;
  secondary: string;
  secondaryLight: string;
  secondaryDark: string;
  accent: string;
  accentLight: string;
  accentDark: string;
  backgroundPrimary: string;
  backgroundSecondary: string;
  backgroundTertiary: string;
};

export function themeColorsFromDto(
  colors: ThemeColorsDto | undefined
): SiteThemeColors | null {
  if (!colors?.primary) return null;
  return {
    primary: colors.primary,
    primaryLight: colors.primaryLight ?? colors.primary,
    primaryDark: colors.primaryDark ?? colors.primary,
    secondary: colors.secondary ?? colors.primary,
    secondaryLight: colors.secondaryLight ?? colors.secondary ?? colors.primary,
    secondaryDark: colors.secondaryDark ?? colors.secondary ?? colors.primary,
    accent: colors.accent ?? colors.primary,
    accentLight: colors.accentLight ?? colors.accent ?? colors.primary,
    accentDark: colors.accentDark ?? colors.accent ?? colors.primary,
    backgroundPrimary: colors.backgroundPrimary ?? "#0f172a",
    backgroundSecondary: colors.backgroundSecondary ?? "#1e293b",
    backgroundTertiary: colors.backgroundTertiary ?? "#334155",
  };
}

export function extensibleConfigToSiteTheme(
  config: Record<string, unknown>
): SiteThemeColors | null {
  const str = (key: string, fallback: string) => {
    const v = config[key];
    return typeof v === "string" && v.length > 0 ? v : fallback;
  };
  if (typeof config.primaryColor !== "string") return null;
  return {
    primary: str("primaryColor", "#6366f1"),
    primaryLight: str("primaryLightColor", "#818cf8"),
    primaryDark: str("primaryDarkColor", "#4f46e5"),
    secondary: str("secondaryColor", "#8b5cf6"),
    secondaryLight: str("secondaryLightColor", "#a78bfa"),
    secondaryDark: str("secondaryDarkColor", "#7c3aed"),
    accent: str("accentColor", "#06b6d4"),
    accentLight: str("accentLightColor", "#22d3ee"),
    accentDark: str("accentDarkColor", "#0891b2"),
    backgroundPrimary: str("backgroundPrimaryColor", "#0f0f23"),
    backgroundSecondary: str("backgroundSecondaryColor", "#1a1a2e"),
    backgroundTertiary: str("backgroundTertiaryColor", "#16213e"),
  };
}

export function siteThemeToExtensibleConfig(
  colors: SiteThemeColors
): Record<string, string> {
  return {
    primaryColor: colors.primary,
    primaryLightColor: colors.primaryLight,
    primaryDarkColor: colors.primaryDark,
    secondaryColor: colors.secondary,
    secondaryLightColor: colors.secondaryLight,
    secondaryDarkColor: colors.secondaryDark,
    accentColor: colors.accent,
    accentLightColor: colors.accentLight,
    accentDarkColor: colors.accentDark,
    backgroundPrimaryColor: colors.backgroundPrimary,
    backgroundSecondaryColor: colors.backgroundSecondary,
    backgroundTertiaryColor: colors.backgroundTertiary,
  };
}

export function applySiteThemeColors(colors: SiteThemeColors): void {
  const root = document.documentElement;
  root.style.setProperty("--site-primary", colors.primary);
  root.style.setProperty("--site-primary-light", colors.primaryLight);
  root.style.setProperty("--site-primary-dark", colors.primaryDark);
  root.style.setProperty("--site-secondary", colors.secondary);
  root.style.setProperty("--site-accent", colors.accent);
  root.style.setProperty("--site-bg-primary", colors.backgroundPrimary);
  root.style.setProperty("--site-bg-secondary", colors.backgroundSecondary);
  root.style.setProperty("--site-bg-tertiary", colors.backgroundTertiary);
  root.style.setProperty(
    "--gradient",
    `linear-gradient(135deg, ${colors.secondary} 0%, ${colors.primary} 100%)`
  );
  root.style.setProperty("--primary", colors.primary);
  root.style.setProperty("--ring", colors.primary);
  localStorage.setItem(STORAGE_KEY, JSON.stringify(colors));
}

export function loadStoredSiteTheme(): SiteThemeColors | null {
  try {
    const raw = localStorage.getItem(STORAGE_KEY);
    if (!raw) return null;
    return JSON.parse(raw) as SiteThemeColors;
  } catch {
    return null;
  }
}

export function clearStoredSiteTheme(): void {
  localStorage.removeItem(STORAGE_KEY);
  const root = document.documentElement;
  [
    "--site-primary",
    "--site-primary-light",
    "--site-primary-dark",
    "--site-secondary",
    "--site-accent",
    "--site-bg-primary",
    "--site-bg-secondary",
    "--site-bg-tertiary",
    "--gradient",
    "--primary",
    "--ring",
  ].forEach((prop) => root.style.removeProperty(prop));
}
