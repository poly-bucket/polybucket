"use client";

import { PanelLayout } from "@/components/layout/panel-layout";
import { useSettingsNavItems } from "@/lib/plugins";

export default function SettingsLayout({
  children,
}: {
  children: React.ReactNode;
}) {
  const items = useSettingsNavItems();

  return (
    <PanelLayout
      title="Settings"
      description="Manage your account, security, and preferences"
      navItems={items}
    >
      {children}
    </PanelLayout>
  );
}
