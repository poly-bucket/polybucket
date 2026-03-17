"use client";

import * as React from "react";
import { Switch } from "@/components/primitives/switch";
import { SettingsRow } from "./settings-row";
import { cn } from "@/lib/utils";

interface SettingsToggleProps {
  label: string;
  description?: string;
  checked: boolean;
  onCheckedChange: (checked: boolean) => void;
  disabled?: boolean;
  className?: string;
}

export function SettingsToggle({
  label,
  description,
  checked,
  onCheckedChange,
  disabled = false,
  className,
}: SettingsToggleProps) {
  return (
    <SettingsRow label={label} description={description} className={className}>
      <Switch
        checked={checked}
        onCheckedChange={onCheckedChange}
        disabled={disabled}
        className="data-[state=checked]:bg-primary"
      />
    </SettingsRow>
  );
}
