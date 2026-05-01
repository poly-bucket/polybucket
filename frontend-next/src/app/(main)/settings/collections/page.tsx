 "use client";

import { useEffect, useState } from "react";
import { FolderOpen } from "lucide-react";
import { toast } from "sonner";
import { SettingsSection } from "@/components/settings/settings-section";
import { Button } from "@/components/primitives/button";
import { getCollectionDefaults, setCollectionDefaults, type CollectionDefaults } from "@/lib/services/contentDefaultsService";

export default function CollectionsSettingsPage() {
  const [defaults, setDefaults] = useState<CollectionDefaults>({ visibility: "Private" });

  useEffect(() => {
    setDefaults(getCollectionDefaults());
  }, []);

  const save = () => {
    setCollectionDefaults(defaults);
    toast.success("Collection defaults saved");
  };

  return (
    <SettingsSection
      title="Collection Defaults"
      description="Set defaults applied when creating new collections."
    >
      <div className="space-y-4">
        <div className="flex items-center gap-2 text-sm text-white/70">
          <FolderOpen className="h-4 w-4" />
          Defaults are saved locally on this device.
        </div>
        <div>
          <label className="mb-1.5 block text-sm font-medium text-white/80">
            Default visibility
          </label>
          <select
            value={defaults.visibility}
            onChange={(e) =>
              setDefaults({ visibility: e.target.value as CollectionDefaults["visibility"] })
            }
            className="h-10 w-full max-w-sm rounded-md border border-white/20 bg-white/5 px-3 text-sm text-white"
          >
            <option value="Public">Public</option>
            <option value="Unlisted">Unlisted</option>
            <option value="Private">Private</option>
          </select>
        </div>
        <Button type="button" size="sm" onClick={save}>
          Save Collection Defaults
        </Button>
      </div>
    </SettingsSection>
  );
}
