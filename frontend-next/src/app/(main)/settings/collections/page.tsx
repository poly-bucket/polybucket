import { Card, CardContent, CardHeader, CardTitle } from "@/components/primitives/card";
import { FolderOpen } from "lucide-react";

export default function CollectionsSettingsPage() {
  return (
    <Card variant="glass" className="border-white/20">
      <CardHeader>
        <div className="flex items-center gap-3">
          <FolderOpen className="h-8 w-8 text-white/60" />
          <div>
            <CardTitle className="text-white">Collection Settings</CardTitle>
            <p className="text-sm text-white/60 mt-1">
              Coming soon
            </p>
          </div>
        </div>
      </CardHeader>
      <CardContent>
        <p className="text-white/80">
          Configure default privacy, permissions, and content settings for new collections. 
          These settings will be applied when you create new collections.
        </p>
      </CardContent>
    </Card>
  );
}
