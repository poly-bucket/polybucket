import { describe, expect, it, vi } from "vitest";
import { render, screen } from "@/test/test-utils";
import UploadPreviewCarousel from "../upload-preview-carousel";
import type { UploadedFile } from "../file-queue";

vi.mock("@/components/model-viewer/model-viewer", () => ({
  default: () => <div data-testid="model-viewer">3d</div>,
}));

vi.mock("@/contexts/UserSettingsContext", async (importOriginal) => {
  const actual = await importOriginal<typeof import("@/contexts/UserSettingsContext")>();
  return {
    ...actual,
    useUserSettings: () => ({ settings: { autoRotateModels: false } }),
  };
});

function makeStlFile(): UploadedFile {
  const file = new File(["solid test\nendsolid test\n"], "part.stl", { type: "model/stl" });
  return {
    id: "file-1",
    name: file.name,
    size: file.size,
    type: file.type,
    file,
    progress: 0,
    isThumbnail: false,
  };
}

describe("UploadPreviewCarousel", () => {
  it("pauses background 3D preview while thumbnail editor is open", () => {
    const uploaded = makeStlFile();

    render(
      <UploadPreviewCarousel
        files={[uploaded]}
        activeFileId={uploaded.id}
        getFileType={(name) => (name.endsWith(".stl") ? "3d" : "unknown")}
        onActiveFileChange={vi.fn()}
        suspendBackground3d
      />
    );

    expect(screen.getByText(/3D preview paused while the thumbnail editor is open/i)).toBeInTheDocument();
    expect(screen.queryByTestId("model-viewer")).not.toBeInTheDocument();
  });

  it("renders 3D preview when suspendBackground3d is false", () => {
    const uploaded = makeStlFile();

    render(
      <UploadPreviewCarousel
        files={[uploaded]}
        activeFileId={uploaded.id}
        getFileType={(name) => (name.endsWith(".stl") ? "3d" : "unknown")}
        onActiveFileChange={vi.fn()}
        suspendBackground3d={false}
      />
    );

    expect(screen.getByTestId("model-viewer")).toBeInTheDocument();
  });
});