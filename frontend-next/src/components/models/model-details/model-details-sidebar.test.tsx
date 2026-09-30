import { describe, it, expect, vi } from "vitest";
import { render, screen } from "@/test/test-utils";
import { ModelDetailsSidebar } from "./model-details-sidebar";

vi.mock("@/lib/plugins", async (importOriginal) => {
  const actual = await importOriginal<typeof import("@/lib/plugins")>();
  return {
    ...actual,
    useModelSidebarCards: () => [],
  };
});
import type { ModelWithReactions } from "@/lib/types/modelReactions";
import { LicenseTypes, PrivacySettings } from "@/lib/api/client";

function createModel(overrides: Partial<ModelWithReactions> = {}): ModelWithReactions {
  return {
    id: "model-1",
    name: "Test Model",
    downloads: 12,
    views: 48,
    privacy: PrivacySettings.Public,
    license: LicenseTypes.MIT,
    authorId: "author-1",
    author: { id: "author-1", username: "maker" },
    ...overrides,
  } as ModelWithReactions;
}

describe("ModelDetailsSidebar", () => {
  it("displays view count from the model", () => {
    render(
      <ModelDetailsSidebar
        model={createModel({ views: 48, downloads: 12 })}
        isOwner={false}
        isFederated={false}
        isAuthenticated={true}
        isDeleting={false}
        onDownload={() => {}}
        onDelete={() => {}}
        onShare={() => {}}
      />
    );

    expect(screen.getByText("Views").nextElementSibling?.textContent).toBe("48");
    expect(screen.getByText("Downloads").nextElementSibling?.textContent).toBe(
      "12"
    );
  });
});
