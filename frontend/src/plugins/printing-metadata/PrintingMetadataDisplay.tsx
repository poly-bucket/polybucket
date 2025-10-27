import React, { useState, useEffect } from 'react';
import { usePlugin } from '../PluginAPI';

interface PrintingMetadataDisplayProps {
  modelId: string;
  className?: string;
}

interface PrintingMetadata {
  printTime: number;
  filamentType: string;
  layerHeight: number;
  infillPercentage: number;
  supports: boolean;
  bedTemperature: number;
  nozzleTemperature: number;
  printSpeed: number;
  difficulty: string;
  notes: string;
}

const PrintingMetadataDisplay: React.FC<PrintingMetadataDisplayProps> = ({
  modelId,
  className = ''
}) => {
  const plugin = usePlugin('printing-metadata-plugin');
  const [metadata, setMetadata] = useState<PrintingMetadata | null>(null);
  const [loading, setLoading] = useState(true);
  const [error, setError] = useState<string>('');

  useEffect(() => {
    loadPrintingMetadata();
  }, [modelId]);

  const loadPrintingMetadata = async () => {
    try {
      setLoading(true);
      setError('');

      // In a real implementation, this would fetch the metadata from your backend
      // For now, we'll simulate the data
      const mockMetadata: PrintingMetadata = {
        printTime: 4.5,
        filamentType: 'PLA',
        layerHeight: 0.2,
        infillPercentage: 20,
        supports: true,
        bedTemperature: 60,
        nozzleTemperature: 200,
        printSpeed: 50,
        difficulty: 'Intermediate',
        notes: 'Use supports for overhangs. Print with 0.2mm layer height for best results.'
      };

      setMetadata(mockMetadata);
    } catch (err) {
      setError('Failed to load printing metadata');
      console.error('Error loading printing metadata:', err);
    } finally {
      setLoading(false);
    }
  };

  const getDifficultyColor = (difficulty: string) => {
    switch (difficulty) {
      case 'Beginner':
        return 'bg-green-100 text-green-800';
      case 'Intermediate':
        return 'bg-yellow-100 text-yellow-800';
      case 'Advanced':
        return 'bg-orange-100 text-orange-800';
      case 'Expert':
        return 'bg-red-100 text-red-800';
      default:
        return 'bg-gray-100 text-gray-800';
    }
  };

  const getFilamentColor = (filamentType: string) => {
    switch (filamentType) {
      case 'PLA':
        return 'bg-blue-100 text-blue-800';
      case 'ABS':
        return 'bg-red-100 text-red-800';
      case 'PETG':
        return 'bg-green-100 text-green-800';
      case 'TPU':
        return 'bg-purple-100 text-purple-800';
      case 'Wood':
        return 'bg-yellow-100 text-yellow-800';
      case 'Metal':
        return 'bg-gray-100 text-gray-800';
      case 'Carbon Fiber':
        return 'bg-black text-white';
      default:
        return 'bg-gray-100 text-gray-800';
    }
  };

  if (loading) {
    return (
      <div className={`animate-pulse ${className}`}>
        <div className="bg-white rounded-lg shadow p-6">
          <div className="h-6 bg-gray-300 rounded w-48 mb-4"></div>
          <div className="grid grid-cols-2 gap-4">
            {[...Array(6)].map((_, i) => (
              <div key={i} className="space-y-2">
                <div className="h-4 bg-gray-300 rounded w-24"></div>
                <div className="h-4 bg-gray-300 rounded w-16"></div>
              </div>
            ))}
          </div>
        </div>
      </div>
    );
  }

  if (error) {
    return (
      <div className={`text-red-600 text-sm ${className}`}>
        {error}
      </div>
    );
  }

  if (!metadata) {
    return null;
  }

  return (
    <div className={`bg-white rounded-lg shadow p-6 ${className}`}>
      <div className="flex items-center justify-between mb-4">
        <h3 className="text-lg font-medium text-gray-900">3D Printing Settings</h3>
        <div className="flex space-x-2">
          <span className={`inline-flex items-center px-2.5 py-0.5 rounded-full text-xs font-medium ${getDifficultyColor(metadata.difficulty)}`}>
            {metadata.difficulty}
          </span>
          <span className={`inline-flex items-center px-2.5 py-0.5 rounded-full text-xs font-medium ${getFilamentColor(metadata.filamentType)}`}>
            {metadata.filamentType}
          </span>
        </div>
      </div>

      <div className="grid grid-cols-2 md:grid-cols-3 gap-4 mb-4">
        <div className="text-center p-3 bg-gray-50 rounded-lg">
          <div className="text-2xl font-bold text-blue-600">{metadata.printTime}h</div>
          <div className="text-sm text-gray-600">Print Time</div>
        </div>

        <div className="text-center p-3 bg-gray-50 rounded-lg">
          <div className="text-2xl font-bold text-green-600">{metadata.layerHeight}mm</div>
          <div className="text-sm text-gray-600">Layer Height</div>
        </div>

        <div className="text-center p-3 bg-gray-50 rounded-lg">
          <div className="text-2xl font-bold text-purple-600">{metadata.infillPercentage}%</div>
          <div className="text-sm text-gray-600">Infill</div>
        </div>

        <div className="text-center p-3 bg-gray-50 rounded-lg">
          <div className="text-2xl font-bold text-orange-600">{metadata.bedTemperature}°C</div>
          <div className="text-sm text-gray-600">Bed Temp</div>
        </div>

        <div className="text-center p-3 bg-gray-50 rounded-lg">
          <div className="text-2xl font-bold text-red-600">{metadata.nozzleTemperature}°C</div>
          <div className="text-sm text-gray-600">Nozzle Temp</div>
        </div>

        <div className="text-center p-3 bg-gray-50 rounded-lg">
          <div className="text-2xl font-bold text-indigo-600">{metadata.printSpeed}mm/s</div>
          <div className="text-sm text-gray-600">Print Speed</div>
        </div>
      </div>

      <div className="space-y-3">
        <div className="flex items-center justify-between">
          <span className="text-sm font-medium text-gray-700">Supports Required:</span>
          <span className={`inline-flex items-center px-2 py-1 rounded-full text-xs font-medium ${
            metadata.supports ? 'bg-red-100 text-red-800' : 'bg-green-100 text-green-800'
          }`}>
            {metadata.supports ? 'Yes' : 'No'}
          </span>
        </div>

        {metadata.notes && (
          <div>
            <span className="text-sm font-medium text-gray-700 block mb-1">Printing Notes:</span>
            <p className="text-sm text-gray-600 bg-gray-50 p-3 rounded-lg">
              {metadata.notes}
            </p>
          </div>
        )}
      </div>

      <div className="mt-4 pt-4 border-t border-gray-200">
        <div className="flex items-center justify-between text-sm text-gray-500">
          <span>Printing metadata provided by 3D Printing Plugin</span>
          <button className="text-blue-600 hover:text-blue-800">
            Edit Settings
          </button>
        </div>
      </div>
    </div>
  );
};

export default PrintingMetadataDisplay;
