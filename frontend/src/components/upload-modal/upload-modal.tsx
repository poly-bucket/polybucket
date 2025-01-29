import { useState, useRef } from 'react';
import { XMarkIcon } from '@heroicons/react/24/outline';

interface UploadModalProps {
  isOpen: boolean;
  onClose: () => void;
  onUpload: (modelData: ModelUploadData) => void;
}

interface ModelUploadData {
  title: string;
  description: string;
  categories: string[];
  license: string;
  file: File | null;
}

const LICENSE_OPTIONS = [
  'MIT License',
  'Creative Commons - Attribution',
  'Creative Commons - Attribution-ShareAlike',
  'Creative Commons - Attribution-NonCommercial',
];

const CATEGORY_OPTIONS = [
  'Art',
  'Toys',
  'Tools',
  'Games',
  'Household',
  'Engineering',
  'Fashion',
  'Medical',
];

export const UploadModal: React.FC<UploadModalProps> = ({ isOpen, onClose, onUpload }) => {
  const [modelData, setModelData] = useState<ModelUploadData>({
    title: '',
    description: '',
    categories: [],
    license: LICENSE_OPTIONS[0],
    file: null,
  });

  const fileInputRef = useRef<HTMLInputElement>(null);

  const handleSubmit = (e: React.FormEvent) => {
    e.preventDefault();
    onUpload(modelData);
    onClose();
  };

  const handleFileChange = (e: React.ChangeEvent<HTMLInputElement>) => {
    const file = e.target.files?.[0] || null;
    setModelData(prev => ({ ...prev, file }));
  };

  const handleCategoryToggle = (category: string) => {
    setModelData(prev => ({
      ...prev,
      categories: prev.categories.includes(category)
        ? prev.categories.filter(c => c !== category)
        : [...prev.categories, category],
    }));
  };

  if (!isOpen) return null;

  return (
    <div className="fixed inset-0 z-50 flex items-center justify-center bg-black/80">
      <div className="w-full max-w-2xl p-6 mx-4 bg-gray-900 border rounded-lg border-green-500/30 glow-container">
        <div className="flex items-center justify-between mb-6">
          <h2 className="text-2xl font-semibold text-green-400">Upload Model</h2>
          <button
            onClick={onClose}
            className="p-1 text-green-400 hover:text-green-300"
            aria-label="Close modal"
          >
            <XMarkIcon className="w-6 h-6" />
          </button>
        </div>

        <form onSubmit={handleSubmit} className="space-y-6">
          {/* Title Input */}
          <div>
            <label htmlFor="title" className="block mb-2 text-sm font-medium text-green-400">
              Title
            </label>
            <input
              type="text"
              id="title"
              value={modelData.title}
              onChange={e => setModelData(prev => ({ ...prev, title: e.target.value }))}
              className="w-full"
              required
            />
          </div>

          {/* Description Input */}
          <div>
            <label htmlFor="description" className="block mb-2 text-sm font-medium text-green-400">
              Description
            </label>
            <textarea
              id="description"
              value={modelData.description}
              onChange={e => setModelData(prev => ({ ...prev, description: e.target.value }))}
              rows={4}
              className="w-full"
              required
            />
          </div>

          {/* License Selection */}
          <div>
            <label htmlFor="license" className="block mb-2 text-sm font-medium text-green-400">
              License
            </label>
            <select
              id="license"
              value={modelData.license}
              onChange={e => setModelData(prev => ({ ...prev, license: e.target.value }))}
              className="w-full"
              required
            >
              {LICENSE_OPTIONS.map(license => (
                <option key={license} value={license}>
                  {license}
                </option>
              ))}
            </select>
          </div>

          {/* Categories Selection */}
          <div>
            <label className="block mb-2 text-sm font-medium text-green-400">
              Categories
            </label>
            <div className="flex flex-wrap gap-2">
              {CATEGORY_OPTIONS.map(category => (
                <button
                  key={category}
                  type="button"
                  onClick={() => handleCategoryToggle(category)}
                  className={`badge ${
                    modelData.categories.includes(category)
                      ? 'bg-green-500/20 border-green-500'
                      : 'hover:bg-gray-800'
                  }`}
                >
                  {category}
                </button>
              ))}
            </div>
          </div>

          {/* File Upload */}
          <div>
            <label htmlFor="model-file" className="block mb-2 text-sm font-medium text-green-400">
              Model File
            </label>
            <div className="flex items-center gap-4">
              <button
                type="button"
                onClick={() => fileInputRef.current?.click()}
                className="btn-secondary"
              >
                Choose File
              </button>
              <span className="text-green-300/80">
                {modelData.file ? modelData.file.name : 'No file chosen'}
              </span>
            </div>
            <input
              ref={fileInputRef}
              id="model-file"
              type="file"
              accept=".stl,.obj,.fbx,.gltf,.glb"
              onChange={handleFileChange}
              className="hidden"
              required
              aria-label="Upload model file"
            />
          </div>

          {/* Submit Button */}
          <div className="flex justify-end gap-4 pt-4 mt-6 border-t border-green-500/30">
            <button type="button" onClick={onClose} className="btn-secondary">
              Cancel
            </button>
            <button type="submit" className="btn-primary">
              Upload Model
            </button>
          </div>
        </form>
      </div>
    </div>
  );
}; 