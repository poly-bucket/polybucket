"use client";

import { useState } from "react";
import { Button } from "@/components/primitives/button";
import { EmailSettingsSection } from "@/components/admin/email-settings-section";
import type { EmailSettings } from "@/lib/services/emailService";

interface EmailSetupStepProps {
  onComplete: (data: Record<string, unknown>) => void;
  onBack: () => void;
  defaultTestRecipient?: string;
}

export default function EmailSetupStep({
  onComplete,
  onBack,
  defaultTestRecipient,
}: EmailSetupStepProps) {
  const [saved, setSaved] = useState<EmailSettings | null>(null);

  return (
    <div className="space-y-6">
      <div>
        <h3 className="text-lg font-medium text-white mb-2">Email (optional)</h3>
        <p className="text-white/70 text-sm">
          Email lets users verify their address and reset forgotten passwords. You can
          skip this and set it up later under Admin, System Settings. If your server
          sets the Email__* environment variables, those values appear here locked.
        </p>
      </div>

      <EmailSettingsSection
        title="Email delivery"
        description="Pick a provider preset or enter your SMTP server, save, then send a test email"
        showTemplatePreview={false}
        defaultTestRecipient={defaultTestRecipient}
        onSaved={setSaved}
      />

      <div className="flex justify-between pt-4">
        <Button
          type="button"
          variant="outline"
          onClick={onBack}
          className="border-white/20 text-white hover:bg-white/10"
        >
          Back
        </Button>
        <div className="flex gap-3">
          {!saved && (
            <Button
              type="button"
              variant="ghost"
              onClick={() => onComplete({ emailConfigured: false })}
              className="text-white/80"
            >
              Skip for now
            </Button>
          )}
          {saved && (
            <Button
              type="button"
              onClick={() => onComplete({ emailConfigured: saved.enabled })}
            >
              Continue
            </Button>
          )}
        </div>
      </div>
    </div>
  );
}
