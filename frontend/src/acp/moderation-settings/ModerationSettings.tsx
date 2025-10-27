import React, { useState, useEffect } from 'react';
import { Box, Button, Checkbox, FormControlLabel, Typography, Paper, TextField, Chip, CircularProgress, Alert } from '@mui/material';
import { useNavigate } from 'react-router-dom';
import { useAppSelector } from '../../utils/hooks';

const ModerationSettings: React.FC = () => {
  const navigate = useNavigate();
  const { user } = useAppSelector(state => state.auth);
  const [loading, setLoading] = useState(false);
  const [saving, setSaving] = useState(false);
  const [settings, setSettings] = useState({
    requireUploadModeration: false,
    moderatorRoles: '',
    isModerationConfigured: false
  });
  const [error, setError] = useState('');
  const [success, setSuccess] = useState('');

  // Check if user is authenticated
  useEffect(() => {
    if (!user) {
      navigate('/login');
    }
  }, [user, navigate]);

  useEffect(() => {
    const fetchSettings = async () => {
      try {
        setLoading(true);
        // In a production environment, you would fetch these from an API
        // For now we'll use default values
        setSettings({
          requireUploadModeration: false,
          moderatorRoles: '',
          isModerationConfigured: false
        });
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
  }, [user]);

  const handleSaveSettings = async () => {
    try {
      setSaving(true);
      
      // In a real implementation, this would save to your API
      // For now, we'll simulate a successful save with a timeout
      setTimeout(async () => {
        try {
          setSuccess('Moderation settings saved successfully!');
          setError('');

          // Clear success message after 3 seconds
          setTimeout(() => {
            setSuccess('');
          }, 3000);
        } catch (err) {
          setError('Failed to update moderation settings. Please try again.');
          console.error('Error updating moderation settings:', err);
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

  const handleToggleModerationRequired = () => {
    setSettings(prev => ({
      ...prev,
      requireUploadModeration: !prev.requireUploadModeration
    }));
  };

  if (!user) {
    return (
      <Box sx={{ p: 3 }}>
        <Alert severity="warning">Please log in to access moderation settings.</Alert>
      </Box>
    );
  }

  return (
    <Paper elevation={2} sx={{ p: 3, maxWidth: 800, mx: 'auto' }}>
      <Typography variant="h5" gutterBottom>
        Moderation Settings
      </Typography>
      
      <Typography variant="body1" paragraph>
        Configure how uploaded models are moderated before being made public.
      </Typography>

      {error && <Alert severity="error" sx={{ mb: 2 }}>{error}</Alert>}
      {success && <Alert severity="success" sx={{ mb: 2 }}>{success}</Alert>}

      <Box component="form" sx={{ mt: 3 }}>
        {/* Upload Moderation Toggle */}
        <Box sx={{ mb: 3 }}>
          <FormControlLabel
            control={
              <Checkbox
                checked={settings.requireUploadModeration}
                onChange={handleToggleModerationRequired}
                color="primary"
              />
            }
            label="Require moderation for uploaded models"
          />
          <Typography variant="body2" color="text.secondary" sx={{ ml: 4 }}>
            When enabled, all uploaded models will need approval before becoming publicly visible.
          </Typography>
        </Box>

        {/* Moderator Roles */}
        <Box sx={{ mb: 3 }}>
          <Typography variant="h6" gutterBottom>
            Moderator Roles
          </Typography>
          <TextField
            fullWidth
            label="Moderator Role Names"
            value={settings.moderatorRoles}
            onChange={(e) => setSettings(prev => ({ ...prev, moderatorRoles: e.target.value }))}
            placeholder="Admin, Moderator, Reviewer"
            helperText="Comma-separated list of roles that can moderate content"
            sx={{ mb: 2 }}
          />
          
          {settings.moderatorRoles && (
            <Box sx={{ display: 'flex', flexWrap: 'wrap', gap: 1, mt: 1 }}>
              {settings.moderatorRoles.split(',').map((role, index) => (
                <Chip 
                  key={index} 
                  label={role.trim()} 
                  variant="outlined" 
                  size="small"
                />
              ))}
            </Box>
          )}
        </Box>

        {/* Additional Settings */}
        <Box sx={{ mb: 3 }}>
          <Typography variant="h6" gutterBottom>
            Additional Options
          </Typography>
          
          <FormControlLabel
            control={<Checkbox />}
            label="Auto-approve uploads from trusted users"
          />
          <Typography variant="body2" color="text.secondary" sx={{ ml: 4, mb: 2 }}>
            Users with specific roles or high reputation can have their uploads automatically approved.
          </Typography>

          <FormControlLabel
            control={<Checkbox />}
            label="Email notifications for new uploads"
          />
          <Typography variant="body2" color="text.secondary" sx={{ ml: 4, mb: 2 }}>
            Send email notifications to moderators when new content needs review.
          </Typography>
        </Box>

        {/* Save Button */}
        <Box sx={{ mt: 4, display: 'flex', justifyContent: 'flex-end' }}>
          <Button
            variant="contained"
            color="primary"
            onClick={handleSaveSettings}
            disabled={saving || loading}
            startIcon={saving ? <CircularProgress size={20} /> : null}
          >
            {saving ? 'Saving...' : 'Save Settings'}
          </Button>
        </Box>
      </Box>
    </Paper>
  );
};

export default ModerationSettings; 