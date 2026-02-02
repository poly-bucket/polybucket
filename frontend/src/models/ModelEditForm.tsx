import React, { useState, useRef } from 'react';
import { Model, LicenseTypes, PrivacySettings, Tag, Category } from '../api/client';
import { ExtendedModel } from '../services/modelsService';

interface ModelEditFormProps {
  model: ExtendedModel;
  onSave: (updatedModel: Partial<Model>) => Promise<void>;
  onCancel: () => void;
  loading?: boolean;
}

interface EditFormData {
  name: string;
  description: string;
  privacy: PrivacySettings;
  license: LicenseTypes;
  categories: string[];
  tags: string[];
  aiGenerated: boolean;
  wip: boolean;
  nsfw: boolean;
  isRemix: boolean;
  remixUrl: string;
  thumbnailFile?: File;
}

const ModelEditForm: React.FC<ModelEditFormProps> = ({
  model,
  onSave,
  onCancel,
  loading = false
}) => {
  const [formData, setFormData] = useState<EditFormData>({
    name: model.name || '',
    description: model.description || '',
    privacy: model.privacy || PrivacySettings.Public,
    license: model.license || LicenseTypes.MIT,
    categories: model.categories?.map(cat => typeof cat === 'string' ? cat : cat.name || '') || [],
    tags: model.tags?.map(tag => tag.name || '') || [],
    aiGenerated: model.aiGenerated || false,
    wip: model.wip || false,
    nsfw: model.nsfw || false,
    isRemix: model.isRemix || false,
    remixUrl: model.remixUrl || '',
  });

  const [newTag, setNewTag] = useState('');
  const [previewImage, setPreviewImage] = useState<string | null>(null);
  const fileInputRef = useRef<HTMLInputElement>(null);

  const licenseOptions = [
    { value: LicenseTypes.MIT, label: 'MIT License' },
    { value: LicenseTypes.GPLv3, label: 'GPL v3' },
    { value: LicenseTypes.Apache2, label: 'Apache 2.0' },
    { value: LicenseTypes.CCBy4, label: 'Creative Commons Attribution 4.0' },
    { value: LicenseTypes.CCBySA4, label: 'Creative Commons Attribution-ShareAlike 4.0' },
    { value: LicenseTypes.CCByND4, label: 'Creative Commons Attribution-NoDerivs 4.0' },
    { value: LicenseTypes.CCByNC4, label: 'Creative Commons Attribution-NonCommercial 4.0' },
    { value: LicenseTypes.CCByNCSA4, label: 'Creative Commons Attribution-NonCommercial-ShareAlike 4.0' },
    { value: LicenseTypes.CCByNCND4, label: 'Creative Commons Attribution-NonCommercial-NoDerivs 4.0' },
    { value: LicenseTypes.BSD, label: 'BSD License' },
  ];

  const privacyOptions = [
    { value: PrivacySettings.Public, label: 'Public - Everyone can see this model' },
    { value: PrivacySettings.Private, label: 'Private - Only you can see this model' },
    { value: PrivacySettings.Unlisted, label: 'Unlisted - Only people with the link can see this model' },
  ];

  const categoryOptions = [
    { value: 'Art', label: 'Art' },
    { value: 'Technology', label: 'Technology' },
    { value: 'Toys', label: 'Toys' },
    { value: 'Tools', label: 'Tools' },
    { value: 'Games', label: 'Games' },
    { value: 'Fashion', label: 'Fashion' },
    { value: 'Gadget', label: 'Gadget' },
    { value: 'Home', label: 'Home' },
    { value: 'Kitchen', label: 'Kitchen' },
    { value: 'Electronics', label: 'Electronics' },
    { value: 'Automotive', label: 'Automotive' },
    { value: 'Sports', label: 'Sports' },
    { value: 'Music', label: 'Music' },
    { value: 'Medical', label: 'Medical' },
    { value: 'Science', label: 'Science' },
    { value: 'Other', label: 'Other' },
  ];

  const handleInputChange = (field: keyof EditFormData, value: any) => {
    setFormData(prev => ({ ...prev, [field]: value }));
  };

  const handleCategoryToggle = (category: string) => {
    setFormData(prev => ({
      ...prev,
      categories: prev.categories.includes(category)
        ? prev.categories.filter(c => c !== category)
        : [...prev.categories, category]
    }));
  };

  const handleAddTag = () => {
    if (newTag.trim() && !formData.tags.includes(newTag.trim())) {
      setFormData(prev => ({
        ...prev,
        tags: [...prev.tags, newTag.trim()]
      }));
      setNewTag('');
    }
  };

  const handleRemoveTag = (tagToRemove: string) => {
    setFormData(prev => ({
      ...prev,
      tags: prev.tags.filter(tag => tag !== tagToRemove)
    }));
  };

  const handleThumbnailSelect = (event: React.ChangeEvent<HTMLInputElement>) => {
    const file = event.target.files?.[0];
    if (file && file.type.startsWith('image/')) {
      setFormData(prev => ({ ...prev, thumbnailFile: file }));
      
      // Create preview
      const reader = new FileReader();
      reader.onload = (e) => {
        setPreviewImage(e.target?.result as string);
      };
      reader.readAsDataURL(file);
    }
  };

  const handleSubmit = async (e: React.FormEvent) => {
    e.preventDefault();
    
    const updatedModel: Partial<Model> = {
      name: formData.name,
      description: formData.description,
      privacy: formData.privacy,
      license: formData.license,
      categories: formData.categories.map(catName => ({ name: catName } as Category)),
      tags: formData.tags.map(tagName => ({ name: tagName } as Tag)),
      aiGenerated: formData.aiGenerated,
      wip: formData.wip,
      nsfw: formData.nsfw,
      isRemix: formData.isRemix,
      remixUrl: formData.isRemix ? formData.remixUrl : undefined,
    };

    await onSave(updatedModel);
  };

  return (
    <div className="bg-white p-6 rounded-lg shadow-lg max-w-4xl mx-auto">
      <h2 className="text-2xl font-bold text-gray-800 mb-6">Edit Model</h2>
      
      <form onSubmit={handleSubmit} className="space-y-6">
        {/* Basic Information */}
        <div className="grid grid-cols-1 md:grid-cols-2 gap-6">
          <div>
            <label className="block text-sm font-medium text-gray-700 mb-2">
              Model Title *
            </label>
            <input
              type="text"
              value={formData.name}
              onChange={(e) => handleInputChange('name', e.target.value)}
              className="w-full px-3 py-2 border border-gray-300 rounded-md focus:outline-none focus:ring-2 focus:ring-green-500 focus:border-transparent"
              placeholder="Enter model title"
              required
            />
          </div>

          <div>
            <label className="block text-sm font-medium text-gray-700 mb-2">
              Privacy Setting
            </label>
            <select
              value={formData.privacy}
              onChange={(e) => handleInputChange('privacy', parseInt(e.target.value) as PrivacySettings)}
              className="w-full px-3 py-2 border border-gray-300 rounded-md focus:outline-none focus:ring-2 focus:ring-green-500 focus:border-transparent"
            >
              {privacyOptions.map(option => (
                <option key={option.value} value={option.value}>
                  {option.label}
                </option>
              ))}
            </select>
          </div>
        </div>

        <div>
          <label className="block text-sm font-medium text-gray-700 mb-2">
            Description
          </label>
          <textarea
            value={formData.description}
            onChange={(e) => handleInputChange('description', e.target.value)}
            rows={4}
            className="w-full px-3 py-2 border border-gray-300 rounded-md focus:outline-none focus:ring-2 focus:ring-green-500 focus:border-transparent resize-none"
            placeholder="Describe your model..."
          />
        </div>

        {/* Thumbnail Upload */}
        <div>
          <label className="block text-sm font-medium text-gray-700 mb-2">
            Thumbnail Image
          </label>
          <div className="flex items-center space-x-4">
            <button
              type="button"
              onClick={() => fileInputRef.current?.click()}
              className="px-4 py-2 bg-gray-100 border border-gray-300 rounded-md hover:bg-gray-200 focus:outline-none focus:ring-2 focus:ring-green-500"
            >
              Choose New Thumbnail
            </button>
            {(previewImage || model.thumbnailUrl) && (
              <div className="w-16 h-16 rounded border overflow-hidden">
                <img
                  src={previewImage || model.thumbnailUrl}
                  alt="Thumbnail preview"
                  className="w-full h-full object-cover"
                />
              </div>
            )}
          </div>
          <input
            ref={fileInputRef}
            type="file"
            accept="image/*"
            onChange={handleThumbnailSelect}
            className="hidden"
          />
        </div>

        {/* License and Advanced Settings */}
        <div className="grid grid-cols-1 md:grid-cols-2 gap-6">
          <div>
            <label className="block text-sm font-medium text-gray-700 mb-2">
              License
            </label>
            <select
              value={formData.license}
              onChange={(e) => handleInputChange('license', parseInt(e.target.value) as LicenseTypes)}
              className="w-full px-3 py-2 border border-gray-300 rounded-md focus:outline-none focus:ring-2 focus:ring-green-500 focus:border-transparent"
            >
              {licenseOptions.map(option => (
                <option key={option.value} value={option.value}>
                  {option.label}
                </option>
              ))}
            </select>
          </div>
        </div>

        {/* Categories */}
        <div>
          <label className="block text-sm font-medium text-gray-700 mb-2">
            Categories
          </label>
          <div className="flex flex-wrap gap-2">
            {categoryOptions.map(category => (
              <button
                key={category.value}
                type="button"
                onClick={() => handleCategoryToggle(category.value)}
                className={`px-3 py-1 rounded-full text-sm border transition-colors ${
                  formData.categories.includes(category.value)
                    ? 'bg-green-600 border-green-500 text-white'
                    : 'bg-gray-100 border-gray-300 text-gray-700 hover:border-green-500 hover:bg-green-50'
                }`}
              >
                {category.label}
              </button>
            ))}
          </div>
        </div>

        {/* Tags */}
        <div>
          <label className="block text-sm font-medium text-gray-700 mb-2">
            Tags
          </label>
          <div className="flex flex-wrap gap-2 mb-2">
            {formData.tags.map((tag, index) => (
              <span
                key={index}
                className="inline-flex items-center px-3 py-1 rounded-full text-sm bg-blue-100 text-blue-800"
              >
                {tag}
                <button
                  type="button"
                  onClick={() => handleRemoveTag(tag)}
                  className="ml-2 text-blue-600 hover:text-blue-800"
                >
                  ×
                </button>
              </span>
            ))}
          </div>
          <div className="flex">
            <input
              type="text"
              value={newTag}
              onChange={(e) => setNewTag(e.target.value)}
              onKeyPress={(e) => e.key === 'Enter' && (e.preventDefault(), handleAddTag())}
              className="flex-1 px-3 py-2 border border-gray-300 rounded-l-md focus:outline-none focus:ring-2 focus:ring-green-500 focus:border-transparent"
              placeholder="Add a tag..."
            />
            <button
              type="button"
              onClick={handleAddTag}
              className="px-4 py-2 bg-green-600 text-white rounded-r-md hover:bg-green-700 focus:outline-none focus:ring-2 focus:ring-green-500"
            >
              Add
            </button>
          </div>
        </div>

        {/* Model Properties */}
        <div>
          <label className="block text-sm font-medium text-gray-700 mb-2">
            Model Properties
          </label>
          <div className="space-y-2">
            <label className="flex items-center">
              <input
                type="checkbox"
                checked={formData.aiGenerated}
                onChange={(e) => handleInputChange('aiGenerated', e.target.checked)}
                className="mr-2 text-green-600 bg-gray-100 border-gray-300 rounded focus:ring-green-500"
              />
              <span className="text-sm text-gray-700">AI Generated</span>
            </label>
            <label className="flex items-center">
              <input
                type="checkbox"
                checked={formData.wip}
                onChange={(e) => handleInputChange('wip', e.target.checked)}
                className="mr-2 text-green-600 bg-gray-100 border-gray-300 rounded focus:ring-green-500"
              />
              <span className="text-sm text-gray-700">Work in Progress</span>
            </label>
            <label className="flex items-center">
              <input
                type="checkbox"
                checked={formData.nsfw}
                onChange={(e) => handleInputChange('nsfw', e.target.checked)}
                className="mr-2 text-green-600 bg-gray-100 border-gray-300 rounded focus:ring-green-500"
              />
              <span className="text-sm text-gray-700">NSFW (Not Safe for Work)</span>
            </label>
            <label className="flex items-center">
              <input
                type="checkbox"
                checked={formData.isRemix}
                onChange={(e) => handleInputChange('isRemix', e.target.checked)}
                className="mr-2 text-green-600 bg-gray-100 border-gray-300 rounded focus:ring-green-500"
              />
              <span className="text-sm text-gray-700">Remix of Another Model</span>
            </label>
          </div>
        </div>

        {/* Remix URL (conditional) */}
        {formData.isRemix && (
          <div>
            <label className="block text-sm font-medium text-gray-700 mb-2">
              Original Model URL
            </label>
            <input
              type="url"
              value={formData.remixUrl}
              onChange={(e) => handleInputChange('remixUrl', e.target.value)}
              className="w-full px-3 py-2 border border-gray-300 rounded-md focus:outline-none focus:ring-2 focus:ring-green-500 focus:border-transparent"
              placeholder="https://..."
            />
          </div>
        )}

        {/* Action Buttons */}
        <div className="flex justify-end space-x-4 pt-4 border-t">
          <button
            type="button"
            onClick={onCancel}
            disabled={loading}
            className="px-6 py-2 text-gray-600 bg-gray-100 rounded-md hover:bg-gray-200 focus:outline-none focus:ring-2 focus:ring-gray-500 disabled:opacity-50"
          >
            Cancel
          </button>
          <button
            type="submit"
            disabled={loading || !formData.name.trim()}
            className="px-6 py-2 bg-green-600 text-white rounded-md hover:bg-green-700 focus:outline-none focus:ring-2 focus:ring-green-500 disabled:opacity-50 disabled:cursor-not-allowed"
          >
            {loading ? 'Saving...' : 'Save Changes'}
          </button>
        </div>
      </form>
    </div>
  );
};

export default ModelEditForm; 