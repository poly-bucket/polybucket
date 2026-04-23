"use client";

import Link from "next/link";
import { UserAvatar } from "@/components/layout/user-avatar";
import { Card, CardContent, CardHeader, CardTitle } from "@/components/primitives/card";
import { Button } from "@/components/primitives/button";
import { formatNumber } from "@/lib/utils/modelUtils";
import { useModelSidebarCards, PluginBoundary } from "@/lib/plugins";
import type { Model } from "@/lib/api/client";
import { LicenseTypes } from "@/lib/api/client";
import { Download, Pencil, Trash2, Share2 } from "lucide-react";

const LICENSE_LABELS: Record<string, string> = {
  [LicenseTypes.MIT]: "MIT License",
  [LicenseTypes.GPLv3]: "GPL v3",
  [LicenseTypes.Apache2]: "Apache 2.0",
  [LicenseTypes.CCBy4]: "CC BY 4.0",
  [LicenseTypes.CCBySA4]: "CC BY-SA 4.0",
  [LicenseTypes.CCByND4]: "CC BY-ND 4.0",
  [LicenseTypes.CCByNC4]: "CC BY-NC 4.0",
  [LicenseTypes.CCByNCSA4]: "CC BY-NC-SA 4.0",
  [LicenseTypes.CCByNCND4]: "CC BY-NC-ND 4.0",
  [LicenseTypes.BSD]: "BSD License",
};

interface ModelDetailsSidebarProps {
  model: Model;
  isOwner: boolean;
  isFederated: boolean;
  isAuthenticated: boolean;
  isDeleting: boolean;
  onDownload: () => void;
  onDelete: () => void;
  onShare: () => void;
  onEdit?: () => void;
}

export function ModelDetailsSidebar({
  model,
  isOwner,
  isFederated,
  isAuthenticated,
  isDeleting,
  onDownload,
  onDelete,
  onShare,
  onEdit,
}: ModelDetailsSidebarProps) {
  const downloads = model.downloads ?? 0;
  const authorName = model.author?.username ?? "Unknown";
  const licenseLabel =
    model.license && LICENSE_LABELS[model.license]
      ? LICENSE_LABELS[model.license]
      : model.license
        ? String(model.license)
        : "Unknown";

  const canEdit = isOwner && !isFederated;
  const canDelete = isOwner && !isFederated;
  const sidebarCards = useModelSidebarCards();

  return (
    <div className="sticky top-6 space-y-6">
      <Card variant="glass" className="border-white/20">
        <CardHeader>
          <CardTitle className="text-2xl font-bold text-white">
            {model.name ?? "Untitled Model"}
          </CardTitle>
        </CardHeader>
        <CardContent className="space-y-4">
          <div className="flex items-center gap-3">
            <UserAvatar
              userId={model.author?.id ?? model.authorId ?? ""}
              username={authorName}
              profilePictureUrl={model.author?.profilePictureUrl}
              avatar={model.author?.avatar}
              size="md"
            />
            <div>
              {model.authorId ? (
                <Link
                  href={`/profile/${model.authorId}`}
                  className="font-medium text-white hover:text-white/80"
                >
                  {authorName}
                </Link>
              ) : (
                <span className="font-medium text-white">{authorName}</span>
              )}
              <p className="text-sm text-white/60">Creator</p>
            </div>
          </div>

          {isFederated && (
            <div className="rounded-lg border border-purple-500/30 bg-purple-500/10 p-3">
              <div className="mb-2 flex items-center gap-2">
                <svg
                  className="h-5 w-5 text-purple-400"
                  fill="none"
                  stroke="currentColor"
                  viewBox="0 0 24 24"
                >
                  <path
                    strokeLinecap="round"
                    strokeLinejoin="round"
                    strokeWidth={2}
                    d="M3.055 11H5a2 2 0 012 2v1a2 2 0 002 2 2 2 0 012 2v2.945M8 3.935V5.5A2.5 2.5 0 0010.5 8h.5a2 2 0 012 2 2 2 0 104 0 2 2 0 012-2h1.064M15 20.488V18a2 2 0 012-2h3.064M21 12a9 9 0 11-18 0 9 9 0 0118 0z"
                  />
                </svg>
                <span className="text-sm font-semibold text-purple-300">
                  Federated Model
                </span>
              </div>
              <div className="space-y-1 text-xs text-white/60">
                {model.remoteInstanceId && (
                  <p>
                    Origin:{" "}
                    <span className="text-white/80">{model.remoteInstanceId}</span>
                  </p>
                )}
                {model.lastFederationSync && (
                  <p>
                    Last Synced:{" "}
                    <span className="text-white/80">
                      {new Date(model.lastFederationSync).toLocaleString()}
                    </span>
                  </p>
                )}
                <p className="pt-2 text-purple-300">
                  This model is a local copy from a federated PolyBucket
                  instance.
                </p>
              </div>
            </div>
          )}

          <div className="flex flex-wrap gap-2">
            {model.wip && (
              <span className="rounded-md bg-amber-500/20 px-2 py-0.5 text-xs text-amber-300">
                Work in Progress
              </span>
            )}
            {model.aiGenerated && (
              <span className="rounded-md bg-blue-500/20 px-2 py-0.5 text-xs text-blue-300">
                AI Generated
              </span>
            )}
            {model.nsfw && (
              <span className="rounded-md bg-red-500/20 px-2 py-0.5 text-xs text-red-300">
                NSFW
              </span>
            )}
            {model.isFeatured && (
              <span className="rounded-md bg-green-500/20 px-2 py-0.5 text-xs text-green-300">
                Featured
              </span>
            )}
          </div>

          <div className="space-y-3">
            <Button
              variant="glass"
              className="w-full"
              onClick={onDownload}
              disabled={!isAuthenticated}
            >
              <Download className="h-5 w-5" />
              Download ({formatNumber(downloads)})
            </Button>

            {isOwner && (
              <>
                <Button
                  variant="outline"
                  className="w-full"
                  onClick={onEdit}
                  disabled={isFederated || !onEdit}
                  title={
                    isFederated
                      ? "Federated models cannot be edited locally"
                      : "Edit model details, versions, and settings"
                  }
                >
                  <Pencil className="h-5 w-5" />
                  Edit Model
                </Button>
                <Button
                  variant="destructive"
                  className="w-full"
                  onClick={onDelete}
                  disabled={!canDelete || isDeleting}
                  title={
                    !canDelete
                      ? "Federated models cannot be edited locally"
                      : undefined
                  }
                >
                  <Trash2 className="h-5 w-5" />
                  {isDeleting ? "Deleting..." : "Delete Model"}
                </Button>
              </>
            )}

            <Button
              variant="outline"
              className="w-full"
              onClick={onShare}
            >
              <Share2 className="h-4 w-4" />
              Share
            </Button>
          </div>
        </CardContent>
      </Card>

      <Card variant="glass" className="border-white/20">
        <CardHeader>
          <CardTitle className="text-white">Statistics</CardTitle>
        </CardHeader>
        <CardContent>
          <div className="space-y-3">
            <div className="flex justify-between">
              <span className="text-white/60">Views</span>
              <span className="font-semibold text-white">
                {formatNumber(downloads * 3)}
              </span>
            </div>
            <div className="flex justify-between">
              <span className="text-white/60">Downloads</span>
              <span className="font-semibold text-white">
                {formatNumber(downloads)}
              </span>
            </div>
            <div className="flex justify-between">
              <span className="text-white/60">Published</span>
              <span className="font-semibold text-white">
                {model.createdAt
                  ? new Date(model.createdAt).toLocaleDateString()
                  : "—"}
              </span>
            </div>
            <div className="flex justify-between">
              <span className="text-white/60">Last Updated</span>
              <span className="font-semibold text-white">
                {model.updatedAt
                  ? new Date(model.updatedAt).toLocaleDateString()
                  : "—"}
              </span>
            </div>
          </div>
        </CardContent>
      </Card>

      <Card variant="glass" className="border-white/20">
        <CardHeader>
          <CardTitle className="text-white">License</CardTitle>
        </CardHeader>
        <CardContent>
          <div className="flex items-center gap-3">
            <div className="flex h-8 w-8 shrink-0 items-center justify-center rounded bg-blue-500/30 text-sm font-bold text-white">
              {model.license ?? "—"}
            </div>
            <div>
              <p className="font-medium text-white">{licenseLabel}</p>
              <p className="text-sm text-white/60">
                Check license terms for usage rights
              </p>
            </div>
          </div>
        </CardContent>
      </Card>

      {sidebarCards.map((card) => {
        const Component = card.component;
        return (
          <PluginBoundary key={card.id} pluginId={card.id}>
            <Component model={model} isOwner={isOwner} />
          </PluginBoundary>
        );
      })}
    </div>
  );
}
