"use client"

import Link from "next/link"
import { usePathname } from "next/navigation"
import type { LucideIcon } from "lucide-react"
import {
  Sidebar,
  SidebarContent,
  SidebarGroup,
  SidebarGroupContent,
  SidebarGroupLabel,
  SidebarHeader,
  SidebarInset,
  SidebarMenu,
  SidebarMenuButton,
  SidebarMenuItem,
  SidebarProvider,
  SidebarTrigger,
} from "@/components/ui/glass/sidebar"
import { cn } from "@/lib/utils"

export interface PanelNavItem {
  id: string
  label: string
  group: string
  icon: LucideIcon
  path: string
  badge?: string
}

export interface PanelLayoutProps {
  title: string
  description?: string
  navItems: PanelNavItem[]
  children: React.ReactNode
}

function getGroupedNavItems<T extends { group: string }>(
  items: T[]
): { group: string; items: T[] }[] {
  const groupMap = new Map<string, T[]>()
  const groupOrder: string[] = []
  for (const item of items) {
    if (!groupMap.has(item.group)) {
      groupOrder.push(item.group)
    }
    const existing = groupMap.get(item.group) ?? []
    existing.push(item)
    groupMap.set(item.group, existing)
  }
  return groupOrder.map((group) => ({ group, items: groupMap.get(group) ?? [] }))
}

export function PanelLayout({
  title,
  description,
  navItems,
  children,
}: PanelLayoutProps) {
  const pathname = usePathname()
  const groups = getGroupedNavItems(navItems)

  return (
    <SidebarProvider>
      <Sidebar variant="glass" className="border-r border-white/20">
        <SidebarHeader className="border-b border-white/10 p-4">
          <div className="flex items-center gap-2">
            <SidebarTrigger className="text-white/70 hover:text-white" />
            <div>
              <h1 className="text-lg font-semibold text-white">{title}</h1>
              {description && (
                <p className="text-xs text-white/60">{description}</p>
              )}
            </div>
          </div>
        </SidebarHeader>
        <SidebarContent>
          {groups.map(({ group, items }) => (
            <SidebarGroup key={group}>
              <SidebarGroupLabel className="text-xs font-semibold uppercase tracking-wider text-white/50 px-3 pt-4 pb-2">
                {group}
              </SidebarGroupLabel>
              <SidebarGroupContent>
                <SidebarMenu>
                  {items.map((item) => {
                    const Icon = item.icon
                    const isActive = pathname === item.path
                    return (
                      <SidebarMenuItem key={item.id}>
                        <SidebarMenuButton asChild isActive={isActive}>
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
                            <span className="truncate">{item.label}</span>
                            {item.badge && (
                              <span className="ml-auto rounded bg-primary/20 px-1.5 py-0.5 text-xs">
                                {item.badge}
                              </span>
                            )}
                          </Link>
                        </SidebarMenuButton>
                      </SidebarMenuItem>
                    )
                  })}
                </SidebarMenu>
              </SidebarGroupContent>
            </SidebarGroup>
          ))}
        </SidebarContent>
      </Sidebar>
      <SidebarInset>
        <header className="flex h-14 shrink-0 items-center gap-2 border-b border-white/10 px-4 md:hidden">
          <SidebarTrigger className="text-white/70 hover:text-white" />
          <span className="text-sm font-medium text-white">{title}</span>
        </header>
        <main className="flex-1 overflow-auto p-4 md:p-6">{children}</main>
      </SidebarInset>
    </SidebarProvider>
  )
}
