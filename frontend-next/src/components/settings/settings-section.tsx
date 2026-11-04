"use client";

import * as React from "react";
import { Card, CardContent, CardHeader, CardTitle, CardDescription } from "@/components/primitives/card";
import { cn } from "@/lib/utils";

interface SettingsSectionProps {
  title: string;
  description?: string;
  children: React.ReactNode;
  className?: string;
}

export function SettingsSection({
  title,
  description,
  children,
  className,
}: SettingsSectionProps) {
  return (
    <Card
      variant="glass"
      className={cn("min-w-0 border-white/20", className)}
    >
      <CardHeader>
        <CardTitle className="text-white">{title}</CardTitle>
        {description && (
          <CardDescription className="text-white/70">{description}</CardDescription>
        )}
      </CardHeader>
      <CardContent className="min-w-0">{children}</CardContent>
    </Card>
  );
}
