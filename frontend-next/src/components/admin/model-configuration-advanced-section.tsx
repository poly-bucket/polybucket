"use client";

import { ChevronDown, ChevronUp } from "lucide-react";
import { useState } from "react";
import { Button } from "@/components/primitives/button";
import { SettingsToggle } from "@/components/settings/settings-toggle";
import type { ModelConfigurationSettingsData } from "@/lib/api/client";

type ToggleDef = {
  key: keyof ModelConfigurationSettingsData;
  label: string;
  description?: string;
};

const ENFORCED_TOGGLES: ToggleDef[] = [
  { key: "allowModelRemixing", label: "Allow model remixing" },
  { key: "requireRemixAttribution", label: "Require remix attribution" },
  { key: "enableModelImport", label: "Enable model import (federation)" },
  { key: "enableModelAnalytics", label: "Enable model analytics (view counts)" },
  { key: "enableModelSharing", label: "Enable model sharing (copy link)" },
  { key: "enableModelReporting", label: "Enable model reporting" },
  { key: "requireReportDetails", label: "Require report details" },
  { key: "enableModelRejection", label: "Enable model rejection" },
  { key: "requireRejectionReason", label: "Require rejection reason" },
  { key: "enableModelDeletion", label: "Enable model deletion (owners)" },
];

interface ModelConfigurationAdvancedSectionProps {
  config: ModelConfigurationSettingsData;
  onChange: (updates: Partial<ModelConfigurationSettingsData>) => void;
}

export function ModelConfigurationAdvancedSection({
  config,
  onChange,
}: ModelConfigurationAdvancedSectionProps) {
  const [open, setOpen] = useState(false);

  return (
    <div className="space-y-4 border-t border-white/10 pt-6">
      <div className="flex items-center justify-between">
        <div>
          <p className="font-medium text-white">Advanced model policies</p>
          <p className="text-sm text-white/60">
            Remix, federation import, analytics, reports, and moderation gates
          </p>
          <p className="mt-1 text-xs text-white/40">
            Additional policies (embedding, backups, appeals, and similar) remain stored in the API
            until their features ship.
          </p>
        </div>
        <Button variant="ghost" size="sm" onClick={() => setOpen((o) => !o)}>
          {open ? (
            <>
              <ChevronUp className="h-4 w-4 mr-1" /> Collapse
            </>
          ) : (
            <>
              <ChevronDown className="h-4 w-4 mr-1" /> Expand
            </>
          )}
        </Button>
      </div>
      {open && (
        <div className="space-y-4">
          {ENFORCED_TOGGLES.map(({ key, label, description }) => (
            <SettingsToggle
              key={key}
              label={label}
              description={description}
              checked={Boolean(config[key])}
              onCheckedChange={(v) => onChange({ [key]: v } as Partial<ModelConfigurationSettingsData>)}
            />
          ))}
        </div>
      )}
    </div>
  );
}