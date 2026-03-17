import { Card, CardContent, CardHeader, CardTitle } from "@/components/primitives/card";
import { Upload } from "lucide-react";

export default function UploadsSettingsPage() {
  return (
    <Card variant="glass" className="border-white/20">
      <CardHeader>
        <div className="flex items-center gap-3">
          <Upload className="h-8 w-8 text-white/60" />
          <div>
            <CardTitle className="text-white">Model Upload Settings</CardTitle>
            <p className="text-sm text-white/60 mt-1">
              Coming soon
            </p>
          </div>
        </div>
      </CardHeader>
      <CardContent>
        <p className="text-white/80">
          Configure default privacy, quality settings, and upload preferences for new models. 
          These settings will be applied when you upload new models.
        </p>
      </CardContent>
    </Card>
  );
}
