import React, { useState } from 'react';

export interface PrintSettings {
  id: string;
  layerHeight: string;
  infill: string;
  supports: boolean;
  printSpeed: string;
  nozzleTemp: string;
  bedTemp: string;
  material: string;
  nozzleSize: string;
  retraction: string;
  buildPlateAdhesion: string;
  notes?: string;
}

interface PrintSettingsEditorProps {
  printSettings: PrintSettings;
  onUpdate: (settings: PrintSettings) => void;
  readonly?: boolean;
}

const PrintSettingsEditor: React.FC<PrintSettingsEditorProps> = ({
  printSettings,
  onUpdate,
  readonly = false
}) => {
  const [settings, setSettings] = useState<PrintSettings>(printSettings);

  const handleInputChange = (field: keyof PrintSettings, value: string | boolean) => {
    const updated = { ...settings, [field]: value };
    setSettings(updated);
    onUpdate(updated);
  };

  const presetConfigs = [
    {
      name: 'Draft Quality',
      layerHeight: '0.3mm',
      infill: '10%',
      printSpeed: '60mm/s',
      supports: false
    },
    {
      name: 'Standard Quality',
      layerHeight: '0.2mm',
      infill: '15%',
      printSpeed: '50mm/s',
      supports: false
    },
    {
      name: 'High Quality',
      layerHeight: '0.1mm',
      infill: '20%',
      printSpeed: '30mm/s',
      supports: true
    },
    {
      name: 'Miniature Detail',
      layerHeight: '0.05mm',
      infill: '25%',
      printSpeed: '20mm/s',
      supports: true
    }
  ];

  const materialPresets = [
    { name: 'PLA', nozzleTemp: '210°C', bedTemp: '60°C' },
    { name: 'ABS', nozzleTemp: '250°C', bedTemp: '100°C' },
    { name: 'PETG', nozzleTemp: '230°C', bedTemp: '80°C' },
    { name: 'TPU', nozzleTemp: '220°C', bedTemp: '50°C' },
    { name: 'ASA', nozzleTemp: '260°C', bedTemp: '100°C' }
  ];

  const applyPreset = (preset: typeof presetConfigs[0]) => {
    const updated = {
      ...settings,
      layerHeight: preset.layerHeight,
      infill: preset.infill,
      printSpeed: preset.printSpeed,
      supports: preset.supports
    };
    setSettings(updated);
    onUpdate(updated);
  };

  const applyMaterialPreset = (material: typeof materialPresets[0]) => {
    const updated = {
      ...settings,
      material: material.name,
      nozzleTemp: material.nozzleTemp,
      bedTemp: material.bedTemp
    };
    setSettings(updated);
    onUpdate(updated);
  };

  return (
    <div className="space-y-6">
      <div className="flex items-center justify-between">
        <h3 className="text-lg font-semibold text-gray-800">Print Settings</h3>
        {!readonly && (
          <div className="text-sm text-gray-500">
            Recommended settings for optimal print quality
          </div>
        )}
      </div>

      {/* Presets */}
      {!readonly && (
        <div className="space-y-4">
          <div>
            <h4 className="text-md font-medium text-gray-700 mb-2">Quality Presets</h4>
            <div className="flex flex-wrap gap-2">
              {presetConfigs.map((preset) => (
                <button
                  key={preset.name}
                  onClick={() => applyPreset(preset)}
                  className="px-3 py-1 text-sm bg-gray-100 border border-gray-300 rounded-md hover:bg-gray-200 focus:outline-none focus:ring-2 focus:ring-green-500"
                >
                  {preset.name}
                </button>
              ))}
            </div>
          </div>

          <div>
            <h4 className="text-md font-medium text-gray-700 mb-2">Material Presets</h4>
            <div className="flex flex-wrap gap-2">
              {materialPresets.map((material) => (
                <button
                  key={material.name}
                  onClick={() => applyMaterialPreset(material)}
                  className="px-3 py-1 text-sm bg-blue-100 border border-blue-300 rounded-md hover:bg-blue-200 focus:outline-none focus:ring-2 focus:ring-blue-500"
                >
                  {material.name}
                </button>
              ))}
            </div>
          </div>
        </div>
      )}

      {/* Settings Grid */}
      <div className="grid grid-cols-1 md:grid-cols-2 lg:grid-cols-3 gap-4">
        {/* Basic Settings */}
        <div className="space-y-4 p-4 bg-gray-50 rounded-lg">
          <h4 className="font-medium text-gray-800">Basic Settings</h4>
          
          <div>
            <label className="block text-sm font-medium text-gray-700 mb-1">
              Layer Height
            </label>
            <input
              type="text"
              value={settings.layerHeight}
              onChange={(e) => handleInputChange('layerHeight', e.target.value)}
              disabled={readonly}
              className="w-full px-3 py-2 border border-gray-300 rounded-md focus:outline-none focus:ring-2 focus:ring-green-500 focus:border-transparent disabled:bg-gray-100 disabled:cursor-not-allowed"
              placeholder="0.2mm"
            />
          </div>

          <div>
            <label className="block text-sm font-medium text-gray-700 mb-1">
              Infill Density
            </label>
            <input
              type="text"
              value={settings.infill}
              onChange={(e) => handleInputChange('infill', e.target.value)}
              disabled={readonly}
              className="w-full px-3 py-2 border border-gray-300 rounded-md focus:outline-none focus:ring-2 focus:ring-green-500 focus:border-transparent disabled:bg-gray-100 disabled:cursor-not-allowed"
              placeholder="15%"
            />
          </div>

          <div>
            <label className="block text-sm font-medium text-gray-700 mb-1">
              Print Speed
            </label>
            <input
              type="text"
              value={settings.printSpeed}
              onChange={(e) => handleInputChange('printSpeed', e.target.value)}
              disabled={readonly}
              className="w-full px-3 py-2 border border-gray-300 rounded-md focus:outline-none focus:ring-2 focus:ring-green-500 focus:border-transparent disabled:bg-gray-100 disabled:cursor-not-allowed"
              placeholder="50mm/s"
            />
          </div>

          <div>
            <label className="flex items-center">
              <input
                type="checkbox"
                checked={settings.supports}
                onChange={(e) => handleInputChange('supports', e.target.checked)}
                disabled={readonly}
                className="mr-2 text-green-600 bg-gray-100 border-gray-300 rounded focus:ring-green-500 disabled:opacity-50"
              />
              <span className="text-sm text-gray-700">Enable Supports</span>
            </label>
          </div>
        </div>

        {/* Temperature Settings */}
        <div className="space-y-4 p-4 bg-gray-50 rounded-lg">
          <h4 className="font-medium text-gray-800">Temperature & Material</h4>
          
          <div>
            <label className="block text-sm font-medium text-gray-700 mb-1">
              Material
            </label>
            <input
              type="text"
              value={settings.material}
              onChange={(e) => handleInputChange('material', e.target.value)}
              disabled={readonly}
              className="w-full px-3 py-2 border border-gray-300 rounded-md focus:outline-none focus:ring-2 focus:ring-green-500 focus:border-transparent disabled:bg-gray-100 disabled:cursor-not-allowed"
              placeholder="PLA"
            />
          </div>

          <div>
            <label className="block text-sm font-medium text-gray-700 mb-1">
              Nozzle Temperature
            </label>
            <input
              type="text"
              value={settings.nozzleTemp}
              onChange={(e) => handleInputChange('nozzleTemp', e.target.value)}
              disabled={readonly}
              className="w-full px-3 py-2 border border-gray-300 rounded-md focus:outline-none focus:ring-2 focus:ring-green-500 focus:border-transparent disabled:bg-gray-100 disabled:cursor-not-allowed"
              placeholder="210°C"
            />
          </div>

          <div>
            <label className="block text-sm font-medium text-gray-700 mb-1">
              Bed Temperature
            </label>
            <input
              type="text"
              value={settings.bedTemp}
              onChange={(e) => handleInputChange('bedTemp', e.target.value)}
              disabled={readonly}
              className="w-full px-3 py-2 border border-gray-300 rounded-md focus:outline-none focus:ring-2 focus:ring-green-500 focus:border-transparent disabled:bg-gray-100 disabled:cursor-not-allowed"
              placeholder="60°C"
            />
          </div>
        </div>

        {/* Advanced Settings */}
        <div className="space-y-4 p-4 bg-gray-50 rounded-lg">
          <h4 className="font-medium text-gray-800">Advanced Settings</h4>
          
          <div>
            <label className="block text-sm font-medium text-gray-700 mb-1">
              Nozzle Size
            </label>
            <input
              type="text"
              value={settings.nozzleSize}
              onChange={(e) => handleInputChange('nozzleSize', e.target.value)}
              disabled={readonly}
              className="w-full px-3 py-2 border border-gray-300 rounded-md focus:outline-none focus:ring-2 focus:ring-green-500 focus:border-transparent disabled:bg-gray-100 disabled:cursor-not-allowed"
              placeholder="0.4mm"
            />
          </div>

          <div>
            <label className="block text-sm font-medium text-gray-700 mb-1">
              Retraction Distance
            </label>
            <input
              type="text"
              value={settings.retraction}
              onChange={(e) => handleInputChange('retraction', e.target.value)}
              disabled={readonly}
              className="w-full px-3 py-2 border border-gray-300 rounded-md focus:outline-none focus:ring-2 focus:ring-green-500 focus:border-transparent disabled:bg-gray-100 disabled:cursor-not-allowed"
              placeholder="6mm"
            />
          </div>

          <div>
            <label className="block text-sm font-medium text-gray-700 mb-1">
              Build Plate Adhesion
            </label>
            <select
              value={settings.buildPlateAdhesion}
              onChange={(e) => handleInputChange('buildPlateAdhesion', e.target.value)}
              disabled={readonly}
              className="w-full px-3 py-2 border border-gray-300 rounded-md focus:outline-none focus:ring-2 focus:ring-green-500 focus:border-transparent disabled:bg-gray-100 disabled:cursor-not-allowed"
            >
              <option value="skirt">Skirt</option>
              <option value="brim">Brim</option>
              <option value="raft">Raft</option>
              <option value="none">None</option>
            </select>
          </div>
        </div>
      </div>

      {/* Notes Section */}
      <div>
        <label className="block text-sm font-medium text-gray-700 mb-2">
          Additional Notes
        </label>
        <textarea
          value={settings.notes || ''}
          onChange={(e) => handleInputChange('notes', e.target.value)}
          disabled={readonly}
          rows={3}
          className="w-full px-3 py-2 border border-gray-300 rounded-md focus:outline-none focus:ring-2 focus:ring-green-500 focus:border-transparent resize-none disabled:bg-gray-100 disabled:cursor-not-allowed"
          placeholder="Any special printing instructions or tips..."
        />
      </div>

      {/* Quick Reference Card */}
      <div className="bg-blue-50 border border-blue-200 rounded-lg p-4">
        <h4 className="font-medium text-blue-800 mb-2">Quick Reference</h4>
        <div className="grid grid-cols-2 md:grid-cols-4 gap-4 text-sm">
          <div>
            <span className="text-blue-600 font-medium">Layer:</span>
            <span className="ml-1 text-blue-800">{settings.layerHeight}</span>
          </div>
          <div>
            <span className="text-blue-600 font-medium">Infill:</span>
            <span className="ml-1 text-blue-800">{settings.infill}</span>
          </div>
          <div>
            <span className="text-blue-600 font-medium">Speed:</span>
            <span className="ml-1 text-blue-800">{settings.printSpeed}</span>
          </div>
          <div>
            <span className="text-blue-600 font-medium">Supports:</span>
            <span className="ml-1 text-blue-800">{settings.supports ? 'Yes' : 'No'}</span>
          </div>
        </div>
      </div>
    </div>
  );
};

export default PrintSettingsEditor; 