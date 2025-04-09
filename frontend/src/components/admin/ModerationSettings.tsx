import React, { useState, useEffect } from 'react';
import { Box, Button, Checkbox, FormControlLabel, Typography, Paper, TextField, Chip, CircularProgress, Alert } from '@mui/material';
import { useNavigate } from 'react-router-dom';
import { useAppDispatch, useAppSelector } from '../../utils/hooks';
import { getSetupStatus, setModerationConfigured } from '../../store/slices/authSlice';
import axios from 'axios';

const ModerationSettings: React.FC = () => {
  const navigate = useNavigate();
  const dispatch = useAppDispatch();
  const { user, setupStatus } = useAppSelector(state => state.auth);
  const [loading, setLoading] = useState(true);
  const [saving, setSaving] = useState(false);
  const [settings, setSettings] = useState({
    requireUploadModeration: false,
    moderatorRoles: '',
    isModerationConfigured: false
  });
  const [error, setError] = useState('');
  const [success, setSuccess] = useState('');

  // Check setup status on component mount
  useEffect(() => {
    if (!setupStatus) {
      dispatch(getSetupStatus());
    }
  }, [dispatch, setupStatus]);

  // Check if user is authenticated and redirect if needed
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
      // Set loading to false when setup status is available
      setLoading(false);
    }
  }, [user, setupStatus, navigate]);

  useEffect(() => {
    const fetchSettings = async () => {
      try {
        setLoading(true);
        // In a production environment, you would fetch these from an API
        // For now we'll use the setupStatus if available
        if (setupStatus) {
          setSettings({
            requireUploadModeration: setupStatus.requireUploadModeration || false,
            moderatorRoles: setupStatus.moderatorRoles || '',
            isModerationConfigured: setupStatus.isModerationConfigured || false
          });
        }
        setError('');
      } catch (err) {
        setError('Failed to load moderation settings. Please try again.');
        console.error('Error fetching moderation settings:', err);
      } finally {
        setLoading(false);
      }
    };

    if (user) {
      fetchSettings();
    }
  }, [user, setupStatus]);

  const handleSaveSettings = async () => {
    try {
      setSaving(true);
      
      // In a real implementation, this would save to your API
      // For now, we'll simulate a successful save with a timeout
      setTimeout(async () => {
        try {
          // Update auth slice to show moderation is configured
          await dispatch(setModerationConfigured(true)).unwrap();
          
          setSuccess('Moderation settings saved successfully! Your system setup is now complete.');
          setError('');
  
          // Clear success message after 2 seconds and redirect to dashboard
          setTimeout(() => {
            setSuccess('');
            navigate('/dashboard');
          }, 2000);
        } catch (err) {
          setError('Failed to update setup status. Please try again.');
          console.error('Error updating setup status:', err);
        } finally {
          setSaving(false);
        }
      }, 1000);
    } catch (err) {
      setError('Failed to save moderation settings. Please try again.');
      console.error('Error saving moderation settings:', err);
      setSaving(false);
    }
  };

  const handleChange = (e: React.ChangeEvent<HTMLInputElement>) => {
    const { name, value, checked, type } = e.target;
    setSettings(prev => ({
      ...prev,
      [name]: type === 'checkbox' ? checked : value
    }));
  };

  if (loading) {
    return (
      <Box sx={{ display: 'flex', justifyContent: 'center', alignItems: 'center', height: '100vh' }}>
        <CircularProgress />
      </Box>
    );
  }

  return (
    <Box sx={{ display: 'flex', justifyContent: 'center', alignItems: 'center', minHeight: '100vh', p: 2 }}>
      <Paper elevation={3} sx={{ p: 4, maxWidth: 800, width: '100%' }}>
        <Typography variant="h4" component="h1" gutterBottom>
          Upload Moderation Settings
        </Typography>
        
        {!settings.isModerationConfigured && (
          <Alert severity="info" sx={{ mb: 3 }}>
            <Typography variant="body2">
              This is the final step in setting up your system. Configure moderation settings to determine whether uploads require approval.
            </Typography>
          </Alert>
        )}
        
        <Box sx={{ mt: 3 }}>
          <FormControlLabel
            control={
              <Checkbox
                checked={settings.requireUploadModeration}
                onChange={handleChange}
                name="requireUploadModeration"
              />
            }
            label="Require moderation for all uploads"
          />

          <Typography variant="body2" color="text.secondary" sx={{ mt: 1, mb: 2 }}>
            When enabled, new model uploads will require approval from a moderator before becoming visible to other users.
          </Typography>

          <TextField
            fullWidth
            label="Moderator Roles (comma-separated role names or IDs)"
            name="moderatorRoles"
            value={settings.moderatorRoles}
            onChange={handleChange}
            margin="normal"
            helperText="Enter the role names or IDs that should have moderation privileges"
          />

          {settings.moderatorRoles && (
            <Box sx={{ display: 'flex', flexWrap: 'wrap', gap: 1, my: 2 }}>
              {settings.moderatorRoles.split(',').map((role, index) => (
                <Chip key={index} label={role.trim()} size="small" />
              ))}
            </Box>
          )}

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
              onClick={() => navigate('/model-upload-settings')}
            >
              Back to Upload Settings
            </Button>
            
            <Button
              variant="contained"
              color="primary"
              onClick={handleSaveSettings}
              disabled={saving}
            >
              {saving ? <CircularProgress size={24} /> : (settings.isModerationConfigured ? 'Update Settings' : 'Complete Setup')}
            </Button>
          </Box>
        </Box>
      </Paper>
    </Box>
  );
};

export default ModerationSettings; 