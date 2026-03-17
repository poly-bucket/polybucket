"use client";

import React, {
  useRef,
  useState,
  useEffect,
  useMemo,
  useCallback,
  startTransition,
  type ComponentProps,
} from "react";
import { Canvas, useFrame, useThree } from "@react-three/fiber";
import { OrbitControls } from "@react-three/drei";
import * as THREE from "three";
import { STLLoader } from "three/examples/jsm/loaders/STLLoader.js";
import { GLTFLoader } from "three/examples/jsm/loaders/GLTFLoader.js";
import { ThreeMFLoader } from "three/examples/jsm/loaders/3MFLoader.js";

interface ModelViewerProps {
  file: File;
  fileName: string;
  autoRotate?: boolean;
  onResetCamera?: () => void;
}

const STLModel = React.memo(
  ({
    fileData,
    renderSettings,
    autoRotate,
    onBoundingBoxCalculated,
  }: {
    fileData: ArrayBuffer | Blob;
    renderSettings: { color: string; metalness: number; roughness: number };
    autoRotate: boolean;
    onBoundingBoxCalculated?: (boundingBox: THREE.Box3) => void;
  }) => {
    const meshRef = useRef<THREE.Mesh>(null);
    const [geometry, setGeometry] = useState<THREE.BufferGeometry | null>(null);
    const invalidate = useThree((s) => s.invalidate);

    useEffect(() => {
      const loadSTL = async () => {
        try {
          const loader = new STLLoader();
          const arrayBuffer =
            fileData instanceof Blob ? await fileData.arrayBuffer() : fileData;
          const geom = loader.parse(arrayBuffer);
          startTransition(() => {
            setGeometry(geom);
            if (onBoundingBoxCalculated) {
              geom.computeBoundingBox();
              const boundingBox = geom.boundingBox!.clone();
              onBoundingBoxCalculated(boundingBox);
            }
          });
        } catch (error) {
          console.error("Error loading STL:", error);
        }
      };
      loadSTL();
    }, [fileData, onBoundingBoxCalculated]);

    useEffect(() => {
      return () => {
        if (geometry) geometry.dispose();
      };
    }, [geometry]);

    useEffect(() => {
      if (geometry) invalidate();
    }, [geometry, invalidate]);

    const material = useMemo(
      () =>
        new THREE.MeshStandardMaterial({
          color: renderSettings.color,
          metalness: renderSettings.metalness,
          roughness: renderSettings.roughness,
        }),
      [renderSettings.color, renderSettings.metalness, renderSettings.roughness]
    );

    useFrame(() => {
      if (meshRef.current && autoRotate) {
        meshRef.current.rotation.y += 0.005;
      }
    });

    useEffect(() => {
      return () => material.dispose();
    }, [material]);

    if (!geometry) return null;
    return <mesh ref={meshRef} geometry={geometry} material={material} />;
  }
);

const GLTFModel = React.memo(
  ({
    fileData,
    autoRotate,
    onBoundingBoxCalculated,
  }: {
    fileData: ArrayBuffer | Blob;
    autoRotate: boolean;
    onBoundingBoxCalculated?: (boundingBox: THREE.Box3) => void;
  }) => {
    const [scene, setScene] = useState<THREE.Group | null>(null);
    const meshRef = useRef<THREE.Group>(null);
    const invalidate = useThree((s) => s.invalidate);

    useEffect(() => {
      const loadGLTF = async () => {
        try {
          const loader = new GLTFLoader();
          const arrayBuffer =
            fileData instanceof Blob ? await fileData.arrayBuffer() : fileData;
          const gltf = await new Promise<{ scene: THREE.Group }>((resolve, reject) => {
            loader.parse(arrayBuffer, "", resolve, reject);
          });
          startTransition(() => {
            setScene(gltf.scene);
            if (onBoundingBoxCalculated) {
              const boundingBox = new THREE.Box3().setFromObject(gltf.scene);
              onBoundingBoxCalculated(boundingBox);
            }
          });
        } catch (error) {
          console.error("Error loading GLTF:", error);
        }
      };
      loadGLTF();
    }, [fileData, onBoundingBoxCalculated]);

    useEffect(() => {
      if (scene) invalidate();
    }, [scene, invalidate]);

    useFrame(() => {
      if (meshRef.current && autoRotate) {
        meshRef.current.rotation.y += 0.005;
      }
    });

    if (!scene) return null;
    return <primitive ref={meshRef} object={scene} />;
  }
);

const ThreeMFModel = React.memo(
  ({
    fileData,
    autoRotate,
    onBoundingBoxCalculated,
  }: {
    fileData: ArrayBuffer | Blob;
    autoRotate: boolean;
    onBoundingBoxCalculated?: (boundingBox: THREE.Box3) => void;
  }) => {
    const [scene, setScene] = useState<THREE.Group | null>(null);
    const meshRef = useRef<THREE.Group>(null);
    const invalidate = useThree((s) => s.invalidate);

    useEffect(() => {
      const load3MF = async () => {
        let blobUrl: string | null = null;
        try {
          const loader = new ThreeMFLoader();
          const arrayBuffer =
            fileData instanceof Blob ? await fileData.arrayBuffer() : fileData;
          let group: THREE.Group;
          try {
            group = loader.parse(arrayBuffer);
          } catch {
            const blob = new Blob([arrayBuffer], { type: "model/3mf" });
            blobUrl = URL.createObjectURL(blob);
            group = await loader.loadAsync(blobUrl);
          }
          if (!group || !(group instanceof THREE.Group)) {
            throw new Error("Invalid 3MF group");
          }
          startTransition(() => {
            setScene(group);
            if (onBoundingBoxCalculated) {
              const boundingBox = new THREE.Box3().setFromObject(group);
              onBoundingBoxCalculated(boundingBox);
            }
          });
        } catch (error) {
          console.error("Error loading 3MF:", error);
        } finally {
          if (blobUrl) URL.revokeObjectURL(blobUrl);
        }
      };
      load3MF();
    }, [fileData, onBoundingBoxCalculated]);

    useEffect(() => {
      if (scene) invalidate();
    }, [scene, invalidate]);

    useFrame(() => {
      if (meshRef.current && autoRotate) {
        meshRef.current.rotation.y += 0.005;
      }
    });

    if (!scene) return null;
    return <primitive ref={meshRef} object={scene} />;
  }
);

function OrbitControlsWithInvalidate(
  props: ComponentProps<typeof OrbitControls>
) {
  const ref = useRef<any>(null);
  const invalidate = useThree((s) => s.invalidate);
  useEffect(() => {
    const controls = ref.current;
    if (!controls) return;
    controls.addEventListener("change", invalidate);
    return () => controls.removeEventListener("change", invalidate);
  }, [invalidate]);
  return <OrbitControls ref={ref} {...props} />;
}

function CameraController({
  cameraSettings,
  resetCamera,
}: {
  cameraSettings: {
    position: [number, number, number];
    minDistance: number;
    maxDistance: number;
    target: [number, number, number];
  };
  resetCamera?: boolean;
}) {
  const { camera, invalidate } = useThree();
  const [hasSetInitialPosition, setHasSetInitialPosition] = useState(false);

  useEffect(() => {
    if (!hasSetInitialPosition || resetCamera) {
      camera.position.set(...cameraSettings.position);
      camera.lookAt(...cameraSettings.target);
      camera.updateMatrixWorld();
      setHasSetInitialPosition(true);
      invalidate();
    }
  }, [camera, cameraSettings.position, cameraSettings.target, resetCamera, hasSetInitialPosition, invalidate]);

  return null;
}

function getFileExtension(fileName: string): string {
  const lastDot = fileName.lastIndexOf(".");
  if (lastDot === -1) return "";
  return fileName.substring(lastDot + 1).toLowerCase();
}

export default function ModelViewer({
  file,
  fileName,
  autoRotate = false,
  onResetCamera,
}: ModelViewerProps) {
  const [fileData, setFileData] = useState<ArrayBuffer | null>(null);
  const [autoRotateState, setAutoRotateState] = useState(autoRotate);
  const [cameraSettings, setCameraSettings] = useState({
    position: [0, 0, 5] as [number, number, number],
    minDistance: 2,
    maxDistance: 2000,
    target: [0, 0, 0] as [number, number, number],
  });
  const [lastBoundingBox, setLastBoundingBox] = useState<THREE.Box3 | null>(null);
  const [shouldResetCamera, setShouldResetCamera] = useState(false);
  const [renderSettings] = useState({
    color: "#888888",
    metalness: 0.5,
    roughness: 0.5,
  });

  useEffect(() => {
    file.arrayBuffer().then((buf) => setFileData(buf));
  }, [file]);

  const calculateCameraPosition = useCallback((boundingBox: THREE.Box3) => {
    const size = boundingBox.getSize(new THREE.Vector3());
    const center = boundingBox.getCenter(new THREE.Vector3());
    const maxDimension = Math.max(size.x, size.y, size.z, 1);
    const optimalDistance = maxDimension * 2.5;
    const clampedDistance = Math.max(optimalDistance, 1);
    const minDistance = Math.max(maxDimension * 0.3, 0.5);
    const maxDistance = Math.max(clampedDistance * 6, 100);
    const cameraPosition: [number, number, number] = [
      center.x,
      center.y,
      center.z + clampedDistance,
    ];
    const newTarget = [center.x, center.y, center.z] as [number, number, number];
    setCameraSettings((prev) => {
      if (
        prev.position[0] === cameraPosition[0] &&
        prev.position[1] === cameraPosition[1] &&
        prev.position[2] === cameraPosition[2] &&
        prev.minDistance === minDistance &&
        prev.maxDistance === maxDistance &&
        prev.target[0] === newTarget[0] &&
        prev.target[1] === newTarget[1] &&
        prev.target[2] === newTarget[2]
      ) {
        return prev;
      }
      return {
        position: cameraPosition,
        minDistance,
        maxDistance,
        target: newTarget,
      };
    });
  }, []);

  const handleBoundingBoxCalculated = useCallback(
    (boundingBox: THREE.Box3) => {
      setLastBoundingBox((prev) => {
        if (prev?.min.equals(boundingBox.min) && prev?.max.equals(boundingBox.max)) {
          return prev;
        }
        return boundingBox.clone();
      });
      startTransition(() => calculateCameraPosition(boundingBox));
    },
    [calculateCameraPosition]
  );

  const handleResetCamera = useCallback(() => {
    if (lastBoundingBox) {
      calculateCameraPosition(lastBoundingBox);
      setShouldResetCamera(true);
      setTimeout(() => setShouldResetCamera(false), 100);
    }
    onResetCamera?.();
  }, [lastBoundingBox, calculateCameraPosition, onResetCamera]);

  const ext = getFileExtension(fileName);
  const isSTL = ext === "stl";
  const isGLTF = ext === "gltf" || ext === "glb";
  const is3MF = ext === "3mf";

  if (!fileData) {
    return (
      <div className="flex h-full min-h-[384px] items-center justify-center text-muted-foreground">
        Loading model...
      </div>
    );
  }

  return (
    <div className="relative h-full w-full min-h-[384px]">
      <div className="absolute top-2 right-2 z-10 flex gap-2">
        <button
          type="button"
          onClick={() => setAutoRotateState((prev) => !prev)}
          className="rounded bg-white/10 px-3 py-1 text-sm text-white backdrop-blur hover:bg-white/20"
        >
          {autoRotateState ? "Stop" : "Auto Rotate"}
        </button>
        <button
          type="button"
          onClick={handleResetCamera}
          className="rounded bg-white/10 px-3 py-1 text-sm text-white backdrop-blur hover:bg-white/20"
        >
          Reset View
        </button>
      </div>
      <Canvas
        camera={{ position: cameraSettings.position, fov: 50 }}
        frameloop={autoRotateState ? "always" : "demand"}
        dpr={Math.min(window.devicePixelRatio, 2)}
        gl={{
          antialias: false,
          alpha: true,
          powerPreference: "high-performance",
          stencil: false,
          premultipliedAlpha: false,
        }}
        style={{ background: "transparent" }}
      >
        <CameraController
          cameraSettings={cameraSettings}
          resetCamera={shouldResetCamera}
        />
        <ambientLight intensity={0.5} />
        <directionalLight position={[10, 10, 5]} intensity={1} />
        {isSTL && (
          <STLModel
            fileData={fileData}
            renderSettings={renderSettings}
            autoRotate={autoRotateState}
            onBoundingBoxCalculated={handleBoundingBoxCalculated}
          />
        )}
        {isGLTF && (
          <GLTFModel
            fileData={fileData}
            autoRotate={autoRotateState}
            onBoundingBoxCalculated={handleBoundingBoxCalculated}
          />
        )}
        {is3MF && (
          <ThreeMFModel
            fileData={fileData}
            autoRotate={autoRotateState}
            onBoundingBoxCalculated={handleBoundingBoxCalculated}
          />
        )}
        <OrbitControlsWithInvalidate
          minDistance={cameraSettings.minDistance}
          maxDistance={cameraSettings.maxDistance}
          target={cameraSettings.target}
        />
      </Canvas>
    </div>
  );
}
