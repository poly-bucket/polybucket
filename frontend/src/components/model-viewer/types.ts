export interface ViewerControls {
  color: string;
  rotationSpeed: number;
  viewMode: 'solid' | 'wireframe' | 'points' | 'normals';
  metalness: number;
  roughness: number;
  lightAzimuth: number;  // Horizontal angle in degrees (0-360)
  lightElevation: number;  // Vertical angle in degrees (-90 to 90)
}

export interface ModelViewerProps {
  modelUrl: string;
  color?: string;
  rotationSpeed?: number;
  viewMode?: ViewerControls['viewMode'];
  metalness?: number;
  roughness?: number;
  lightAzimuth?: number;
  lightElevation?: number;
  onControlsChange?: (controls: ViewerControls) => void;
} 