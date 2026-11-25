import { getDefaultSettings, type UserSettingsData } from "@/lib/services/userSettingsService";

export type ModelCardSize = "small" | "medium" | "large";
export type ModelCardSpacing = "compact" | "normal" | "spacious";

export interface ResolvedModelBrowseLayout {
  containerClassName: string;
  gapClassName: string;
  cardSize: ModelCardSize;
}

const LG_GRID_COLUMNS: Record<number, string> = {
  2: "lg:grid-cols-2",
  3: "lg:grid-cols-3",
  4: "lg:grid-cols-4",
  5: "lg:grid-cols-5",
  6: "lg:grid-cols-6",
};

const CARD_SIZES = new Set<ModelCardSize>(["small", "medium", "large"]);
const CARD_SPACINGS = new Set<ModelCardSpacing>(["compact", "normal", "spacious"]);

function clampGridColumns(value: number | undefined): number {
  const n = value ?? 4;
  if (n < 2 || n > 6) return 4;
  return n;
}

function clampCardSize(value: string | undefined): ModelCardSize {
  if (value && CARD_SIZES.has(value as ModelCardSize)) {
    return value as ModelCardSize;
  }
  return "medium";
}

function clampCardSpacing(value: string | undefined): ModelCardSpacing {
  if (value && CARD_SPACINGS.has(value as ModelCardSpacing)) {
    return value as ModelCardSpacing;
  }
  return "normal";
}

function gapClassForSpacing(spacing: ModelCardSpacing): string {
  switch (spacing) {
    case "compact":
      return "gap-2";
    case "spacious":
      return "gap-6";
    default:
      return "gap-4";
  }
}

export type ModelBrowseLayoutInput = Pick<
  UserSettingsData,
  "cardSize" | "cardSpacing" | "gridColumns" | "dashboardViewType"
>;

export function resolveModelBrowseLayout(
  input?: Partial<ModelBrowseLayoutInput>
): ResolvedModelBrowseLayout {
  const defaults = getDefaultSettings();
  const merged: ModelBrowseLayoutInput = {
    cardSize: input?.cardSize ?? defaults.cardSize,
    cardSpacing: input?.cardSpacing ?? defaults.cardSpacing,
    gridColumns: input?.gridColumns ?? defaults.gridColumns,
    dashboardViewType: input?.dashboardViewType ?? defaults.dashboardViewType,
  };

  const cardSize = clampCardSize(merged.cardSize);
  const spacing = clampCardSpacing(merged.cardSpacing);
  const columns = clampGridColumns(merged.gridColumns);
  const lgCols = LG_GRID_COLUMNS[columns] ?? LG_GRID_COLUMNS[4];

  return {
    containerClassName: `grid grid-cols-2 sm:grid-cols-3 ${lgCols}`,
    gapClassName: gapClassForSpacing(spacing),
    cardSize,
  };
}