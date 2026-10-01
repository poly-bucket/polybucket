"use client";

import { useCallback, useEffect, useMemo, useState } from "react";
import { CheckCircle2, Eye, Lock, Mail, XCircle } from "lucide-react";
import { toast } from "sonner";
import { SettingsSection } from "@/components/settings/settings-section";
import { SettingsToggle } from "@/components/settings/settings-toggle";
import { SettingsField } from "@/components/settings/settings-field";
import { SettingsFooter } from "@/components/settings/settings-footer";
import { Button } from "@/components/primitives/button";
import { Input } from "@/components/primitives/input";
import { Badge } from "@/components/ui/badge";
import { ProfileSettingsFormSkeleton } from "@/components/ui/skeletons";
import {
  Select,
  SelectContent,
  SelectItem,
  SelectTrigger,
  SelectValue,
} from "@/components/ui/select";
import {
  getEmailApiErrorMessage,
  getEmailSettings,
  previewEmailTemplate,
  testEmailConfiguration,
  updateEmailSettings,
  type EmailSecurityMode,
  type EmailSettingField,
  type EmailSettings,
  type EmailTemplateKey,
  type EmailTestResult,
  type EmailTransportKind,
  type RenderedEmail,
} from "@/lib/services/emailService";

export interface EmailFormState {
  transport: EmailTransportKind;
  smtpHost: string;
  smtpPort: number;
  smtpSecurity: EmailSecurityMode;
  smtpUsername: string;
  smtpPassword: string;
  clearPassword: boolean;
  allowInvalidCertificates: boolean;
  fromAddress: string;
  fromName: string;
  replyTo: string;
  publicBaseUrl: string;
  requireEmailVerification: boolean;
}

interface SmtpPreset {
  id: string;
  label: string;
  host: string;
  port: number;
  security: EmailSecurityMode;
  username?: string;
  hint: string;
}

export const SMTP_PRESETS: SmtpPreset[] = [
  { id: "gmail", label: "Gmail / Google Workspace", host: "smtp.gmail.com", port: 587, security: "StartTls", hint: "Use an app password; regular account passwords are rejected." },
  { id: "microsoft365", label: "Microsoft 365", host: "smtp.office365.com", port: 587, security: "StartTls", hint: "SMTP AUTH must be enabled for the mailbox in the Microsoft 365 admin center." },
  { id: "ses", label: "Amazon SES", host: "email-smtp.us-east-1.amazonaws.com", port: 587, security: "StartTls", hint: "Use SES SMTP credentials and change the region in the host if needed." },
  { id: "sendgrid", label: "SendGrid", host: "smtp.sendgrid.net", port: 587, security: "StartTls", username: "apikey", hint: "The username is literally \"apikey\"; the password is your API key." },
  { id: "mailgun", label: "Mailgun", host: "smtp.mailgun.org", port: 587, security: "StartTls", hint: "Use the SMTP credentials for your sending domain." },
  { id: "postmark", label: "Postmark", host: "smtp.postmarkapp.com", port: 587, security: "StartTls", hint: "Use your server API token as both username and password." },
  { id: "mailpit", label: "Mailpit (development)", host: "mailpit", port: 1025, security: "None", hint: "Start it with: docker compose --profile mail up -d. View mail at http://localhost:8025." },
];

const SECURITY_LABELS: Record<EmailSecurityMode, string> = {
  Auto: "Automatic (recommended)",
  StartTls: "STARTTLS (usually port 587)",
  SslOnConnect: "SSL/TLS on connect (usually port 465)",
  None: "None (local relays only)",
};

const TRANSPORT_LABELS: Record<EmailTransportKind, string> = {
  Disabled: "Disabled",
  Smtp: "SMTP server",
  Log: "Log only (development)",
};

const TEMPLATE_LABELS: Record<EmailTemplateKey, string> = {
  Test: "Test message",
  VerifyEmail: "Verify email",
  PasswordReset: "Password reset",
  Welcome: "Welcome",
  AdminCreatedAccount: "Account invite",
  PasswordChanged: "Password changed",
  TwoFactorChanged: "Two-factor changed",
  EmailChangeRequested: "Confirm new email",
  EmailChangedNotice: "Email changed notice",
  Notification: "Notification",
};

export function toEmailForm(settings: EmailSettings): EmailFormState {
  return {
    transport: settings.transport,
    smtpHost: settings.smtpHost ?? "",
    smtpPort: settings.smtpPort || 587,
    smtpSecurity: settings.smtpSecurity ?? "Auto",
    smtpUsername: settings.smtpUsername ?? "",
    smtpPassword: "",
    clearPassword: false,
    allowInvalidCertificates: settings.allowInvalidCertificates ?? false,
    fromAddress: settings.fromAddress ?? "",
    fromName: settings.fromName ?? "PolyBucket",
    replyTo: settings.replyTo ?? "",
    publicBaseUrl: settings.publicBaseUrl ?? "",
    requireEmailVerification: settings.requireEmailVerification ?? false,
  };
}

interface EmailSettingsSectionProps {
  title?: string;
  description?: string;
  showRequireVerification?: boolean;
  showTemplatePreview?: boolean;
  defaultTestRecipient?: string;
  onSaved?: (settings: EmailSettings) => void;
}

export function EmailSettingsSection({
  title = "Email Configuration",
  description = "How PolyBucket sends account and notification emails",
  showRequireVerification = true,
  showTemplatePreview = true,
  defaultTestRecipient = "",
  onSaved,
}: EmailSettingsSectionProps) {
  const [settings, setSettings] = useState<EmailSettings | null>(null);
  const [form, setForm] = useState<EmailFormState | null>(null);
  const [dirty, setDirty] = useState(false);
  const [loading, setLoading] = useState(true);
  const [saving, setSaving] = useState(false);
  const [testing, setTesting] = useState(false);
  const [error, setError] = useState<string | null>(null);
  const [testRecipient, setTestRecipient] = useState(defaultTestRecipient);
  const [testResult, setTestResult] = useState<EmailTestResult | null>(null);
  const [presetHint, setPresetHint] = useState<string | null>(null);
  const [previewKey, setPreviewKey] = useState<EmailTemplateKey>("VerifyEmail");
  const [preview, setPreview] = useState<RenderedEmail | null>(null);

  const load = useCallback(async () => {
    try {
      setLoading(true);
      setError(null);
      const result = await getEmailSettings();
      setSettings(result);
      setForm(toEmailForm(result));
      setDirty(false);
    } catch (err) {
      setError(getEmailApiErrorMessage(err, "Failed to load email settings"));
    } finally {
      setLoading(false);
    }
  }, []);

  useEffect(() => {
    load();
  }, [load]);

  const managed = useMemo(
    () => new Set<EmailSettingField>(settings?.managedByEnvironment ?? []),
    [settings]
  );
  const isManaged = (field: EmailSettingField) => managed.has(field);

  const update = (updates: Partial<EmailFormState>) => {
    setForm((prev) => (prev ? { ...prev, ...updates } : prev));
    setDirty(true);
    setTestResult(null);
  };

  const applyPreset = (presetId: string) => {
    const preset = SMTP_PRESETS.find((p) => p.id === presetId);
    if (!preset) return;
    update({
      transport: isManaged("transport") ? form!.transport : "Smtp",
      smtpHost: isManaged("smtpHost") ? form!.smtpHost : preset.host,
      smtpPort: isManaged("smtpPort") ? form!.smtpPort : preset.port,
      smtpSecurity: isManaged("smtpSecurity") ? form!.smtpSecurity : preset.security,
      smtpUsername:
        preset.username && !isManaged("smtpUsername") ? preset.username : form!.smtpUsername,
    });
    setPresetHint(preset.hint);
  };

  const save = async () => {
    if (!form) return;
    setSaving(true);
    setError(null);
    try {
      const result = await updateEmailSettings({
        transport: form.transport,
        smtpHost: form.smtpHost,
        smtpPort: form.smtpPort,
        smtpSecurity: form.smtpSecurity,
        smtpUsername: form.smtpUsername,
        smtpPassword: form.smtpPassword || undefined,
        clearPassword: form.clearPassword,
        allowInvalidCertificates: form.allowInvalidCertificates,
        fromAddress: form.fromAddress,
        fromName: form.fromName,
        replyTo: form.replyTo || undefined,
        publicBaseUrl: form.publicBaseUrl,
        requireEmailVerification: form.requireEmailVerification,
      });
      setSettings(result);
      setForm(toEmailForm(result));
      setDirty(false);
      toast.success("Email settings saved");
      onSaved?.(result);
    } catch (err) {
      setError(getEmailApiErrorMessage(err, "Failed to save email settings"));
    } finally {
      setSaving(false);
    }
  };

  const runTest = async () => {
    const recipient = testRecipient.trim() || form?.fromAddress || "";
    if (!recipient) return;
    setTesting(true);
    setError(null);
    try {
      const result = await testEmailConfiguration(recipient);
      setTestResult(result);
      if (result.success) {
        toast.success("Test email sent");
        const refreshed = await getEmailSettings();
        setSettings(refreshed);
      }
    } catch (err) {
      setError(getEmailApiErrorMessage(err, "Email test failed"));
    } finally {
      setTesting(false);
    }
  };

  const loadPreview = async () => {
    try {
      setPreview(await previewEmailTemplate(previewKey));
    } catch (err) {
      setError(getEmailApiErrorMessage(err, "Failed to load preview"));
    }
  };

  if (loading || !form) {
    return (
      <SettingsSection title={title} description={description}>
        {error ? (
          <div className="py-6 text-center text-red-400">{error}</div>
        ) : (
          <ProfileSettingsFormSkeleton />
        )}
      </SettingsSection>
    );
  }

  const smtpEnabled = form.transport === "Smtp";
  const managedBadge = (field: EmailSettingField) =>
    isManaged(field) ? (
      <Badge variant="outline" className="ml-2 border-amber-400/50 text-amber-300">
        <Lock className="h-3 w-3" />
        Set by environment
      </Badge>
    ) : null;
  const lastTest = settings?.lastSuccessfulTestAt
    ? new Date(settings.lastSuccessfulTestAt).toLocaleString()
    : null;

  return (
    <SettingsSection title={title} description={description}>
      {error && (
        <div
          role="alert"
          className="mb-3 rounded-lg border border-red-500/50 bg-red-500/10 px-4 py-3 text-sm text-red-400"
        >
          {error}
        </div>
      )}

      <div className="mb-3 flex flex-wrap items-center gap-2 text-sm">
        {settings?.isConfigured ? (
          <Badge className="bg-emerald-500/20 text-emerald-300">Delivering email</Badge>
        ) : (
          <Badge className="bg-white/10 text-white/70">
            {form.transport === "Disabled" ? "Email disabled" : "Needs attention"}
          </Badge>
        )}
        <span className="text-white/60">
          {lastTest ? `Last successful test: ${lastTest}` : "No successful test yet"}
        </span>
      </div>

      {managed.size > 0 && (
        <p className="mb-3 text-xs text-white/60">
          Some fields are set through environment variables (the Email__* settings) and
          can only be changed there.
        </p>
      )}

      {settings && settings.validationErrors.length > 0 && !dirty && (
        <ul className="mb-3 list-disc pl-5 text-sm text-amber-300">
          {settings.validationErrors.map((message) => (
            <li key={message}>{message}</li>
          ))}
        </ul>
      )}

      <SettingsField label="Delivery method" description="Log only writes emails to the server log instead of sending them">
        <div className="flex items-center">
          <Select
            value={form.transport}
            disabled={isManaged("transport")}
            onValueChange={(v) => update({ transport: v as EmailTransportKind })}
          >
            <SelectTrigger variant="glass" aria-label="Delivery method">
              <SelectValue />
            </SelectTrigger>
            <SelectContent variant="glass">
              {(Object.keys(TRANSPORT_LABELS) as EmailTransportKind[]).map((kind) => (
                <SelectItem key={kind} value={kind}>
                  {TRANSPORT_LABELS[kind]}
                </SelectItem>
              ))}
            </SelectContent>
          </Select>
          {managedBadge("transport")}
        </div>
      </SettingsField>

      {smtpEnabled && (
        <>
          <SettingsField label="Provider preset" description="Fills in the host, port, and security mode">
            <Select onValueChange={applyPreset}>
              <SelectTrigger variant="glass" aria-label="Provider preset">
                <SelectValue placeholder="Choose a provider (optional)" />
              </SelectTrigger>
              <SelectContent variant="glass">
                {SMTP_PRESETS.map((preset) => (
                  <SelectItem key={preset.id} value={preset.id}>
                    {preset.label}
                  </SelectItem>
                ))}
              </SelectContent>
            </Select>
            {presetHint && <p className="mt-2 text-xs text-white/60">{presetHint}</p>}
          </SettingsField>

          <SettingsField label="SMTP host">
            <div className="flex items-center">
              <Input
                variant="glass"
                aria-label="SMTP host"
                value={form.smtpHost}
                disabled={isManaged("smtpHost")}
                onChange={(e) => update({ smtpHost: e.target.value })}
                placeholder="smtp.example.com"
                className="text-white"
              />
              {managedBadge("smtpHost")}
            </div>
          </SettingsField>

          <SettingsField label="SMTP port">
            <div className="flex items-center">
              <Input
                variant="glass"
                type="number"
                aria-label="SMTP port"
                value={form.smtpPort}
                disabled={isManaged("smtpPort")}
                onChange={(e) => update({ smtpPort: parseInt(e.target.value, 10) || 587 })}
                className="text-white"
              />
              {managedBadge("smtpPort")}
            </div>
          </SettingsField>

          <SettingsField label="Connection security">
            <div className="flex items-center">
              <Select
                value={form.smtpSecurity}
                disabled={isManaged("smtpSecurity")}
                onValueChange={(v) => update({ smtpSecurity: v as EmailSecurityMode })}
              >
                <SelectTrigger variant="glass" aria-label="Connection security">
                  <SelectValue />
                </SelectTrigger>
                <SelectContent variant="glass">
                  {(Object.keys(SECURITY_LABELS) as EmailSecurityMode[]).map((mode) => (
                    <SelectItem key={mode} value={mode}>
                      {SECURITY_LABELS[mode]}
                    </SelectItem>
                  ))}
                </SelectContent>
              </Select>
              {managedBadge("smtpSecurity")}
            </div>
          </SettingsField>

          <SettingsField label="SMTP username">
            <div className="flex items-center">
              <Input
                variant="glass"
                aria-label="SMTP username"
                value={form.smtpUsername}
                disabled={isManaged("smtpUsername")}
                onChange={(e) => update({ smtpUsername: e.target.value })}
                placeholder="Optional"
                autoComplete="off"
                className="text-white"
              />
              {managedBadge("smtpUsername")}
            </div>
          </SettingsField>

          <SettingsField
            label="SMTP password"
            description={
              settings?.hasPassword
                ? "A password is stored (encrypted). Leave blank to keep it."
                : "No password stored."
            }
          >
            <div className="flex items-center gap-3">
              <Input
                variant="glass"
                type="password"
                aria-label="SMTP password"
                value={form.smtpPassword}
                disabled={isManaged("smtpPassword") || form.clearPassword}
                onChange={(e) => update({ smtpPassword: e.target.value })}
                placeholder={settings?.hasPassword ? "Leave blank to keep current" : ""}
                autoComplete="new-password"
                className="text-white"
              />
              {managedBadge("smtpPassword")}
              {settings?.hasPassword && !isManaged("smtpPassword") && (
                <Button
                  type="button"
                  variant="ghost"
                  size="sm"
                  onClick={() => update({ clearPassword: !form.clearPassword, smtpPassword: "" })}
                >
                  {form.clearPassword ? "Keep password" : "Remove password"}
                </Button>
              )}
            </div>
          </SettingsField>

          <SettingsToggle
            label="Allow invalid TLS certificates"
            description="Only for internal relays with self-signed certificates"
            checked={form.allowInvalidCertificates}
            disabled={isManaged("allowInvalidCertificates")}
            onCheckedChange={(v) => update({ allowInvalidCertificates: v })}
          />
        </>
      )}

      {form.transport !== "Disabled" && (
        <>
          <SettingsField label="From address">
            <div className="flex items-center">
              <Input
                variant="glass"
                type="email"
                aria-label="From address"
                value={form.fromAddress}
                disabled={isManaged("fromAddress")}
                onChange={(e) => update({ fromAddress: e.target.value })}
                placeholder="noreply@example.com"
                className="text-white"
              />
              {managedBadge("fromAddress")}
            </div>
          </SettingsField>

          <SettingsField label="From name">
            <div className="flex items-center">
              <Input
                variant="glass"
                aria-label="From name"
                value={form.fromName}
                disabled={isManaged("fromName")}
                onChange={(e) => update({ fromName: e.target.value })}
                placeholder="PolyBucket"
                className="text-white"
              />
              {managedBadge("fromName")}
            </div>
          </SettingsField>

          <SettingsField label="Reply-To address" description="Optional; where replies to automated emails go">
            <div className="flex items-center">
              <Input
                variant="glass"
                type="email"
                aria-label="Reply-To address"
                value={form.replyTo}
                disabled={isManaged("replyTo")}
                onChange={(e) => update({ replyTo: e.target.value })}
                className="text-white"
              />
              {managedBadge("replyTo")}
            </div>
          </SettingsField>

          <SettingsField
            label="Public site URL"
            description="Used to build links in emails, for example https://models.example.com"
          >
            <div className="flex items-center">
              <Input
                variant="glass"
                type="url"
                aria-label="Public site URL"
                value={form.publicBaseUrl}
                disabled={isManaged("publicBaseUrl")}
                onChange={(e) => update({ publicBaseUrl: e.target.value })}
                placeholder="https://models.example.com"
                className="text-white"
              />
              {managedBadge("publicBaseUrl")}
            </div>
          </SettingsField>
        </>
      )}

      {showRequireVerification && (
        <SettingsToggle
          label="Require email verification"
          description="New accounts must confirm their address. Needs a successful test email in the last 24 hours."
          checked={form.requireEmailVerification}
          disabled={isManaged("requireEmailVerification") || form.transport === "Disabled"}
          onCheckedChange={(v) => update({ requireEmailVerification: v })}
        />
      )}

      <SettingsFooter
        onSave={save}
        onCancel={() => {
          if (settings) setForm(toEmailForm(settings));
          setDirty(false);
        }}
        isSaving={saving}
        isDirty={dirty}
      />

      {form.transport !== "Disabled" && (
        <div className="space-y-3 pt-4">
          <div className="flex flex-wrap items-end gap-3">
            <div className="min-w-[240px] flex-1">
              <label htmlFor="email-test-recipient" className="text-sm font-medium text-white">
                Send a test email to
              </label>
              <Input
                id="email-test-recipient"
                variant="glass"
                type="email"
                value={testRecipient}
                onChange={(e) => setTestRecipient(e.target.value)}
                placeholder={form.fromAddress || "you@example.com"}
                className="mt-1 text-white"
              />
            </div>
            <Button
              variant="outline"
              onClick={runTest}
              disabled={testing || dirty || !(testRecipient.trim() || form.fromAddress)}
            >
              <Mail className="mr-2 h-4 w-4" />
              {testing ? "Testing..." : "Send test email"}
            </Button>
          </div>
          {dirty && <p className="text-xs text-white/60">Save your changes before testing.</p>}
          {testResult && (
            <div
              className="rounded-lg border border-white/10 bg-white/5 p-3"
              aria-label="Email test results"
            >
              <p className={testResult.success ? "text-emerald-300" : "text-red-400"}>
                {testResult.message}
              </p>
              <ol className="mt-2 space-y-1 text-sm">
                {testResult.stages.map((stage) => (
                  <li key={stage.stage} className="flex items-start gap-2 text-white/80">
                    {stage.success ? (
                      <CheckCircle2 className="mt-0.5 h-4 w-4 shrink-0 text-emerald-400" aria-label="passed" />
                    ) : (
                      <XCircle className="mt-0.5 h-4 w-4 shrink-0 text-red-400" aria-label="failed" />
                    )}
                    <span>
                      <span className="font-medium">{stage.stage}</span>: {stage.message}
                      {stage.elapsedMilliseconds > 0 && (
                        <span className="text-white/40"> ({stage.elapsedMilliseconds} ms)</span>
                      )}
                    </span>
                  </li>
                ))}
              </ol>
            </div>
          )}
        </div>
      )}

      {showTemplatePreview && (
        <div className="space-y-3 border-t border-white/10 pt-4 mt-4">
          <div className="flex flex-wrap items-center gap-3">
            <Select value={previewKey} onValueChange={(v) => setPreviewKey(v as EmailTemplateKey)}>
              <SelectTrigger variant="glass" aria-label="Template to preview" className="w-[240px]">
                <SelectValue />
              </SelectTrigger>
              <SelectContent variant="glass">
                {(Object.keys(TEMPLATE_LABELS) as EmailTemplateKey[]).map((key) => (
                  <SelectItem key={key} value={key}>
                    {TEMPLATE_LABELS[key]}
                  </SelectItem>
                ))}
              </SelectContent>
            </Select>
            <Button variant="ghost" onClick={loadPreview}>
              <Eye className="mr-2 h-4 w-4" />
              Preview template
            </Button>
          </div>
          {preview && (
            <div className="space-y-2">
              <p className="text-sm text-white/80">
                <span className="font-medium">Subject:</span> {preview.subject}
              </p>
              <iframe
                title="Email preview"
                sandbox=""
                srcDoc={preview.htmlBody}
                className="h-[420px] w-full rounded-lg border border-white/10 bg-white"
              />
            </div>
          )}
        </div>
      )}
    </SettingsSection>
  );
}
