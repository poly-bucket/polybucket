"use client";

import { useState, useEffect, useCallback } from "react";
import { ExternalLink, RefreshCw } from "lucide-react";
import Link from "next/link";
import { SettingsSection } from "@/components/settings/settings-section";
import { SettingsToggle } from "@/components/settings/settings-toggle";
import { SettingsField } from "@/components/settings/settings-field";
import { SettingsFooter } from "@/components/settings/settings-footer";
import { Button } from "@/components/primitives/button";
import { Input } from "@/components/primitives/input";
import { EmailSettingsSection } from "@/components/admin/email-settings-section";
import { EmailOutboxSection } from "@/components/admin/email-outbox-section";
import {
  getTokenSettings,
  updateTokenSettings,
  getSetupStatus,
} from "@/lib/services/adminService";
import type { TokenSettings } from "@/lib/api/client";

export function SystemTab() {
  const [tokenSettings, setTokenSettings] = useState<TokenSettings | null>(null);
  const [setupStatus, setSetupStatus] = useState<{
    isFirstTimeSetup?: boolean;
    completedSteps?: number;
    totalSteps?: number;
  } | null>(null);
  const [loading, setLoading] = useState(true);
  const [savingToken, setSavingToken] = useState(false);
  const [tokenDirty, setTokenDirty] = useState(false);
  const [error, setError] = useState<string | null>(null);

  const fetchData = useCallback(async () => {
    try {
      setLoading(true);
      setError(null);
      const [tokenRes, statusRes] = await Promise.all([
        getTokenSettings(),
        getSetupStatus(),
      ]);
      setTokenSettings(tokenRes ?? null);
      setSetupStatus(statusRes ?? null);
    } catch {
      setError("Failed to load system settings");
    } finally {
      setLoading(false);
    }
  }, []);

  useEffect(() => {
    fetchData();
  }, [fetchData]);

  const handleTokenChange = (updates: Partial<TokenSettings>) => {
    setTokenSettings((prev) =>
      prev ? ({ ...prev, ...updates } as TokenSettings) : null
    );
    setTokenDirty(true);
  };

  const saveToken = async () => {
    if (!tokenSettings) return;
    setSavingToken(true);
    try {
      await updateTokenSettings(tokenSettings);
      setTokenDirty(false);
    } catch {
      setError("Failed to save token settings");
    } finally {
      setSavingToken(false);
    }
  };

  if (loading) {
    return (
      <div className="space-y-6">
        <h2 className="text-2xl font-bold text-white">System Settings</h2>
        <div className="text-center text-white/60 py-12">Loading settings...</div>
      </div>
    );
  }

  return (
    <div className="space-y-6">
      <h2 className="text-2xl font-bold text-white">System Settings</h2>

      {error && (
        <div className="rounded-lg border border-red-500/50 bg-red-500/10 px-4 py-3 text-red-400">
          {error}
        </div>
      )}

      <SettingsSection title="Setup Status" description="First-time setup progress">
        <div className="space-y-4">
          <div className="flex items-center justify-between rounded-lg bg-white/5 p-4">
            <div>
              <p className="font-medium text-white">Setup status</p>
              <p className="text-sm text-white/60">
                {setupStatus?.isFirstTimeSetup
                  ? "Setup incomplete"
                  : "Setup completed"}
              </p>
            </div>
            <div className="text-right">
              <p className="font-medium text-white">
                {setupStatus?.completedSteps ?? 0}/{setupStatus?.totalSteps ?? 0}
              </p>
              <p className="text-sm text-white/60">Steps completed</p>
            </div>
          </div>
          <div className="flex gap-3">
            <Link href="/setup">
              <Button variant="outline" className="flex items-center gap-2">
                <ExternalLink className="h-4 w-4" />
                {setupStatus?.isFirstTimeSetup
                  ? "Continue Setup"
                  : "Access Setup"}
              </Button>
            </Link>
            <Button variant="ghost" onClick={() => fetchData()}>
              <RefreshCw className="h-4 w-4 mr-2" />
              Refresh
            </Button>
          </div>
        </div>
      </SettingsSection>

      <EmailSettingsSection />

      <EmailOutboxSection />

      <SettingsSection title="Token Settings" description="JWT and session token configuration">
        {tokenSettings && (
          <>
            <SettingsField
              label="Access token expiry (hours)"
              description="How long access tokens remain valid"
            >
              <Input
                variant="glass"
                type="number"
                value={tokenSettings.accessTokenExpiryHours ?? ""}
                onChange={(e) =>
                  handleTokenChange({
                    accessTokenExpiryHours: parseInt(e.target.value, 10) || undefined,
                  })
                }
                className="text-white"
              />
            </SettingsField>
            <SettingsField
              label="Refresh token expiry (days)"
              description="How long refresh tokens remain valid"
            >
              <Input
                variant="glass"
                type="number"
                value={tokenSettings.refreshTokenExpiryDays ?? ""}
                onChange={(e) =>
                  handleTokenChange({
                    refreshTokenExpiryDays: parseInt(e.target.value, 10) || undefined,
                  })
                }
                className="text-white"
              />
            </SettingsField>
            <SettingsToggle
              label="Enable refresh tokens"
              checked={tokenSettings.enableRefreshTokens ?? false}
              onCheckedChange={(v) =>
                handleTokenChange({ enableRefreshTokens: v })
              }
            />
            <SettingsFooter
              onSave={saveToken}
              isSaving={savingToken}
              isDirty={tokenDirty}
            />
          </>
        )}
      </SettingsSection>
    </div>
  );
}
