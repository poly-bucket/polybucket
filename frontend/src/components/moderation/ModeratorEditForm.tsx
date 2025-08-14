import React, { useState, useEffect } from 'react';
import { 
  TextField, 
  Button, 
  FormControl, 
  InputLabel, 
  Select, 
  MenuItem, 
  Grid, 
  Typography, 
  Box,
  Chip,
  OutlinedInput,
  SelectChangeEvent,
  FormHelperText,
  Dialog,
  DialogTitle,
  DialogContent,
  DialogActions,
  Alert,
  FormControlLabel
} from '@mui/material';
import { Model, LicenseTypes, PrivacySettings } from '../../services/api.client';
import { ModelCategories } from '../../types/api.types';

interface ModeratorEditFormProps {
  model: Model;
  isOpen: boolean;
  onClose: () => void;
  onSave: (editedModel: ModeratorEditRequest) => Promise<void>;
  loading?: boolean;
}

export interface ModeratorEditRequest {
  name: string;
  description: string;
  license?: LicenseTypes;
  privacy: PrivacySettings;
  aiGenerated: boolean;
  wip: boolean;
  nsfw: boolean;
  isRemix: boolean;
  remixUrl?: string;
  isPublic: boolean;
  isFeatured: boolean;
  tags: string[];
  categories: string[];
  moderationNotes?: string;
  action: ModerationAction;
}

export enum ModerationAction {
  Edit = 0,
  ApproveWithChanges = 1,
  FlagForReview = 2,
  FeatureModel = 3,
  UnfeatureModel = 4
}

const ModeratorEditForm: React.FC<ModeratorEditFormProps> = ({
  model,
  isOpen,
  onClose,
  onSave,
  loading = false
}) => {
  const [formData, setFormData] = useState<ModeratorEditRequest>({
    name: model.name || '',
    description: model.description || '',
    categories: model.categories?.map(cat => cat.name || '') || [],
    tags: model.tags?.map(tag => tag.name || '') || [],
    license: model.license || LicenseTypes.MIT,
    privacy: model.privacy || PrivacySettings.Public,
    aiGenerated: model.aiGenerated || false,
    wip: model.wip || false,
    nsfw: model.nsfw || false,
    isRemix: model.isRemix || false,
    remixUrl: model.remixUrl || '',
    isPublic: model.isPublic || false,
    isFeatured: model.isFeatured || false,
    moderationNotes: '',
    action: ModerationAction.Edit
  });

  const [newTag, setNewTag] = useState('');
  const [error, setError] = useState('');

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
    { value: PrivacySettings.Private, label: 'Private - Only author can see this model' },
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

  const moderationActionOptions = [
    { value: ModerationAction.Edit, label: 'Save Changes' },
    { value: ModerationAction.ApproveWithChanges, label: 'Approve with Changes' },
    { value: ModerationAction.FlagForReview, label: 'Flag for Further Review' },
    { value: ModerationAction.FeatureModel, label: 'Feature this Model' },
    { value: ModerationAction.UnfeatureModel, label: 'Remove from Featured' },
  ];

  const handleInputChange = (field: keyof ModeratorEditRequest, value: any) => {
    setFormData(prev => ({ ...prev, [field]: value }));
    setError('');
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

  const handleCategoryToggle = (category: string) => {
    setFormData(prev => ({
      ...prev,
      categories: prev.categories.includes(category)
        ? prev.categories.filter(c => c !== category)
        : [...prev.categories, category]
    }));
  };

  const handleSubmit = async () => {
    try {
      setError('');
      
      if (!formData.name.trim()) {
        setError('Model name is required');
        return;
      }
      
      if (!formData.description.trim()) {
        setError('Model description is required');
        return;
      }

      if (formData.isRemix && !formData.remixUrl?.trim()) {
        setError('Remix URL is required when marking as a remix');
        return;
      }

      await onSave(formData);
    } catch (err) {
      setError('Failed to save changes. Please try again.');
      console.error('Error saving model:', err);
    }
  };

  return (
    <Dialog open={isOpen} onClose={onClose} maxWidth="md" fullWidth>
      <DialogTitle>
        <Typography variant="h5" component="h2">
          Moderate Model: {model.name}
        </Typography>
        <Typography variant="body2" color="text.secondary">
          Author: {model.author?.username || 'Unknown'} • Created: {new Date(model.createdAt || '').toLocaleDateString()}
        </Typography>
      </DialogTitle>
      
      <DialogContent>
        {error && (
          <Alert severity="error" sx={{ mb: 2 }}>
            {error}
          </Alert>
        )}

        <Grid container spacing={3}>
          <Grid item xs={12}>
            <TextField
              fullWidth
              label="Model Name"
              value={formData.name}
              onChange={(e) => handleInputChange('name', e.target.value)}
              required
            />
          </Grid>
          
          <Grid item xs={12}>
            <TextField
              fullWidth
              multiline
              rows={4}
              label="Description"
              value={formData.description}
              onChange={(e) => handleInputChange('description', e.target.value)}
            />
          </Grid>
          
          <Grid item xs={12}>
            <FormControl fullWidth>
              <InputLabel>Categories</InputLabel>
              <Select
                multiple
                value={formData.categories}
                onChange={(e: SelectChangeEvent<string[]>) => handleInputChange('categories', e.target.value as string[])}
                input={<OutlinedInput label="Categories" />}
                renderValue={(selected) => (
                  <Box sx={{ display: 'flex', flexWrap: 'wrap', gap: 0.5 }}>
                    {selected.map((value) => (
                      <Chip key={value} label={value} />
                    ))}
                  </Box>
                )}
              >
                {Object.values(ModelCategories).map((category) => (
                  <MenuItem key={category} value={category}>
                    {category}
                  </MenuItem>
                ))}
              </Select>
            </FormControl>
          </Grid>
          
          <Grid item xs={12}>
            <FormControl fullWidth>
              <InputLabel>Tags</InputLabel>
              <Select
                multiple
                value={formData.tags}
                onChange={(e: SelectChangeEvent<string[]>) => handleInputChange('tags', e.target.value as string[])}
                input={<OutlinedInput label="Tags" />}
                renderValue={(selected) => (
                  <Box sx={{ display: 'flex', flexWrap: 'wrap', gap: 0.5 }}>
                    {selected.map((value) => (
                      <Chip key={value} label={value} />
                    ))}
                  </Box>
                )}
              >
                {/* availableTags is not defined in the original file, assuming it's a placeholder */}
                {/* For now, we'll just show a placeholder message */}
                <MenuItem disabled>
                  <em>No tags available</em>
                </MenuItem>
              </Select>
            </FormControl>
          </Grid>
          
          <Grid item xs={6}>
            <FormControl fullWidth>
              <InputLabel>License</InputLabel>
              <Select
                value={formData.license}
                onChange={(e: SelectChangeEvent<LicenseTypes>) => handleInputChange('license', e.target.value as LicenseTypes)}
                label="License"
              >
                {Object.values(LicenseTypes).map((license) => (
                  <MenuItem key={license} value={license}>
                    {license}
                  </MenuItem>
                ))}
              </Select>
            </FormControl>
          </Grid>
          
          <Grid item xs={6}>
            <FormControl fullWidth>
              <InputLabel>Privacy</InputLabel>
              <Select
                value={formData.privacy}
                onChange={(e: SelectChangeEvent<PrivacySettings>) => handleInputChange('privacy', e.target.value as PrivacySettings)}
                label="Privacy"
              >
                {Object.values(PrivacySettings).map((privacy) => (
                  <MenuItem key={privacy} value={privacy}>
                    {privacy}
                  </MenuItem>
                ))}
              </Select>
            </FormControl>
          </Grid>
          
          <Grid item xs={12}>
            <Box sx={{ display: 'flex', gap: 2, flexWrap: 'wrap' }}>
              <FormControl component="fieldset">
                <FormHelperText>Model Properties</FormHelperText>
                <Box sx={{ display: 'flex', gap: 2, flexWrap: 'wrap' }}>
                  <FormControlLabel
                    control={
                      <input
                        type="checkbox"
                        checked={formData.aiGenerated}
                        onChange={(e) => handleInputChange('aiGenerated', e.target.checked)}
                      />
                    }
                    label="AI Generated"
                  />
                  <FormControlLabel
                    control={
                      <input
                        type="checkbox"
                        checked={formData.wip}
                        onChange={(e) => handleInputChange('wip', e.target.checked)}
                      />
                    }
                    label="Work in Progress"
                  />
                  <FormControlLabel
                    control={
                      <input
                        type="checkbox"
                        checked={formData.nsfw}
                        onChange={(e) => handleInputChange('nsfw', e.target.checked)}
                      />
                    }
                    label="NSFW"
                  />
                  <FormControlLabel
                    control={
                      <input
                        type="checkbox"
                        checked={formData.isRemix}
                        onChange={(e) => handleInputChange('isRemix', e.target.checked)}
                      />
                    }
                    label="Remix"
                  />
                  <FormControlLabel
                    control={
                      <input
                        type="checkbox"
                        checked={formData.isFeatured}
                        onChange={(e) => handleInputChange('isFeatured', e.target.checked)}
                      />
                    }
                    label="Featured"
                  />
                </Box>
              </FormControl>
            </Box>
          </Grid>
          
          {formData.isRemix && (
            <Grid item xs={12}>
              <TextField
                fullWidth
                label="Remix URL"
                value={formData.remixUrl}
                onChange={(e) => handleInputChange('remixUrl', e.target.value)}
                placeholder="https://..."
              />
            </Grid>
          )}
          
          <Grid item xs={12}>
            <Box sx={{ display: 'flex', gap: 2, justifyContent: 'flex-end' }}>
              <Button variant="outlined" onClick={onClose}>
                Cancel
              </Button>
              <Button variant="contained" onClick={handleSubmit} disabled={loading}>
                {loading ? 'Saving...' : 'Save Changes'}
              </Button>
            </Box>
          </Grid>
        </Grid>
      </DialogContent>
      
      <DialogActions>
        <Button onClick={onClose}>Cancel</Button>
        <Button onClick={handleSubmit} variant="contained" disabled={loading}>
          {loading ? 'Saving...' : 'Save Changes'}
        </Button>
      </DialogActions>
    </Dialog>
  );
};

export default ModeratorEditForm; 