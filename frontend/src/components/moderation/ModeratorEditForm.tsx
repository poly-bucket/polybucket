import React, { useState } from 'react';
import {
  Box,
  Card,
  CardContent,
  Typography,
  TextField,
  FormControl,
  InputLabel,
  Select,
  MenuItem,
  FormControlLabel,
  Checkbox,
  Chip,
  Button,
  Grid,
  Alert,
  Dialog,
  DialogTitle,
  DialogContent,
  DialogActions,
  Divider,
  FormGroup
} from '@mui/material';
import { Model, LicenseTypes, PrivacySettings, ModelCategories } from '../../services/api.client';

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
    license: model.license,
    privacy: model.privacy || PrivacySettings.Public,
    aiGenerated: model.aiGenerated || false,
    wip: model.wip || false,
    nsfw: model.nsfw || false,
    isRemix: model.isRemix || false,
    remixUrl: model.remixUrl || '',
    isPublic: model.isPublic || false,
    isFeatured: model.isFeatured || false,
    tags: model.tags?.map(tag => tag.name || '') || [],
    categories: model.categoryCollection?.map(cat => cat.name || '') || [],
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
          {/* Basic Information */}
          <Grid item xs={12}>
            <Typography variant="h6" gutterBottom>
              Basic Information
            </Typography>
          </Grid>
          
          <Grid item xs={12}>
            <TextField
              fullWidth
              label="Model Name"
              value={formData.name}
              onChange={(e) => handleInputChange('name', e.target.value)}
              required
              helperText="The display name for this model"
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
              required
              helperText="Detailed description of the model"
            />
          </Grid>

          {/* Licensing and Privacy */}
          <Grid item xs={12}>
            <Divider sx={{ my: 2 }} />
            <Typography variant="h6" gutterBottom>
              Licensing & Privacy
            </Typography>
          </Grid>
          
          <Grid item xs={6}>
            <FormControl fullWidth>
              <InputLabel>License</InputLabel>
              <Select
                value={formData.license || ''}
                onChange={(e) => handleInputChange('license', e.target.value)}
                label="License"
              >
                {licenseOptions.map(option => (
                  <MenuItem key={option.value} value={option.value}>
                    {option.label}
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
                onChange={(e) => handleInputChange('privacy', e.target.value)}
                label="Privacy"
              >
                {privacyOptions.map(option => (
                  <MenuItem key={option.value} value={option.value}>
                    {option.label}
                  </MenuItem>
                ))}
              </Select>
            </FormControl>
          </Grid>

          {/* Content Flags */}
          <Grid item xs={12}>
            <Divider sx={{ my: 2 }} />
            <Typography variant="h6" gutterBottom>
              Content Flags
            </Typography>
          </Grid>
          
          <Grid item xs={12}>
            <FormGroup row>
              <FormControlLabel
                control={
                  <Checkbox
                    checked={formData.aiGenerated}
                    onChange={(e) => handleInputChange('aiGenerated', e.target.checked)}
                  />
                }
                label="AI Generated"
              />
              <FormControlLabel
                control={
                  <Checkbox
                    checked={formData.wip}
                    onChange={(e) => handleInputChange('wip', e.target.checked)}
                  />
                }
                label="Work in Progress"
              />
              <FormControlLabel
                control={
                  <Checkbox
                    checked={formData.nsfw}
                    onChange={(e) => handleInputChange('nsfw', e.target.checked)}
                  />
                }
                label="NSFW"
              />
              <FormControlLabel
                control={
                  <Checkbox
                    checked={formData.isRemix}
                    onChange={(e) => handleInputChange('isRemix', e.target.checked)}
                  />
                }
                label="Is Remix"
              />
            </FormGroup>
          </Grid>

          {formData.isRemix && (
            <Grid item xs={12}>
              <TextField
                fullWidth
                label="Original Model URL"
                value={formData.remixUrl}
                onChange={(e) => handleInputChange('remixUrl', e.target.value)}
                helperText="URL to the original model this is based on"
              />
            </Grid>
          )}

          {/* Moderation Controls */}
          <Grid item xs={12}>
            <Divider sx={{ my: 2 }} />
            <Typography variant="h6" gutterBottom>
              Moderation Controls
            </Typography>
          </Grid>
          
          <Grid item xs={12}>
            <FormGroup row>
              <FormControlLabel
                control={
                  <Checkbox
                    checked={formData.isPublic}
                    onChange={(e) => handleInputChange('isPublic', e.target.checked)}
                  />
                }
                label="Public"
              />
              <FormControlLabel
                control={
                  <Checkbox
                    checked={formData.isFeatured}
                    onChange={(e) => handleInputChange('isFeatured', e.target.checked)}
                  />
                }
                label="Featured"
              />
            </FormGroup>
          </Grid>

          {/* Tags */}
          <Grid item xs={12}>
            <Divider sx={{ my: 2 }} />
            <Typography variant="h6" gutterBottom>
              Tags
            </Typography>
          </Grid>
          
          <Grid item xs={12}>
            <Box sx={{ display: 'flex', gap: 1, mb: 2, flexWrap: 'wrap' }}>
              {formData.tags.map(tag => (
                <Chip
                  key={tag}
                  label={tag}
                  onDelete={() => handleRemoveTag(tag)}
                  color="primary"
                  variant="outlined"
                />
              ))}
            </Box>
            <Box sx={{ display: 'flex', gap: 1 }}>
              <TextField
                label="Add Tag"
                value={newTag}
                onChange={(e) => setNewTag(e.target.value)}
                onKeyPress={(e) => e.key === 'Enter' && handleAddTag()}
                size="small"
              />
              <Button onClick={handleAddTag} variant="outlined">
                Add
              </Button>
            </Box>
          </Grid>

          {/* Categories */}
          <Grid item xs={12}>
            <Typography variant="h6" gutterBottom>
              Categories
            </Typography>
            <Box sx={{ display: 'flex', flexWrap: 'wrap', gap: 1 }}>
              {categoryOptions.map(category => (
                <Chip
                  key={category.value}
                  label={category.label}
                  clickable
                  color={formData.categories.includes(category.value) ? 'primary' : 'default'}
                  onClick={() => handleCategoryToggle(category.value)}
                  variant={formData.categories.includes(category.value) ? 'filled' : 'outlined'}
                />
              ))}
            </Box>
          </Grid>

          {/* Moderation Action */}
          <Grid item xs={12}>
            <Divider sx={{ my: 2 }} />
            <Typography variant="h6" gutterBottom>
              Moderation Action
            </Typography>
          </Grid>
          
          <Grid item xs={12}>
            <FormControl fullWidth>
              <InputLabel>Action</InputLabel>
              <Select
                value={formData.action}
                onChange={(e) => handleInputChange('action', e.target.value)}
                label="Action"
              >
                {moderationActionOptions.map(option => (
                  <MenuItem key={option.value} value={option.value}>
                    {option.label}
                  </MenuItem>
                ))}
              </Select>
            </FormControl>
          </Grid>
          
          <Grid item xs={12}>
            <TextField
              fullWidth
              multiline
              rows={3}
              label="Moderation Notes"
              value={formData.moderationNotes}
              onChange={(e) => handleInputChange('moderationNotes', e.target.value)}
              helperText="Internal notes about this moderation action (not visible to the model author)"
            />
          </Grid>
        </Grid>
      </DialogContent>
      
      <DialogActions>
        <Button onClick={onClose} disabled={loading}>
          Cancel
        </Button>
        <Button
          onClick={handleSubmit}
          variant="contained"
          disabled={loading}
          color="primary"
        >
          {loading ? 'Saving...' : 'Save Changes'}
        </Button>
      </DialogActions>
    </Dialog>
  );
};

export default ModeratorEditForm; 