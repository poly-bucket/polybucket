import { useLoader } from '@react-three/fiber';
import { STLLoader } from 'three/addons/loaders/STLLoader.js';

interface ModelProps {
  url: string;
  color: string;
  metalness?: number;
  roughness?: number;
}

export const Model: React.FC<ModelProps> = ({ 
  url, 
  color, 
  metalness = 0.5,
  roughness = 0.5
}) => {
  const geometry = useLoader(STLLoader, url);
  
  return (
    <mesh geometry={geometry}>
      <meshStandardMaterial 
        color={color}
        metalness={metalness}
        roughness={roughness}
      />
    </mesh>
  );
}; 