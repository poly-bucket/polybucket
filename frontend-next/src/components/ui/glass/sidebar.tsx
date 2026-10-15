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
  SidebarMenuButton as BaseSidebarMenuButton,
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

const glassSidebarMenuButtonClassName =
  "text-white/70 hover:bg-white/10 hover:text-white active:bg-white/10 active:text-white data-[active=true]:bg-white/20 data-[active=true]:text-white data-[active=true]:font-medium"

function SidebarMenuButton({
  className,
  ...props
}: React.ComponentProps<typeof BaseSidebarMenuButton>) {
  return (
    <BaseSidebarMenuButton
      className={cn(glassSidebarMenuButtonClassName, className)}
      {...props}
    />
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
