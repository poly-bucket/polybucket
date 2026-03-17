"use client";

import { SettingsSection } from "@/components/settings/settings-section";

export function ModerationActionsSection() {
  return (
    <SettingsSection
      title="Moderation Actions"
      description="Manage models awaiting moderation"
    >
      <div className="rounded-lg border border-white/10 glass-bg p-8 text-center">
        <p className="text-white/80 mb-2">
          Moderation API is not yet implemented.
        </p>
        <p className="text-sm text-white/50">
          Bulk approve, reject, hide, and show actions for models awaiting
          moderation will be available when the backend supports them.
        </p>
      </div>
    </SettingsSection>
  );
}
