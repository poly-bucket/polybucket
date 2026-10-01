"use client";

import { useCallback, useEffect, useState } from "react";
import { RefreshCw, RotateCcw } from "lucide-react";
import { toast } from "sonner";
import { SettingsSection } from "@/components/settings/settings-section";
import { Button } from "@/components/primitives/button";
import { DataTablePagination } from "@/components/primitives/pagination";
import { Badge } from "@/components/ui/badge";
import { TableRowsSkeleton } from "@/components/ui/skeletons";
import {
  getEmailApiErrorMessage,
  getEmailOutbox,
  retryEmailMessage,
  type EmailMessageStatus,
  type EmailOutboxPage,
} from "@/lib/services/emailService";

const STATUS_FILTERS: { value: EmailMessageStatus | "All"; label: string }[] = [
  { value: "All", label: "All" },
  { value: "Pending", label: "Pending" },
  { value: "Sending", label: "Sending" },
  { value: "Failed", label: "Retrying" },
  { value: "DeadLetter", label: "Dead-lettered" },
  { value: "Sent", label: "Sent" },
];

const STATUS_STYLES: Record<EmailMessageStatus, string> = {
  Pending: "bg-sky-500/20 text-sky-300",
  Sending: "bg-indigo-500/20 text-indigo-300",
  Sent: "bg-emerald-500/20 text-emerald-300",
  Failed: "bg-amber-500/20 text-amber-300",
  DeadLetter: "bg-red-500/20 text-red-300",
};

export function EmailOutboxSection() {
  const [status, setStatus] = useState<EmailMessageStatus | "All">("All");
  const [page, setPage] = useState(1);
  const [data, setData] = useState<EmailOutboxPage | null>(null);
  const [loading, setLoading] = useState(true);
  const [retrying, setRetrying] = useState<string | null>(null);
  const [error, setError] = useState<string | null>(null);

  const load = useCallback(async () => {
    try {
      setLoading(true);
      setError(null);
      setData(await getEmailOutbox(status === "All" ? undefined : status, page, 25));
    } catch (err) {
      setError(getEmailApiErrorMessage(err, "Failed to load the email outbox"));
    } finally {
      setLoading(false);
    }
  }, [status, page]);

  useEffect(() => {
    load();
  }, [load]);

  const retry = async (id: string) => {
    setRetrying(id);
    try {
      await retryEmailMessage(id);
      toast.success("Email queued for another attempt");
      await load();
    } catch (err) {
      toast.error(getEmailApiErrorMessage(err, "Failed to retry email"));
    } finally {
      setRetrying(null);
    }
  };

  return (
    <SettingsSection
      title="Email Outbox"
      description="Every email is queued here first and retried automatically if delivery fails"
    >
      <div className="mb-3 flex flex-wrap items-center gap-2">
        {STATUS_FILTERS.map((filter) => {
          const count =
            filter.value === "All"
              ? undefined
              : data?.statusCounts?.[filter.value as EmailMessageStatus];
          return (
            <Button
              key={filter.value}
              size="sm"
              variant={status === filter.value ? "default" : "ghost"}
              onClick={() => {
                setStatus(filter.value);
                setPage(1);
              }}
            >
              {filter.label}
              {count !== undefined && <span className="ml-1 text-white/60">({count})</span>}
            </Button>
          );
        })}
        <Button size="sm" variant="ghost" onClick={load} aria-label="Refresh outbox">
          <RefreshCw className="h-4 w-4" />
        </Button>
      </div>

      {error && (
        <div role="alert" className="rounded-lg border border-red-500/50 bg-red-500/10 px-4 py-3 text-sm text-red-400">
          {error}
        </div>
      )}

      {loading && !data ? (
        <TableRowsSkeleton variant="default" rows={6} />
      ) : data && data.items.length === 0 ? (
        <div className="py-6 text-center text-white/60">No emails match this filter.</div>
      ) : (
        data && (
          <div className="overflow-x-auto">
            <table className="w-full text-left text-sm text-white/80">
              <thead className="text-xs uppercase text-white/50">
                <tr>
                  <th className="py-2 pr-3">Template</th>
                  <th className="py-2 pr-3">Recipient</th>
                  <th className="py-2 pr-3">Status</th>
                  <th className="py-2 pr-3">Attempts</th>
                  <th className="py-2 pr-3">Created</th>
                  <th className="py-2 pr-3">Details</th>
                  <th className="py-2" />
                </tr>
              </thead>
              <tbody>
                {data.items.map((item) => (
                  <tr key={item.id} className="border-t border-white/10 align-top">
                    <td className="py-2 pr-3">{item.template}</td>
                    <td className="py-2 pr-3 break-all">{item.recipient}</td>
                    <td className="py-2 pr-3">
                      <Badge className={STATUS_STYLES[item.status]}>{item.status}</Badge>
                    </td>
                    <td className="py-2 pr-3">{item.attempts}</td>
                    <td className="py-2 pr-3 whitespace-nowrap">
                      {new Date(item.createdAt).toLocaleString()}
                    </td>
                    <td className="py-2 pr-3 text-xs text-white/60">
                      {item.status === "Sent" && item.sentAt
                        ? `Sent ${new Date(item.sentAt).toLocaleString()}`
                        : item.status === "Failed"
                          ? `Next attempt ${new Date(item.nextAttemptAt).toLocaleString()}${item.lastError ? `: ${item.lastError}` : ""}`
                          : item.lastError ?? ""}
                    </td>
                    <td className="py-2">
                      {(item.status === "Failed" || item.status === "DeadLetter") && (
                        <Button
                          size="sm"
                          variant="outline"
                          onClick={() => retry(item.id)}
                          disabled={retrying === item.id}
                        >
                          <RotateCcw className="mr-1 h-3 w-3" />
                          Retry now
                        </Button>
                      )}
                    </td>
                  </tr>
                ))}
              </tbody>
            </table>
            {data.totalPages > 1 && (
              <div className="pt-3">
                <DataTablePagination page={data.page} totalPages={data.totalPages} onPageChange={setPage} />
              </div>
            )}
          </div>
        )
      )}
    </SettingsSection>
  );
}
