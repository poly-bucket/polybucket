"use client";

import { useCallback, useEffect, useState } from "react";
import { Input } from "@/components/primitives/input";
import { Button } from "@/components/primitives/button";
import { SettingsSection } from "@/components/settings/settings-section";
import { DataTablePagination } from "@/components/primitives/pagination";
import {
  browseMarketplacePlugins,
  getMarketplaceBaseUrl,
  installMarketplacePlugin,
  updateMarketplaceBaseUrl,
  type MarketplaceCatalogItem,
} from "@/lib/services/pluginsAdminService";
import { toast } from "sonner";

export function MarketplacePluginsSection() {
  const [baseUrl, setBaseUrl] = useState("");
  const [baseUrlDraft, setBaseUrlDraft] = useState("");
  const [search, setSearch] = useState("");
  const [targetFilter, setTargetFilter] = useState("");
  const [page, setPage] = useState(1);
  const [pageSize] = useState(10);
  const [totalPages, setTotalPages] = useState(1);
  const [plugins, setPlugins] = useState<MarketplaceCatalogItem[]>([]);
  const [loading, setLoading] = useState(true);
  const [installingId, setInstallingId] = useState<string | null>(null);

  const loadBaseUrl = useCallback(async () => {
    const url = await getMarketplaceBaseUrl();
    setBaseUrl(url);
    setBaseUrlDraft(url);
  }, []);

  const loadCatalog = useCallback(async () => {
    setLoading(true);
    try {
      const data = await browseMarketplacePlugins({
        page,
        pageSize,
        search: search.trim() || undefined,
        target: targetFilter || undefined,
      });
      setPlugins(data.plugins);
      setTotalPages(Math.max(1, data.totalPages));
    } catch (error) {
      toast.error(error instanceof Error ? error.message : "Failed to load marketplace catalog");
      setPlugins([]);
    } finally {
      setLoading(false);
    }
  }, [page, pageSize, search, targetFilter]);

  useEffect(() => {
    loadBaseUrl().catch(() => undefined);
  }, [loadBaseUrl]);

  useEffect(() => {
    loadCatalog().catch(() => undefined);
  }, [loadCatalog]);

  const handleSaveBaseUrl = async () => {
    try {
      const saved = await updateMarketplaceBaseUrl(baseUrlDraft.trim());
      setBaseUrl(saved);
      toast.success("Marketplace URL saved");
      await loadCatalog();
    } catch (error) {
      toast.error(error instanceof Error ? error.message : "Failed to save marketplace URL");
    }
  };

  const handleInstall = async (plugin: MarketplaceCatalogItem) => {
    setInstallingId(plugin.id);
    try {
      const result = await installMarketplacePlugin(plugin.id);
      if (result.restartRequired) {
        toast.success(`${result.message} The API will restart shortly.`);
      } else {
        toast.success(result.message);
      }
    } catch (error) {
      toast.error(error instanceof Error ? error.message : "Install failed");
    } finally {
      setInstallingId(null);
    }
  };

  return (
    <div className="space-y-6">
      <SettingsSection
        title="Marketplace connection"
        description="PolyBucket loads the plugin catalog from this standalone marketplace URL"
      >
        <div className="flex flex-col gap-3 md:flex-row">
          <Input
            value={baseUrlDraft}
            onChange={(e) => setBaseUrlDraft(e.target.value)}
            placeholder="http://localhost:5001"
            className="border-white/20 bg-white/5 text-white"
          />
          <Button variant="outline" onClick={handleSaveBaseUrl}>
            Save URL
          </Button>
        </div>
        {baseUrl ? (
          <p className="text-xs text-white/50 mt-2">Active catalog: {baseUrl}</p>
        ) : null}
      </SettingsSection>

      <SettingsSection title="Browse marketplace" description="Install approved plugins onto this instance">
        <div className="mb-4 flex flex-col gap-3 md:flex-row">
          <Input
            value={search}
            onChange={(e) => {
              setSearch(e.target.value);
              setPage(1);
            }}
            placeholder="Search plugins"
            className="border-white/20 bg-white/5 text-white md:max-w-sm"
          />
          <select
            value={targetFilter}
            onChange={(e) => {
              setTargetFilter(e.target.value);
              setPage(1);
            }}
            className="h-9 rounded-md border border-white/20 bg-white/5 px-3 text-sm text-white"
          >
            <option value="">All targets</option>
            <option value="frontend">Frontend</option>
            <option value="api">API</option>
          </select>
        </div>

        {loading ? (
          <p className="text-white/60">Loading marketplace plugins…</p>
        ) : plugins.length === 0 ? (
          <p className="text-white/60">No plugins matched your filters.</p>
        ) : (
          <div className="space-y-3">
            {plugins.map((plugin) => (
              <div
                key={plugin.id}
                className="flex flex-col gap-3 rounded-lg border border-white/10 glass-bg p-4 md:flex-row md:items-center md:justify-between"
              >
                <div>
                  <p className="font-medium text-white">{plugin.name}</p>
                  <p className="text-sm text-white/60">{plugin.description}</p>
                  <p className="mt-1 text-xs text-white/40">
                    v{plugin.version} · {plugin.category}
                    {plugin.targets?.length ? ` · ${plugin.targets.join(", ")}` : ""}
                  </p>
                </div>
                <Button
                  variant="glass"
                  disabled={installingId === plugin.id}
                  onClick={() => handleInstall(plugin)}
                >
                  {installingId === plugin.id ? "Installing…" : "Install"}
                </Button>
              </div>
            ))}
          </div>
        )}

        <div className="mt-4">
          <DataTablePagination page={page} totalPages={totalPages} onPageChange={setPage} />
        </div>
      </SettingsSection>
    </div>
  );
}
