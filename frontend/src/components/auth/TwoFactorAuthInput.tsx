import React, { useState } from 'react';
import {
  Box,
  TextField,
  Button,
  Typography,
  Alert,
  CircularProgress,
  IconButton,
  InputAdornment,
  Link
} from '@mui/material';
import {
  Visibility,
  VisibilityOff,
  Security
} from '@mui/icons-material';

interface TwoFactorAuthInputProps {
  onSubmit: (token: string, isBackupCode: boolean) => void;
  onCancel: () => void;
  loading?: boolean;
  error?: string;
  onUseBackupCode?: () => void;
  showBackupCodeOption?: boolean;
}

const TwoFactorAuthInput: React.FC<TwoFactorAuthInputProps> = ({
  onSubmit,
  onCancel,
  loading = false,
  error,
  onUseBackupCode,
  showBackupCodeOption = true
}) => {
  const [token, setToken] = useState('');
  const [showToken, setShowToken] = useState(false);
  const [isBackupCode, setIsBackupCode] = useState(false);

  const handleSubmit = (e: React.FormEvent) => {
    e.preventDefault();
    if (token.trim()) {
      onSubmit(token.trim(), isBackupCode);
    }
  };

  const handleUseBackupCode = () => {
    setIsBackupCode(true);
    setToken('');
    onUseBackupCode?.();
  };

  const handleUseToken = () => {
    setIsBackupCode(false);
    setToken('');
  };

  const handleKeyPress = (e: React.KeyboardEvent) => {
    if (e.key === 'Enter' && token.trim()) {
      handleSubmit(e);
    }
  };

  return (
    <Box component="form" onSubmit={handleSubmit} sx={{ width: '100%', maxWidth: 400 }}>
      <Box display="flex" alignItems="center" mb={2}>
        <Security sx={{ mr: 1, color: 'primary.main' }} />
        <Typography variant="h6">Two-Factor Authentication</Typography>
      </Box>

      <Typography variant="body2" color="text.secondary" sx={{ mb: 3 }}>
        {isBackupCode 
          ? 'Enter one of your backup codes to access your account.'
          : 'Enter the 6-digit code from your authenticator app.'
        }
      </Typography>

      {error && (
        <Alert severity="error" sx={{ mb: 2 }}>
          {error}
        </Alert>
      )}

      <TextField
        fullWidth
        label={isBackupCode ? 'Backup Code' : '2FA Token'}
        value={token}
        onChange={(e) => setToken(e.target.value)}
        type={showToken ? 'text' : 'password'}
        placeholder={isBackupCode ? 'Enter backup code' : '000000'}
        inputProps={{
          maxLength: isBackupCode ? 8 : 6,
          pattern: isBackupCode ? '[A-Z0-9]*' : '[0-9]*'
        }}
        onKeyPress={handleKeyPress}
        InputProps={{
          endAdornment: (
            <InputAdornment position="end">
              <IconButton
                onClick={() => setShowToken(!showToken)}
                edge="end"
              >
                {showToken ? <VisibilityOff /> : <Visibility />}
              </IconButton>
            </InputAdornment>
          )
        }}
        sx={{ mb: 2 }}
        autoFocus
      />

      <Box display="flex" gap={2} mb={2}>
        <Button
          type="submit"
          variant="contained"
          fullWidth
          disabled={loading || !token.trim()}
          startIcon={loading ? <CircularProgress size={20} /> : undefined}
        >
          {loading ? 'Verifying...' : 'Verify'}
        </Button>
        <Button
          variant="outlined"
          onClick={onCancel}
          disabled={loading}
        >
          Cancel
        </Button>
      </Box>

      {showBackupCodeOption && (
        <Box textAlign="center">
          {isBackupCode ? (
            <Link
              component="button"
              variant="body2"
              onClick={handleUseToken}
              disabled={loading}
              sx={{ cursor: 'pointer' }}
            >
              Use authenticator app instead
            </Link>
          ) : (
            <Link
              component="button"
              variant="body2"
              onClick={handleUseBackupCode}
              disabled={loading}
              sx={{ cursor: 'pointer' }}
            >
              Use backup code instead
            </Link>
          )}
        </Box>
      )}

      {isBackupCode && (
        <Alert severity="info" sx={{ mt: 2 }}>
          <Typography variant="body2">
            <strong>Note:</strong> Each backup code can only be used once. 
            Make sure to save your backup codes in a secure location.
          </Typography>
        </Alert>
      )}
    </Box>
  );
};

export default TwoFactorAuthInput; 