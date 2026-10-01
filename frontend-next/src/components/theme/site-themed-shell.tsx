import { SiteThemeApplier } from "@/components/theme/site-theme-applier";

export function SiteThemedShell({ children }: { children: React.ReactNode }) {
  return (
    <>
      <SiteThemeApplier />
      <div className="site-themed-shell min-h-screen">{children}</div>
    </>
  );
}
