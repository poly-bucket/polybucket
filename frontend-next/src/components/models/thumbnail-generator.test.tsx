import type { ReactNode } from "react";
import { BoxGeometry } from "three";
import { describe, expect, it, vi, beforeEach } from "vitest";
import { render, screen } from "@/test/test-utils";
import ThumbnailGenerator from "./thumbnail-generator";

vi.mock("three/examples/jsm/loaders/STLLoader.js", () => ({
  STLLoader: class {
    parse() {
      return new BoxGeometry(1, 1, 1);
    }
  },
}));

const stageProps: { environment?: unknown }[] = [];

vi.mock("@react-three/drei", () => ({
  Stage: (props: { environment?: unknown; children?: ReactNode }) => {
    stageProps.push({ environment: props.environment });
    return <div data-testid="stage">{props.children}</div>;
  },
  OrbitControls: () => null,
}));

vi.mock("@react-three/fiber", () => ({
  Canvas: ({ children }: { children?: ReactNode }) => (
    <div data-testid="r3f-canvas">{children}</div>
  ),
  useThree: () => ({
    camera: {
      position: { set: vi.fn() },
      lookAt: vi.fn(),
      updateMatrixWorld: vi.fn(),
    },
    gl: {
      domElement: document.createElement("canvas"),
      setClearColor: vi.fn(),
      setClearAlpha: vi.fn(),
      render: vi.fn(),
    },
    scene: {},
    invalidate: vi.fn(),
  }),
}));

describe("ThumbnailGenerator", () => {
  beforeEach(() => {
    stageProps.length = 0;
  });

  it("renders dialog with accessible title and disables HDR environment on Stage", () => {
    const stl = new File(["solid test\nendsolid test\n"], "part.stl", { type: "model/stl" });

    render(
      <ThumbnailGenerator
        modelFile={stl}
        open
        onOpenChange={vi.fn()}
        onThumbnailGenerated={vi.fn()}
      />
    );

    expect(screen.getByRole("dialog")).toBeInTheDocument();
    expect(
      screen.getByRole("heading", { name: /generate custom thumbnail/i })
    ).toBeInTheDocument();
    expect(stageProps.some((props) => props.environment === null)).toBe(true);
  });
});