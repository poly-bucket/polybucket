import { ConnectedCollectionsBar } from "@/components/collections/connected-collections-bar";

export default function WithCollectionsBarLayout({
  children,
}: {
  children: React.ReactNode;
}) {
  return (
    <div className="flex min-h-[calc(100vh-5rem)]">
      <ConnectedCollectionsBar />
      <div className="min-w-0 flex-1">{children}</div>
    </div>
  );
}
