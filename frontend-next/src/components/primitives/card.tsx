"use client"

import * as React from "react"

import { cn } from "@/lib/utils"
import { hoverEffects, type HoverEffect } from "@/lib/hover-effects"

const cardVariants = {
  default: "bg-card border",
  glass: "glass-bg border border-white/10",
  frosted: "glass-frosted border border-white/20",
  fluted: "glass-fluted border border-white/10",
  crystal: "glass-crystal border border-white/20",
}

export interface CardProps extends React.ComponentProps<"div"> {
  variant?: keyof typeof cardVariants
  gradient?: boolean
  animated?: boolean
  hover?: HoverEffect
}

const Card = React.forwardRef<HTMLDivElement, CardProps>(
  (
    {
      className,
      variant = "glass",
      gradient = false,
      animated = false,
      hover = "none",
      children,
      ...props
    },
    ref
  ) => {
    return (
      <div
        ref={ref}
        data-slot="card"
        data-variant={variant}
        className={cn(
          "text-card-foreground relative flex flex-col gap-6 overflow-hidden rounded-xl py-6 shadow-sm",
          cardVariants[variant],
          gradient &&
            "bg-gradient-to-br from-purple-500/10 via-blue-500/10 to-pink-500/10",
          animated &&
            "transition-all duration-300 hover:scale-[1.02] hover:shadow-[var(--glass-shadow-lg)]",
          hoverEffects({ hover }),
          className
        )}
        {...props}
      >
        {children}
      </div>
    )
  }
)
Card.displayName = "Card"

function CardHeader({ className, ...props }: React.ComponentProps<"div">) {
  return (
    <div
      data-slot="card-header"
      className={cn(
        "@container/card-header grid auto-rows-min grid-rows-[auto_auto] items-start gap-2 px-6 has-data-[slot=card-action]:grid-cols-[1fr_auto] [.border-b]:pb-6",
        className
      )}
      {...props}
    />
  )
}

function CardTitle({ className, ...props }: React.ComponentProps<"div">) {
  return (
    <div
      data-slot="card-title"
      className={cn("leading-none font-semibold", className)}
      {...props}
    />
  )
}

function CardDescription({ className, ...props }: React.ComponentProps<"div">) {
  return (
    <div
      data-slot="card-description"
      className={cn("text-muted-foreground text-sm", className)}
      {...props}
    />
  )
}

function CardAction({ className, ...props }: React.ComponentProps<"div">) {
  return (
    <div
      data-slot="card-action"
      className={cn(
        "col-start-2 row-span-2 row-start-1 self-start justify-self-end",
        className
      )}
      {...props}
    />
  )
}

function CardContent({ className, ...props }: React.ComponentProps<"div">) {
  return (
    <div
      data-slot="card-content"
      className={cn("px-6", className)}
      {...props}
    />
  )
}

function CardFooter({ className, ...props }: React.ComponentProps<"div">) {
  return (
    <div
      data-slot="card-footer"
      className={cn("flex items-center px-6 [.border-t]:pt-6", className)}
      {...props}
    />
  )
}

export {
  Card,
  CardHeader,
  CardFooter,
  CardTitle,
  CardAction,
  CardDescription,
  CardContent,
}
