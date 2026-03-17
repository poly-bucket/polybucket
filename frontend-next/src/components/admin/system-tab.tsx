"use client";

import { useState, useEffect, useCallback } from "react";
import { ExternalLink, Mail, RefreshCw } from "lucide-react";
import Link from "next/link";
import { SettingsSection } from "@/components/settings/settings-section";
import { SettingsToggle } from "@/components/settings/settings-toggle";
import { SettingsField } from "@/components/settings/settings-field";
import { SettingsFooter } from "@/components/settings/settings-footer";
import { Button } from "@/components/primitives/button";
import { Input } from "@/components/primitives/input";
import {
  getEmailSettings,
  updateEmailSettings,
  testEmailConfiguration,
  getTokenSettings,
  updateTokenSettings,
  getSetupStatus,
} from "@/lib/services/adminService";
import type { TokenSettings } from "@/lib/api/client";
import {
  UpdateEmailSettingsCommand,
  TestEmailConfigurationCommand,
  EmailSettings,
} from "@/lib/api/client";

interface EmailFormState {
  enabled: boolean;
  smtpServer: string;
  smtpPort: number;
  smtpUsername: string;
  smtpPassword: string;
  useSsl: boolean;
  fromAddress: string;
  fromName: string;
  requireEmailVerification: boolean;
}

const initialEmailForm: EmailFormState = {
  enabled: false,
  smtpServer: "",
  smtpPort: 587,
  smtpUsername: "",
  smtpPassword: "",
  useSsl: true,
  fromAddress: "",
  fromName: "",
  requireEmailVerification: false,
};

export function SystemTab() {
  const [emailForm, setEmailForm] = useState<EmailFormState>(initialEmailForm);
  const [tokenSettings, setTokenSettings] = useState<TokenSettings | null>(null);
  const [setupStatus, setSetupStatus] = useState<{
    isFirstTimeSetup?: boolean;
    completedSteps?: number;
    totalSteps?: number;
  } | null>(null);
  const [loading, setLoading] = useState(true);
  const [savingEmail, setSavingEmail] = useState(false);
  const [savingToken, setSavingToken] = useState(false);
  const [testingEmail, setTestingEmail] = useState(false);
  const [emailDirty, setEmailDirty] = useState(false);
  const [tokenDirty, setTokenDirty] = useState(false);
  const [error, setError] = useState<string | null>(null);

  const fetchData = useCallback(async () => {
    try {
      setLoading(true);
      setError(null);
      const [emailRes, tokenRes, statusRes] = await Promise.all([
        getEmailSettings(),
        getTokenSettings(),
        getSetupStatus(),
      ]);
      if (emailRes) {
        setEmailForm({
          enabled: emailRes.enabled ?? false,
          smtpServer: emailRes.smtpServer ?? "",
          smtpPort: emailRes.smtpPort ?? 587,
          smtpUsername: emailRes.smtpUsername ?? "",
          smtpPassword: "",
          useSsl: true,
          fromAddress: emailRes.fromAddress ?? "",
          fromName: emailRes.fromName ?? "",
          requireEmailVerification: emailRes.requireEmailVerification ?? false,
        });
      }
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

  const handleEmailChange = (updates: Partial<EmailFormState>) => {
    setEmailForm((prev) => ({ ...prev, ...updates }));
    setEmailDirty(true);
  };

  const handleTokenChange = (updates: Partial<TokenSettings>) => {
    setTokenSettings((prev) =>
      prev ? ({ ...prev, ...updates } as TokenSettings) : null
    );
    setTokenDirty(true);
  };

  const saveEmail = async () => {
    setSavingEmail(true);
    try {
      const cmd = new UpdateEmailSettingsCommand({
        enabled: emailForm.enabled,
        smtpServer: emailForm.smtpServer || "localhost",
        smtpPort: emailForm.smtpPort,
        smtpUsername: emailForm.smtpUsername || undefined,
        smtpPassword: emailForm.smtpPassword || undefined,
        useSsl: emailForm.useSsl,
        fromAddress: emailForm.fromAddress || "noreply@localhost",
        fromName: emailForm.fromName || "PolyBucket",
        requireEmailVerification: emailForm.requireEmailVerification,
      });
      await updateEmailSettings(cmd);
      setEmailDirty(false);
    } catch {
      setError("Failed to save email settings");
    } finally {
      setSavingEmail(false);
    }
  };

  const testEmail = async () => {
    if (!emailForm.fromAddress) return;
    setTestingEmail(true);
    try {
      await testEmailConfiguration(
        new TestEmailConfigurationCommand({
          testEmailAddress: emailForm.fromAddress,
          emailSettings: new EmailSettings({
            enabled: emailForm.enabled,
            smtpServer: emailForm.smtpServer || "localhost",
            smtpPort: emailForm.smtpPort,
            smtpUsername: emailForm.smtpUsername,
            smtpPassword: emailForm.smtpPassword,
            fromAddress: emailForm.fromAddress,
            fromName: emailForm.fromName,
          }),
        })
      );
    } catch {
      setError("Email test failed");
    } finally {
      setTestingEmail(false);
    }
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

      <SettingsSection title="Email Configuration" description="SMTP and email delivery settings">
        <SettingsToggle
          label="Enable email"
          checked={emailForm.enabled}
          onCheckedChange={(v) => handleEmailChange({ enabled: v })}
        />
        <SettingsField label="SMTP Server">
          <Input
            variant="glass"
            value={emailForm.smtpServer}
            onChange={(e) => handleEmailChange({ smtpServer: e.target.value })}
            placeholder="smtp.example.com"
            className="text-white"
          />
        </SettingsField>
        <SettingsField label="SMTP Port">
          <Input
            variant="glass"
            type="number"
            value={emailForm.smtpPort}
            onChange={(e) =>
              handleEmailChange({ smtpPort: parseInt(e.target.value, 10) || 587 })
            }
            className="text-white"
          />
        </SettingsField>
        <SettingsField label="SMTP Username">
          <Input
            variant="glass"
            value={emailForm.smtpUsername}
            onChange={(e) => handleEmailChange({ smtpUsername: e.target.value })}
            placeholder="Optional"
            className="text-white"
          />
        </SettingsField>
        <SettingsField label="SMTP Password">
          <Input
            variant="glass"
            type="password"
            value={emailForm.smtpPassword}
            onChange={(e) => handleEmailChange({ smtpPassword: e.target.value })}
            placeholder="Leave blank to keep current"
            className="text-white"
          />
        </SettingsField>
        <SettingsField label="From Address">
          <Input
            variant="glass"
            type="email"
            value={emailForm.fromAddress}
            onChange={(e) => handleEmailChange({ fromAddress: e.target.value })}
            placeholder="noreply@example.com"
            className="text-white"
          />
        </SettingsField>
        <SettingsField label="From Name">
          <Input
            variant="glass"
            value={emailForm.fromName}
            onChange={(e) => handleEmailChange({ fromName: e.target.value })}
            placeholder="PolyBucket"
            className="text-white"
          />
        </SettingsField>
        <SettingsToggle
          label="Use SSL/TLS"
          checked={emailForm.useSsl}
          onCheckedChange={(v) => handleEmailChange({ useSsl: v })}
        />
        <SettingsToggle
          label="Require email verification"
          checked={emailForm.requireEmailVerification}
          onCheckedChange={(v) =>
            handleEmailChange({ requireEmailVerification: v })
          }
        />
        <SettingsFooter
          onSave={saveEmail}
          isSaving={savingEmail}
          isDirty={emailDirty}
        />
        <div className="pt-4">
          <Button
            variant="outline"
            onClick={testEmail}
            disabled={testingEmail || !emailForm.fromAddress}
          >
            <Mail className="h-4 w-4 mr-2" />
            {testingEmail ? "Testing..." : "Test Configuration"}
          </Button>
        </div>
      </SettingsSection>

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
