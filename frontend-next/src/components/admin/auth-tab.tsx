"use client";

import { useState } from "react";
import { Github, Lock, Mail } from "lucide-react";
import { SettingsSection } from "@/components/settings/settings-section";
import { SettingsToggle } from "@/components/settings/settings-toggle";
import { SettingsField } from "@/components/settings/settings-field";
import { SettingsFooter } from "@/components/settings/settings-footer";
import { Button } from "@/components/primitives/button";
import { Input } from "@/components/primitives/input";
import {
  Select,
  SelectContent,
  SelectItem,
  SelectTrigger,
  SelectValue,
} from "@/components/ui/select";
import {
  getAuthenticationSettings,
  updateAuthenticationSettings,
  getOAuthProviders,
} from "@/lib/services/adminService";
import { useAdminQuery } from "@/lib/hooks/use-admin-query";
import { useAdminMutation } from "@/lib/hooks/use-admin-mutation";
import type { AuthenticationSettings as AuthSettingsType } from "@/lib/api/client";
import { LoginMethod } from "@/lib/api/client";
import { toast } from "sonner";

const providerIcons: Record<string, React.ComponentType<{ className?: string }>> = {
  Google: Mail,
  GitHub: Github,
};

function getProviderIcon(name?: string) {
  if (!name) return Lock;
  for (const [key, Icon] of Object.entries(providerIcons)) {
    if (name.toLowerCase().includes(key.toLowerCase())) return Icon;
  }
  return Lock;
}

export function AuthTab() {
  const {
    data: authSettings,
    isLoading: loadingAuth,
    error: authError,
    refetch: refetchAuth,
  } = useAdminQuery(getAuthenticationSettings);

  const {
    data: oauthProviders,
    isLoading: loadingOAuth,
    error: oauthError,
  } = useAdminQuery(getOAuthProviders);

  const updateMutation = useAdminMutation(
    (s: AuthSettingsType) => updateAuthenticationSettings(s),
    {
      onSuccess: refetchAuth,
      successMessage: "Authentication settings saved",
    }
  );

  const [form, setForm] = useState<Partial<AuthSettingsType>>({});
  const [dirty, setDirty] = useState(false);

  const merged = { ...authSettings, ...form };

  const handleChange = (updates: Partial<AuthSettingsType>) => {
    setForm((p) => ({ ...p, ...updates }));
    setDirty(true);
  };

  const handleSave = async () => {
    if (!merged) return;
    await updateMutation.mutate({
      ...merged,
    } as AuthSettingsType);
    setDirty(false);
    setForm({});
  };

  const isLoading = loadingAuth;
  const error = authError ?? oauthError;

  if (isLoading) {
    return (
      <div className="space-y-6">
        <h2 className="text-2xl font-bold text-white">Authentication</h2>
        <div className="text-center text-white/60 py-12">
          Loading authentication settings...
        </div>
      </div>
    );
  }

  return (
    <div className="space-y-6">
      <h2 className="text-2xl font-bold text-white">Authentication</h2>

      {error && (
        <div className="rounded-lg border border-red-500/50 bg-red-500/10 px-4 py-3 text-red-400">
          {error}
        </div>
      )}

      <SettingsSection
        title="OAuth Providers"
        description="Configure external login providers"
      >
        {loadingOAuth ? (
          <div className="text-white/60 py-4">Loading providers...</div>
        ) : oauthProviders && oauthProviders.length > 0 ? (
          <div className="space-y-4">
            {oauthProviders.map((p) => {
              const Icon = getProviderIcon(p.providerName);
              return (
                <div
                  key={p.providerName ?? p.pluginId ?? "unknown"}
                  className="rounded-lg border border-white/10 glass-bg p-4"
                >
                  <div className="flex items-center justify-between">
                    <div className="flex items-center gap-3">
                      <Icon className="h-8 w-8 text-white/80" />
                      <div>
                        <p className="font-medium text-white">
                          {p.providerName ?? p.pluginName ?? "Unknown"}
                        </p>
                        <p className="text-sm text-white/50">
                          {p.pluginName && p.pluginName !== p.providerName
                            ? p.pluginName
                            : "OAuth provider"}
                        </p>
                      </div>
                    </div>
                    <span
                      className={`rounded px-2 py-1 text-xs ${
                        p.isEnabled
                          ? "bg-green-500/20 text-green-400"
                          : "bg-white/10 text-white/60"
                      }`}
                    >
                      {p.isEnabled ? "Enabled" : "Disabled"}
                    </span>
                  </div>
                  <div className="mt-3 space-y-1 text-sm">
                    <p className="text-white/60">
                      Client ID: {p.clientId ? "••••••••" : "Not configured"}
                    </p>
                    <p className="text-white/60">
                      Redirect: Configure in plugin settings
                    </p>
                  </div>
                  <p className="mt-2 text-xs text-white/40">
                    OAuth client ID and secret are managed via the plugin
                    configuration. Use the Plugins panel to configure.
                  </p>
                </div>
              );
            })}
          </div>
        ) : (
          <div className="rounded-lg border border-white/10 glass-bg p-6 text-center text-white/60">
            No OAuth providers installed. Install an OAuth plugin to enable
            Google, GitHub, or other providers.
          </div>
        )}
      </SettingsSection>

      <SettingsSection
        title="Password Policy"
        description="Configure password requirements and lockout"
      >
        {merged && (
          <>
            <SettingsField
              label="Minimum password length"
              description="Minimum number of characters required"
            >
              <Input
                variant="glass"
                type="number"
                min={6}
                max={128}
                value={merged.passwordMinLength ?? 8}
                onChange={(e) =>
                  handleChange({
                    passwordMinLength: parseInt(e.target.value, 10) || 8,
                  })
                }
                className="text-white"
              />
            </SettingsField>
            <SettingsToggle
              label="Require strong passwords"
              description="Enforce uppercase, numbers, and special characters"
              checked={merged.requireStrongPasswords ?? false}
              onCheckedChange={(v) =>
                handleChange({ requireStrongPasswords: v })
              }
            />
            <SettingsField
              label="Max failed login attempts"
              description="Lock account after this many failed attempts"
            >
              <Input
                variant="glass"
                type="number"
                min={0}
                value={merged.maxFailedLoginAttempts ?? 5}
                onChange={(e) =>
                  handleChange({
                    maxFailedLoginAttempts: parseInt(e.target.value, 10) || 0,
                  })
                }
                className="text-white"
              />
            </SettingsField>
            <SettingsField
              label="Lockout duration (minutes)"
              description="How long the account stays locked"
            >
              <Input
                variant="glass"
                type="number"
                min={0}
                value={merged.lockoutDurationMinutes ?? 15}
                onChange={(e) =>
                  handleChange({
                    lockoutDurationMinutes:
                      parseInt(e.target.value, 10) || 0,
                  })
                }
                className="text-white"
              />
            </SettingsField>
            <SettingsFooter
              onSave={handleSave}
              isSaving={updateMutation.isLoading}
              isDirty={dirty}
            />
          </>
        )}
      </SettingsSection>

      <SettingsSection
        title="Login Configuration"
        description="How users can sign in"
      >
        {merged && (
          <>
            <SettingsField
              label="Login method"
              description="Allow email, username, or both"
            >
              <Select
                value={merged.loginMethod ?? LoginMethod.Email}
                onValueChange={(v) =>
                  handleChange({
                    loginMethod: v as LoginMethod,
                    allowEmailLogin:
                      v === LoginMethod.Email || v === LoginMethod.Both,
                    allowUsernameLogin:
                      v === LoginMethod.Username || v === LoginMethod.Both,
                  })
                }
              >
                <SelectTrigger variant="glass">
                  <SelectValue />
                </SelectTrigger>
                <SelectContent variant="glass">
                  <SelectItem value={LoginMethod.Email}>Email only</SelectItem>
                  <SelectItem value={LoginMethod.Username}>
                    Username only
                  </SelectItem>
                  <SelectItem value={LoginMethod.Both}>
                    Email or username
                  </SelectItem>
                </SelectContent>
              </Select>
            </SettingsField>
            <SettingsToggle
              label="Require email verification"
              description="Users must verify their email before full access"
              checked={merged.requireEmailVerification ?? false}
              onCheckedChange={(v) =>
                handleChange({ requireEmailVerification: v })
              }
            />
            <SettingsFooter
              onSave={handleSave}
              isSaving={updateMutation.isLoading}
              isDirty={dirty}
            />
          </>
        )}
      </SettingsSection>
    </div>
  );
}
