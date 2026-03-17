"use client";

import Link from "next/link";
import { usePathname } from "next/navigation";
import { useSettingsNavItems, getSettingsGroups } from "@/lib/plugins";
import { cn } from "@/lib/utils";

export default function SettingsLayout({
  children,
}: {
  children: React.ReactNode;
}) {
  const pathname = usePathname();
  const items = useSettingsNavItems();
  const groups = getSettingsGroups(items);

  return (
    <div className="mx-auto max-w-7xl px-4 py-8 sm:px-6 lg:px-8">
      <h1 className="mb-8 text-2xl font-semibold text-white">Settings</h1>
      <div className="flex flex-col gap-8 lg:flex-row">
        <nav
          className="lg:w-56 flex-shrink-0"
          aria-label="Settings navigation"
        >
          <div className="space-y-6">
            {groups.map(({ group, items }) => (
              <div key={group}>
                <h2 className="mb-2 text-xs font-semibold uppercase tracking-wider text-white/50">
                  {group}
                </h2>
                <ul className="space-y-1">
                  {items.map((item) => {
                    const isActive = pathname === item.path;
                    const Icon = item.icon;
                    return (
                      <li key={item.id}>
                        <Link
                          href={item.path}
                          className={cn(
                            "flex items-center gap-2 rounded-md px-3 py-2 text-sm transition-colors",
                            isActive
                              ? "bg-white/20 text-white"
                              : "text-white/70 hover:bg-white/10 hover:text-white"
                          )}
                        >
                          <Icon className="h-4 w-4 shrink-0" />
                          {item.label}
                          {item.badge && (
                            <span className="ml-auto rounded bg-primary/20 px-1.5 py-0.5 text-xs">
                              {item.badge}
                            </span>
                          )}
                        </Link>
                      </li>
                    );
                  })}
                </ul>
              </div>
            ))}
          </div>
        </nav>
        <main className="min-w-0 flex-1">{children}</main>
      </div>
    </div>
  );
}
