import { NavigationBar } from "@/components/layout/navigation-bar";
import { EmailVerificationBanner } from "@/components/auth/email-verification-banner";
import { UserSettingsProvider } from "@/contexts/UserSettingsContext";
import { SiteThemedShell } from "@/components/theme/site-themed-shell";

export default function MainLayout({
  children,
}: {
  children: React.ReactNode;
}) {
  return (
    <UserSettingsProvider>
      <SiteThemedShell>
        <NavigationBar />
        <main className="pt-20">
          <EmailVerificationBanner />
          {children}
        </main>
      </SiteThemedShell>
    </UserSettingsProvider>
  );
}
