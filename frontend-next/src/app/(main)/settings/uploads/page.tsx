 "use client";

import { useEffect, useState } from "react";
import { Upload } from "lucide-react";
import { toast } from "sonner";
import { SettingsSection } from "@/components/settings/settings-section";
import { SettingsToggle } from "@/components/settings/settings-toggle";
import { Button } from "@/components/primitives/button";
import { Input } from "@/components/primitives/input";
import { getUploadDefaults, setUploadDefaults, type UploadDefaults } from "@/lib/services/contentDefaultsService";

export default function UploadsSettingsPage() {
  const [defaults, setDefaults] = useState<UploadDefaults>({
    privacy: "Public",
    license: "MIT",
    aiGenerated: false,
    workInProgress: false,
    nsfw: false,
    remix: false,
  });

  useEffect(() => {
    setDefaults(getUploadDefaults());
  }, []);

  const save = () => {
    setUploadDefaults(defaults);
    toast.success("Upload defaults saved");
  };

  return (
    <SettingsSection
      title="Model Upload Defaults"
      description="Set the defaults applied when you open the model upload flow."
    >
      <div className="space-y-4">
        <div className="flex items-center gap-2 text-sm text-white/70">
          <Upload className="h-4 w-4" />
          Defaults are saved locally on this device.
        </div>

        <div className="grid grid-cols-1 gap-4 sm:grid-cols-2">
          <div>
            <label className="mb-1.5 block text-sm font-medium text-white/80">
              Default privacy
            </label>
            <select
              value={defaults.privacy}
              onChange={(e) =>
                setDefaults((prev) => ({
                  ...prev,
                  privacy: e.target.value as UploadDefaults["privacy"],
                }))
              }
              className="h-10 w-full rounded-md border border-white/20 bg-white/5 px-3 text-sm text-white"
            >
              <option value="Public">Public</option>
              <option value="Private">Private</option>
              <option value="Unlisted">Unlisted</option>
            </select>
          </div>
          <div>
            <label className="mb-1.5 block text-sm font-medium text-white/80">
              Default license
            </label>
            <Input
              value={defaults.license}
              onChange={(e) =>
                setDefaults((prev) => ({ ...prev, license: e.target.value }))
              }
              className="border-white/20 bg-white/5 text-white"
              placeholder="MIT"
            />
          </div>
        </div>

        <SettingsToggle
          label="AI Generated"
          description="Default this option on model uploads"
          checked={defaults.aiGenerated}
          onCheckedChange={(v) => setDefaults((prev) => ({ ...prev, aiGenerated: v }))}
        />
        <SettingsToggle
          label="Work in Progress"
          description="Default this option on model uploads"
          checked={defaults.workInProgress}
          onCheckedChange={(v) => setDefaults((prev) => ({ ...prev, workInProgress: v }))}
        />
        <SettingsToggle
          label="NSFW"
          description="Default this option on model uploads"
          checked={defaults.nsfw}
          onCheckedChange={(v) => setDefaults((prev) => ({ ...prev, nsfw: v }))}
        />
        <SettingsToggle
          label="Remix"
          description="Default this option on model uploads"
          checked={defaults.remix}
          onCheckedChange={(v) => setDefaults((prev) => ({ ...prev, remix: v }))}
        />

        <Button type="button" size="sm" onClick={save}>
          Save Upload Defaults
        </Button>
      </div>
    </SettingsSection>
  );
}
