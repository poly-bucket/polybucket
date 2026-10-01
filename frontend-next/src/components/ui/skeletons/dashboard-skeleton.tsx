import { Card, CardContent, CardHeader, CardTitle } from "@/components/primitives/card";
import { Skeleton } from "@/components/ui/glass/skeleton";

export function DashboardSkeleton() {
  return (
    <div className="space-y-6">
      <h2 className="text-2xl font-bold text-white">Analytics Dashboard</h2>
      <Card variant="glass" className="border-white/20">
        <CardHeader>
          <CardTitle className="text-white">Model Statistics Overview</CardTitle>
        </CardHeader>
        <CardContent>
          <div className="grid grid-cols-1 gap-4 md:grid-cols-2 lg:grid-cols-4">
            {[1, 2, 3, 4].map((i) => (
              <div
                key={i}
                className="flex flex-col gap-2 rounded-lg border border-white/10 p-4"
              >
                <div className="flex items-center space-x-2">
                  <Skeleton className="h-5 w-5 rounded" />
                  <Skeleton className="h-8 w-16" />
                </div>
                <Skeleton className="h-4 w-24" />
              </div>
            ))}
          </div>
        </CardContent>
      </Card>
      <div className="grid grid-cols-1 gap-6 lg:grid-cols-2">
        <Card variant="glass" className="border-white/20">
          <CardHeader>
            <Skeleton className="h-6 w-40" />
          </CardHeader>
          <CardContent>
            <div className="space-y-3">
              {[1, 2, 3, 4].map((i) => (
                <Skeleton key={i} className="h-12 w-full rounded-lg" />
              ))}
            </div>
          </CardContent>
        </Card>
        <Card variant="glass" className="border-white/20">
          <CardHeader>
            <Skeleton className="h-6 w-32" />
          </CardHeader>
          <CardContent>
            <div className="space-y-3">
              {[1, 2, 3].map((i) => (
                <Skeleton key={i} className="h-12 w-full rounded-lg" />
              ))}
            </div>
          </CardContent>
        </Card>
      </div>
    </div>
  );
}
