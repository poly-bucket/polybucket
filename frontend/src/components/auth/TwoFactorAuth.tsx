import React, { useState, useEffect } from 'react';
import {
  Box,
  Card,
  CardContent,
  Typography,
  Button,
  Chip,
  Alert,
  Dialog,
  DialogTitle,
  DialogContent,
  DialogActions,
  TextField,
  CircularProgress,
  IconButton,
  Tooltip,
  Divider,
  Paper,
  Grid
} from '@mui/material';
import {
  Security,
  QrCode2,
  Refresh,
  Visibility,
  VisibilityOff,
  ContentCopy,
  CheckCircle,
  Error
} from '@mui/icons-material';
import QRCode from 'react-qr-code';
import { twoFactorAuthService, TwoFactorAuthStatus, InitializeTwoFactorAuthResponse } from '../../services/twoFactorAuthService';

interface TwoFactorAuthProps {
  onStatusChange?: (status: TwoFactorAuthStatus) => void;
}

const TwoFactorAuth: React.FC<TwoFactorAuthProps> = ({ onStatusChange }) => {
  const [status, setStatus] = useState<TwoFactorAuthStatus | null>(null);
  const [loading, setLoading] = useState(true);
  const [error, setError] = useState<string | null>(null);
  const [success, setSuccess] = useState<string | null>(null);
  
  // 2FA initialization states
  const [showInitializeDialog, setShowInitializeDialog] = useState(false);
  const [initializationData, setInitializationData] = useState<InitializeTwoFactorAuthResponse | null>(null);
  const [initializationLoading, setInitializationLoading] = useState(false);
  
  // 2FA enable states
  const [showEnableDialog, setShowEnableDialog] = useState(false);
  const [enableToken, setEnableToken] = useState('');
  const [enableLoading, setEnableLoading] = useState(false);
  const [showToken, setShowToken] = useState(false);
  const [enabledBackupCodes, setEnabledBackupCodes] = useState<string[]>([]);
  const [showEnabledBackupCodes, setShowEnabledBackupCodes] = useState(false);
  
  // 2FA disable states
  const [showDisableDialog, setShowDisableDialog] = useState(false);
  const [disableLoading, setDisableLoading] = useState(false);
  
  // Backup codes states
  const [showBackupCodes, setShowBackupCodes] = useState(false);
  const [copiedCodes, setCopiedCodes] = useState<Set<number>>(new Set());

  const loadStatus = async () => {
    try {
      setLoading(true);
      setError(null);
      const statusData = await twoFactorAuthService.getStatus();
      setStatus(statusData);
      onStatusChange?.(statusData);
    } catch (err) {
      setError('Failed to load 2FA status');
      console.error('Error loading 2FA status:', err);
    } finally {
      setLoading(false);
    }
  };

  useEffect(() => {
    loadStatus();
  }, []);

  const handleInitialize = async () => {
    try {
      setInitializationLoading(true);
      setError(null);
      const data = await twoFactorAuthService.initialize();
      setInitializationData(data);
      setShowInitializeDialog(true);
    } catch (err) {
      setError('Failed to initialize 2FA');
      console.error('Error initializing 2FA:', err);
    } finally {
      setInitializationLoading(false);
    }
  };

  const handleEnable = async () => {
    if (!enableToken.trim()) {
      setError('Please enter a valid 2FA token');
      return;
    }

    try {
      setEnableLoading(true);
      setError(null);
      const response = await twoFactorAuthService.enable(enableToken);
      
      if (response.success) {
        setSuccess('Two-factor authentication enabled successfully');
        setShowEnableDialog(false);
        setEnableToken('');
        
        // Handle backup codes if provided
        if (response.backupCodes && response.backupCodes.length > 0) {
          setEnabledBackupCodes(response.backupCodes);
          setShowEnabledBackupCodes(true);
        }
        
        await loadStatus();
      } else {
        setError(response.message || 'Failed to enable 2FA');
      }
    } catch (err) {
      setError('Failed to enable 2FA. Please check your token and try again.');
      console.error('Error enabling 2FA:', err);
    } finally {
      setEnableLoading(false);
    }
  };

  const handleDisable = async () => {
    try {
      setDisableLoading(true);
      setError(null);
      await twoFactorAuthService.disable();
      setSuccess('Two-factor authentication disabled successfully');
      setShowDisableDialog(false);
      await loadStatus();
    } catch (err) {
      setError('Failed to disable 2FA');
      console.error('Error disabling 2FA:', err);
    } finally {
      setDisableLoading(false);
    }
  };

  const copyToClipboard = async (text: string, index: number) => {
    try {
      await navigator.clipboard.writeText(text);
      setCopiedCodes(prev => new Set(prev).add(index));
      setTimeout(() => {
        setCopiedCodes(prev => {
          const newSet = new Set(prev);
          newSet.delete(index);
          return newSet;
        });
      }, 2000);
    } catch (err) {
      console.error('Failed to copy to clipboard:', err);
    }
  };

  const copyAllBackupCodes = async () => {
    // This function is no longer needed since backup codes are generated during enable
    return;
  };

  const copyAllEnabledBackupCodes = async () => {
    if (!enabledBackupCodes.length) return;
    
    try {
      const codesText = enabledBackupCodes.join('\n');
      await navigator.clipboard.writeText(codesText);
      setSuccess('All backup codes copied to clipboard');
    } catch (err) {
      console.error('Failed to copy backup codes:', err);
    }
  };

  if (loading) {
    return (
      <Box display="flex" justifyContent="center" p={3}>
        <CircularProgress />
      </Box>
    );
  }

  if (!status) {
    return (
      <Alert severity="error">
        Failed to load 2FA status
      </Alert>
    );
  }

  return (
    <Box>
      {error && (
        <Alert severity="error" sx={{ mb: 2 }} onClose={() => setError(null)}>
          {error}
        </Alert>
      )}
      
      {success && (
        <Alert severity="success" sx={{ mb: 2 }} onClose={() => setSuccess(null)}>
          {success}
        </Alert>
      )}

      <Card>
        <CardContent>
          <Box display="flex" alignItems="center" mb={2}>
            <Security sx={{ mr: 1 }} />
            <Typography variant="h6">Two-Factor Authentication</Typography>
          </Box>

          <Box mb={3}>
            <Typography variant="body2" color="text.secondary" gutterBottom>
              Two-factor authentication adds an extra layer of security to your account by requiring a second form of verification in addition to your password.
            </Typography>
          </Box>

          <Box display="flex" alignItems="center" mb={2}>
            <Chip
              icon={status.isEnabled ? <CheckCircle /> : <Error />}
              label={status.isEnabled ? 'Enabled' : 'Disabled'}
              color={status.isEnabled ? 'success' : 'default'}
              variant="outlined"
            />
            {status.isEnabled && status.enabledAt && (
              <Typography variant="body2" color="text.secondary" sx={{ ml: 2 }}>
                Enabled on {new Date(status.enabledAt).toLocaleDateString()}
              </Typography>
            )}
          </Box>

          {status.isEnabled && (
            <Box mb={3}>
              <Typography variant="body2" color="text.secondary">
                Remaining backup codes: {status.remainingBackupCodes}
              </Typography>
            </Box>
          )}

          <Box display="flex" gap={2} flexWrap="wrap">
            {!status.isInitialized ? (
              <Button
                variant="contained"
                color="primary"
                onClick={handleInitialize}
                disabled={initializationLoading}
                startIcon={initializationLoading ? <CircularProgress size={20} /> : <Security />}
              >
                {initializationLoading ? 'Initializing...' : 'Setup 2FA'}
              </Button>
            ) : !status.isEnabled ? (
              <>
                <Button
                  variant="contained"
                  color="primary"
                  onClick={() => setShowEnableDialog(true)}
                  startIcon={<Security />}
                >
                  Enable 2FA
                </Button>
                <Button
                  variant="outlined"
                  onClick={handleInitialize}
                  disabled={initializationLoading}
                  startIcon={initializationLoading ? <CircularProgress size={20} /> : <Refresh />}
                >
                  {initializationLoading ? 'Refreshing...' : 'Refresh QR Code'}
                </Button>
              </>
            ) : (
              <>
                <Button
                  variant="outlined"
                  color="error"
                  onClick={() => setShowDisableDialog(true)}
                  startIcon={<Security />}
                >
                  Disable 2FA
                </Button>
                <Button
                  variant="outlined"
                  onClick={handleInitialize}
                  disabled={initializationLoading}
                  startIcon={initializationLoading ? <CircularProgress size={20} /> : <Refresh />}
                >
                  {initializationLoading ? 'Refreshing...' : 'Refresh QR Code'}
                </Button>
              </>
            )}
          </Box>
        </CardContent>
      </Card>

      {/* Initialize 2FA Dialog */}
      <Dialog
        open={showInitializeDialog}
        onClose={() => setShowInitializeDialog(false)}
        maxWidth="md"
        fullWidth
      >
        <DialogTitle>Setup Two-Factor Authentication</DialogTitle>
        <DialogContent>
          {initializationData && (
            <Box>
              <Typography variant="body2" color="text.secondary" gutterBottom>
                Scan this QR code with your authenticator app (like Google Authenticator, Authy, or Microsoft Authenticator):
              </Typography>
              
              <Box display="flex" justifyContent="center" mb={2}>
                <div style={{ background: 'white', padding: '16px', borderRadius: '4px', border: '1px solid #ddd' }}>
                  <QRCode
                    value={initializationData.qrCodeUrl}
                    size={200}
                    level="M"
                    bgColor="#FFFFFF"
                    fgColor="#000000"
                    style={{ width: '100%', height: 'auto' }}
                  />
                </div>
              </Box>

              <Alert severity="warning" sx={{ mt: 2 }}>
                <Typography variant="body2">
                  <strong>Important:</strong> After scanning the QR code, click "Enable 2FA" and enter the 6-digit code from your authenticator app to complete the setup.
                </Typography>
              </Alert>
            </Box>
          )}
        </DialogContent>
        <DialogActions>
          <Button onClick={() => setShowInitializeDialog(false)}>Close</Button>
          <Button
            variant="contained"
            onClick={() => {
              setShowInitializeDialog(false);
              setShowEnableDialog(true);
            }}
          >
            Enable 2FA
          </Button>
        </DialogActions>
      </Dialog>

      {/* Enable 2FA Dialog */}
      <Dialog
        open={showEnableDialog}
        onClose={() => setShowEnableDialog(false)}
        maxWidth="sm"
        fullWidth
      >
        <DialogTitle>Enable Two-Factor Authentication</DialogTitle>
        <DialogContent>
          <Typography variant="body2" color="text.secondary" gutterBottom>
            Enter the 6-digit code from your authenticator app to enable two-factor authentication:
          </Typography>
          
          <TextField
            fullWidth
            label="2FA Token"
            value={enableToken}
            onChange={(e) => setEnableToken(e.target.value)}
            type={showToken ? 'text' : 'password'}
            placeholder="000000"
            inputProps={{ maxLength: 6, pattern: '[0-9]*' }}
            InputProps={{
              endAdornment: (
                <IconButton
                  onClick={() => setShowToken(!showToken)}
                  edge="end"
                >
                  {showToken ? <VisibilityOff /> : <Visibility />}
                </IconButton>
              )
            }}
            sx={{ mt: 2 }}
          />
        </DialogContent>
        <DialogActions>
          <Button onClick={() => setShowEnableDialog(false)}>Cancel</Button>
          <Button
            variant="contained"
            onClick={handleEnable}
            disabled={enableLoading || !enableToken.trim()}
            startIcon={enableLoading ? <CircularProgress size={20} /> : undefined}
          >
            {enableLoading ? 'Enabling...' : 'Enable 2FA'}
          </Button>
        </DialogActions>
      </Dialog>

      {/* Disable 2FA Dialog */}
      <Dialog
        open={showDisableDialog}
        onClose={() => setShowDisableDialog(false)}
        maxWidth="sm"
        fullWidth
      >
        <DialogTitle>Disable Two-Factor Authentication</DialogTitle>
        <DialogContent>
          <Alert severity="warning" sx={{ mb: 2 }}>
            <Typography variant="body2">
              <strong>Warning:</strong> Disabling two-factor authentication will reduce the security of your account. 
              Are you sure you want to proceed?
            </Typography>
          </Alert>
        </DialogContent>
        <DialogActions>
          <Button onClick={() => setShowDisableDialog(false)}>Cancel</Button>
          <Button
            variant="contained"
            color="error"
            onClick={handleDisable}
            disabled={disableLoading}
            startIcon={disableLoading ? <CircularProgress size={20} /> : undefined}
          >
            {disableLoading ? 'Disabling...' : 'Disable 2FA'}
          </Button>
        </DialogActions>
      </Dialog>

      {/* Backup Codes Dialog */}
      <Dialog
        open={showEnabledBackupCodes}
        onClose={() => setShowEnabledBackupCodes(false)}
        maxWidth="md"
        fullWidth
      >
        <DialogTitle>Backup Codes Generated</DialogTitle>
        <DialogContent>
          <Alert severity="info" sx={{ mb: 2 }}>
            <Typography variant="body2">
              <strong>Important:</strong> Save these backup codes in a secure location. You can use them to access your account if you lose your authenticator device.
            </Typography>
          </Alert>
          
          <Typography variant="body2" color="text.secondary" gutterBottom>
            Each code can only be used once. Generate new codes if you run out.
          </Typography>
          
          <Box sx={{ mt: 2 }}>
            <Box
              sx={{
                display: 'grid',
                gridTemplateColumns: 'repeat(auto-fit, minmax(120px, 1fr))',
                gap: 1
              }}
            >
              {enabledBackupCodes.map((code, index) => (
                <Paper
                  key={index}
                  sx={{
                    p: 1,
                    textAlign: 'center',
                    fontFamily: 'monospace',
                    fontSize: '0.875rem',
                    cursor: 'pointer',
                    '&:hover': {
                      backgroundColor: 'action.hover'
                    }
                  }}
                  onClick={() => copyToClipboard(code, index)}
                >
                  {code}
                  {copiedCodes.has(index) && (
                    <CheckCircle sx={{ ml: 1, fontSize: '1rem', color: 'success.main' }} />
                  )}
                </Paper>
              ))}
            </Box>
          </Box>
        </DialogContent>
        <DialogActions>
          <Button onClick={copyAllEnabledBackupCodes}>
            Copy All
          </Button>
          <Button
            variant="contained"
            onClick={() => setShowEnabledBackupCodes(false)}
          >
            I've Saved My Codes
          </Button>
        </DialogActions>
      </Dialog>
    </Box>
  );
};

export default TwoFactorAuth; 