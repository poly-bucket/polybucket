import { ModelQueueTab } from "@/components/moderation/model-queue-tab";

export default function ModerationModelsPage() {
  return (
    <div className="space-y-6">
      <div>
        <h2 className="text-xl font-semibold text-white">Model queue</h2>
        <p className="text-sm text-white/60">
          Review uploads before they appear in public browse and search.
        </p>
      </div>
      <ModelQueueTab />
    </div>
  );
}
