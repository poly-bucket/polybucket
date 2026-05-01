"use client";

import { toast } from "sonner";
import { useUserSettings } from "@/contexts/UserSettingsContext";
import { SettingsSection } from "@/components/settings/settings-section";
import { SettingsToggle } from "@/components/settings/settings-toggle";

export default function NotificationsSettingsPage() {
  const { settings, updateSettings } = useUserSettings();

  const handleToggle = async (key: "emailNotifications" | "notifyOnMentions" | "notifyOnFollows" | "notifyOnLikes" | "notifyOnComments", value: boolean) => {
    try {
      const success = await updateSettings({ [key]: value });
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
        label="Enable Email Notifications"
        description="Master switch for receiving account emails"
        checked={settings.emailNotifications ?? true}
        onCheckedChange={(checked) => handleToggle("emailNotifications", checked)}
      />
      <SettingsToggle
        label="Mentions"
        description="Notify me when someone mentions me"
        checked={settings.notifyOnMentions ?? true}
        onCheckedChange={(checked) => handleToggle("notifyOnMentions", checked)}
      />
      <SettingsToggle
        label="Follows"
        description="Notify me when someone follows my profile"
        checked={settings.notifyOnFollows ?? true}
        onCheckedChange={(checked) => handleToggle("notifyOnFollows", checked)}
      />
      <SettingsToggle
        label="Likes"
        description="Notify me when someone likes my models"
        checked={settings.notifyOnLikes ?? true}
        onCheckedChange={(checked) => handleToggle("notifyOnLikes", checked)}
      />
      <SettingsToggle
        label="Comments"
        description="Notify me about new comments on my content"
        checked={settings.notifyOnComments ?? true}
        onCheckedChange={(checked) => handleToggle("notifyOnComments", checked)}
      />
    </SettingsSection>
  );
}
