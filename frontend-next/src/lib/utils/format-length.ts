export type MeasurementSystem = "metric" | "imperial";

export function formatMillimeters(mm: number, system: MeasurementSystem): string {
  if (system === "imperial") {
    const inches = mm / 25.4;
    return `${inches.toFixed(2)} in`;
  }
  return `${Math.round(mm)} mm`;
}

export function formatNozzleDiameter(system: MeasurementSystem): string {
  return system === "imperial" ? "0.016 in" : "0.4 mm";
}