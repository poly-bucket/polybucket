import { describe, it, expect } from "vitest";
import { execFileSync } from "node:child_process";
import fs from "node:fs";
import os from "node:os";
import path from "node:path";

describe("validate-next-artifacts.sh", () => {
  it("fails when placeholder remains in artifacts", () => {
    const dir = fs.mkdtempSync(path.join(os.tmpdir(), "next-artifacts-"));
    fs.writeFileSync(path.join(dir, "chunk.js"), "__NEXT_PUBLIC_API_URL__");
    const script = path.join(process.cwd(), "scripts/validate-next-artifacts.sh");
    expect(() => execFileSync("sh", [script, dir], { stdio: "pipe" })).toThrow();
  });
});
