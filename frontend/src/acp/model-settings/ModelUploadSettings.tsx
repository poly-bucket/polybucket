import React, { useState, useEffect } from 'react';
import { Box, Button, FormControl, InputLabel, Select, MenuItem, TextField, Typography, Paper, Alert, CircularProgress } from '@mui/material';
import { useNavigate } from 'react-router-dom';
import { useAppSelector } from '../../utils/hooks';
import fileService from '../../services/fileService';

interface FileConfiguration {
  maxFileSizeBytes: number;
  supportedExtensions: Array<{
    extension: string;
    category: string;
    maxSize?: number;
  }>;
  perCategoryFileSizeLimits: Record<string, number>;
}

interface ModelUploadSettingsData {
  maxFileSize: number;
  allowedExtensions: string[];
  requiresModeration: boolean;
  autoPublish: boolean;
  maxModelsPerUser: number;
  perCategoryFileSizeLimits: Record<string, number>;
}

const ModelUploadSettings: React.FC = () => {
  const navigate = useNavigate();
  const { user } = useAppSelector(state => state.auth);
  const [loading, setLoading] = useState(true);
  const [saving, setSaving] = useState(false);
  const [fileConfig, setFileConfig] = useState<FileConfiguration | null>(null);
  const [settings, setSettings] = useState<ModelUploadSettingsData>({
    maxFileSize: 100 * 1024 * 1024, // 100MB default
    allowedExtensions: ['.stl', '.obj', '.ply', '.3mf'],
    requiresModeration: false,
    autoPublish: true,
    maxModelsPerUser: 50,
    perCategoryFileSizeLimits: {
      '3D Model': 100 * 1024 * 1024,
      'Texture': 50 * 1024 * 1024,
      'Documentation': 10 * 1024 * 1024
    }
  });
  const [error, setError] = useState('');
  const [success, setSuccess] = useState('');

  // Load file configuration from backend
  useEffect(() => {
    const initializeSettings = async () => {
      setLoading(true);
      try {
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
  }, []);

  // Check if user is authenticated
  useEffect(() => {
    if (!user) {
      navigate('/login');
    }
  }, [user, navigate]);

  const handleSaveSettings = async () => {
    try {
      setSaving(true);
      
      // In a real implementation, this would save to your API
      // For now, we'll simulate a successful save
      setTimeout(() => {
        setSuccess('Model upload settings saved successfully!');
        setError('');
        setSaving(false);
        
        // Clear success message after 3 seconds
        setTimeout(() => setSuccess(''), 3000);
      }, 1000);
    } catch (err) {
      setError('Failed to save model upload settings. Please try again.');
      console.error('Error saving settings:', err);
      setSaving(false);
    }
  };

  const formatFileSize = (bytes: number): string => {
    if (bytes >= 1024 * 1024 * 1024) {
      return `${(bytes / (1024 * 1024 * 1024)).toFixed(1)} GB`;
    } else if (bytes >= 1024 * 1024) {
      return `${(bytes / (1024 * 1024)).toFixed(1)} MB`;
    } else if (bytes >= 1024) {
      return `${(bytes / 1024).toFixed(1)} KB`;
    }
    return `${bytes} bytes`;
  };

  if (loading) {
    return (
      <Box sx={{ display: 'flex', justifyContent: 'center', alignItems: 'center', minHeight: 400 }}>
        <CircularProgress />
        <Typography sx={{ ml: 2 }}>Loading settings...</Typography>
      </Box>
    );
  }

  if (!user) {
    return (
      <Box sx={{ p: 3 }}>
        <Alert severity="warning">Please log in to access model upload settings.</Alert>
      </Box>
    );
  }

  return (
    <Paper elevation={2} sx={{ p: 3, maxWidth: 800, mx: 'auto' }}>
      <Typography variant="h5" gutterBottom>
        Model Upload Settings
      </Typography>
      
      {error && <Alert severity="error" sx={{ mb: 2 }}>{error}</Alert>}
      {success && <Alert severity="success" sx={{ mb: 2 }}>{success}</Alert>}

      <Box component="form" noValidate sx={{ mt: 2 }}>
        {/* File Size Limits */}
        <Typography variant="h6" gutterBottom sx={{ mt: 3 }}>
          File Size Limits
        </Typography>
        
        <TextField
          fullWidth
          label="Maximum File Size (bytes)"
          type="number"
          value={settings.maxFileSize}
          onChange={(e) => setSettings(prev => ({ ...prev, maxFileSize: Number(e.target.value) }))}
          helperText={`Current limit: ${formatFileSize(settings.maxFileSize)}`}
          sx={{ mb: 2 }}
        />

        {/* Allowed Extensions */}
        <Typography variant="h6" gutterBottom sx={{ mt: 3 }}>
          Allowed File Extensions
        </Typography>
        
        <TextField
          fullWidth
          label="Allowed Extensions"
          value={settings.allowedExtensions.join(', ')}
          onChange={(e) => setSettings(prev => ({ 
            ...prev, 
            allowedExtensions: e.target.value.split(',').map(ext => ext.trim()) 
          }))}
          helperText="Comma-separated list of file extensions (e.g., .stl, .obj, .ply)"
          sx={{ mb: 2 }}
        />

        {/* Moderation Settings */}
        <Typography variant="h6" gutterBottom sx={{ mt: 3 }}>
          Moderation
        </Typography>
        
        <FormControl fullWidth sx={{ mb: 2 }}>
          <InputLabel>Requires Moderation</InputLabel>
          <Select
            value={settings.requiresModeration}
            onChange={(e) => setSettings(prev => ({ ...prev, requiresModeration: Boolean(e.target.value) }))}
          >
            <MenuItem value={false}>No - Auto-publish uploads</MenuItem>
            <MenuItem value={true}>Yes - Require approval</MenuItem>
          </Select>
        </FormControl>

        {/* User Limits */}
        <Typography variant="h6" gutterBottom sx={{ mt: 3 }}>
          User Limits
        </Typography>
        
        <TextField
          fullWidth
          label="Maximum Models Per User"
          type="number"
          value={settings.maxModelsPerUser}
          onChange={(e) => setSettings(prev => ({ ...prev, maxModelsPerUser: Number(e.target.value) }))}
          sx={{ mb: 2 }}
        />

        {/* Current Configuration Display */}
        {fileConfig && (
          <Box sx={{ mt: 3, p: 2, bgcolor: 'grey.100', borderRadius: 1 }}>
            <Typography variant="h6" gutterBottom>
              Current Backend Configuration
            </Typography>
            <Typography variant="body2">
              Max File Size: {formatFileSize(fileConfig.maxFileSizeBytes)}
            </Typography>
            <Typography variant="body2">
              Supported Extensions: {fileConfig.supportedExtensions.map(ext => ext.extension).join(', ')}
            </Typography>
          </Box>
        )}

        {/* Save Button */}
        <Box sx={{ mt: 4, display: 'flex', justifyContent: 'flex-end' }}>
          <Button
            variant="contained"
            color="primary"
            onClick={handleSaveSettings}
            disabled={saving}
            startIcon={saving ? <CircularProgress size={20} /> : null}
          >
            {saving ? 'Saving...' : 'Save Settings'}
          </Button>
        </Box>
      </Box>
    </Paper>
  );
};

export default ModelUploadSettings; 