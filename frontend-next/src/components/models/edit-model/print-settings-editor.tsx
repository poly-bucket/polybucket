"use client";

import { useState, useEffect } from "react";
import { Input } from "@/components/primitives/input";
import { Textarea } from "@/components/ui/glass/textarea";
import { Card, CardContent } from "@/components/primitives/card";
import { Button } from "@/components/primitives/button";
import { Switch } from "@/components/primitives/switch";
import {
  Select,
  SelectContent,
  SelectItem,
  SelectTrigger,
  SelectValue,
} from "@/components/ui/select";
import { Settings } from "lucide-react";

export interface PrintSettings {
  id: string;
  layerHeight: string;
  infill: string;
  supports: boolean;
  printSpeed: string;
  nozzleTemp: string;
  bedTemp: string;
  material: string;
  nozzleSize: string;
  retraction: string;
  buildPlateAdhesion: string;
  notes?: string;
}

interface PrintSettingsEditorProps {
  printSettings: PrintSettings;
  onUpdate: (settings: PrintSettings) => void;
  readonly?: boolean;
}

const QUALITY_PRESETS = [
  {
    name: "Draft Quality",
    layerHeight: "0.3mm",
    infill: "10%",
    printSpeed: "60mm/s",
    supports: false,
  },
  {
    name: "Standard Quality",
    layerHeight: "0.2mm",
    infill: "15%",
    printSpeed: "50mm/s",
    supports: false,
  },
  {
    name: "High Quality",
    layerHeight: "0.1mm",
    infill: "20%",
    printSpeed: "30mm/s",
    supports: true,
  },
  {
    name: "Miniature Detail",
    layerHeight: "0.05mm",
    infill: "25%",
    printSpeed: "20mm/s",
    supports: true,
  },
];

const MATERIAL_PRESETS = [
  { name: "PLA", nozzleTemp: "210°C", bedTemp: "60°C" },
  { name: "ABS", nozzleTemp: "250°C", bedTemp: "100°C" },
  { name: "PETG", nozzleTemp: "230°C", bedTemp: "80°C" },
  { name: "TPU", nozzleTemp: "220°C", bedTemp: "50°C" },
  { name: "ASA", nozzleTemp: "260°C", bedTemp: "100°C" },
];

const BUILD_PLATE_OPTIONS = [
  { value: "skirt", label: "Skirt" },
  { value: "brim", label: "Brim" },
  { value: "raft", label: "Raft" },
  { value: "none", label: "None" },
];

export function PrintSettingsEditor({
  printSettings,
  onUpdate,
  readonly = false,
}: PrintSettingsEditorProps) {
  const [settings, setSettings] = useState<PrintSettings>(printSettings);

  useEffect(() => {
    setSettings(printSettings);
  }, [printSettings.id]);

  const handleChange = (field: keyof PrintSettings, value: string | boolean) => {
    const updated = { ...settings, [field]: value };
    setSettings(updated);
    onUpdate(updated);
  };

  const applyQualityPreset = (preset: (typeof QUALITY_PRESETS)[0]) => {
    const updated = {
      ...settings,
      layerHeight: preset.layerHeight,
      infill: preset.infill,
      printSpeed: preset.printSpeed,
      supports: preset.supports,
    };
    setSettings(updated);
    onUpdate(updated);
  };

  const applyMaterialPreset = (preset: (typeof MATERIAL_PRESETS)[0]) => {
    const updated = {
      ...settings,
      material: preset.name,
      nozzleTemp: preset.nozzleTemp,
      bedTemp: preset.bedTemp,
    };
    setSettings(updated);
    onUpdate(updated);
  };

  return (
    <div className="space-y-6" role="region" aria-label="Print settings">
      <div className="flex flex-col sm:flex-row sm:items-center sm:justify-between gap-4">
        <h3 className="text-lg font-semibold text-white">Print Settings</h3>
        {!readonly && (
          <p className="text-sm text-white/60">
            Recommended settings for optimal print quality
          </p>
        )}
      </div>

      {!readonly && (
        <div className="space-y-4">
          <div>
            <h4 className="text-md font-medium text-white/80 mb-2">
              Quality Presets
            </h4>
            <div className="flex flex-wrap gap-2">
              {QUALITY_PRESETS.map((preset) => (
                <Button
                  key={preset.name}
                  variant="outline"
                  size="sm"
                  onClick={() => applyQualityPreset(preset)}
                >
                  {preset.name}
                </Button>
              ))}
            </div>
          </div>
          <div>
            <h4 className="text-md font-medium text-white/80 mb-2">
              Material Presets
            </h4>
            <div className="flex flex-wrap gap-2">
              {MATERIAL_PRESETS.map((preset) => (
                <Button
                  key={preset.name}
                  variant="outline"
                  size="sm"
                  onClick={() => applyMaterialPreset(preset)}
                >
                  {preset.name}
                </Button>
              ))}
            </div>
          </div>
        </div>
      )}

      <div className="grid grid-cols-1 md:grid-cols-2 lg:grid-cols-3 gap-4">
        <Card variant="glass" className="border-white/20">
          <CardContent className="pt-6 space-y-4">
            <h4 className="font-medium text-white flex items-center gap-2">
              <Settings className="h-4 w-4" />
              Basic Settings
            </h4>
            <div>
              <label className="block text-sm font-medium text-white/80 mb-2">
                Layer Height
              </label>
              <Input
                variant="glass"
                value={settings.layerHeight}
                onChange={(e) => handleChange("layerHeight", e.target.value)}
                disabled={readonly}
                placeholder="0.2mm"
              />
            </div>
            <div>
              <label className="block text-sm font-medium text-white/80 mb-2">
                Infill Density
              </label>
              <Input
                variant="glass"
                value={settings.infill}
                onChange={(e) => handleChange("infill", e.target.value)}
                disabled={readonly}
                placeholder="15%"
              />
            </div>
            <div>
              <label className="block text-sm font-medium text-white/80 mb-2">
                Print Speed
              </label>
              <Input
                variant="glass"
                value={settings.printSpeed}
                onChange={(e) => handleChange("printSpeed", e.target.value)}
                disabled={readonly}
                placeholder="50mm/s"
              />
            </div>
            <div className="flex items-center gap-2">
              <Switch
                checked={settings.supports}
                onCheckedChange={(v) => handleChange("supports", v)}
                disabled={readonly}
              />
              <span className="text-sm text-white/80">Enable Supports</span>
            </div>
          </CardContent>
        </Card>

        <Card variant="glass" className="border-white/20">
          <CardContent className="pt-6 space-y-4">
            <h4 className="font-medium text-white">Temperature & Material</h4>
            <div>
              <label className="block text-sm font-medium text-white/80 mb-2">
                Material
              </label>
              <Input
                variant="glass"
                value={settings.material}
                onChange={(e) => handleChange("material", e.target.value)}
                disabled={readonly}
                placeholder="PLA"
              />
            </div>
            <div>
              <label className="block text-sm font-medium text-white/80 mb-2">
                Nozzle Temperature
              </label>
              <Input
                variant="glass"
                value={settings.nozzleTemp}
                onChange={(e) => handleChange("nozzleTemp", e.target.value)}
                disabled={readonly}
                placeholder="210°C"
              />
            </div>
            <div>
              <label className="block text-sm font-medium text-white/80 mb-2">
                Bed Temperature
              </label>
              <Input
                variant="glass"
                value={settings.bedTemp}
                onChange={(e) => handleChange("bedTemp", e.target.value)}
                disabled={readonly}
                placeholder="60°C"
              />
            </div>
          </CardContent>
        </Card>

        <Card variant="glass" className="border-white/20">
          <CardContent className="pt-6 space-y-4">
            <h4 className="font-medium text-white">Advanced Settings</h4>
            <div>
              <label className="block text-sm font-medium text-white/80 mb-2">
                Nozzle Size
              </label>
              <Input
                variant="glass"
                value={settings.nozzleSize}
                onChange={(e) => handleChange("nozzleSize", e.target.value)}
                disabled={readonly}
                placeholder="0.4mm"
              />
            </div>
            <div>
              <label className="block text-sm font-medium text-white/80 mb-2">
                Retraction Distance
              </label>
              <Input
                variant="glass"
                value={settings.retraction}
                onChange={(e) => handleChange("retraction", e.target.value)}
                disabled={readonly}
                placeholder="6mm"
              />
            </div>
            <div>
              <label className="block text-sm font-medium text-white/80 mb-2">
                Build Plate Adhesion
              </label>
              <Select
                value={settings.buildPlateAdhesion}
                onValueChange={(v) => handleChange("buildPlateAdhesion", v)}
                disabled={readonly}
              >
                <SelectTrigger variant="glass" className="w-full">
                  <SelectValue placeholder="Select adhesion" />
                </SelectTrigger>
                <SelectContent variant="glass">
                  {BUILD_PLATE_OPTIONS.map((opt) => (
                    <SelectItem key={opt.value} value={opt.value}>
                      {opt.label}
                    </SelectItem>
                  ))}
                </SelectContent>
              </Select>
            </div>
          </CardContent>
        </Card>
      </div>

      <div>
        <label className="block text-sm font-medium text-white/80 mb-2">
          Additional Notes
        </label>
        <Textarea
          variant="glass"
          value={settings.notes ?? ""}
          onChange={(e) => handleChange("notes", e.target.value)}
          disabled={readonly}
          rows={3}
          className="resize-none"
          placeholder="Any special printing instructions or tips..."
        />
      </div>

      <Card
        variant="glass"
        className="border-white/20 border-blue-500/30 bg-blue-500/10"
      >
        <CardContent className="pt-6">
          <h4 className="font-medium text-blue-300 mb-2">Quick Reference</h4>
          <div className="grid grid-cols-2 md:grid-cols-4 gap-4 text-sm">
            <div>
              <span className="text-blue-400 font-medium">Layer:</span>
              <span className="ml-1 text-blue-200">{settings.layerHeight}</span>
            </div>
            <div>
              <span className="text-blue-400 font-medium">Infill:</span>
              <span className="ml-1 text-blue-200">{settings.infill}</span>
            </div>
            <div>
              <span className="text-blue-400 font-medium">Speed:</span>
              <span className="ml-1 text-blue-200">{settings.printSpeed}</span>
            </div>
            <div>
              <span className="text-blue-400 font-medium">Supports:</span>
              <span className="ml-1 text-blue-200">
                {settings.supports ? "Yes" : "No"}
              </span>
            </div>
          </div>
        </CardContent>
      </Card>
    </div>
  );
}
