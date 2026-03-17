"use client";

import { toast } from "sonner";
import { useUserSettings } from "@/contexts/UserSettingsContext";
import { SettingsSection } from "@/components/settings/settings-section";
import { SettingsToggle } from "@/components/settings/settings-toggle";

export default function NotificationsSettingsPage() {
  const { settings, updateSettings } = useUserSettings();

  const handleToggle = async (value: boolean) => {
    const previous = settings?.emailNotifications ?? true;
    try {
      const success = await updateSettings({ emailNotifications: value });
      if (success) {
        toast.success("Notification settings updated");
      } else {
        toast.error("Failed to update notification settings");
      }
    } catch (err) {
      console.error("Failed to update notification:", err);
      toast.error("Failed to update notification settings");
    }
  };

  if (!settings) {
    return (
      <div className="flex min-h-48 items-center justify-center">
        <div className="h-10 w-10 animate-spin rounded-full border-2 border-white/30 border-t-white" />
      </div>
    );
  }

  return (
    <SettingsSection
      title="Email Notifications"
      description="Choose which emails you want to receive"
    >
      <SettingsToggle
        label="Email Notifications"
        description="Receive emails about updates, comments, and system announcements"
        checked={settings.emailNotifications ?? true}
        onCheckedChange={handleToggle}
      />
      <p className="mt-4 text-xs text-white/50">
        More granular notification controls (mentions, followers, likes, comments) will be available when the backend supports them.
      </p>
    </SettingsSection>
  );
}
