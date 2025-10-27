import React, { useState, useEffect } from 'react';
import { usePlugin } from '../PluginAPI';

interface PrintingMetadataFormProps {
  modelId?: string;
  initialValues?: Record<string, any>;
  onSave?: (values: Record<string, any>) => void;
  onCancel?: () => void;
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

const PrintingMetadataForm: React.FC<PrintingMetadataFormProps> = ({
  modelId,
  initialValues = {},
  onSave,
  onCancel,
  className = ''
}) => {
  const plugin = usePlugin('printing-metadata-plugin');
  const [formData, setFormData] = useState<PrintingMetadata>({
    printTime: 0,
    filamentType: 'PLA',
    layerHeight: 0.2,
    infillPercentage: 20,
    supports: false,
    bedTemperature: 60,
    nozzleTemperature: 200,
    printSpeed: 50,
    difficulty: 'Beginner',
    notes: '',
    ...initialValues
  });
  const [loading, setLoading] = useState(false);
  const [errors, setErrors] = useState<Record<string, string>>({});

  const handleInputChange = (field: keyof PrintingMetadata, value: any) => {
    setFormData(prev => ({
      ...prev,
      [field]: value
    }));
    
    // Clear error for this field
    if (errors[field]) {
      setErrors(prev => ({
        ...prev,
        [field]: ''
      }));
    }
  };

  const validateForm = async () => {
    try {
      const response = await fetch(`/api/plugins/metadata/validate/model/${modelId || 'new'}`, {
        method: 'POST',
        headers: {
          'Content-Type': 'application/json',
          'Authorization': `Bearer ${localStorage.getItem('accessToken')}`
        },
        body: JSON.stringify({
          fieldValues: formData
        })
      });

      if (response.ok) {
        const result = await response.json();
        if (!result.isValid) {
          setErrors({ general: result.errorMessage });
          return false;
        }
      }
    } catch (error) {
      console.error('Validation error:', error);
    }

    return true;
  };

  const handleSave = async () => {
    if (!(await validateForm())) {
      return;
    }

    try {
      setLoading(true);
      
      // Transform the data
      const response = await fetch('/api/plugins/metadata/transform/model', {
        method: 'POST',
        headers: {
          'Content-Type': 'application/json',
          'Authorization': `Bearer ${localStorage.getItem('accessToken')}`
        },
        body: JSON.stringify({
          fieldValues: formData
        })
      });

      if (response.ok) {
        const transformedData = await response.json();
        onSave?.(transformedData);
      } else {
        setErrors({ general: 'Failed to save printing metadata' });
      }
    } catch (error) {
      console.error('Save error:', error);
      setErrors({ general: 'Failed to save printing metadata' });
    } finally {
      setLoading(false);
    }
  };

  const filamentTypes = ['PLA', 'ABS', 'PETG', 'TPU', 'Wood', 'Metal', 'Carbon Fiber'];
  const difficultyLevels = ['Beginner', 'Intermediate', 'Advanced', 'Expert'];

  return (
    <div className={`bg-white rounded-lg shadow p-6 ${className}`}>
      <h3 className="text-lg font-medium text-gray-900 mb-4">3D Printing Settings</h3>
      
      {errors.general && (
        <div className="mb-4 bg-red-50 border border-red-200 rounded-md p-4">
          <p className="text-sm text-red-700">{errors.general}</p>
        </div>
      )}

      <div className="grid grid-cols-1 md:grid-cols-2 gap-4">
        {/* Print Time */}
        <div>
          <label className="block text-sm font-medium text-gray-700 mb-1">
            Print Time (hours)
          </label>
          <input
            type="number"
            step="0.1"
            min="0"
            max="1000"
            value={formData.printTime}
            onChange={(e) => handleInputChange('printTime', parseFloat(e.target.value) || 0)}
            className="w-full px-3 py-2 border border-gray-300 rounded-md shadow-sm focus:outline-none focus:ring-blue-500 focus:border-blue-500"
          />
        </div>

        {/* Filament Type */}
        <div>
          <label className="block text-sm font-medium text-gray-700 mb-1">
            Filament Type
          </label>
          <select
            value={formData.filamentType}
            onChange={(e) => handleInputChange('filamentType', e.target.value)}
            className="w-full px-3 py-2 border border-gray-300 rounded-md shadow-sm focus:outline-none focus:ring-blue-500 focus:border-blue-500"
          >
            {filamentTypes.map(type => (
              <option key={type} value={type}>{type}</option>
            ))}
          </select>
        </div>

        {/* Layer Height */}
        <div>
          <label className="block text-sm font-medium text-gray-700 mb-1">
            Layer Height (mm)
          </label>
          <input
            type="number"
            step="0.05"
            min="0.05"
            max="1.0"
            value={formData.layerHeight}
            onChange={(e) => handleInputChange('layerHeight', parseFloat(e.target.value) || 0.2)}
            className="w-full px-3 py-2 border border-gray-300 rounded-md shadow-sm focus:outline-none focus:ring-blue-500 focus:border-blue-500"
          />
        </div>

        {/* Infill Percentage */}
        <div>
          <label className="block text-sm font-medium text-gray-700 mb-1">
            Infill Percentage
          </label>
          <input
            type="number"
            min="0"
            max="100"
            value={formData.infillPercentage}
            onChange={(e) => handleInputChange('infillPercentage', parseInt(e.target.value) || 20)}
            className="w-full px-3 py-2 border border-gray-300 rounded-md shadow-sm focus:outline-none focus:ring-blue-500 focus:border-blue-500"
          />
        </div>

        {/* Bed Temperature */}
        <div>
          <label className="block text-sm font-medium text-gray-700 mb-1">
            Bed Temperature (°C)
          </label>
          <input
            type="number"
            min="0"
            max="200"
            value={formData.bedTemperature}
            onChange={(e) => handleInputChange('bedTemperature', parseInt(e.target.value) || 60)}
            className="w-full px-3 py-2 border border-gray-300 rounded-md shadow-sm focus:outline-none focus:ring-blue-500 focus:border-blue-500"
          />
        </div>

        {/* Nozzle Temperature */}
        <div>
          <label className="block text-sm font-medium text-gray-700 mb-1">
            Nozzle Temperature (°C)
          </label>
          <input
            type="number"
            min="0"
            max="300"
            value={formData.nozzleTemperature}
            onChange={(e) => handleInputChange('nozzleTemperature', parseInt(e.target.value) || 200)}
            className="w-full px-3 py-2 border border-gray-300 rounded-md shadow-sm focus:outline-none focus:ring-blue-500 focus:border-blue-500"
          />
        </div>

        {/* Print Speed */}
        <div>
          <label className="block text-sm font-medium text-gray-700 mb-1">
            Print Speed (mm/s)
          </label>
          <input
            type="number"
            min="1"
            max="200"
            value={formData.printSpeed}
            onChange={(e) => handleInputChange('printSpeed', parseInt(e.target.value) || 50)}
            className="w-full px-3 py-2 border border-gray-300 rounded-md shadow-sm focus:outline-none focus:ring-blue-500 focus:border-blue-500"
          />
        </div>

        {/* Difficulty */}
        <div>
          <label className="block text-sm font-medium text-gray-700 mb-1">
            Print Difficulty
          </label>
          <select
            value={formData.difficulty}
            onChange={(e) => handleInputChange('difficulty', e.target.value)}
            className="w-full px-3 py-2 border border-gray-300 rounded-md shadow-sm focus:outline-none focus:ring-blue-500 focus:border-blue-500"
          >
            {difficultyLevels.map(level => (
              <option key={level} value={level}>{level}</option>
            ))}
          </select>
        </div>

        {/* Supports */}
        <div className="md:col-span-2">
          <div className="flex items-center">
            <input
              type="checkbox"
              id="supports"
              checked={formData.supports}
              onChange={(e) => handleInputChange('supports', e.target.checked)}
              className="h-4 w-4 text-blue-600 focus:ring-blue-500 border-gray-300 rounded"
            />
            <label htmlFor="supports" className="ml-2 block text-sm text-gray-700">
              Supports Required
            </label>
          </div>
        </div>

        {/* Notes */}
        <div className="md:col-span-2">
          <label className="block text-sm font-medium text-gray-700 mb-1">
            Printing Notes
          </label>
          <textarea
            value={formData.notes}
            onChange={(e) => handleInputChange('notes', e.target.value)}
            rows={3}
            maxLength={1000}
            className="w-full px-3 py-2 border border-gray-300 rounded-md shadow-sm focus:outline-none focus:ring-blue-500 focus:border-blue-500"
            placeholder="Additional notes for printing this model..."
          />
          <p className="text-xs text-gray-500 mt-1">
            {formData.notes.length}/1000 characters
          </p>
        </div>
      </div>

      <div className="mt-6 flex justify-end space-x-3">
        <button
          onClick={onCancel}
          className="px-4 py-2 border border-gray-300 shadow-sm text-sm font-medium rounded-md text-gray-700 bg-white hover:bg-gray-50"
        >
          Cancel
        </button>
        <button
          onClick={handleSave}
          disabled={loading}
          className="px-4 py-2 border border-transparent shadow-sm text-sm font-medium rounded-md text-white bg-blue-600 hover:bg-blue-700 disabled:opacity-50 disabled:cursor-not-allowed"
        >
          {loading ? 'Saving...' : 'Save Settings'}
        </button>
      </div>
    </div>
  );
};

export default PrintingMetadataForm;
