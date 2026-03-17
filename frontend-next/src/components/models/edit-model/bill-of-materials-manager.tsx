"use client";

import { useState } from "react";
import { Button } from "@/components/primitives/button";
import { Input } from "@/components/primitives/input";
import { Textarea } from "@/components/ui/glass/textarea";
import { Card, CardContent } from "@/components/primitives/card";
import { Switch } from "@/components/primitives/switch";
import { Package, Pencil, Trash2, ExternalLink } from "lucide-react";

export interface BillOfMaterial {
  id: string;
  name: string;
  quantity: number;
  price?: number;
  url?: string;
  required: boolean;
  notes?: string;
}

interface BillOfMaterialsManagerProps {
  billOfMaterials: BillOfMaterial[];
  onUpdate: (bom: BillOfMaterial[]) => void;
  readonly?: boolean;
}

function formatPrice(price: number | undefined): string {
  if (!price && price !== 0) return "N/A";
  return `$${price.toFixed(2)}`;
}

function getTotalCost(bom: BillOfMaterial[]): number {
  return bom.reduce((total, item) => {
    return total + ((item.price ?? 0) * item.quantity);
  }, 0);
}

interface BOMItemFormProps {
  item: Partial<BillOfMaterial>;
  onSave: (item: BillOfMaterial) => void;
  onCancel: () => void;
  readonly?: boolean;
}

function BOMItemForm({
  item,
  onSave,
  onCancel,
  readonly = false,
}: BOMItemFormProps) {
  const [name, setName] = useState(item.name ?? "");
  const [quantity, setQuantity] = useState(item.quantity ?? 1);
  const [price, setPrice] = useState(item.price !== undefined ? String(item.price) : "");
  const [url, setUrl] = useState(item.url ?? "");
  const [required, setRequired] = useState(item.required ?? true);
  const [notes, setNotes] = useState(item.notes ?? "");

  const handleSave = () => {
    if (!name.trim()) return;
    onSave({
      id: item.id ?? Math.random().toString(36).substring(2, 11),
      name: name.trim(),
      quantity: Math.max(1, quantity),
      price: price ? parseFloat(price) : undefined,
      url: url.trim() || undefined,
      required,
      notes: notes.trim() || undefined,
    });
  };

  return (
    <Card variant="glass" className="border-white/20">
      <CardContent className="pt-6 space-y-4">
        <div className="grid grid-cols-1 md:grid-cols-2 gap-4">
          <div>
            <label className="block text-sm font-medium text-white mb-2">
              Item Name *
            </label>
            <Input
              variant="glass"
              value={name}
              onChange={(e) => setName(e.target.value)}
              placeholder="e.g., PLA Filament"
            />
          </div>
          <div>
            <label className="block text-sm font-medium text-white mb-2">
              Quantity
            </label>
            <Input
              variant="glass"
              type="number"
              min={1}
              value={quantity}
              onChange={(e) => setQuantity(parseInt(e.target.value, 10) || 1)}
            />
          </div>
          <div>
            <label className="block text-sm font-medium text-white mb-2">
              Price (per unit)
            </label>
            <Input
              variant="glass"
              type="number"
              step="0.01"
              min={0}
              value={price}
              onChange={(e) => setPrice(e.target.value)}
              placeholder="0.00"
            />
          </div>
          <div>
            <label className="block text-sm font-medium text-white mb-2">
              Purchase URL
            </label>
            <Input
              variant="glass"
              type="url"
              value={url}
              onChange={(e) => setUrl(e.target.value)}
              placeholder="https://..."
            />
          </div>
        </div>
        <div>
          <label className="block text-sm font-medium text-white mb-2">
            Notes
          </label>
          <Textarea
            variant="glass"
            value={notes}
            onChange={(e) => setNotes(e.target.value)}
            placeholder="Optional notes..."
            rows={2}
            className="resize-none"
          />
        </div>
        <div className="flex items-center gap-2">
          <Switch
            checked={required}
            onCheckedChange={setRequired}
            disabled={readonly}
          />
          <span className="text-sm text-white/80">Required item</span>
        </div>
        {!readonly && (
          <div className="flex justify-end gap-2 pt-2">
            <Button variant="outline" onClick={onCancel}>
              Cancel
            </Button>
            <Button variant="glass" onClick={handleSave} disabled={!name.trim()}>
              Save
            </Button>
          </div>
        )}
      </CardContent>
    </Card>
  );
}

export function BillOfMaterialsManager({
  billOfMaterials,
  onUpdate,
  readonly = false,
}: BillOfMaterialsManagerProps) {
  const [editingItem, setEditingItem] = useState<BillOfMaterial | null>(null);
  const [showAddForm, setShowAddForm] = useState(false);

  const handleAddItem = (item: BillOfMaterial) => {
    onUpdate([...billOfMaterials, item]);
    setShowAddForm(false);
  };

  const handleUpdateItem = (updatedItem: BillOfMaterial) => {
    onUpdate(
      billOfMaterials.map((i) => (i.id === updatedItem.id ? updatedItem : i))
    );
    setEditingItem(null);
  };

  const handleDeleteItem = (id: string) => {
    const item = billOfMaterials.find((i) => i.id === id);
    const name = item?.name ?? "this item";
    if (window.confirm(`Delete "${name}" from the bill of materials?`)) {
      onUpdate(billOfMaterials.filter((i) => i.id !== id));
      if (editingItem?.id === id) setEditingItem(null);
    }
  };

  const requiredCount = billOfMaterials.filter((i) => i.required).length;

  return (
    <div className="space-y-4" role="region" aria-label="Bill of materials">
      <div className="flex flex-col sm:flex-row sm:items-center sm:justify-between gap-4">
        <h3 className="text-lg font-semibold text-white">Bill of Materials</h3>
        {!readonly && (
          <Button
            variant="glass"
            size="sm"
            onClick={() => setShowAddForm(true)}
            disabled={showAddForm}
          >
            <Package className="h-4 w-4 mr-2" />
            Add Item
          </Button>
        )}
      </div>

      <Card variant="glass" className="border-white/20">
        <CardContent className="pt-6">
          <div
            className="grid grid-cols-1 sm:grid-cols-3 gap-4 text-sm"
            role="group"
            aria-label="BOM summary"
          >
            <div>
              <span className="text-white/60">Total Items:</span>
              <span className="ml-2 font-semibold text-white">
                {billOfMaterials.length}
              </span>
            </div>
            <div>
              <span className="text-white/60">Required Items:</span>
              <span className="ml-2 font-semibold text-white">
                {requiredCount}
              </span>
            </div>
            <div>
              <span className="text-white/60">Total Cost:</span>
              <span className="ml-2 font-semibold text-white">
                {formatPrice(getTotalCost(billOfMaterials))}
              </span>
            </div>
          </div>
        </CardContent>
      </Card>

      {showAddForm && !readonly && (
        <div className="space-y-2">
          <h4 className="text-md font-medium text-white">Add New Item</h4>
          <BOMItemForm
            item={{}}
            onSave={handleAddItem}
            onCancel={() => setShowAddForm(false)}
          />
        </div>
      )}

      <div className="space-y-2" role="list" aria-label="BOM items list">
        {billOfMaterials.map((item) => (
          <div key={item.id} role="listitem">
            {editingItem?.id === item.id ? (
              <BOMItemForm
                item={item}
                onSave={handleUpdateItem}
                onCancel={() => setEditingItem(null)}
              />
            ) : (
              <Card variant="glass" className="border-white/20">
                <CardContent className="pt-4 pb-4">
                  <div className="flex flex-col sm:flex-row sm:items-center sm:justify-between gap-4">
                    <div className="flex-1 min-w-0">
                      <div className="flex flex-wrap items-center gap-2">
                        <h4 className="font-medium text-white">{item.name}</h4>
                        {item.required && (
                          <span className="rounded-full bg-red-500/20 px-2 py-0.5 text-xs text-red-300">
                            Required
                          </span>
                        )}
                      </div>
                      <div className="mt-1 text-sm text-white/60 space-x-4">
                        <span>Quantity: {item.quantity}</span>
                        {item.price !== undefined && (
                          <>
                            <span>
                              Unit: {formatPrice(item.price)} • Total:{" "}
                              {formatPrice(item.price * item.quantity)}
                            </span>
                          </>
                        )}
                      </div>
                      {item.notes && (
                        <p className="mt-1 text-sm text-white/50">{item.notes}</p>
                      )}
                    </div>
                    <div className="flex items-center gap-2 shrink-0">
                      {item.url && (
                        <Button
                          variant="outline"
                          size="sm"
                          asChild
                          className="min-h-[32px]"
                        >
                          <a
                            href={item.url}
                            target="_blank"
                            rel="noopener noreferrer"
                            aria-label={`Buy ${item.name}`}
                          >
                            <ExternalLink className="h-4 w-4" />
                            Buy
                          </a>
                        </Button>
                      )}
                      {!readonly && (
                        <>
                          <Button
                            variant="outline"
                            size="icon"
                            className="h-9 w-9 min-h-[32px]"
                            onClick={() => setEditingItem(item)}
                            aria-label={`Edit ${item.name}`}
                          >
                            <Pencil className="h-4 w-4" />
                          </Button>
                          <Button
                            variant="destructive"
                            size="icon"
                            className="h-9 w-9 min-h-[32px]"
                            onClick={() => handleDeleteItem(item.id)}
                            aria-label={`Delete ${item.name}`}
                          >
                            <Trash2 className="h-4 w-4" />
                          </Button>
                        </>
                      )}
                    </div>
                  </div>
                </CardContent>
              </Card>
            )}
          </div>
        ))}
      </div>

      {billOfMaterials.length === 0 && !showAddForm && (
        <Card variant="glass" className="border-white/20">
          <CardContent className="py-12 text-center">
            <Package className="mx-auto h-12 w-12 text-white/40 mb-4" />
            <p className="text-white/80 mb-2">No materials added yet</p>
            {!readonly && (
              <p className="text-sm text-white/60 mb-6">
                Click &quot;Add Item&quot; to start building your bill of materials
              </p>
            )}
            {!readonly && (
              <Button variant="glass" onClick={() => setShowAddForm(true)}>
                Add Item
              </Button>
            )}
          </CardContent>
        </Card>
      )}
    </div>
  );
}
