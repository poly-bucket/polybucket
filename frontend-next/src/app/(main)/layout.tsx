import { NavigationBar } from "@/components/layout/navigation-bar";
import { UserSettingsProvider } from "@/contexts/UserSettingsContext";

export default function MainLayout({
  children,
}: {
  children: React.ReactNode;
}) {
  return (
    <UserSettingsProvider>
      <div className="min-h-screen bg-gradient-to-br from-slate-900 via-slate-800 to-slate-900">
        <NavigationBar />
        <main className="pt-20">{children}</main>
      </div>
    </UserSettingsProvider>
  );
}
