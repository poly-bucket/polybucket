import { Card, CardContent, CardHeader, CardTitle } from "@/components/primitives/card";
import { AlertTriangle } from "lucide-react";

export default function DangerZoneSettingsPage() {
  return (
    <Card variant="glass" className="border-red-500/20">
      <CardHeader>
        <div className="flex items-center gap-3">
          <AlertTriangle className="h-8 w-8 text-red-400" />
          <div>
            <CardTitle className="text-white">Danger Zone</CardTitle>
            <p className="text-sm text-white/60 mt-1">
              Coming soon
            </p>
          </div>
        </div>
      </CardHeader>
      <CardContent>
        <p className="text-white/80">
          This section will include account deletion, active sessions management (log out everywhere), 
          and data export options.
        </p>
      </CardContent>
    </Card>
  );
}
