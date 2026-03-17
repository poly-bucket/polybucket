"use client"

import * as React from "react"
import {
  Sidebar as BaseSidebar,
  SidebarContent,
  SidebarFooter,
  SidebarHeader,
  SidebarGroup,
  SidebarGroupContent,
  SidebarGroupLabel,
  SidebarMenu,
  SidebarMenuButton,
  SidebarMenuItem,
  SidebarProvider,
  SidebarTrigger,
  SidebarInset,
  useSidebar,
} from "@/components/ui/sidebar"
import { cn } from "@/lib/utils"
import { hoverEffects, type HoverEffect } from "@/lib/hover-effects"

export interface GlassSidebarProps
  extends Omit<React.ComponentProps<typeof BaseSidebar>, "variant"> {
  variant?: "glass" | "default"
  glow?: boolean
  hover?: HoverEffect
}

export function Sidebar({
  className,
  variant = "glass",
  glow = false,
  hover = "none",
  ...props
}: GlassSidebarProps) {
  const glassClasses =
    variant === "glass"
      ? cn(
          "[&_[data-sidebar=sidebar]]:!bg-transparent [&_[data-sidebar=sidebar]]:!border-sidebar-border",
          "[&_[data-sidebar=sidebar]]:glass-bg [&_[data-sidebar=sidebar]]:border",
          glow &&
            "[&_[data-sidebar=sidebar]]:shadow-lg [&_[data-sidebar=sidebar]]:shadow-purple-500/20",
          hoverEffects({ hover })
        )
      : ""

  return (
    <BaseSidebar className={cn(glassClasses, className)} {...props} />
  )
}

export {
  SidebarHeader,
  SidebarContent,
  SidebarFooter,
  SidebarGroup,
  SidebarGroupContent,
  SidebarGroupLabel,
  SidebarMenu,
  SidebarMenuButton,
  SidebarMenuItem,
  SidebarProvider,
  SidebarTrigger,
  SidebarInset,
  useSidebar,
}
