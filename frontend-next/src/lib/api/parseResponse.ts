import type { FileResponse } from "./client";

export async function parseFileResponseAsJson<T>(
  fileResponse: FileResponse
): Promise<T> {
  if (fileResponse.data instanceof Blob) {
    const text = await fileResponse.data.text();
    return JSON.parse(text);
  }
  throw new Error("Invalid response format");
}
