"use client";

import { useState, useRef, useCallback, useEffect, Suspense } from "react";
import { Canvas, useThree } from "@react-three/fiber";
import { OrbitControls, Stage } from "@react-three/drei";
import * as THREE from "three";
import { STLLoader } from "three/examples/jsm/loaders/STLLoader.js";
import { GLTFLoader } from "three/examples/jsm/loaders/GLTFLoader.js";
import { OBJLoader } from "three/examples/jsm/loaders/OBJLoader.js";
import {
  Dialog,
  DialogContent,
  DialogHeader,
  DialogTitle,
  DialogDescription,
} from "@/components/ui/dialog";
import { Button } from "@/components/primitives/button";
import { Card, CardContent } from "@/components/primitives/card";
import {
  Select,
  SelectContent,
  SelectItem,
  SelectTrigger,
  SelectValue,
} from "@/components/ui/select";
import { Switch } from "@/components/primitives/switch";
import { ImageIcon } from "lucide-react";

interface RenderSettings {
  backgroundColor: string;
  modelColor: string;
  metalness: number;
  roughness: number;
  autoRotate: boolean;
  cameraDistance: number;
  lightIntensity: number;
  lightColor: string;
  viewMode: "solid" | "wireframe" | "points";
}

interface ThumbnailGeneratorProps {
  modelFile: File;
  open: boolean;
  onOpenChange: (open: boolean) => void;
  onThumbnailGenerated: (thumbnailBlob: Blob, fileName: string) => void;
}

function ThumbnailModel({
  file,
  settings,
  onGeometryLoaded,
}: {
  file: File;
  settings: RenderSettings;
  onGeometryLoaded?: (geometry: THREE.BufferGeometry | null) => void;
}) {
  const [centeredGeometry, setCenteredGeometry] =
    useState<THREE.BufferGeometry | null>(null);
  const [isLoading, setIsLoading] = useState(true);

  useEffect(() => {
    if (!file) {
      setCenteredGeometry(null);
      onGeometryLoaded?.(null);
      setIsLoading(false);
      return;
    }

    let cancelled = false;
    const loadModel = async () => {
      setIsLoading(true);
      setCenteredGeometry(null);
      try {
        const ext = file.name.toLowerCase().substring(file.name.lastIndexOf("."));
        let loadedGeometry: THREE.BufferGeometry | null = null;

        if (ext === ".stl") {
          const loader = new STLLoader();
          const arrayBuffer = await file.arrayBuffer();
          if (cancelled) return;
          loadedGeometry = loader.parse(arrayBuffer);
        } else if (ext === ".gltf" || ext === ".glb") {
          const loader = new GLTFLoader();
          const arrayBuffer = await file.arrayBuffer();
          if (cancelled) return;
          const gltf = await new Promise<{ scene: THREE.Group }>(
            (resolve, reject) => {
              loader.parse(arrayBuffer, "", resolve, reject);
            }
          );
          const box = new THREE.Box3().setFromObject(gltf.scene);
          const size = box.getSize(new THREE.Vector3());
          loadedGeometry = new THREE.BoxGeometry(size.x, size.y, size.z);
        } else if (ext === ".obj") {
          const loader = new OBJLoader();
          const arrayBuffer = await file.arrayBuffer();
          if (cancelled) return;
          const object = loader.parse(
            new TextDecoder().decode(arrayBuffer)
          );
          const box = new THREE.Box3().setFromObject(object);
          const size = box.getSize(new THREE.Vector3());
          loadedGeometry = new THREE.BoxGeometry(size.x, size.y, size.z);
        } else if (ext === ".step" || ext === ".stp") {
          loadedGeometry = new THREE.BoxGeometry(1, 1, 1);
        }

        if (loadedGeometry && !cancelled) {
          loadedGeometry.computeBoundingBox();
          const boundingBox = loadedGeometry.boundingBox!;
          const center = boundingBox.getCenter(new THREE.Vector3());
          const centered = loadedGeometry.clone();
          centered.translate(-center.x, -center.y, -center.z);
          const size = boundingBox.getSize(new THREE.Vector3());
          const maxDim = Math.max(size.x, size.y, size.z);
          const scale = 1 / maxDim;
          centered.scale(scale, scale, scale);
          setCenteredGeometry(centered);
          onGeometryLoaded?.(centered);
        }
      } catch (err) {
        console.error("Error loading model:", err);
      } finally {
        if (!cancelled) setIsLoading(false);
      }
    };
    loadModel();
    return () => {
      cancelled = true;
    };
  }, [file, onGeometryLoaded]);

  if (isLoading) return null;
  if (!centeredGeometry) return null;

  if (settings.viewMode === "points") {
    return (
      <group>
        <points geometry={centeredGeometry}>
          <pointsMaterial
            size={0.5}
            color={settings.modelColor}
            sizeAttenuation
          />
        </points>
      </group>
    );
  }

  return (
    <group>
      <mesh geometry={centeredGeometry}>
        <meshStandardMaterial
          color={settings.modelColor}
          metalness={settings.metalness}
          roughness={settings.roughness}
          wireframe={settings.viewMode === "wireframe"}
          side={THREE.DoubleSide}
        />
      </mesh>
    </group>
  );
}

function ThumbnailCameraController({
  settings,
  onRendererReady,
}: {
  settings: RenderSettings;
  onRendererReady: (renderer: THREE.WebGLRenderer) => void;
}) {
  const { camera, gl } = useThree();
  useEffect(() => {
    camera.position.set(0, 0, settings.cameraDistance);
    camera.lookAt(0, 0, 0);
    camera.updateMatrixWorld();
    onRendererReady(gl);
  }, [camera, settings.cameraDistance, gl, onRendererReady]);
  return null;
}

function useScreenshot() {
  const { gl, scene, camera } = useThree();
  return useCallback((): Promise<Blob> => {
    return new Promise((resolve, reject) => {
      try {
        gl.render(scene, camera);
        gl.domElement.toBlob(
          (blob) => {
            if (blob && blob.size > 0) resolve(blob);
            else reject(new Error("Failed to capture screenshot"));
          },
          "image/png",
          0.9
        );
      } catch (e) {
        reject(e);
      }
    });
  }, [gl, scene, camera]);
}

function ScreenshotProvider({
  onReady,
}: {
  onReady: (fn: () => Promise<Blob>) => void;
}) {
  const takeScreenshot = useScreenshot();
  useEffect(() => {
    onReady(takeScreenshot);
  }, [takeScreenshot, onReady]);
  return null;
}

function WebGLMonitor({ onContextLost }: { onContextLost: () => void }) {
  const { gl } = useThree();
  useEffect(() => {
    const handler = () => onContextLost();
    gl.domElement.addEventListener("webglcontextlost", handler);
    return () =>
      gl.domElement.removeEventListener("webglcontextlost", handler);
  }, [gl, onContextLost]);
  return null;
}

const VIEW_MODES = [
  { value: "solid" as const, label: "Solid" },
  { value: "wireframe" as const, label: "Wireframe" },
  { value: "points" as const, label: "Points" },
];

export function ThumbnailGenerator({
  modelFile,
  open,
  onOpenChange,
  onThumbnailGenerated,
}: ThumbnailGeneratorProps) {
  const [prefersReducedMotion, setPrefersReducedMotion] = useState(false);
  useEffect(() => {
    if (typeof window === "undefined") return;
    const mq = window.matchMedia("(prefers-reduced-motion: reduce)");
    setPrefersReducedMotion(mq.matches);
    const handler = () => setPrefersReducedMotion(mq.matches);
    mq.addEventListener("change", handler);
    return () => mq.removeEventListener("change", handler);
  }, []);

  const [renderSettings, setRenderSettings] = useState<RenderSettings>({
    backgroundColor: "#1a1a1a",
    modelColor: "#888888",
    metalness: 0.5,
    roughness: 0.5,
    autoRotate: false,
    cameraDistance: 2.5,
    lightIntensity: 1.0,
    lightColor: "#ffffff",
    viewMode: "solid",
  });

  const [isGenerating, setIsGenerating] = useState(false);
  const [webglError, setWebglError] = useState<string | null>(null);
  const takeScreenshotRef = useRef<(() => Promise<Blob>) | null>(null);
  const [cameraReady, setCameraReady] = useState(false);

  useEffect(() => {
    if (!open) return;
    const canvas = document.createElement("canvas");
    const gl = canvas.getContext("webgl") ?? canvas.getContext("webgl2");
    if (!gl) {
      setWebglError("WebGL is not supported");
    } else {
      setWebglError(null);
    }
  }, [open]);

  const handleSettingChange = (key: keyof RenderSettings, value: unknown) => {
    setRenderSettings((prev) => ({ ...prev, [key]: value }));
  };

  const handleRefresh = () => {
    setWebglError(null);
    setCameraReady(false);
  };

  const handleGenerate = useCallback(async () => {
    if (!takeScreenshotRef.current || !cameraReady) return;
    setIsGenerating(true);
    try {
      await new Promise((r) => setTimeout(r, 300));
      const blob = await takeScreenshotRef.current();
      const fileName = `${modelFile.name.replace(/\.[^/.]+$/, "")}-thumbnail.png`;
      onThumbnailGenerated(blob, fileName);
      onOpenChange(false);
    } catch (err) {
      setWebglError(
        err instanceof Error ? err.message : "Failed to generate thumbnail"
      );
    } finally {
      setIsGenerating(false);
    }
  }, [cameraReady, modelFile.name, onThumbnailGenerated, onOpenChange]);

  return (
    <Dialog open={open} onOpenChange={onOpenChange}>
      <DialogContent
        variant="glass"
        className="max-w-4xl max-h-[90vh] flex flex-col overflow-hidden"
      >
        <DialogHeader>
          <DialogTitle id="thumbnail-dialog-title">
            Generate Custom Thumbnail
          </DialogTitle>
          <DialogDescription id="thumbnail-dialog-description">
            Preview and capture an image from your 3D model
          </DialogDescription>
        </DialogHeader>

        <div className="grid grid-cols-1 lg:grid-cols-3 gap-6 flex-1 min-h-0 overflow-y-auto">
          <div className="lg:col-span-2">
            <Card
              variant="glass"
              className="border-white/20 overflow-hidden h-64 lg:h-96"
            >
              <Canvas
                camera={{
                  position: [0, 0, renderSettings.cameraDistance],
                  fov: 45,
                }}
                gl={{ preserveDrawingBuffer: true }}
                style={{
                  background: renderSettings.backgroundColor,
                  width: "100%",
                  height: "100%",
                }}
              >
                <ThumbnailCameraController
                  settings={renderSettings}
                  onRendererReady={() => {
                    setCameraReady(true);
                  }}
                />
                <ScreenshotProvider
                  onReady={(fn) => {
                    takeScreenshotRef.current = fn;
                  }}
                />
                <WebGLMonitor
                  onContextLost={() =>
                    setWebglError("WebGL context was lost")
                  }
                />
                <ambientLight intensity={0.4} />
                <directionalLight
                  position={[10, 10, 5]}
                  intensity={renderSettings.lightIntensity}
                  color={renderSettings.lightColor}
                />
                <Suspense fallback={null}>
                  <Stage environment="city" intensity={0.6}>
                    <ThumbnailModel file={modelFile} settings={renderSettings} />
                  </Stage>
                </Suspense>
                <OrbitControls
                  autoRotate={renderSettings.autoRotate}
                  autoRotateSpeed={1}
                  enableZoom
                  enablePan
                  enableRotate
                  maxDistance={10}
                  minDistance={0.5}
                  target={[0, 0, 0]}
                />
              </Canvas>
            </Card>
          </div>

          <Card variant="glass" className="border-white/20 h-fit">
            <CardContent className="pt-6 space-y-4">
              <h3 className="text-lg font-semibold text-white">
                Render Settings
              </h3>

              <div>
                <label className="block text-sm text-white/80 mb-1">
                  Background
                </label>
                <input
                  type="color"
                  value={renderSettings.backgroundColor}
                  onChange={(e) =>
                    handleSettingChange("backgroundColor", e.target.value)
                  }
                  className="w-full h-10 rounded border border-white/20 bg-white/10 cursor-pointer"
                  aria-label="Background color"
                />
              </div>

              <div>
                <label className="block text-sm text-white/80 mb-1">
                  Model Color
                </label>
                <input
                  type="color"
                  value={renderSettings.modelColor}
                  onChange={(e) =>
                    handleSettingChange("modelColor", e.target.value)
                  }
                  className="w-full h-10 rounded border border-white/20 bg-white/10 cursor-pointer"
                  aria-label="Model color"
                />
              </div>

              <div>
                <label className="block text-sm text-white/80 mb-1">
                  Metalness: {renderSettings.metalness}
                </label>
                <input
                  type="range"
                  min={0}
                  max={1}
                  step={0.1}
                  value={renderSettings.metalness}
                  onChange={(e) =>
                    handleSettingChange(
                      "metalness",
                      parseFloat(e.target.value)
                    )
                  }
                  className="w-full accent-white/80"
                />
              </div>

              <div>
                <label className="block text-sm text-white/80 mb-1">
                  Roughness: {renderSettings.roughness}
                </label>
                <input
                  type="range"
                  min={0}
                  max={1}
                  step={0.1}
                  value={renderSettings.roughness}
                  onChange={(e) =>
                    handleSettingChange(
                      "roughness",
                      parseFloat(e.target.value)
                    )
                  }
                  className="w-full accent-white/80"
                />
              </div>

              <div>
                <label className="block text-sm text-white/80 mb-1">
                  View Mode
                </label>
                <Select
                  value={renderSettings.viewMode}
                  onValueChange={(v) =>
                    handleSettingChange("viewMode", v as RenderSettings["viewMode"])
                  }
                >
                  <SelectTrigger variant="glass" className="w-full">
                    <SelectValue />
                  </SelectTrigger>
                  <SelectContent variant="glass">
                    {VIEW_MODES.map((m) => (
                      <SelectItem key={m.value} value={m.value}>
                        {m.label}
                      </SelectItem>
                    ))}
                  </SelectContent>
                </Select>
              </div>

              <div>
                <label className="block text-sm text-white/80 mb-1">
                  Camera Distance: {renderSettings.cameraDistance}
                </label>
                <input
                  type="range"
                  min={1}
                  max={5}
                  step={0.1}
                  value={renderSettings.cameraDistance}
                  onChange={(e) =>
                    handleSettingChange(
                      "cameraDistance",
                      parseFloat(e.target.value)
                    )
                  }
                  className="w-full accent-white/80"
                />
              </div>

              <div>
                <label className="block text-sm text-white/80 mb-1">
                  Light Intensity: {renderSettings.lightIntensity}
                </label>
                <input
                  type="range"
                  min={0}
                  max={2}
                  step={0.1}
                  value={renderSettings.lightIntensity}
                  onChange={(e) =>
                    handleSettingChange(
                      "lightIntensity",
                      parseFloat(e.target.value)
                    )
                  }
                  className="w-full accent-white/80"
                />
              </div>

              <div>
                <label className="block text-sm text-white/80 mb-1">
                  Light Color
                </label>
                <input
                  type="color"
                  value={renderSettings.lightColor}
                  onChange={(e) =>
                    handleSettingChange("lightColor", e.target.value)
                  }
                  className="w-full h-10 rounded border border-white/20 bg-white/10 cursor-pointer"
                  aria-label="Light color"
                />
              </div>

              <div className="flex items-center gap-2">
                <Switch
                  checked={renderSettings.autoRotate}
                  onCheckedChange={(v) =>
                    handleSettingChange("autoRotate", v)
                  }
                  disabled={prefersReducedMotion}
                  aria-label="Auto rotate 3D preview"
                />
                <span className="text-sm text-white/80">
                  Auto Rotate
                  {prefersReducedMotion && (
                    <span className="block text-xs text-white/50">
                      (disabled for reduced motion preference)
                    </span>
                  )}
                </span>
              </div>

              {webglError && (
                <div className="p-3 rounded-lg border border-red-500/50 bg-red-500/20 text-red-200 text-sm">
                  <strong>Error:</strong> {webglError}
                  <Button
                    variant="outline"
                    size="sm"
                    className="mt-2 w-full"
                    onClick={handleRefresh}
                  >
                    Refresh Canvas
                  </Button>
                </div>
              )}

              <div className="space-y-2 pt-2">
                <Button
                  variant="glass"
                  className="w-full"
                  onClick={handleGenerate}
                  disabled={isGenerating || !cameraReady}
                >
                  <ImageIcon className="h-4 w-4 mr-2" />
                  {isGenerating ? "Generating..." : "Generate Image"}
                </Button>
                <Button
                  variant="outline"
                  className="w-full"
                  onClick={() => onOpenChange(false)}
                >
                  Cancel
                </Button>
              </div>
            </CardContent>
          </Card>
        </div>
      </DialogContent>
    </Dialog>
  );
}
