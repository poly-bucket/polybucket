import React, { useState, useEffect } from 'react';
import { Box, Tabs, Tab, Typography, Paper, Container, Alert, Stepper, Step, StepLabel, Button } from '@mui/material';
import { useAuth } from '../../context/AuthContext';
import ModerationSettings from './ModerationSettings';
import RoleManagement from './RoleManagement';
import axios from 'axios';

interface TabPanelProps {
  children?: React.ReactNode;
  index: number;
  value: number;
}

function TabPanel(props: TabPanelProps) {
  const { children, value, index, ...other } = props;

  return (
    <div
      role="tabpanel"
      hidden={value !== index}
      id={`admin-tabpanel-${index}`}
      aria-labelledby={`admin-tab-${index}`}
      {...other}
    >
      {value === index && (
        <Box sx={{ p: 3 }}>
          {children}
        </Box>
      )}
    </div>
  );
}

const AdminDashboard: React.FC = () => {
  const { token, user } = useAuth();
  const [tabValue, setTabValue] = useState(0);
  const [setupStatus, setSetupStatus] = useState({
    isAdminConfigured: false,
    isRoleConfigured: false,
    isModerationConfigured: false,
    requireUploadModeration: false
  });
  const [error, setError] = useState('');

  useEffect(() => {
    const fetchSetupStatus = async () => {
      try {
        const response = await axios.get('/api/admin/settings', {
          headers: { Authorization: `Bearer ${token}` }
        });
        
        setSetupStatus({
          isAdminConfigured: response.data.isAdminConfigured,
          isRoleConfigured: response.data.isRoleConfigured,
          isModerationConfigured: response.data.isModerationConfigured,
          requireUploadModeration: response.data.requireUploadModeration
        });

        // Auto-focus on the next unconfigured step
        if (!response.data.isRoleConfigured) {
          setTabValue(0); // Role Management
        } else if (!response.data.isModerationConfigured) {
          setTabValue(1); // Moderation Settings
        }
      } catch (err) {
        setError('Failed to load system settings. Please refresh the page or contact support.');
        console.error('Error fetching system settings:', err);
      }
    };

    if (token) {
      fetchSetupStatus();
    }
  }, [token]);

  const handleTabChange = (event: React.SyntheticEvent, newValue: number) => {
    setTabValue(newValue);
  };

  // Determine active step for setup stepper
  const getActiveStep = () => {
    if (!setupStatus.isRoleConfigured) return 0;
    if (!setupStatus.isModerationConfigured) return 1;
    return 2;
  };

  // If roles are not yet configured, guide admin to set them up first
  const showRoleSetupGuide = !setupStatus.isRoleConfigured && tabValue === 1;

  return (
    <Container maxWidth="lg" sx={{ mt: 4, mb: 4 }}>
      <Paper elevation={3} sx={{ p: 3 }}>
        <Typography variant="h4" component="h1" gutterBottom>
          Admin Dashboard
        </Typography>
        
        {error && <Alert severity="error" sx={{ mb: 3 }}>{error}</Alert>}
        
        {/* System Setup Progress */}
        <Box sx={{ mb: 4 }}>
          <Typography variant="h6" gutterBottom>
            System Setup Progress
          </Typography>
          <Stepper activeStep={getActiveStep()}>
            <Step completed={setupStatus.isRoleConfigured}>
              <StepLabel>Configure Roles</StepLabel>
            </Step>
            <Step completed={setupStatus.isModerationConfigured}>
              <StepLabel>Setup Moderation</StepLabel>
            </Step>
            <Step completed={setupStatus.isAdminConfigured && setupStatus.isRoleConfigured && setupStatus.isModerationConfigured}>
              <StepLabel>System Ready</StepLabel>
            </Step>
          </Stepper>
          
          {getActiveStep() === 0 && (
            <Alert severity="info" sx={{ mt: 2 }}>
              <Typography variant="body2">
                Please configure user roles first. This will define what permissions different user groups have.
              </Typography>
              <Button 
                color="primary" 
                variant="contained" 
                size="small" 
                sx={{ mt: 1 }}
                onClick={() => setTabValue(0)}
              >
                Go to Role Management
              </Button>
            </Alert>
          )}
          
          {getActiveStep() === 1 && (
            <Alert severity="info" sx={{ mt: 2 }}>
              <Typography variant="body2">
                Now that roles are configured, you can set up moderation for model uploads.
                Decide if users need approval before their models appear publicly.
              </Typography>
              <Button 
                color="primary" 
                variant="contained" 
                size="small" 
                sx={{ mt: 1 }}
                onClick={() => setTabValue(1)}
              >
                Go to Moderation Settings
              </Button>
            </Alert>
          )}
          
          {getActiveStep() === 2 && (
            <Alert severity="success" sx={{ mt: 2 }}>
              <Typography variant="body2">
                System setup is complete! You can now manage your 3D model repository.
              </Typography>
            </Alert>
          )}
        </Box>
        
        <Box sx={{ borderBottom: 1, borderColor: 'divider' }}>
          <Tabs value={tabValue} onChange={handleTabChange} aria-label="admin dashboard tabs">
            <Tab label="Role Management" />
            <Tab label="Moderation Settings" disabled={!setupStatus.isRoleConfigured} />
            <Tab label="Site Configuration" />
          </Tabs>
        </Box>
        
        <TabPanel value={tabValue} index={0}>
          <RoleManagement />
        </TabPanel>
        
        <TabPanel value={tabValue} index={1}>
          {showRoleSetupGuide ? (
            <Alert severity="warning" sx={{ mb: 2 }}>
              <Typography variant="body1">
                Please configure user roles first before setting up moderation.
                Moderation settings allow you to select which roles can approve or reject model uploads.
              </Typography>
              <Button 
                color="primary" 
                variant="contained" 
                size="small" 
                sx={{ mt: 1 }}
                onClick={() => setTabValue(0)}
              >
                Go to Role Management
              </Button>
            </Alert>
          ) : (
            <ModerationSettings />
          )}
        </TabPanel>
        
        <TabPanel value={tabValue} index={2}>
          <Typography variant="h6">Site Configuration</Typography>
          <Typography>Additional site configuration options will be available here.</Typography>
        </TabPanel>
      </Paper>
    </Container>
  );
};

export default AdminDashboard; 