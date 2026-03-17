"use client";

import { useState, useRef } from "react";
import { Plus, Pencil, Trash2, Activity, Link2 } from "lucide-react";
import { SettingsSection } from "@/components/settings/settings-section";
import { Button } from "@/components/primitives/button";
import { Input } from "@/components/primitives/input";
import { Switch } from "@/components/primitives/switch";
import { Tabs, TabsList, TabsTrigger, TabsContent } from "@/components/primitives/tabs";
import {
  Dialog,
  DialogContent,
  DialogDescription,
  DialogFooter,
  DialogHeader,
  DialogTitle,
} from "@/components/ui/dialog";
import {
  getFederatedInstances,
  createFederatedInstance,
  updateFederatedInstance,
  deleteFederatedInstance,
  getFederationHealth,
  initiateHandshake,
} from "@/lib/services/adminService";
import { useAdminQuery } from "@/lib/hooks/use-admin-query";
import { useAdminMutation } from "@/lib/hooks/use-admin-mutation";
import { formatDate } from "@/lib/utils/format";
import {
  CreateFederatedInstanceRequest,
  UpdateFederatedInstanceRequest,
  InitiateHandshakeRequest,
} from "@/lib/api/client";
import { toast } from "sonner";
import type { FederatedInstanceDto } from "@/lib/api/client";

const HEALTH_COOLDOWN_MS = 10000;

function statusBadgeClass(status?: string): string {
  switch (status?.toLowerCase()) {
    case "active":
      return "bg-green-500/20 text-green-400";
    case "pending":
      return "bg-yellow-500/20 text-yellow-400";
    case "error":
      return "bg-red-500/20 text-red-400";
    case "disabled":
      return "bg-white/10 text-white/60";
    default:
      return "bg-white/10 text-white/60";
  }
}

function InstancesTab() {
  const {
    data: instances,
    isLoading,
    error,
    refetch,
  } = useAdminQuery(getFederatedInstances);

  const createMutation = useAdminMutation(
    (r: CreateFederatedInstanceRequest) => createFederatedInstance(r),
    { onSuccess: refetch, successMessage: "Instance added" }
  );
  const updateMutation = useAdminMutation(
    ([id, r]: [string, UpdateFederatedInstanceRequest]) =>
      updateFederatedInstance(id, r),
    { onSuccess: refetch, successMessage: "Instance updated" }
  );
  const deleteMutation = useAdminMutation(
    (id: string) => deleteFederatedInstance(id),
    { onSuccess: refetch, successMessage: "Instance deleted" }
  );

  const [addOpen, setAddOpen] = useState(false);
  const [editTarget, setEditTarget] = useState<FederatedInstanceDto | null>(null);
  const [deleteTarget, setDeleteTarget] = useState<FederatedInstanceDto | null>(null);
  const [healthInstanceId, setHealthInstanceId] = useState<string | null>(null);
  const healthCooldownRef = useRef<Record<string, number>>({});

  const [addForm, setAddForm] = useState({
    name: "",
    baseUrl: "",
    description: "",
    publicKey: "",
    sharedSecret: "",
    isEnabled: true,
  });
  const [editForm, setEditForm] = useState({
    name: "",
    baseUrl: "",
    description: "",
    isEnabled: true,
  });

  const canRunHealthCheck = (id: string) => {
    const last = healthCooldownRef.current[id];
    return !last || Date.now() - last > HEALTH_COOLDOWN_MS;
  };

  const runHealthCheck = async (instance: FederatedInstanceDto) => {
    if (!instance.id) return;
    if (!canRunHealthCheck(instance.id)) {
      toast.error("Please wait 10 seconds between health checks");
      return;
    }
    setHealthInstanceId(instance.id);
    healthCooldownRef.current[instance.id] = Date.now();
    try {
      const res = await getFederationHealth(instance.id);
      if (res.isReachable) {
        toast.success(`Healthy: ${res.responseTimeMs ?? 0}ms`);
      } else {
        toast.error(res.errorMessage ?? "Instance unreachable");
      }
    } catch {
      toast.error("Health check failed");
    } finally {
      setHealthInstanceId(null);
    }
  };

  const handleCreate = async () => {
    if (!addForm.name.trim() || !addForm.baseUrl.trim()) return;
    await createMutation.mutate(
      new CreateFederatedInstanceRequest({
        name: addForm.name.trim(),
        baseUrl: addForm.baseUrl.trim(),
        description: addForm.description.trim() || undefined,
        publicKey: addForm.publicKey.trim() || undefined,
        sharedSecret: addForm.sharedSecret.trim() || undefined,
        isEnabled: addForm.isEnabled,
      })
    );
    setAddOpen(false);
    setAddForm({
      name: "",
      baseUrl: "",
      description: "",
      publicKey: "",
      sharedSecret: "",
      isEnabled: true,
    });
  };

  const handleUpdate = async () => {
    if (!editTarget?.id) return;
    await updateMutation.mutate([
      editTarget.id,
      new UpdateFederatedInstanceRequest({
        name: editForm.name.trim(),
        baseUrl: editForm.baseUrl.trim(),
        description: editForm.description.trim() || undefined,
        isEnabled: editForm.isEnabled,
      }),
    ]);
    setEditTarget(null);
  };

  const handleDelete = async () => {
    if (!deleteTarget?.id) return;
    await deleteMutation.mutate(deleteTarget.id);
    setDeleteTarget(null);
  };

  const openEdit = (inst: FederatedInstanceDto) => {
    setEditTarget(inst);
    setEditForm({
      name: inst.name ?? "",
      baseUrl: inst.baseUrl ?? "",
      description: inst.description ?? "",
      isEnabled: inst.isEnabled ?? true,
    });
  };

  const list = instances ?? [];

  return (
    <SettingsSection
      title="Federated Instances"
      description="Manage connected federation instances"
    >
      {error && (
        <div className="rounded-lg border border-red-500/50 bg-red-500/10 px-4 py-3 text-red-400 mb-4">
          {error}
        </div>
      )}

      <div className="flex justify-end mb-4">
        <Button variant="glass" onClick={() => setAddOpen(true)}>
          <Plus className="h-4 w-4 mr-2" />
          Add Instance
        </Button>
      </div>

      {isLoading ? (
        <div className="text-white/60 py-8">Loading instances...</div>
      ) : list.length === 0 ? (
        <div className="rounded-lg border border-white/10 glass-bg p-8 text-center text-white/60">
          No federated instances. Add one to enable federation.
        </div>
      ) : (
        <div className="rounded-lg border border-white/10 overflow-hidden">
          <table className="w-full">
            <thead>
              <tr className="border-b border-white/10 bg-white/5">
                <th className="text-left py-3 px-4 text-sm font-medium text-white/80">
                  Name
                </th>
                <th className="text-left py-3 px-4 text-sm font-medium text-white/80">
                  Base URL
                </th>
                <th className="text-left py-3 px-4 text-sm font-medium text-white/80">
                  Status
                </th>
                <th className="text-left py-3 px-4 text-sm font-medium text-white/80">
                  Last Sync
                </th>
                <th className="text-right py-3 px-4 text-sm font-medium text-white/80">
                  Actions
                </th>
              </tr>
            </thead>
            <tbody>
              {list.map((inst) => (
                <tr
                  key={inst.id}
                  className="border-b border-white/5 hover:bg-white/5"
                >
                  <td className="py-3 px-4 text-white">{inst.name}</td>
                  <td className="py-3 px-4 text-white/80 font-mono text-sm">
                    {inst.baseUrl}
                  </td>
                  <td className="py-3 px-4">
                    <span
                      className={`rounded px-2 py-1 text-xs ${statusBadgeClass(
                        inst.isEnabled === false ? "disabled" : inst.status
                      )}`}
                    >
                      {inst.isEnabled === false
                        ? "Disabled"
                        : inst.status ?? "Unknown"}
                    </span>
                  </td>
                  <td className="py-3 px-4 text-white/60 text-sm">
                    {formatDate(inst.lastSyncAt)}
                  </td>
                  <td className="py-3 px-4 text-right">
                    <div className="flex justify-end gap-2">
                      <Button
                        variant="ghost"
                        size="icon"
                        title="Health check"
                        onClick={() => runHealthCheck(inst)}
                        disabled={
                          healthInstanceId === inst.id ||
                          !canRunHealthCheck(inst.id ?? "")
                        }
                        className="text-white/80 hover:text-white"
                      >
                        <Activity className="h-4 w-4" />
                      </Button>
                      <Button
                        variant="ghost"
                        size="icon"
                        onClick={() => openEdit(inst)}
                        className="text-white/80 hover:text-white"
                      >
                        <Pencil className="h-4 w-4" />
                      </Button>
                      <Button
                        variant="ghost"
                        size="icon"
                        onClick={() => setDeleteTarget(inst)}
                        className="text-red-400 hover:text-red-300"
                      >
                        <Trash2 className="h-4 w-4" />
                      </Button>
                    </div>
                  </td>
                </tr>
              ))}
            </tbody>
          </table>
        </div>
      )}

      <Dialog open={addOpen} onOpenChange={setAddOpen}>
        <DialogContent variant="glass" className="sm:max-w-md">
          <DialogHeader>
            <DialogTitle>Add Federated Instance</DialogTitle>
            <DialogDescription>
              Add a remote instance to connect with.
            </DialogDescription>
          </DialogHeader>
          <div className="space-y-4">
            <div>
              <label className="text-sm font-medium text-white/80">Name</label>
              <Input
                variant="glass"
                value={addForm.name}
                onChange={(e) => setAddForm((p) => ({ ...p, name: e.target.value }))}
                placeholder="Instance name"
                className="mt-1 text-white"
              />
            </div>
            <div>
              <label className="text-sm font-medium text-white/80">
                Base URL
              </label>
              <Input
                variant="glass"
                value={addForm.baseUrl}
                onChange={(e) =>
                  setAddForm((p) => ({ ...p, baseUrl: e.target.value }))
                }
                placeholder="https://example.com"
                className="mt-1 text-white"
              />
            </div>
            <div>
              <label className="text-sm font-medium text-white/80">
                Description
              </label>
              <Input
                variant="glass"
                value={addForm.description}
                onChange={(e) =>
                  setAddForm((p) => ({ ...p, description: e.target.value }))
                }
                placeholder="Optional"
                className="mt-1 text-white"
              />
            </div>
            <div>
              <label className="text-sm font-medium text-white/80">
                Public Key
              </label>
              <Input
                variant="glass"
                value={addForm.publicKey}
                onChange={(e) =>
                  setAddForm((p) => ({ ...p, publicKey: e.target.value }))
                }
                placeholder="Optional"
                className="mt-1 text-white"
              />
            </div>
            <div>
              <label className="text-sm font-medium text-white/80">
                Shared Secret
              </label>
              <Input
                variant="glass"
                type="password"
                value={addForm.sharedSecret}
                onChange={(e) =>
                  setAddForm((p) => ({ ...p, sharedSecret: e.target.value }))
                }
                placeholder="Write-only, never displayed"
                className="mt-1 text-white"
              />
            </div>
            <div className="flex items-center gap-2">
              <Switch
                checked={addForm.isEnabled}
                onCheckedChange={(v: boolean) =>
                  setAddForm((p) => ({ ...p, isEnabled: v }))
                }
              />
              <label className="text-sm text-white/80">Enabled</label>
            </div>
          </div>
          <DialogFooter>
            <Button variant="outline" onClick={() => setAddOpen(false)}>
              Cancel
            </Button>
            <Button
              variant="glass"
              onClick={handleCreate}
              disabled={
                createMutation.isLoading ||
                !addForm.name.trim() ||
                !addForm.baseUrl.trim()
              }
            >
              {createMutation.isLoading ? "Adding..." : "Add"}
            </Button>
          </DialogFooter>
        </DialogContent>
      </Dialog>

      <Dialog
        open={!!editTarget}
        onOpenChange={(open) => !open && setEditTarget(null)}
      >
        <DialogContent variant="glass" className="sm:max-w-md">
          <DialogHeader>
            <DialogTitle>Edit Instance</DialogTitle>
            <DialogDescription>
              Update instance configuration.
            </DialogDescription>
          </DialogHeader>
          <div className="space-y-4">
            <div>
              <label className="text-sm font-medium text-white/80">Name</label>
              <Input
                variant="glass"
                value={editForm.name}
                onChange={(e) =>
                  setEditForm((p) => ({ ...p, name: e.target.value }))
                }
                className="mt-1 text-white"
              />
            </div>
            <div>
              <label className="text-sm font-medium text-white/80">
                Base URL
              </label>
              <Input
                variant="glass"
                value={editForm.baseUrl}
                onChange={(e) =>
                  setEditForm((p) => ({ ...p, baseUrl: e.target.value }))
                }
                className="mt-1 text-white"
              />
            </div>
            <div>
              <label className="text-sm font-medium text-white/80">
                Description
              </label>
              <Input
                variant="glass"
                value={editForm.description}
                onChange={(e) =>
                  setEditForm((p) => ({ ...p, description: e.target.value }))
                }
                className="mt-1 text-white"
              />
            </div>
            <div className="flex items-center gap-2">
              <Switch
                checked={editForm.isEnabled}
                onCheckedChange={(v: boolean) =>
                  setEditForm((p) => ({ ...p, isEnabled: v }))
                }
              />
              <label className="text-sm text-white/80">Enabled</label>
            </div>
          </div>
          <DialogFooter>
            <Button variant="outline" onClick={() => setEditTarget(null)}>
              Cancel
            </Button>
            <Button
              variant="glass"
              onClick={handleUpdate}
              disabled={updateMutation.isLoading}
            >
              {updateMutation.isLoading ? "Saving..." : "Save"}
            </Button>
          </DialogFooter>
        </DialogContent>
      </Dialog>

      <Dialog
        open={!!deleteTarget}
        onOpenChange={(open) => !open && setDeleteTarget(null)}
      >
        <DialogContent variant="glass">
          <DialogHeader>
            <DialogTitle>Delete Instance</DialogTitle>
            <DialogDescription>
              Are you sure you want to delete &quot;{deleteTarget?.name}&quot;?
            </DialogDescription>
          </DialogHeader>
          <DialogFooter>
            <Button variant="outline" onClick={() => setDeleteTarget(null)}>
              Cancel
            </Button>
            <Button
              variant="destructive"
              onClick={handleDelete}
              disabled={deleteMutation.isLoading}
            >
              {deleteMutation.isLoading ? "Deleting..." : "Delete"}
            </Button>
          </DialogFooter>
        </DialogContent>
      </Dialog>
    </SettingsSection>
  );
}

function HandshakesTab() {
  const [initiateOpen, setInitiateOpen] = useState(false);
  const [remoteUrl, setRemoteUrl] = useState("");
  const [remoteName, setRemoteName] = useState("");
  const [initiating, setInitiating] = useState(false);

  const initiateMutation = useAdminMutation(
    (r: InitiateHandshakeRequest) => initiateHandshake(r),
    { successMessage: "Handshake initiated" }
  );

  const handleInitiate = async () => {
    if (!remoteUrl.trim()) return;
    setInitiating(true);
    try {
      await initiateMutation.mutate(
        new InitiateHandshakeRequest({
          initiatorUrl: remoteUrl.trim(),
          initiatorName: remoteName.trim() || undefined,
        })
      );
      setInitiateOpen(false);
      setRemoteUrl("");
      setRemoteName("");
    } finally {
      setInitiating(false);
    }
  };

  return (
    <SettingsSection
      title="Handshakes"
      description="Initiate federation handshakes with remote instances"
    >
      <div className="flex justify-end mb-4">
        <Button variant="glass" onClick={() => setInitiateOpen(true)}>
          <Link2 className="h-4 w-4 mr-2" />
          Initiate Handshake
        </Button>
      </div>

      <div className="rounded-lg border border-white/10 glass-bg p-6 text-center text-white/60">
        <p className="mb-2">
          Handshake history is not yet available. Initiate a handshake to connect
          with a remote instance.
        </p>
        <p className="text-sm text-white/40">
          Provide the remote instance base URL and name to begin.
        </p>
      </div>

      <Dialog open={initiateOpen} onOpenChange={setInitiateOpen}>
        <DialogContent variant="glass">
          <DialogHeader>
            <DialogTitle>Initiate Handshake</DialogTitle>
            <DialogDescription>
              Connect with a remote federation instance. Enter the remote
              instance&apos;s base URL and optionally its name.
            </DialogDescription>
          </DialogHeader>
          <div className="space-y-4">
            <div>
              <label className="text-sm font-medium text-white/80">
                Remote instance URL
              </label>
              <Input
                variant="glass"
                value={remoteUrl}
                onChange={(e) => setRemoteUrl(e.target.value)}
                placeholder="https://remote.example.com"
                className="mt-1 text-white"
              />
            </div>
            <div>
              <label className="text-sm font-medium text-white/80">
                Remote instance name
              </label>
              <Input
                variant="glass"
                value={remoteName}
                onChange={(e) => setRemoteName(e.target.value)}
                placeholder="Optional"
                className="mt-1 text-white"
              />
            </div>
          </div>
          <DialogFooter>
            <Button variant="outline" onClick={() => setInitiateOpen(false)}>
              Cancel
            </Button>
            <Button
              variant="glass"
              onClick={handleInitiate}
              disabled={initiating || !remoteUrl.trim()}
            >
              {initiating ? "Initiating..." : "Initiate"}
            </Button>
          </DialogFooter>
        </DialogContent>
      </Dialog>
    </SettingsSection>
  );
}

export function FederationTab() {
  return (
    <div className="space-y-6">
      <h2 className="text-2xl font-bold text-white">Federation</h2>

      <Tabs defaultValue="instances" className="w-full">
        <TabsList variant="glass">
          <TabsTrigger variant="glass" value="instances">
            Instances
          </TabsTrigger>
          <TabsTrigger variant="glass" value="handshakes">
            Handshakes
          </TabsTrigger>
        </TabsList>
        <TabsContent variant="glass" value="instances">
          <InstancesTab />
        </TabsContent>
        <TabsContent variant="glass" value="handshakes">
          <HandshakesTab />
        </TabsContent>
      </Tabs>
    </div>
  );
}
