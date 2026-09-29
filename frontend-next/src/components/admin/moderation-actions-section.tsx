"use client";

import { SettingsSection } from "@/components/settings/settings-section";

export function ModerationActionsSection() {
  return (
    <SettingsSection
      title="Moderation Actions"
      description="Manage models awaiting moderation"
    >
      <div className="rounded-lg border border-white/10 glass-bg p-8 text-center">
        <p className="text-white/80 mb-4">
          Approve or reject models from the moderation model queue.
        </p>
        <a
          href="/moderation/models"
          className="text-sm font-medium text-primary hover:underline"
        >
          Open model queue
        </a>
      </div>
    </SettingsSection>
  );
}
