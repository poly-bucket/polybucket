import * as THREE from 'three';
import { useLoader } from '@react-three/fiber';
import { STLLoader } from 'three/addons/loaders/STLLoader.js';
import { PointsMaterial, DoubleSide } from 'three';
import { useEffect, useMemo } from 'react';
import { VertexNormalsHelper } from 'three/addons/helpers/VertexNormalsHelper.js';

interface ModelProps {
  url: string;
  color: string;
  metalness: number;
  roughness: number;
  viewMode: 'solid' | 'wireframe' | 'points' | 'normals';
}

export const Model = ({ url, color, metalness, roughness, viewMode }: ModelProps) => {
  const geometry = useLoader(STLLoader, url);

  // For points mode, adjust size and add more properties
  const pointsMaterial = useMemo(() => 
    new PointsMaterial({ 
      size: 0.5,  // Increased size
      color: color,
      sizeAttenuation: true,
      transparent: true,
      opacity: 1,
      alphaTest: 0.5,
      vertexColors: false
    }), [color]);

  if (viewMode === 'points') {
    // Scale the points to match the mesh size
    return (
      <points geometry={geometry} material={pointsMaterial} scale={[1, 1, 1]}>
        <pointsMaterial 
          size={0.5}
          color={color}
          sizeAttenuation={true}
        />
      </points>
    );
  }

  return (
    <>
      <mesh geometry={geometry}>
        <meshStandardMaterial 
          color={color}
          metalness={metalness}
          roughness={roughness}
          wireframe={viewMode === 'wireframe'}
          side={DoubleSide}
        />
      </mesh>
      
      {/* Show normals helper when in normals mode */}
      {viewMode === 'normals' && (
        <primitive 
          object={new VertexNormalsHelper(
            new THREE.Mesh(geometry), 
            2, 
            0x00ff00
          )} 
        />
      )}
    </>
  );
}; 