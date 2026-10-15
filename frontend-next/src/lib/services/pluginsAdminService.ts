import axiosInstance from "@/lib/api/axiosConfig";

export async function reloadPlugins(): Promise<void> {
  await axiosInstance.post("/api/plugins/reload");
}
