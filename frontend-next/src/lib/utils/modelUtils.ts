export type ModelFileType = "3d" | "image" | "pdf" | "markdown" | "unknown";

export function getFileType(fileName: string): ModelFileType {
  const fileExtension = fileName
    .toLowerCase()
    .substring(fileName.lastIndexOf("."));
  if (
    [
      ".stl",
      ".obj",
      ".fbx",
      ".gltf",
      ".glb",
      ".3mf",
      ".step",
      ".stp",
    ].includes(fileExtension)
  )
    return "3d";
  if (
    [".jpg", ".jpeg", ".png", ".gif", ".webp", ".bmp"].includes(fileExtension)
  )
    return "image";
  if (fileName.toLowerCase().endsWith(".pdf")) return "pdf";
  if ([".md", ".markdown"].includes(fileExtension)) return "markdown";
  return "unknown";
}

export function isMarkdownFile(fileName: string): boolean {
  const fileExtension = fileName
    .toLowerCase()
    .substring(fileName.lastIndexOf("."));
  return fileExtension === ".md" || fileExtension === ".markdown";
}

export function formatNumber(num: number | undefined): string {
  if (num === undefined || num === null) return "0";
  if (num >= 1000000) return (num / 1000000).toFixed(1) + "M";
  if (num >= 1000) return (num / 1000).toFixed(1) + "K";
  return num.toString();
}
