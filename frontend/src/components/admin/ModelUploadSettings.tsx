import React, { useState, useEffect } from 'react';
import { Box, Button, Typography, Paper, TextField, CircularProgress, Alert, Chip, FormControl, FormControlLabel, Checkbox, Divider } from '@mui/material';
import { useNavigate } from 'react-router-dom';
import { useAppDispatch, useAppSelector } from '../../utils/hooks';
import { getSetupStatus } from '../../store/slices/authSlice';
import fileService, { FileUploadConfiguration, FileExtensionInfo } from '../../services/fileService';

// Define interface for settings state
interface UploadSettings {
  maxFileSize: number;
  allowedExtensions: string[];
  requireModeration: boolean;
  autoPublishUploads: boolean;
  userQuota: number;
  perCategoryFileSizeLimits: Record<string, number>;
}

// Conversion helpers
const bytesToMB = (bytes: number): number => {
  return Math.round((bytes / (1024 * 1024)) * 100) / 100; // Round to 2 decimal places
};

const mbToBytes = (mb: number): number => {
  return Math.round(mb * 1024 * 1024);
};

const ModelUploadSettings: React.FC = () => {
  const navigate = useNavigate();
  const dispatch = useAppDispatch();
  const { user, setupStatus } = useAppSelector(state => state.auth);
  const [loading, setLoading] = useState(true);
  const [saving, setSaving] = useState(false);
  const [error, setError] = useState('');
  const [success, setSuccess] = useState('');
  const [fileConfig, setFileConfig] = useState<FileUploadConfiguration | null>(null);
  
  const [settings, setSettings] = useState<UploadSettings>({
    maxFileSize: 1073741824, // 1GB default
    allowedExtensions: ['.stl', '.obj', '.fbx', '.glb', '.gltf', '.ply'],
    requireModeration: false,
    autoPublishUploads: true,
    userQuota: 50, // Default number of models a user can upload
    perCategoryFileSizeLimits: {
      "3D Model": 1073741824, // 1GB default for 3D models
      "Image": 10485760,      // 10MB default for images
      "Vector": 52428800,     // 50MB default for vector files
      "Document": 26214400    // 25MB default for documents
    }
  });

  // Check setup status on component mount
  useEffect(() => {
    const initializeSettings = async () => {
      setLoading(true);
      try {
        // Load setup status
        if (!setupStatus) {
          await dispatch(getSetupStatus());
        }
        
        // Load file configuration from backend
        const config = await fileService.getFileConfiguration();
        setFileConfig(config);
        
        // Update default settings based on configuration
        setSettings(prev => ({
          ...prev,
          maxFileSize: config.maxFileSizeBytes,
          allowedExtensions: config.supportedExtensions
            .filter(ext => ext.category === "3D Model")
            .map(ext => ext.extension),
          perCategoryFileSizeLimits: config.perCategoryFileSizeLimits || prev.perCategoryFileSizeLimits
        }));
      } catch (err) {
        console.error('Failed to load settings:', err);
        setError('Failed to load file configuration. Please try again.');
      } finally {
        setLoading(false);
      }
    };

    initializeSettings();
  }, [dispatch, setupStatus]);

  // Redirect if needed
  useEffect(() => {
    if (!user) {
      navigate('/login');
    } else if (setupStatus) {
      // If admin isn't configured, redirect to admin setup
      if (!setupStatus.isAdminConfigured) {
        navigate('/admin-setup');
      } 
      // If roles aren't configured, redirect to role setup
      else if (!setupStatus.isRoleConfigured) {
        navigate('/custom-role-setup');
      }
    }
  }, [user, setupStatus, navigate]);

  const handleChange = (e: React.ChangeEvent<HTMLInputElement>) => {
    const { name, value, checked, type } = e.target;
    
    if (name === 'maxFileSize') {
      const mbValue = Number(value);
      // Validate that MB value is reasonable
      if (!isNaN(mbValue) && mbValue >= 0 && mbValue <= 10000) { // Max 10,000 MB (10 GB)
        setSettings(prev => ({
          ...prev,
          maxFileSize: mbToBytes(mbValue)
        }));
      }
    } else {
      setSettings(prev => ({
        ...prev,
        [name]: type === 'checkbox' ? checked : value
      }));
    }
  };

  const handleExtensionChange = (e: React.ChangeEvent<HTMLInputElement>) => {
    const value = e.target.value;
    
    // Split by commas, trim whitespace, ensure they start with dot
    const extensions = value.split(',')
      .map(ext => ext.trim())
      .map(ext => ext.startsWith('.') ? ext : `.${ext}`);
      
    setSettings(prev => ({
      ...prev,
      allowedExtensions: extensions
    }));
  };

  // Toggle a specific extension
  const handleToggleExtension = (extension: string) => {
    setSettings(prev => {
      const isIncluded = prev.allowedExtensions.includes(extension);
      
      return {
        ...prev,
        allowedExtensions: isIncluded
          ? prev.allowedExtensions.filter(ext => ext !== extension)
          : [...prev.allowedExtensions, extension]
      };
    });
  };

  // Toggle all extensions in a category
  const handleToggleCategory = (category: string) => {
    if (!fileConfig) return;
    
    const categoryExtensions = fileConfig.supportedExtensions
      .filter(ext => ext.category === category)
      .map(ext => ext.extension);
      
    // Check if all extensions in this category are already selected
    const allSelected = categoryExtensions.every(ext => 
      settings.allowedExtensions.includes(ext)
    );
    
    setSettings(prev => {
      if (allSelected) {
        // Remove all extensions in this category
        return {
          ...prev,
          allowedExtensions: prev.allowedExtensions.filter(ext => 
            !categoryExtensions.includes(ext)
          )
        };
      } else {
        // Add all extensions in this category that aren't already included
        const newExtensions = categoryExtensions.filter(ext => 
          !prev.allowedExtensions.includes(ext)
        );
        
        return {
          ...prev,
          allowedExtensions: [...prev.allowedExtensions, ...newExtensions]
        };
      }
    });
  };

  const handleCategoryFileSizeChange = (category: string, value: string) => {
    const sizeInMB = Number(value);
    
    // Validate that MB value is reasonable
    if (!isNaN(sizeInMB) && sizeInMB >= 0 && sizeInMB <= 10000) { // Max 10,000 MB (10 GB)
      setSettings(prev => ({
        ...prev,
        perCategoryFileSizeLimits: {
          ...prev.perCategoryFileSizeLimits,
          [category]: mbToBytes(sizeInMB)
        }
      }));
    }
  };

  const handleSaveSettings = async () => {
    try {
      setSaving(true);
      
      // Create configuration data to send to API
      const configToSave = {
        maxFileSizeBytes: settings.maxFileSize,
        perCategoryFileSizeLimits: settings.perCategoryFileSizeLimits,
        allowedExtensions: settings.allowedExtensions,
        requireModeration: settings.requireModeration,
        autoPublishUploads: settings.autoPublishUploads,
        userQuota: settings.userQuota
      };
      
      // Send the updated configuration to the backend
      await fileService.updateFileConfiguration(configToSave);
      
      setSuccess('Model upload settings saved successfully! You will now be directed to the moderation settings.');
      setError('');
      
      // Navigate to next step (moderation settings) after a brief delay
      setTimeout(() => {
        navigate('/moderation-settings');
      }, 2000);
      
    } catch (err) {
      setError('Failed to save upload settings. Please try again.');
      console.error('Error saving upload settings:', err);
    } finally {
      setSaving(false);
    }
  };

  if (loading) {
    return (
      <Box sx={{ display: 'flex', justifyContent: 'center', alignItems: 'center', height: '100vh' }}>
        <CircularProgress />
      </Box>
    );
  }

  // Group extensions by category
  const extensionsByCategory = fileConfig?.supportedExtensions.reduce((acc, ext) => {
    if (!acc[ext.category]) {
      acc[ext.category] = [];
    }
    acc[ext.category].push(ext);
    return acc;
  }, {} as Record<string, FileExtensionInfo[]>) || {};

  return (
    <Box sx={{ display: 'flex', justifyContent: 'center', alignItems: 'center', minHeight: '100vh', p: 2 }}>
      <Paper elevation={3} sx={{ p: 4, maxWidth: 800, width: '100%' }}>
        <Typography variant="h4" component="h1" gutterBottom>
          Model Upload Settings
        </Typography>
        
        <Alert severity="info" sx={{ mb: 3 }}>
          Configure the rules and settings for model uploads. These settings control what users can upload and how uploads are processed.
        </Alert>
        
        <Box component="form" sx={{ mt: 3 }}>
          <Typography variant="h6" gutterBottom>
            File Settings
          </Typography>
          
          <FormControl fullWidth sx={{ mb: 3 }}>
            <TextField
              label="Maximum Total Upload Size (MB)"
              name="maxFileSize"
              type="number"
              value={bytesToMB(settings.maxFileSize)}
              onChange={handleChange}
              helperText={`Current maximum total upload size: ${fileService.formatFileSize(settings.maxFileSize)}`}
              fullWidth
              margin="normal"
              sx={{ maxWidth: 300 }}
              InputProps={{
                endAdornment: <Typography variant="body2" color="text.secondary">MB</Typography>,
                inputProps: { min: 0, step: 1 }
              }}
            />
          </FormControl>
          
          <Typography variant="h6" gutterBottom>
            Per-Category File Size Limits
          </Typography>
          
          <Alert severity="info" sx={{ mb: 2 }}>
            Configure maximum file size limits for each file category. This allows you to set different limits based on file type 
            (e.g., smaller limits for images but larger limits for 3D models).
          </Alert>
          
          <Box sx={{ mb: 3, display: 'grid', gridTemplateColumns: { xs: '1fr', md: 'repeat(2, 1fr)' }, gap: 2 }}>
            {Object.entries(settings.perCategoryFileSizeLimits).map(([category, sizeLimit]) => (
              <FormControl key={category} sx={{ mb: 2 }}>
                <Typography variant="subtitle2" gutterBottom>{category} Files</Typography>
                <TextField
                  label="Maximum Size (MB)"
                  type="number"
                  value={bytesToMB(sizeLimit)}
                  onChange={(e) => handleCategoryFileSizeChange(category, e.target.value)}
                  helperText={`Max: ${fileService.formatFileSize(sizeLimit)}`}
                  fullWidth
                  margin="normal"
                  sx={{ maxWidth: 200 }}
                  InputProps={{
                    endAdornment: <Typography variant="body2" color="text.secondary">MB</Typography>,
                    inputProps: { min: 0, step: 1 }
                  }}
                />
              </FormControl>
            ))}
          </Box>
          
          <Typography variant="h6" gutterBottom sx={{ mt: 3 }}>
            Allowed File Extensions
          </Typography>
          
          {Object.entries(extensionsByCategory).map(([category, extensions]) => (
            <Box key={category} sx={{ mb: 3 }}>
              <FormControlLabel
                control={
                  <Checkbox 
                    checked={extensions.every(ext => 
                      settings.allowedExtensions.includes(ext.extension)
                    )}
                    indeterminate={
                      extensions.some(ext => settings.allowedExtensions.includes(ext.extension)) &&
                      !extensions.every(ext => settings.allowedExtensions.includes(ext.extension))
                    }
                    onChange={() => handleToggleCategory(category)}
                  />
                }
                label={`${category} Files`}
              />
              
              <Box sx={{ display: 'flex', flexWrap: 'wrap', gap: 1, ml: 4, mt: 1 }}>
                {extensions.map(ext => (
                  <Chip
                    key={ext.id}
                    label={ext.extension}
                    color={settings.allowedExtensions.includes(ext.extension) ? "primary" : "default"}
                    onClick={() => handleToggleExtension(ext.extension)}
                    sx={{ cursor: 'pointer' }}
                  />
                ))}
              </Box>
            </Box>
          ))}
          
          <FormControl fullWidth sx={{ mb: 3 }}>
            <TextField
              label="Allowed File Extensions (manual)"
              name="allowedExtensions"
              value={settings.allowedExtensions.join(', ')}
              onChange={handleExtensionChange}
              helperText="Comma-separated list of file extensions (e.g., .stl, .obj)"
              fullWidth
              margin="normal"
            />
          </FormControl>
          
          <Divider sx={{ my: 3 }} />
          
          <Typography variant="h6" gutterBottom>
            Upload Behavior
          </Typography>
          
          <FormControl fullWidth component="fieldset" sx={{ mb: 2 }}>
            <FormControlLabel
              control={
                <Checkbox 
                  checked={settings.requireModeration}
                  onChange={handleChange}
                  name="requireModeration"
                />
              }
              label="Require moderation for all uploads"
            />
            <Typography variant="body2" color="text.secondary" sx={{ ml: 4 }}>
              When enabled, all model uploads will need to be approved by a moderator before they are visible to other users.
            </Typography>
          </FormControl>
          
          <FormControl fullWidth component="fieldset" sx={{ mb: 2 }}>
            <FormControlLabel
              control={
                <Checkbox 
                  checked={settings.autoPublishUploads}
                  onChange={handleChange}
                  name="autoPublishUploads"
                />
              }
              label="Auto-publish uploads when moderation is not required"
            />
            <Typography variant="body2" color="text.secondary" sx={{ ml: 4 }}>
              When enabled, uploads will be automatically published if moderation is not required.
            </Typography>
          </FormControl>
          
          <FormControl fullWidth sx={{ mb: 3 }}>
            <TextField
              label="User Upload Quota"
              name="userQuota"
              type="number"
              value={settings.userQuota}
              onChange={handleChange}
              helperText="Maximum number of models a user can upload (0 for unlimited)"
              fullWidth
              margin="normal"
            />
          </FormControl>

          {error && (
            <Alert severity="error" sx={{ mt: 2, mb: 2 }}>
              {error}
            </Alert>
          )}
          
          {success && (
            <Alert severity="success" sx={{ mt: 2, mb: 2 }}>
              {success}
            </Alert>
          )}
          
          <Box sx={{ mt: 4, display: 'flex', justifyContent: 'space-between' }}>
            <Button
              variant="outlined"
              onClick={() => navigate('/custom-role-setup')}
            >
              Back to Roles
            </Button>
            
            <Button
              variant="contained"
              color="primary"
              onClick={handleSaveSettings}
              disabled={saving}
            >
              {saving ? <CircularProgress size={24} /> : 'Continue to Moderation Settings'}
            </Button>
          </Box>
        </Box>
      </Paper>
    </Box>
  );
};

export default ModelUploadSettings; 