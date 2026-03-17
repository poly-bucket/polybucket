"use client";

import { useState, useEffect, useRef, useCallback } from "react";
import { toast } from "sonner";
import {
  type Model,
  type IModelVersion,
  UpdateModelVersionRequest,
} from "@/lib/api/client";
import { ApiClientFactory } from "@/lib/api/clientFactory";
import { Input } from "@/components/primitives/input";
import { Textarea } from "@/components/ui/glass/textarea";
import { Button } from "@/components/primitives/button";
import { Card, CardContent } from "@/components/primitives/card";
import {
  Select,
  SelectContent,
  SelectItem,
  SelectTrigger,
  SelectValue,
} from "@/components/ui/select";
import { Tabs, TabsList, TabsTrigger, TabsContent } from "@/components/primitives/tabs";
import { useDebouncedValue } from "@/lib/hooks/use-debounced-value";
import { BillOfMaterialsManager, type BillOfMaterial } from "./bill-of-materials-manager";
import { PrintSettingsEditor, type PrintSettings } from "./print-settings-editor";
import { FileText, Package, Settings } from "lucide-react";

export interface ExtendedModelVersion extends IModelVersion {
  billOfMaterials?: BillOfMaterial[];
  printSettings?: PrintSettings;
}

interface VersionEditorProps {
  model: Model;
  onVersionUpdate: (versionId: string, updated: ExtendedModelVersion) => void;
  onCancel: () => void;
}

function defaultPrintSettings(versionId: string): PrintSettings {
  return {
    id: versionId,
    layerHeight: "0.2mm",
    infill: "15%",
    supports: false,
    printSpeed: "50mm/s",
    nozzleTemp: "210°C",
    bedTemp: "60°C",
    material: "PLA",
    nozzleSize: "0.4mm",
    retraction: "6mm",
    buildPlateAdhesion: "skirt",
    notes: "",
  };
}

export function VersionEditor({
  model,
  onVersionUpdate,
  onCancel,
}: VersionEditorProps) {
  const [extendedData, setExtendedData] = useState<
    Record<string, { billOfMaterials: BillOfMaterial[]; printSettings: PrintSettings }>
  >({});

  const versions: ExtendedModelVersion[] = (model.versions ?? []).map((v) => {
    const vid = v.id ?? "";
    const ext = extendedData[vid];
    return {
      ...v,
      billOfMaterials: ext?.billOfMaterials ?? [],
      printSettings: ext?.printSettings ?? defaultPrintSettings(vid),
    };
  });

  const [selectedVersionId, setSelectedVersionId] = useState<string>(
    versions.length > 0 ? versions[0].id ?? "" : ""
  );
  const [activeTab, setActiveTab] = useState<"details" | "bom" | "print">("details");
  const [localName, setLocalName] = useState("");
  const [localNotes, setLocalNotes] = useState("");
  const lastSavedRef = useRef<{ name: string; notes: string }>({ name: "", notes: "" });

  const selectedVersion = versions.find((v) => v.id === selectedVersionId);
  const debouncedName = useDebouncedValue(localName, 1000);
  const debouncedNotes = useDebouncedValue(localNotes, 1000);

  useEffect(() => {
    if (selectedVersion) {
      setLocalName(selectedVersion.name ?? "");
      setLocalNotes(selectedVersion.notes ?? "");
      lastSavedRef.current = {
        name: selectedVersion.name ?? "",
        notes: selectedVersion.notes ?? "",
      };
    }
  }, [selectedVersionId, selectedVersion?.id]);

  const saveVersion = useCallback(
    async (name: string, notes: string) => {
      if (!model.id || !selectedVersionId) return;
      if (
        name === lastSavedRef.current.name &&
        notes === lastSavedRef.current.notes
      )
        return;

      try {
        const client = ApiClientFactory.getApiClient();
        const req = UpdateModelVersionRequest.fromJS({ name, notes });
        await client.updateModelVersion_UpdateModelVersion(
          model.id,
          selectedVersionId,
          req
        );
        lastSavedRef.current = { name, notes };
        onVersionUpdate(selectedVersionId, {
          ...selectedVersion!,
          name,
          notes,
        } as ExtendedModelVersion);
      } catch (err) {
        toast.error(err instanceof Error ? err.message : "Failed to save version");
      }
    },
    [model.id, selectedVersionId, selectedVersion, onVersionUpdate]
  );

  useEffect(() => {
    if (
      selectedVersion &&
      selectedVersionId &&
      debouncedName === localName &&
      debouncedNotes === localNotes
    ) {
      saveVersion(debouncedName, debouncedNotes);
    }
  }, [debouncedName, debouncedNotes, localName, localNotes, selectedVersion, selectedVersionId, saveVersion]);

  const handleBOMUpdate = useCallback(
    (bom: BillOfMaterial[]) => {
      setExtendedData((prev) => ({
        ...prev,
        [selectedVersionId]: {
          ...prev[selectedVersionId],
          billOfMaterials: bom,
          printSettings:
            prev[selectedVersionId]?.printSettings ??
            defaultPrintSettings(selectedVersionId),
        },
      }));
    },
    [selectedVersionId]
  );

  const handlePrintSettingsUpdate = useCallback(
    (settings: PrintSettings) => {
      setExtendedData((prev) => ({
        ...prev,
        [selectedVersionId]: {
          ...prev[selectedVersionId],
          billOfMaterials: prev[selectedVersionId]?.billOfMaterials ?? [],
          printSettings: settings,
        },
      }));
    },
    [selectedVersionId]
  );

  if (versions.length === 0) {
    return (
      <Card variant="glass" className="border-white/20">
        <CardContent className="py-12 text-center">
          <FileText className="mx-auto h-12 w-12 text-white/40 mb-4" />
          <h3 className="text-lg font-medium text-white mb-2">No Versions Available</h3>
          <p className="text-white/60 mb-6">
            This model doesn&apos;t have any versions yet. Create one from the New Version tab.
          </p>
          <Button variant="outline" onClick={onCancel}>
            Close
          </Button>
        </CardContent>
      </Card>
    );
  }

  if (!selectedVersion) {
    return (
      <div className="py-8 text-center text-white/60">
        <Button variant="outline" onClick={onCancel}>
          Close
        </Button>
      </div>
    );
  }

  return (
    <div className="space-y-6 py-4">
      <div>
        <label className="block text-sm font-medium text-white mb-2">
          Select Version to Edit
        </label>
        <Select
          value={selectedVersionId}
          onValueChange={setSelectedVersionId}
        >
          <SelectTrigger variant="glass" className="w-full max-w-md">
            <SelectValue placeholder="Select version" />
          </SelectTrigger>
          <SelectContent variant="glass">
            {versions.map((v) => (
              <SelectItem key={v.id} value={v.id!}>
                {v.name ?? "Unnamed"} (v{v.versionNumber ?? "?"})
              </SelectItem>
            ))}
          </SelectContent>
        </Select>
      </div>

      <Tabs value={activeTab} onValueChange={(v) => setActiveTab(v as typeof activeTab)}>
        <TabsList
          variant="glass"
          className="w-full"
          aria-label="Version edit sections"
        >
          <TabsTrigger variant="glass" value="details">
            <FileText className="h-4 w-4 mr-2" />
            Version Details
          </TabsTrigger>
          <TabsTrigger variant="glass" value="bom">
            <Package className="h-4 w-4 mr-2" />
            Bill of Materials
          </TabsTrigger>
          <TabsTrigger variant="glass" value="print">
            <Settings className="h-4 w-4 mr-2" />
            Print Settings
          </TabsTrigger>
        </TabsList>

        <TabsContent value="details" variant="glass" className="mt-4">
          <Card variant="glass" className="border-white/20">
            <CardContent className="pt-6 space-y-4">
              <div className="grid grid-cols-1 sm:grid-cols-2 gap-4">
                <div>
                  <label className="block text-sm font-medium text-white mb-2">
                    Version Name
                  </label>
                  <Input
                    variant="glass"
                    value={localName}
                    onChange={(e) => setLocalName(e.target.value)}
                    placeholder="Version name"
                  />
                </div>
                <div>
                  <label className="block text-sm font-medium text-white mb-2">
                    Version Number
                  </label>
                  <p className="text-white/60 py-2">
                    v{selectedVersion.versionNumber ?? "?"}
                  </p>
                </div>
              </div>
              <div>
                <label className="block text-sm font-medium text-white mb-2">
                  Version Notes
                </label>
                <Textarea
                  variant="glass"
                  value={localNotes}
                  onChange={(e) => setLocalNotes(e.target.value)}
                  placeholder="What's new in this version?"
                  rows={4}
                  className="resize-none"
                />
              </div>
              <div>
                <label className="block text-sm font-medium text-white mb-2">
                  Files in this Version
                </label>
                <div
                  className="rounded-lg border border-white/20 bg-white/5 p-4"
                  role="list"
                  aria-live="polite"
                >
                  {selectedVersion.files && selectedVersion.files.length > 0 ? (
                    <div className="space-y-2">
                      {selectedVersion.files.map((f, i) => (
                        <div
                          key={f.id ?? i}
                          className="flex justify-between text-sm text-white/80"
                        >
                          <span>{f.name ?? `File ${i + 1}`}</span>
                          <span>{f.size ? `${(f.size / 1024).toFixed(1)} KB` : "—"}</span>
                        </div>
                      ))}
                    </div>
                  ) : (
                    <p className="text-sm text-white/40">No files</p>
                  )}
                </div>
              </div>
              <p className="text-xs text-white/40">Changes save automatically</p>
            </CardContent>
          </Card>
        </TabsContent>

        <TabsContent value="bom" variant="glass" className="mt-4">
          <p className="text-xs text-white/40 mb-2">
            Saved locally this session (not persisted to server yet)
          </p>
          <BillOfMaterialsManager
            billOfMaterials={selectedVersion.billOfMaterials ?? []}
            onUpdate={handleBOMUpdate}
          />
        </TabsContent>

        <TabsContent value="print" variant="glass" className="mt-4">
          <p className="text-xs text-white/40 mb-2">
            Saved locally this session (not persisted to server yet)
          </p>
          <PrintSettingsEditor
            printSettings={
              selectedVersion.printSettings ?? defaultPrintSettings(selectedVersionId)
            }
            onUpdate={handlePrintSettingsUpdate}
          />
        </TabsContent>
      </Tabs>

      <div className="flex justify-end pt-6 border-t border-white/10">
        <Button variant="outline" onClick={onCancel}>
          Close
        </Button>
      </div>
    </div>
  );
}
