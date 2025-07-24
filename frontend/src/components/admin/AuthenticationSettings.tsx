import React, { useState, useEffect } from 'react';
import {
  Box,
  Card,
  CardContent,
  Typography,
  FormControl,
  FormControlLabel,
  Switch,
  RadioGroup,
  Radio,
  Button,
  Alert,
  Divider,
  TextField,
  InputAdornment
} from '@mui/material';
import { Save as SaveIcon } from '@mui/icons-material';

interface AuthenticationSettings {
  loginMethod: 'email' | 'username' | 'both';
  allowEmailLogin: boolean;
  allowUsernameLogin: boolean;
  requireEmailVerification: boolean;
  maxFailedLoginAttempts: number;
  lockoutDurationMinutes: number;
  requireStrongPasswords: boolean;
  passwordMinLength: number;
}

const AuthenticationSettings: React.FC = () => {
  const [settings, setSettings] = useState<AuthenticationSettings>({
    loginMethod: 'email',
    allowEmailLogin: true,
    allowUsernameLogin: false,
    requireEmailVerification: false,
    maxFailedLoginAttempts: 5,
    lockoutDurationMinutes: 15,
    requireStrongPasswords: true,
    passwordMinLength: 8
  });

  const [loading, setLoading] = useState(false);
  const [success, setSuccess] = useState('');
  const [error, setError] = useState('');

  useEffect(() => {
    loadSettings();
  }, []);

  const loadSettings = async () => {
    try {
      setLoading(true);
      // TODO: Implement API call to load settings
      // const response = await fetch('/api/system-settings/auth');
      // const data = await response.json();
      // setSettings(data);
    } catch (err) {
      setError('Failed to load authentication settings');
    } finally {
      setLoading(false);
    }
  };

  const handleSave = async () => {
    try {
      setLoading(true);
      setError('');
      setSuccess('');

      // TODO: Implement API call to save settings
      // const response = await fetch('/api/system-settings/auth', {
      //   method: 'PUT',
      //   headers: { 'Content-Type': 'application/json' },
      //   body: JSON.stringify(settings)
      // });

      // if (!response.ok) {
      //   throw new Error('Failed to save settings');
      // }

      setSuccess('Authentication settings saved successfully');
    } catch (err) {
      setError('Failed to save authentication settings');
    } finally {
      setLoading(false);
    }
  };

  const handleLoginMethodChange = (method: 'email' | 'username' | 'both') => {
    setSettings(prev => ({
      ...prev,
      loginMethod: method,
      allowEmailLogin: method === 'email' || method === 'both',
      allowUsernameLogin: method === 'username' || method === 'both'
    }));
  };

  return (
    <Box sx={{ p: 3 }}>
      <Typography variant="h4" component="h1" gutterBottom>
        Authentication Settings
      </Typography>

      {error && <Alert severity="error" sx={{ mb: 2 }}>{error}</Alert>}
      {success && <Alert severity="success" sx={{ mb: 2 }}>{success}</Alert>}

      <Card>
        <CardContent>
          <Typography variant="h6" gutterBottom>
            Login Methods
          </Typography>
          
          <FormControl component="fieldset" sx={{ mb: 3 }}>
            <Typography variant="subtitle1" gutterBottom>
              Choose which login methods are allowed:
            </Typography>
            <RadioGroup
              value={settings.loginMethod}
              onChange={(e) => handleLoginMethodChange(e.target.value as 'email' | 'username' | 'both')}
            >
              <FormControlLabel
                value="email"
                control={<Radio />}
                label="Email only - Users can only login with their email address"
              />
              <FormControlLabel
                value="username"
                control={<Radio />}
                label="Username only - Users can only login with their username"
              />
              <FormControlLabel
                value="both"
                control={<Radio />}
                label="Both email and username - Users can login with either email or username"
              />
            </RadioGroup>
          </FormControl>

          <Divider sx={{ my: 3 }} />

          <Typography variant="h6" gutterBottom>
            Security Settings
          </Typography>

          <Box sx={{ display: 'grid', gridTemplateColumns: '1fr 1fr', gap: 2, mb: 3 }}>
            <TextField
              label="Max Failed Login Attempts"
              type="number"
              value={settings.maxFailedLoginAttempts}
              onChange={(e) => setSettings(prev => ({
                ...prev,
                maxFailedLoginAttempts: parseInt(e.target.value) || 5
              }))}
              inputProps={{ min: 1, max: 10 }}
              InputProps={{
                endAdornment: <InputAdornment position="end">attempts</InputAdornment>
              }}
            />

            <TextField
              label="Lockout Duration"
              type="number"
              value={settings.lockoutDurationMinutes}
              onChange={(e) => setSettings(prev => ({
                ...prev,
                lockoutDurationMinutes: parseInt(e.target.value) || 15
              }))}
              inputProps={{ min: 5, max: 60 }}
              InputProps={{
                endAdornment: <InputAdornment position="end">minutes</InputAdornment>
              }}
            />

            <TextField
              label="Password Minimum Length"
              type="number"
              value={settings.passwordMinLength}
              onChange={(e) => setSettings(prev => ({
                ...prev,
                passwordMinLength: parseInt(e.target.value) || 8
              }))}
              inputProps={{ min: 6, max: 20 }}
              InputProps={{
                endAdornment: <InputAdornment position="end">characters</InputAdornment>
              }}
            />
          </Box>

          <FormControlLabel
            control={
              <Switch
                checked={settings.requireStrongPasswords}
                onChange={(e) => setSettings(prev => ({
                  ...prev,
                  requireStrongPasswords: e.target.checked
                }))}
              />
            }
            label="Require strong passwords (uppercase, lowercase, numbers, symbols)"
          />

          <FormControlLabel
            control={
              <Switch
                checked={settings.requireEmailVerification}
                onChange={(e) => setSettings(prev => ({
                  ...prev,
                  requireEmailVerification: e.target.checked
                }))}
              />
            }
            label="Require email verification for new accounts"
          />

          <Box sx={{ mt: 3 }}>
            <Button
              variant="contained"
              startIcon={<SaveIcon />}
              onClick={handleSave}
              disabled={loading}
            >
              {loading ? 'Saving...' : 'Save Settings'}
            </Button>
          </Box>
        </CardContent>
      </Card>
    </Box>
  );
};

export default AuthenticationSettings; 