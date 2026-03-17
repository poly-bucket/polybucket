"use client"

import * as React from "react"

import { cn } from "@/lib/utils"
import { hoverEffects, type HoverEffect } from "@/lib/hover-effects"

export interface InputProps extends React.ComponentProps<"input"> {
  variant?: "default" | "glass"
  icon?: React.ReactNode
  error?: boolean
  hover?: HoverEffect
}

const Input = React.forwardRef<HTMLInputElement, InputProps>(
  (
    {
      className,
      variant = "glass",
      icon,
      error,
      hover = "none",
      type,
      ...props
    },
    ref
  ) => {
    return (
      <div className="relative">
        {icon && (
          <div className="absolute left-3 top-1/2 z-10 -translate-y-1/2 pointer-events-none text-muted-foreground">
            {icon}
          </div>
        )}
        <input
          ref={ref}
          type={type}
          data-slot="input"
          className={cn(
            "file:text-foreground placeholder:text-muted-foreground selection:bg-primary selection:text-primary-foreground dark:bg-input/30 border-input h-9 w-full min-w-0 rounded-md border bg-transparent px-3 py-1 text-base shadow-xs outline-none file:inline-flex file:h-7 file:border-0 file:bg-transparent file:text-sm file:font-medium disabled:pointer-events-none disabled:cursor-not-allowed disabled:opacity-50 md:text-sm",
            "focus-visible:border-ring focus-visible:ring-ring/50 focus-visible:ring-[3px]",
            "aria-invalid:ring-destructive/20 dark:aria-invalid:ring-destructive/40 aria-invalid:border-destructive",
            variant === "glass" && "glass-bg border-white/20",
            "relative overflow-hidden",
            icon && "pl-10",
            error && "border-destructive focus-visible:ring-destructive",
            "transition-all duration-200 focus-visible:scale-[1.02]",
            hoverEffects({ hover }),
            className
          )}
          {...props}
        />
      </div>
    )
  }
)
Input.displayName = "Input"

export { Input }
