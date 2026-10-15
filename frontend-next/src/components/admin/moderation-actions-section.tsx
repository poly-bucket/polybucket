"use client";

import { SettingsSection } from "@/components/settings/settings-section";

export function ModerationActionsSection() {
  return (
    <SettingsSection
      title="Moderation Actions"
      description="Manage models awaiting moderation"
    >
      <div className="rounded-lg border border-white/10 glass-bg p-8 text-center space-y-3">
        <p className="text-white/80">
          Workflow settings (auto-approve, upload moderation, and related flags) are
          configured in the Moderation Control Panel.
        </p>
        <a
          href="/moderation/settings"
          className="text-sm font-medium text-primary hover:underline block"
        >
          Open moderation settings
        </a>
        <a
          href="/moderation/models"
          className="text-sm font-medium text-primary hover:underline block"
        >
          Open model queue
        </a>
      </div>
    </SettingsSection>
  );
}
