import axiosInstance from "@/lib/api/axiosConfig";

export async function reloadPlugins(): Promise<void> {
  await axiosInstance.post("/api/plugins/reload");
}

export interface MarketplaceCatalogItem {
  id: string;
  name: string;
  description: string;
  version: string;
  category: string;
  repositoryUrl: string;
  targets?: string[];
  downloads?: number;
}

export interface MarketplaceCatalogPage {
  plugins: MarketplaceCatalogItem[];
  totalCount: number;
  page: number;
  pageSize: number;
  totalPages: number;
}

export async function getMarketplaceBaseUrl(): Promise<string> {
  const { data } = await axiosInstance.get<{ baseUrl: string }>(
    "/api/admin/marketplace/base-url"
  );
  return data.baseUrl ?? "";
}

export async function updateMarketplaceBaseUrl(baseUrl: string): Promise<string> {
  const { data } = await axiosInstance.put<{ baseUrl: string }>(
    "/api/admin/marketplace/base-url",
    { baseUrl }
  );
  return data.baseUrl;
}

export async function browseMarketplacePlugins(params: {
  page?: number;
  pageSize?: number;
  search?: string;
  category?: string;
  target?: string;
}): Promise<MarketplaceCatalogPage> {
  const query = new URLSearchParams();
  query.set("page", String(params.page ?? 1));
  query.set("pageSize", String(params.pageSize ?? 20));
  if (params.search) query.set("search", params.search);
  if (params.category) query.set("category", params.category);
  if (params.target) query.set("target", params.target);

  const { data } = await axiosInstance.get<{
    plugins: MarketplaceCatalogItem[];
    totalCount: number;
    page: number;
    pageSize: number;
    totalPages: number;
  }>(`/api/plugins/marketplace?${query.toString()}`);

  return {
    plugins: data.plugins ?? [],
    totalCount: data.totalCount ?? 0,
    page: data.page ?? 1,
    pageSize: data.pageSize ?? 20,
    totalPages: data.totalPages ?? 1,
  };
}

export async function installMarketplacePlugin(pluginId: string): Promise<{
  success: boolean;
  message: string;
  restartRequired?: boolean;
}> {
  const { data } = await axiosInstance.post<{
    success: boolean;
    message: string;
    restartRequired?: boolean;
  }>(`/api/plugins/marketplace/${encodeURIComponent(pluginId)}/install`);
  return data;
}

export interface InstalledPluginSummary {
  id: string;
  name: string;
  version: string;
  targets?: string[];
  cssAssets?: string[];
}

export async function getInstalledPlugins(): Promise<InstalledPluginSummary[]> {
  const { data } = await axiosInstance.get<InstalledPluginSummary[]>("/api/plugins/installed");
  return data ?? [];
}
