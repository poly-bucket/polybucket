import { ModerationSettingsSection } from "@/components/moderation/moderation-settings-section";

export default function ModerationSettingsPage() {
  return (
    <div className="space-y-6">
      <h2 className="text-2xl font-bold text-white">Moderation settings</h2>
      <ModerationSettingsSection />
    </div>
  );
}
