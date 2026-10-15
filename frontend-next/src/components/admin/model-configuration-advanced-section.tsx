"use client";

import { ChevronDown, ChevronUp } from "lucide-react";
import { useState } from "react";
import { Button } from "@/components/primitives/button";
import { Input } from "@/components/primitives/input";
import { SettingsToggle } from "@/components/settings/settings-toggle";
import { SettingsField } from "@/components/settings/settings-field";
import type { ModelConfigurationSettingsData } from "@/lib/api/client";

type ToggleDef = {
  key: keyof ModelConfigurationSettingsData;
  label: string;
  description?: string;
};

const ADVANCED_TOGGLES: ToggleDef[] = [
  { key: "allowModelRemixing", label: "Allow model remixing" },
  { key: "requireRemixAttribution", label: "Require remix attribution" },
  { key: "enableModelSharing", label: "Enable model sharing" },
  { key: "enableModelEmbedding", label: "Enable model embedding" },
  { key: "requireModelPreview", label: "Require model preview" },
  { key: "autoGenerateModelPreviews", label: "Auto-generate model previews" },
  { key: "enableModelAnalytics", label: "Enable model analytics" },
  { key: "requireUserAgreement", label: "Require user agreement" },
  { key: "allowCustomLicenses", label: "Allow custom licenses" },
  { key: "enableModelExport", label: "Enable model export" },
  { key: "enableModelImport", label: "Enable model import" },
  { key: "requireModelValidation", label: "Require model validation" },
  { key: "enableModelBackup", label: "Enable model backup" },
  { key: "enableModelArchiving", label: "Enable model archiving" },
  { key: "enableAutoModeration", label: "Enable auto moderation" },
  { key: "requireContentRating", label: "Require content rating" },
  { key: "enableModelFlagging", label: "Enable model flagging" },
  { key: "requireFlagReason", label: "Require flag reason" },
  { key: "enableModelReporting", label: "Enable model reporting" },
  { key: "requireReportDetails", label: "Require report details" },
  { key: "enableModelBlocking", label: "Enable model blocking" },
  { key: "requireBlockReason", label: "Require block reason" },
  { key: "enableModelWhitelisting", label: "Enable model whitelisting" },
  { key: "enableModelBlacklisting", label: "Enable model blacklisting" },
  { key: "requireModelApproval", label: "Require model approval" },
  { key: "enableModelRejection", label: "Enable model rejection" },
  { key: "requireRejectionReason", label: "Require rejection reason" },
  { key: "enableModelAppeals", label: "Enable model appeals" },
  { key: "requireAppealDetails", label: "Require appeal details" },
  { key: "enableModelLocking", label: "Enable model locking" },
  { key: "requireLockReason", label: "Require lock reason" },
  { key: "enableModelUnlocking", label: "Enable model unlocking" },
  { key: "requireUnlockApproval", label: "Require unlock approval" },
  { key: "enableModelDeletion", label: "Enable model deletion" },
  { key: "requireDeletionApproval", label: "Require deletion approval" },
  { key: "requireDeletionReason", label: "Require deletion reason" },
  { key: "enableModelRestoration", label: "Enable model restoration" },
  { key: "requireRestorationApproval", label: "Require restoration approval" },
];

interface ModelConfigurationAdvancedSectionProps {
  config: ModelConfigurationSettingsData;
  onChange: (updates: Partial<ModelConfigurationSettingsData>) => void;
  showAdvancedHint?: boolean;
}

export function ModelConfigurationAdvancedSection({
  config,
  onChange,
  showAdvancedHint,
}: ModelConfigurationAdvancedSectionProps) {
  const [open, setOpen] = useState(false);

  return (
    <div className="space-y-4 border-t border-white/10 pt-6">
      <div className="flex items-center justify-between">
        <div>
          <p className="font-medium text-white">Advanced model policies</p>
          <p className="text-sm text-white/60">
            Remix, legal, backup, and extended moderation workflow flags
          </p>
          {showAdvancedHint && (
            <p className="mt-1 text-xs text-white/40">
              Site “show advanced options” is enabled in System settings.
            </p>
          )}
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
          <SettingsField
            label="User agreement text"
            description="Shown when users must accept your upload agreement"
          >
            <Input
              variant="glass"
              value={config.userAgreementText ?? ""}
              onChange={(e) => onChange({ userAgreementText: e.target.value })}
              className="text-white"
            />
          </SettingsField>
          <SettingsField label="Model backup retention (days)">
            <Input
              variant="glass"
              type="number"
              value={config.modelBackupRetentionDays ?? ""}
              onChange={(e) =>
                onChange({
                  modelBackupRetentionDays: parseInt(e.target.value, 10) || undefined,
                })
              }
              className="text-white"
            />
          </SettingsField>
          <SettingsField label="Archive threshold (days)">
            <Input
              variant="glass"
              type="number"
              value={config.modelArchiveThresholdDays ?? ""}
              onChange={(e) =>
                onChange({
                  modelArchiveThresholdDays: parseInt(e.target.value, 10) || undefined,
                })
              }
              className="text-white"
            />
          </SettingsField>
          {ADVANCED_TOGGLES.map(({ key, label, description }) => (
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
