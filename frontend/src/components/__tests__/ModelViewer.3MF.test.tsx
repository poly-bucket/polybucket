import React from 'react';
import { render, screen, fireEvent, waitFor } from '@testing-library/react';
import '@testing-library/jest-dom';
import ModelViewer from '../ModelViewer';

// Mock the Three.js and related libraries
jest.mock('@react-three/fiber', () => ({
  Canvas: ({ children }: { children: React.ReactNode }) => <div data-testid="canvas">{children}</div>,
  useFrame: jest.fn(),
  useThree: jest.fn(() => ({ camera: {} }))
}));

jest.mock('@react-three/drei', () => ({
  OrbitControls: ({ children }: { children: React.ReactNode }) => <div data-testid="orbit-controls">{children}</div>,
  useGLTF: jest.fn(),
  useProgress: jest.fn(() => ({ progress: 100 })),
  Html: ({ children }: { children: React.ReactNode }) => <div data-testid="html">{children}</div>,
  PerformanceMonitor: ({ children }: { children: React.ReactNode }) => <div data-testid="performance-monitor">{children}</div>
}));

jest.mock('three', () => ({
  Box3: jest.fn(() => ({
    getSize: jest.fn(() => ({ x: 10, y: 10, z: 10 })),
    getCenter: jest.fn(() => ({ x: 0, y: 0, z: 0 })),
    clone: jest.fn(() => ({ x: 0, y: 0, z: 0 }))
  })),
  Vector3: jest.fn(() => ({ x: 0, y: 0, z: 0 })),
  Scene: jest.fn(() => ({ add: jest.fn() })),
  MeshStandardMaterial: jest.fn(() => ({ dispose: jest.fn() })),
  Mesh: jest.fn(() => ({ dispose: jest.fn() }))
}));

jest.mock('three/examples/jsm/loaders/STLLoader.js', () => ({
  STLLoader: jest.fn(() => ({
    parse: jest.fn(() => ({
      computeBoundingBox: jest.fn(),
      boundingBox: { clone: jest.fn() },
      dispose: jest.fn()
    }))
  }))
}));

jest.mock('three/examples/jsm/loaders/GLTFLoader.js', () => ({
  GLTFLoader: jest.fn(() => ({
    parse: jest.fn(() => Promise.resolve({ scene: { dispose: jest.fn() } }))
  }))
}));

jest.mock('three-stdlib/loaders/3MFLoader.js', () => ({
  ThreeMFLoader: jest.fn(() => ({
    parse: jest.fn(() => ({
      dispose: jest.fn()
    }))
  }))
}));

jest.mock('three-3mf-exporter', () => ({
  exportTo3MF: jest.fn(() => Promise.resolve(new Blob(['test'], { type: 'application/3mf' })))
}));

// Mock the API config
jest.mock('../../api/config', () => ({
  API_CONFIG: {
    baseUrl: 'http://localhost:5000'
  }
}));

describe('ModelViewer 3MF Support', () => {
  const mockFileData = new ArrayBuffer(8);
  const mockFileName = 'test-model.3mf';

  beforeEach(() => {
    jest.clearAllMocks();
  });

  it('should detect 3MF files correctly', () => {
    render(
      <ModelViewer
        fileData={mockFileData}
        fileName={mockFileName}
        fileType="3mf"
        showControls={true}
        width={400}
        height={300}
      />
    );

    // The component should render without showing "Unsupported format" message
    expect(screen.queryByText(/Unsupported format/)).not.toBeInTheDocument();
  });

  it('should render 3MF export button when controls are shown', () => {
    render(
      <ModelViewer
        fileData={mockFileData}
        fileName={mockFileName}
        fileType="3mf"
        showControls={true}
        width={400}
        height={300}
      />
    );

    // Click on view controls to show the panel
    const viewControlsButton = screen.getByTitle('View Mode Controls');
    fireEvent.click(viewControlsButton);

    // Check if export button is present
    const exportButton = screen.getByText('Export to 3MF');
    expect(exportButton).toBeInTheDocument();
  });

  it('should handle 3MF file loading from URL', () => {
    render(
      <ModelViewer
        modelUrl="http://example.com/model.3mf"
        showControls={true}
        width={400}
        height={300}
      />
    );

    // The component should render without showing "Unsupported format" message
    expect(screen.queryByText(/Unsupported format/)).not.toBeInTheDocument();
  });

  it('should handle 3MF file loading from fileId', () => {
    render(
      <ModelViewer
        fileId="123"
        showControls={true}
        width={400}
        height={300}
      />
    );

    // The component should render without showing "Unsupported format" message
    expect(screen.queryByText(/Unsupported format/)).not.toBeInTheDocument();
  });

  it('should handle 3MF file loading from modelId and fileName', () => {
    render(
      <ModelViewer
        modelId="456"
        fileName="model.3mf"
        showControls={true}
        width={400}
        height={300}
      />
    );

    // The component should render without showing "Unsupported format" message
    expect(screen.queryByText(/Unsupported format/)).not.toBeInTheDocument();
  });

  it('should show unsupported format message for non-3MF files', () => {
    render(
      <ModelViewer
        fileData={mockFileData}
        fileName="test-model.xyz"
        fileType="xyz"
        showControls={true}
        width={400}
        height={300}
      />
    );

    // Should show unsupported format message
    expect(screen.getByText(/Unsupported format: xyz/)).toBeInTheDocument();
  });
});
