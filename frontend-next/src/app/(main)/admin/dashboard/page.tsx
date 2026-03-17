import dynamic from "next/dynamic";

const DashboardTab = dynamic(
  () =>
    import("@/components/admin/dashboard-tab").then((mod) => ({
      default: mod.DashboardTab,
    })),
  { loading: () => <div className="text-white/60 py-8">Loading...</div> }
);

export default function AdminDashboardPage() {
  return <DashboardTab />;
}
