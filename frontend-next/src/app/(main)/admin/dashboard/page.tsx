import dynamic from "next/dynamic";
import { DashboardSkeleton } from "@/components/ui/skeletons";

const DashboardTab = dynamic(
  () =>
    import("@/components/admin/dashboard-tab").then((mod) => ({
      default: mod.DashboardTab,
    })),
  { loading: () => <DashboardSkeleton /> }
);

export default function AdminDashboardPage() {
  return <DashboardTab />;
}
