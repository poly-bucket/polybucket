import { useEffect, useRef, useState } from 'react';
import * as THREE from 'three';
import { STLLoader } from 'three/addons/loaders/STLLoader.js';
import { OrbitControls } from 'three/addons/controls/OrbitControls.js';
import { ModelViewerProps, ViewerControls } from './types';

export const ModelViewer: React.FC<ModelViewerProps> = ({ 
  modelUrl, 
  color: initialColor = '#808080',
  rotationSpeed: initialRotationSpeed = 1,
  viewMode: initialViewMode = 'solid',
  metalness: initialMetalness = 0.5,
  roughness: initialRoughness = 0.5,
  lightAzimuth: initialLightAzimuth = 45,
  lightElevation: initialLightElevation = 45,
  onControlsChange
}) => {
  // Internal state for controls
  const [color, setColor] = useState(initialColor);
  const [rotationSpeed, setRotationSpeed] = useState(initialRotationSpeed);
  const [viewMode, setViewMode] = useState(initialViewMode);
  const [metalness, setMetalness] = useState(initialMetalness);
  const [roughness, setRoughness] = useState(initialRoughness);
  const [lightAzimuth, setLightAzimuth] = useState(initialLightAzimuth);
  const [lightElevation, setLightElevation] = useState(initialLightElevation);

  // Update internal state when props change
  useEffect(() => { setColor(initialColor); }, [initialColor]);
  useEffect(() => { setRotationSpeed(initialRotationSpeed); }, [initialRotationSpeed]);
  useEffect(() => { setViewMode(initialViewMode); }, [initialViewMode]);
  useEffect(() => { setMetalness(initialMetalness); }, [initialMetalness]);
  useEffect(() => { setRoughness(initialRoughness); }, [initialRoughness]);
  useEffect(() => { setLightAzimuth(initialLightAzimuth); }, [initialLightAzimuth]);
  useEffect(() => { setLightElevation(initialLightElevation); }, [initialLightElevation]);

  const containerRef = useRef<HTMLDivElement>(null);
  const [error, setError] = useState(false);
  const sceneRef = useRef<THREE.Scene | null>(null);
  const meshRef = useRef<THREE.Mesh | null>(null);
  const geometryRef = useRef<THREE.BufferGeometry | null>(null);
  const rendererRef = useRef<THREE.WebGLRenderer | null>(null);
  const cameraRef = useRef<THREE.PerspectiveCamera | null>(null);
  const controlsRef = useRef<OrbitControls | null>(null);
  const autoRotateRef = useRef<boolean>(true);
  const directionalLightRef = useRef<THREE.DirectionalLight | null>(null);

  // Function to update the view mode
  const updateView = () => {
    if (!sceneRef.current || !geometryRef.current) return;

    const scene = sceneRef.current;
    const geometry = geometryRef.current;

    // Remove existing mesh and any view-specific objects
    if (meshRef.current) {
      scene.remove(meshRef.current);
    }
    
    // Remove any other objects (points, lines) except lights
    scene.children = scene.children.filter(child => 
      child instanceof THREE.AmbientLight || 
      child instanceof THREE.DirectionalLight
    );

    // Create appropriate material based on view mode
    let material;
    let points: THREE.Points | null = null;
    let normalLines: THREE.LineSegments | null = null;

    switch (viewMode) {
      case 'wireframe':
        material = new THREE.MeshStandardMaterial({ 
          color: color,
          wireframe: true,
          metalness: metalness,
          roughness: roughness
        });
        break;
      case 'points':
        points = new THREE.Points(
          geometry,
          new THREE.PointsMaterial({
            color: color,
            size: 0.05,
            sizeAttenuation: true
          })
        );
        points.scale.copy(meshRef.current?.scale || new THREE.Vector3(1, 1, 1));
        scene.add(points);
        return;
      case 'normals':
        material = new THREE.MeshStandardMaterial({ 
          color: color,
          metalness: metalness,
          roughness: roughness
        });
        normalLines = new THREE.LineSegments(
          new THREE.EdgesGeometry(geometry),
          new THREE.LineBasicMaterial({ color: 0x00ff00 })
        );
        normalLines.scale.copy(meshRef.current?.scale || new THREE.Vector3(1, 1, 1));
        scene.add(normalLines);
        break;
      default: // 'solid'
        material = new THREE.MeshStandardMaterial({ 
          color: color,
          metalness: metalness,
          roughness: roughness
        });
    }

    // Create and add the mesh
    const mesh = new THREE.Mesh(geometry, material);
    if (meshRef.current) {
      mesh.scale.copy(meshRef.current.scale);
    }
    meshRef.current = mesh;
    scene.add(mesh);
  };

  // Function to update light position
  const updateLightPosition = () => {
    if (!directionalLightRef.current) return;

    // Convert spherical coordinates to Cartesian
    const azimuthRad = (lightAzimuth * Math.PI) / 180;
    const elevationRad = (lightElevation * Math.PI) / 180;
    
    const x = Math.cos(elevationRad) * Math.cos(azimuthRad);
    const y = Math.sin(elevationRad);
    const z = Math.cos(elevationRad) * Math.sin(azimuthRad);

    directionalLightRef.current.position.set(x, y, z);
  };

  useEffect(() => {
    if (!containerRef.current) return;

    const scene = new THREE.Scene();
    sceneRef.current = scene;
    scene.background = new THREE.Color(0x1a1a1a);

    const camera = new THREE.PerspectiveCamera(
      75,
      containerRef.current.clientWidth / containerRef.current.clientHeight,
      0.1,
      1000
    );
    camera.position.z = 5;
    cameraRef.current = camera;

    const renderer = new THREE.WebGLRenderer({ antialias: true });
    renderer.setSize(containerRef.current.clientWidth, containerRef.current.clientHeight);
    containerRef.current.appendChild(renderer.domElement);
    rendererRef.current = renderer;

    // Controls
    const controls = new OrbitControls(camera, renderer.domElement);
    controls.enableDamping = true;
    controls.autoRotate = autoRotateRef.current;
    controls.autoRotateSpeed = rotationSpeed;
    controlsRef.current = controls;

    // Lighting
    const ambientLight = new THREE.AmbientLight(0xffffff, 0.6);
    scene.add(ambientLight);

    const directionalLight = new THREE.DirectionalLight(0xffffff, 0.8);
    directionalLightRef.current = directionalLight;
    updateLightPosition(); // Set initial position
    scene.add(directionalLight);

    // Load the STL file
    const loader = new STLLoader();
    loader.load(
      modelUrl,
      (geometry: THREE.BufferGeometry) => {
        geometryRef.current = geometry;

        // Center the model
        geometry.computeBoundingBox();
        const boundingBox = geometry.boundingBox!;
        const center = new THREE.Vector3();
        boundingBox.getCenter(center);
        geometry.translate(-center.x, -center.y, -center.z);

        // Calculate scale
        const maxDim = Math.max(
          boundingBox.max.x - boundingBox.min.x,
          boundingBox.max.y - boundingBox.min.y,
          boundingBox.max.z - boundingBox.min.z
        );
        const scale = 4 / maxDim;

        // Create initial mesh with scale
        const material = new THREE.MeshStandardMaterial({ 
          color: color,
          metalness: metalness,
          roughness: roughness
        });
        const mesh = new THREE.Mesh(geometry, material);
        mesh.scale.set(scale, scale, scale);
        meshRef.current = mesh;
        
        // Update the view based on current mode
        updateView();
      },
      undefined,
      (err: unknown) => {
        console.error('Error loading STL:', err);
        setError(true);
      }
    );

    // Animation loop
    let animationFrameId: number;
    const animate = () => {
      animationFrameId = requestAnimationFrame(animate);
      if (controlsRef.current) {
        controlsRef.current.update();
      }
      renderer.render(scene, camera);
    };
    animate();

    // Handle window resize
    const handleResize = () => {
      if (!containerRef.current || !cameraRef.current || !rendererRef.current) return;
      
      const width = containerRef.current.clientWidth;
      const height = containerRef.current.clientHeight;

      cameraRef.current.aspect = width / height;
      cameraRef.current.updateProjectionMatrix();
      rendererRef.current.setSize(width, height);
    };
    window.addEventListener('resize', handleResize);

    // Cleanup
    return () => {
      window.removeEventListener('resize', handleResize);
      cancelAnimationFrame(animationFrameId);
      
      if (containerRef.current?.contains(renderer.domElement)) {
        containerRef.current.removeChild(renderer.domElement);
      }
      renderer.dispose();
      if (meshRef.current) {
        meshRef.current.geometry.dispose();
        (meshRef.current.material as THREE.Material).dispose();
      }
      if (geometryRef.current) {
        geometryRef.current.dispose();
      }
      if (controlsRef.current) {
        controlsRef.current.dispose();
      }
    };
  }, [modelUrl]);

  // Update view when properties change
  useEffect(() => {
    updateView();
  }, [color, viewMode, metalness, roughness]);

  // Update rotation speed
  useEffect(() => {
    if (controlsRef.current) {
      controlsRef.current.autoRotateSpeed = rotationSpeed;
    }
  }, [rotationSpeed]);

  // Update light position when angles change
  useEffect(() => {
    updateLightPosition();
  }, [lightAzimuth, lightElevation]);

  // Update Controls UI handler
  const handleControlsChange = (updates: Partial<ViewerControls>) => {
    // Update internal state
    if ('color' in updates) setColor(updates.color!);
    if ('rotationSpeed' in updates) setRotationSpeed(updates.rotationSpeed!);
    if ('viewMode' in updates) setViewMode(updates.viewMode!);
    if ('metalness' in updates) setMetalness(updates.metalness!);
    if ('roughness' in updates) setRoughness(updates.roughness!);
    if ('lightAzimuth' in updates) setLightAzimuth(updates.lightAzimuth!);
    if ('lightElevation' in updates) setLightElevation(updates.lightElevation!);

    // Notify parent if callback provided
    if (onControlsChange) {
      onControlsChange({
        color,
        rotationSpeed,
        viewMode,
        metalness,
        roughness,
        lightAzimuth,
        lightElevation,
        ...updates
      });
    }
  };

  // Update material colors directly for immediate feedback
  useEffect(() => {
    if (meshRef.current) {
      const material = meshRef.current.material as THREE.MeshStandardMaterial;
      if (material.color) {
        material.color.set(color);
      }
    }
  }, [color]);

  if (error) {
    return (
      <div className="flex items-center justify-center w-full h-full bg-gray-800">
        <div className="text-sm text-green-400/60">
          Failed to load model
        </div>
      </div>
    );
  }

  return (
    <div className="relative">
      <div ref={containerRef} className="w-full h-full aspect-[16/9]" />
      
      {/* Controls Overlay */}
      <div className="absolute bottom-0 left-0 right-0 p-4 bg-black/50 backdrop-blur-sm">
        <div className="flex flex-wrap items-center gap-4">
          {/* Color Picker */}
          <div className="flex items-center gap-2">
            <label className="text-sm text-green-400">Color:</label>
            <input
              type="color"
              value={color}
              onChange={(e) => handleControlsChange({ color: e.target.value })}
              className="w-8 h-8 bg-transparent border rounded cursor-pointer"
              title="Choose model color"
              aria-label="Choose model color"
            />
          </div>

          {/* Rotation Speed */}
          <div className="flex items-center gap-2">
            <label className="text-sm text-green-400">Speed:</label>
            <input
              type="range"
              min="0"
              max="5"
              step="0.1"
              value={rotationSpeed}
              onChange={(e) => handleControlsChange({ rotationSpeed: parseFloat(e.target.value) })}
              className="w-24 accent-green-500"
              title="Adjust rotation speed"
              aria-label="Adjust rotation speed"
            />
          </div>

          {/* View Mode */}
          <div className="flex items-center gap-2">
            <label className="text-sm text-green-400">View:</label>
            <select
              value={viewMode}
              onChange={(e) => handleControlsChange({ viewMode: e.target.value as ViewerControls['viewMode'] })}
              className="px-2 py-1 text-sm text-green-400 bg-black border rounded border-green-500/30"
              title="Select view mode"
              aria-label="Select view mode"
            >
              <option value="solid">Solid</option>
              <option value="wireframe">Wireframe</option>
              <option value="points">Points</option>
              <option value="normals">Normals</option>
            </select>
          </div>

          {/* Material Properties */}
          <div className="flex items-center gap-4">
            <div className="flex items-center gap-2">
              <label className="text-sm text-green-400">Metalness:</label>
              <input
                type="range"
                min="0"
                max="1"
                step="0.1"
                value={metalness}
                onChange={(e) => handleControlsChange({ metalness: parseFloat(e.target.value) })}
                className="w-24 accent-green-500"
                title="Adjust metalness"
                aria-label="Adjust metalness"
              />
            </div>
            <div className="flex items-center gap-2">
              <label className="text-sm text-green-400">Roughness:</label>
              <input
                type="range"
                min="0"
                max="1"
                step="0.1"
                value={roughness}
                onChange={(e) => handleControlsChange({ roughness: parseFloat(e.target.value) })}
                className="w-24 accent-green-500"
                title="Adjust roughness"
                aria-label="Adjust roughness"
              />
            </div>
          </div>

          {/* Light Position Controls */}
          <div className="flex items-center gap-4">
            <div className="flex items-center gap-2">
              <label className="text-sm text-green-400">Light Angle:</label>
              <input
                type="range"
                min="0"
                max="360"
                step="1"
                value={lightAzimuth}
                onChange={(e) => handleControlsChange({ lightAzimuth: parseFloat(e.target.value) })}
                className="w-24 accent-green-500"
                title="Adjust light horizontal angle"
                aria-label="Adjust light horizontal angle"
              />
            </div>
            <div className="flex items-center gap-2">
              <label className="text-sm text-green-400">Light Height:</label>
              <input
                type="range"
                min="-90"
                max="90"
                step="1"
                value={lightElevation}
                onChange={(e) => handleControlsChange({ lightElevation: parseFloat(e.target.value) })}
                className="w-24 accent-green-500"
                title="Adjust light elevation"
                aria-label="Adjust light elevation"
              />
            </div>
          </div>
        </div>
      </div>
    </div>
  );
}; 