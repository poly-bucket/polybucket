import React, { useState, useEffect } from 'react';
import { 
  Box, 
  Tabs, 
  Tab, 
  Typography, 
  Paper, 
  Container, 
  Grid, 
  Card, 
  CardContent, 
  Button,
  Chip,
  Divider,
  List,
  ListItem,
  ListItemText,
  ListItemIcon,
  Switch,
  FormControlLabel,
  TextField,
  Alert,
  Badge,
  Dialog,
  DialogTitle,
  DialogContent,
  DialogActions,
  MenuItem,
  Select,
  FormControl,
  InputLabel,
  Snackbar,
  CircularProgress,
  InputAdornment,
  IconButton
} from '@mui/material';
import {
  People as PeopleIcon,
  ViewInAr as ModelIcon,
  Settings as SettingsIcon,
  Person as PersonIcon,
  Security as SecurityIcon,
  Extension as ExtensionIcon,
  Dashboard as DashboardIcon,
  Gavel as ModerationIcon,
  AdminPanelSettings as RoleIcon,
  Storage as StorageIcon,
  Notifications as NotificationIcon,
  Analytics as AnalyticsIcon,
  Add as AddIcon,
  Visibility,
  VisibilityOff,
  ContentCopy as CopyIcon
} from '@mui/icons-material';
import RoleManagement from './RoleManagement';

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

// Role Management Panel
const RoleManagementPanel: React.FC = () => {
  return <RoleManagement />;
};

// User Management Panel
const UserManagementPanel: React.FC = () => {
  const [userTab, setUserTab] = useState(0);
  const [selectedUser, setSelectedUser] = useState<string | null>(null);
  const [searchQuery, setSearchQuery] = useState('');
  const [createUserOpen, setCreateUserOpen] = useState(false);
  const [isCreatingUser, setIsCreatingUser] = useState(false);
  const [snackbar, setSnackbar] = useState({ open: false, message: '', severity: 'success' as 'success' | 'error' });
  const [showPassword, setShowPassword] = useState(false);
  const [newUserForm, setNewUserForm] = useState({
    email: '',
    username: '',
    firstName: '',
    lastName: '',
    country: '',
    role: 'User' as 'User' | 'Moderator' | 'Admin'
  });
  const [createdUserPassword, setCreatedUserPassword] = useState('');

  const allUsers = [
    { email: 'admin@example.com', username: 'admin', role: 'Admin', status: 'Online', lastActive: 'Online', isModerator: true },
    { email: 'mod1@example.com', username: 'moderator1', role: 'Moderator', status: 'Active', lastActive: '2 hours ago', isModerator: true },
    { email: 'mod2@example.com', username: 'moderator2', role: 'Moderator', status: 'Active', lastActive: '1 day ago', isModerator: true },
    { email: 'user1@example.com', username: 'user1', role: 'User', status: 'Active', lastActive: '1 day ago', isModerator: false },
    { email: 'user2@example.com', username: 'user2', role: 'User', status: 'Inactive', lastActive: '3 days ago', isModerator: false },
    { email: 'user3@example.com', username: 'user3', role: 'User', status: 'Active', lastActive: '5 hours ago', isModerator: false },
  ];

  const moderators = allUsers.filter(user => user.isModerator);
  const displayUsers = userTab === 0 ? allUsers : moderators;
  const filteredUsers = displayUsers.filter(user => 
    user.email.toLowerCase().includes(searchQuery.toLowerCase()) ||
    user.username.toLowerCase().includes(searchQuery.toLowerCase())
  );

  const handleUserClick = (userEmail: string) => {
    setSelectedUser(selectedUser === userEmail ? null : userEmail);
  };

  const handleUserAction = (action: string, userEmail: string) => {
    console.log(`Action: ${action} for user: ${userEmail}`);
    // Backend integration will go here
  };

  const handleCreateUser = async () => {
    setIsCreatingUser(true);
    try {
      const userService = await import('../../services/userService');
      const response = await userService.default.createUser({
        email: newUserForm.email,
        username: newUserForm.username,
        role: newUserForm.role,
        firstName: newUserForm.firstName || undefined,
        lastName: newUserForm.lastName || undefined,
        country: newUserForm.country || undefined,
      });

      setCreatedUserPassword(response.generatedPassword);
      setSnackbar({
        open: true,
        message: `User ${response.username} created successfully!`,
        severity: 'success'
      });

      // Reset form
      setNewUserForm({
        email: '',
        username: '',
        firstName: '',
        lastName: '',
        country: '',
        role: 'User'
      });
    } catch (error: any) {
      console.error('User creation failed:', error);
      setSnackbar({
        open: true,
        message: error.message || 'Failed to create user. Please try again.',
        severity: 'error'
      });
    } finally {
      setIsCreatingUser(false);
    }
  };

  const handleFormChange = (field: string, value: string) => {
    setNewUserForm(prev => ({ ...prev, [field]: value }));
  };

  const handleCloseCreateDialog = () => {
    setCreateUserOpen(false);
    setCreatedUserPassword('');
    setShowPassword(false);
  };

  const copyPasswordToClipboard = () => {
    navigator.clipboard.writeText(createdUserPassword);
    setSnackbar({
      open: true,
      message: 'Password copied to clipboard',
      severity: 'success'
    });
  };

  return (
    <Grid container spacing={3}>
      <Grid item xs={12} md={8}>
        <Card>
          <CardContent>
            <Typography variant="h6" gutterBottom>User Management</Typography>
            
            {/* User Management Tabs */}
            <Box sx={{ borderBottom: 1, borderColor: 'divider', mb: 2 }}>
              <Tabs value={userTab} onChange={(e, newValue) => setUserTab(newValue)}>
                <Tab label={`All Users (${allUsers.length})`} />
                <Tab label={`Moderators (${moderators.length})`} />
              </Tabs>
            </Box>

            <Box sx={{ mb: 2 }}>
              <TextField 
                fullWidth 
                placeholder="Search users..." 
                variant="outlined" 
                size="small"
                value={searchQuery}
                onChange={(e) => setSearchQuery(e.target.value)}
                sx={{ mb: 2 }}
              />
            </Box>

            <List>
              {filteredUsers.map((user, index) => (
                <Box key={user.email}>
                  <ListItem 
                    button
                    onClick={() => handleUserClick(user.email)}
                    divider
                    sx={{ 
                      backgroundColor: selectedUser === user.email ? 'action.selected' : 'transparent',
                      '&:hover': { backgroundColor: 'action.hover' }
                    }}
                  >
                    <ListItemIcon>
                      <PersonIcon />
                    </ListItemIcon>
                    <ListItemText 
                      primary={
                        <Box sx={{ display: 'flex', alignItems: 'center', gap: 1 }}>
                          <Typography variant="body1">{user.username}</Typography>
                          <Typography variant="body2" color="text.secondary">({user.email})</Typography>
                        </Box>
                      }
                      secondary={`Last active: ${user.lastActive} • Status: ${user.status}`}
                    />
                    <Chip 
                      label={user.role} 
                      color={user.role === 'Admin' ? 'primary' : user.role === 'Moderator' ? 'secondary' : 'default'} 
                      size="small" 
                    />
                  </ListItem>
                  
                  {/* User Actions Panel */}
                  {selectedUser === user.email && (
                    <Box sx={{ p: 2, backgroundColor: 'grey.50', borderRadius: 1, mb: 1 }}>
                      <Typography variant="subtitle2" gutterBottom>Quick Actions for {user.username}</Typography>
                      <Box sx={{ display: 'flex', gap: 1, flexWrap: 'wrap' }}>
                        <Button 
                          size="small" 
                          variant="outlined"
                          onClick={() => handleUserAction('edit', user.email)}
                        >
                          Edit Profile
                        </Button>
                        <Button 
                          size="small" 
                          variant="outlined"
                          onClick={() => handleUserAction('ban', user.email)}
                          color="error"
                        >
                          {user.status === 'Active' ? 'Ban User' : 'Unban User'}
                        </Button>
                        <Button 
                          size="small" 
                          variant="outlined"
                          onClick={() => handleUserAction('role', user.email)}
                        >
                          Change Role
                        </Button>
                        {!user.isModerator && (
                          <Button 
                            size="small" 
                            variant="outlined"
                            onClick={() => handleUserAction('promote', user.email)}
                            color="success"
                          >
                            Make Moderator
                          </Button>
                        )}
                        {user.isModerator && user.role !== 'Admin' && (
                          <Button 
                            size="small" 
                            variant="outlined"
                            onClick={() => handleUserAction('demote', user.email)}
                            color="warning"
                          >
                            Remove Moderator
                          </Button>
                        )}
                        <Button 
                          size="small" 
                          variant="outlined"
                          onClick={() => handleUserAction('reset-password', user.email)}
                        >
                          Reset Password
                        </Button>
                        <Button 
                          size="small" 
                          variant="outlined"
                          onClick={() => handleUserAction('view-activity', user.email)}
                        >
                          View Activity
                        </Button>
                      </Box>
                    </Box>
                  )}
                </Box>
              ))}
            </List>
            
            {filteredUsers.length === 0 && (
              <Box sx={{ textAlign: 'center', py: 4 }}>
                <Typography variant="body2" color="text.secondary">
                  No users found matching your search.
                </Typography>
              </Box>
            )}
          </CardContent>
        </Card>
      </Grid>
      <Grid item xs={12} md={4}>
        <Card>
          <CardContent>
            <Typography variant="h6" gutterBottom>
              {userTab === 0 ? 'User Statistics' : 'Moderation Statistics'}
            </Typography>
            {userTab === 0 ? (
              <>
                <Typography variant="body2" color="text.secondary">Total Users: {allUsers.length}</Typography>
                <Typography variant="body2" color="text.secondary">Active Today: {allUsers.filter(u => u.status === 'Active').length}</Typography>
                <Typography variant="body2" color="text.secondary">Moderators: {moderators.length}</Typography>
                <Typography variant="body2" color="text.secondary">Banned Users: 0</Typography>
              </>
            ) : (
              <>
                <Typography variant="body2" color="text.secondary">Total Moderators: {moderators.length}</Typography>
                <Typography variant="body2" color="text.secondary">Active Moderators: {moderators.filter(m => m.status === 'Active').length}</Typography>
                <Typography variant="body2" color="text.secondary">Online Now: {moderators.filter(m => m.lastActive === 'Online').length}</Typography>
                <Button variant="contained" fullWidth sx={{ mt: 2 }}>
                  Add New Moderator
                </Button>
              </>
            )}
            <Button 
              variant="contained" 
              fullWidth 
              startIcon={<AddIcon />}
              sx={{ mt: 2 }}
              onClick={() => setCreateUserOpen(true)}
            >
              Create New User
            </Button>
            <Button variant="outlined" fullWidth sx={{ mt: 1 }}>
              {userTab === 0 ? 'Export User Data' : 'Export Moderator Report'}
            </Button>
          </CardContent>
        </Card>
        
        {userTab === 1 && (
          <Card sx={{ mt: 2 }}>
            <CardContent>
              <Typography variant="h6" gutterBottom>Moderator Permissions</Typography>
              <List dense>
                <ListItem>
                  <ListItemText 
                    primary="Model Approval" 
                    secondary="Can approve/reject model uploads"
                  />
                </ListItem>
                <ListItem>
                  <ListItemText 
                    primary="User Moderation" 
                    secondary="Can ban/warn users"
                  />
                </ListItem>
                <ListItem>
                  <ListItemText 
                    primary="Comment Management" 
                    secondary="Can delete inappropriate comments"
                  />
                </ListItem>
              </List>
              <Button variant="outlined" fullWidth sx={{ mt: 1 }}>
                Manage Permissions
              </Button>
            </CardContent>
          </Card>
        )}
      </Grid>

      {/* Create User Dialog */}
      <Dialog 
        open={createUserOpen} 
        onClose={handleCloseCreateDialog}
        maxWidth="sm"
        fullWidth
      >
        <DialogTitle>
          Create New User
        </DialogTitle>
        <DialogContent>
          {!createdUserPassword ? (
            <Box sx={{ pt: 1 }}>
              <TextField
                autoFocus
                margin="dense"
                label="Email Address"
                type="email"
                fullWidth
                variant="outlined"
                value={newUserForm.email}
                onChange={(e) => handleFormChange('email', e.target.value)}
                required
                sx={{ mb: 2 }}
              />
              
              <TextField
                margin="dense"
                label="Username"
                fullWidth
                variant="outlined"
                value={newUserForm.username}
                onChange={(e) => handleFormChange('username', e.target.value)}
                required
                sx={{ mb: 2 }}
              />

              <FormControl fullWidth sx={{ mb: 2 }}>
                <InputLabel>Role</InputLabel>
                <Select
                  value={newUserForm.role}
                  label="Role"
                  onChange={(e) => handleFormChange('role', e.target.value)}
                >
                  <MenuItem value="User">User</MenuItem>
                  <MenuItem value="Moderator">Moderator</MenuItem>
                  <MenuItem value="Admin">Admin</MenuItem>
                </Select>
              </FormControl>

              <TextField
                margin="dense"
                label="First Name (Optional)"
                fullWidth
                variant="outlined"
                value={newUserForm.firstName}
                onChange={(e) => handleFormChange('firstName', e.target.value)}
                sx={{ mb: 2 }}
              />
              
              <TextField
                margin="dense"
                label="Last Name (Optional)"
                fullWidth
                variant="outlined"
                value={newUserForm.lastName}
                onChange={(e) => handleFormChange('lastName', e.target.value)}
                sx={{ mb: 2 }}
              />

              <TextField
                margin="dense"
                label="Country (Optional)"
                fullWidth
                variant="outlined"
                value={newUserForm.country}
                onChange={(e) => handleFormChange('country', e.target.value)}
              />

              <Alert severity="info" sx={{ mt: 2 }}>
                A secure password will be automatically generated for this user. 
                The password will be displayed after creation and sent to the user's email.
              </Alert>
            </Box>
          ) : (
            <Box sx={{ pt: 1, textAlign: 'center' }}>
              <Alert severity="success" sx={{ mb: 3 }}>
                User created successfully!
              </Alert>
              
              <Typography variant="h6" gutterBottom>
                Generated Password
              </Typography>
              
              <TextField
                fullWidth
                value={createdUserPassword}
                type={showPassword ? 'text' : 'password'}
                variant="outlined"
                InputProps={{
                  readOnly: true,
                  endAdornment: (
                    <InputAdornment position="end">
                      <IconButton
                        onClick={() => setShowPassword(!showPassword)}
                        edge="end"
                      >
                        {showPassword ? <VisibilityOff /> : <Visibility />}
                      </IconButton>
                      <IconButton
                        onClick={copyPasswordToClipboard}
                        edge="end"
                      >
                        <CopyIcon />
                      </IconButton>
                    </InputAdornment>
                  ),
                }}
                sx={{ mb: 2 }}
              />
              
              <Alert severity="warning">
                <strong>Important:</strong> Save this password securely. 
                The user should change it after their first login.
                This password will not be shown again.
              </Alert>
            </Box>
          )}
        </DialogContent>
        <DialogActions>
          {!createdUserPassword ? (
            <>
              <Button onClick={handleCloseCreateDialog}>Cancel</Button>
              <Button 
                onClick={handleCreateUser}
                variant="contained"
                disabled={isCreatingUser || !newUserForm.email || !newUserForm.username}
              >
                {isCreatingUser ? <CircularProgress size={20} /> : 'Create User'}
              </Button>
            </>
          ) : (
            <Button onClick={handleCloseCreateDialog} variant="contained">
              Done
            </Button>
          )}
        </DialogActions>
      </Dialog>

      {/* Snackbar for notifications */}
      <Snackbar
        open={snackbar.open}
        autoHideDuration={6000}
        onClose={() => setSnackbar(prev => ({ ...prev, open: false }))}
      >
        <Alert 
          onClose={() => setSnackbar(prev => ({ ...prev, open: false }))} 
          severity={snackbar.severity}
        >
          {snackbar.message}
        </Alert>
      </Snackbar>
    </Grid>
  );
};

// Model Management Panel
const ModelManagementPanel: React.FC = () => (
  <Grid container spacing={3}>
    <Grid item xs={12} md={8}>
      <Card>
        <CardContent>
          <Typography variant="h6" gutterBottom>Model Management</Typography>
          <Box sx={{ mb: 2 }}>
            <TextField 
              fullWidth 
              placeholder="Search models..." 
              variant="outlined" 
              size="small"
              sx={{ mb: 2 }}
            />
          </Box>
          <List>
            {['Dragon Figurine', 'Miniature House', 'Robot Model'].map((model, index) => (
              <ListItem key={index} divider>
                <ListItemIcon>
                  <ModelIcon />
                </ListItemIcon>
                <ListItemText 
                  primary={model} 
                  secondary={`Uploaded by user${index + 1} • ${index + 1} downloads`}
                />
                <Chip 
                  label={index === 0 ? 'Featured' : 'Public'} 
                  color={index === 0 ? 'primary' : 'default'} 
                  size="small" 
                />
                <Button size="small" sx={{ ml: 1 }}>Edit</Button>
              </ListItem>
            ))}
          </List>
        </CardContent>
      </Card>
    </Grid>
    <Grid item xs={12} md={4}>
      <Card>
        <CardContent>
          <Typography variant="h6" gutterBottom>Model Statistics</Typography>
          <Typography variant="body2" color="text.secondary">Total Models: 5,678</Typography>
          <Typography variant="body2" color="text.secondary">Public: 4,891</Typography>
          <Typography variant="body2" color="text.secondary">Pending Review: 23</Typography>
          <Button variant="outlined" fullWidth sx={{ mt: 2 }}>Bulk Actions</Button>
        </CardContent>
      </Card>
    </Grid>
  </Grid>
);

// System Settings Panel
const SystemSettingsPanel: React.FC = () => {
  const [emailSettings, setEmailSettings] = useState({
    enabled: false,
    smtpServer: '',
    smtpPort: 587,
    smtpUsername: '',
    smtpPassword: '',
    hasPassword: false,
    useSsl: true,
    fromAddress: '',
    fromName: '',
    requireEmailVerification: true,
    isConfigured: false
  });
  const [emailLoading, setEmailLoading] = useState(false);
  const [emailTestLoading, setEmailTestLoading] = useState(false);
  const [testEmail, setTestEmail] = useState('');
  const [emailMessage, setEmailMessage] = useState('');
  const [showPassword, setShowPassword] = useState(false);

  useEffect(() => {
    fetchEmailSettings();
  }, []);

  const fetchEmailSettings = async () => {
    try {
      setEmailLoading(true);
      const response = await fetch('/api/system-settings/email', {
        headers: {
          'Authorization': `Bearer ${localStorage.getItem('token')}`
        }
      });
      if (response.ok) {
        const data = await response.json();
        setEmailSettings(data);
      }
    } catch (error) {
      console.error('Failed to fetch email settings:', error);
    } finally {
      setEmailLoading(false);
    }
  };

  const handleEmailSettingsChange = (field: string, value: any) => {
    setEmailSettings(prev => ({ ...prev, [field]: value }));
  };

  const saveEmailSettings = async () => {
    try {
      setEmailLoading(true);
      setEmailMessage('');
      
      const response = await fetch('/api/system-settings/email', {
        method: 'PUT',
        headers: {
          'Content-Type': 'application/json',
          'Authorization': `Bearer ${localStorage.getItem('token')}`
        },
        body: JSON.stringify({
          enabled: emailSettings.enabled,
          smtpServer: emailSettings.smtpServer,
          smtpPort: emailSettings.smtpPort,
          smtpUsername: emailSettings.smtpUsername,
          smtpPassword: emailSettings.smtpPassword,
          useSsl: emailSettings.useSsl,
          fromAddress: emailSettings.fromAddress,
          fromName: emailSettings.fromName,
          requireEmailVerification: emailSettings.requireEmailVerification
        })
      });

      if (response.ok) {
        setEmailMessage('Email settings saved successfully!');
        await fetchEmailSettings(); // Refresh settings
      } else {
        const error = await response.json();
        setEmailMessage(error.message || 'Failed to save email settings');
      }
    } catch (error) {
      setEmailMessage('Failed to save email settings');
      console.error('Failed to save email settings:', error);
    } finally {
      setEmailLoading(false);
    }
  };

  const testEmailConfiguration = async () => {
    if (!testEmail) {
      setEmailMessage('Please enter a test email address');
      return;
    }

    try {
      setEmailTestLoading(true);
      setEmailMessage('');
      
      const response = await fetch('/api/system-settings/email/test', {
        method: 'POST',
        headers: {
          'Content-Type': 'application/json',
          'Authorization': `Bearer ${localStorage.getItem('token')}`
        },
        body: JSON.stringify({
          testEmailAddress: testEmail
        })
      });

      const result = await response.json();
      setEmailMessage(result.message);
    } catch (error) {
      setEmailMessage('Failed to send test email');
      console.error('Failed to test email configuration:', error);
    } finally {
      setEmailTestLoading(false);
    }
  };

  return (
    <Grid container spacing={3}>
      <Grid item xs={12} md={6}>
        <Card>
          <CardContent>
            <Typography variant="h6" gutterBottom>General Settings</Typography>
            <Box sx={{ display: 'flex', flexDirection: 'column', gap: 2 }}>
              <TextField label="Site Name" defaultValue="PolyBucket" fullWidth />
              <TextField label="Site Description" defaultValue="3D Model Repository" fullWidth />
              <TextField label="Contact Email" defaultValue="admin@polybucket.com" fullWidth />
              <FormControlLabel control={<Switch defaultChecked />} label="Enable User Registration" />
              <FormControlLabel control={<Switch defaultChecked />} label="Public Model Browse" />
            </Box>
          </CardContent>
        </Card>
      </Grid>
      <Grid item xs={12} md={6}>
        <Card>
          <CardContent>
            <Typography variant="h6" gutterBottom>File Upload Settings</Typography>
            <Box sx={{ display: 'flex', flexDirection: 'column', gap: 2 }}>
              <TextField label="Max File Size (MB)" defaultValue="100" type="number" />
              <TextField label="Allowed File Types" defaultValue=".stl,.obj,.3mf" />
              <TextField label="Max Files per Upload" defaultValue="5" type="number" />
              <FormControlLabel control={<Switch defaultChecked />} label="Enable File Compression" />
              <FormControlLabel control={<Switch />} label="Auto-generate Previews" />
            </Box>
          </CardContent>
        </Card>
      </Grid>
      <Grid item xs={12}>
        <Card>
          <CardContent>
            <Box sx={{ display: 'flex', alignItems: 'center', gap: 2, mb: 2 }}>
              <Typography variant="h6">Email Service Configuration</Typography>
              <Chip 
                label={emailSettings.isConfigured ? 'Configured' : 'Not Configured'} 
                color={emailSettings.isConfigured ? 'success' : 'warning'}
                size="small"
              />
            </Box>
            
            {emailLoading ? (
              <Box sx={{ display: 'flex', justifyContent: 'center', p: 3 }}>
                <CircularProgress />
              </Box>
            ) : (
              <Grid container spacing={3}>
                <Grid item xs={12} md={6}>
                  <Box sx={{ display: 'flex', flexDirection: 'column', gap: 2 }}>
                    <FormControlLabel
                      control={
                        <Switch
                          checked={emailSettings.enabled}
                          onChange={(e) => handleEmailSettingsChange('enabled', e.target.checked)}
                        />
                      }
                      label="Enable Email Service"
                    />
                    
                    <TextField
                      label="SMTP Server"
                      value={emailSettings.smtpServer}
                      onChange={(e) => handleEmailSettingsChange('smtpServer', e.target.value)}
                      disabled={!emailSettings.enabled}
                      fullWidth
                      required={emailSettings.enabled}
                    />
                    
                    <TextField
                      label="SMTP Port"
                      type="number"
                      value={emailSettings.smtpPort}
                      onChange={(e) => handleEmailSettingsChange('smtpPort', parseInt(e.target.value))}
                      disabled={!emailSettings.enabled}
                      fullWidth
                      required={emailSettings.enabled}
                    />
                    
                    <TextField
                      label="SMTP Username"
                      value={emailSettings.smtpUsername}
                      onChange={(e) => handleEmailSettingsChange('smtpUsername', e.target.value)}
                      disabled={!emailSettings.enabled}
                      fullWidth
                    />
                    
                    <TextField
                      label={emailSettings.hasPassword ? "SMTP Password (configured)" : "SMTP Password"}
                      type={showPassword ? "text" : "password"}
                      value={emailSettings.smtpPassword}
                      onChange={(e) => handleEmailSettingsChange('smtpPassword', e.target.value)}
                      disabled={!emailSettings.enabled}
                      fullWidth
                      placeholder={emailSettings.hasPassword ? "Leave empty to keep existing password" : ""}
                      InputProps={{
                        endAdornment: (
                          <InputAdornment position="end">
                            <IconButton
                              onClick={() => setShowPassword(!showPassword)}
                              edge="end"
                            >
                              {showPassword ? <VisibilityOff /> : <Visibility />}
                            </IconButton>
                          </InputAdornment>
                        )
                      }}
                    />
                  </Box>
                </Grid>
                
                <Grid item xs={12} md={6}>
                  <Box sx={{ display: 'flex', flexDirection: 'column', gap: 2 }}>
                    <FormControlLabel
                      control={
                        <Switch
                          checked={emailSettings.useSsl}
                          onChange={(e) => handleEmailSettingsChange('useSsl', e.target.checked)}
                          disabled={!emailSettings.enabled}
                        />
                      }
                      label="Use SSL/TLS"
                    />
                    
                    <TextField
                      label="From Email Address"
                      type="email"
                      value={emailSettings.fromAddress}
                      onChange={(e) => handleEmailSettingsChange('fromAddress', e.target.value)}
                      disabled={!emailSettings.enabled}
                      fullWidth
                      required={emailSettings.enabled}
                    />
                    
                    <TextField
                      label="From Name"
                      value={emailSettings.fromName}
                      onChange={(e) => handleEmailSettingsChange('fromName', e.target.value)}
                      disabled={!emailSettings.enabled}
                      fullWidth
                      required={emailSettings.enabled}
                    />
                    
                    <FormControlLabel
                      control={
                        <Switch
                          checked={emailSettings.requireEmailVerification}
                          onChange={(e) => handleEmailSettingsChange('requireEmailVerification', e.target.checked)}
                          disabled={!emailSettings.enabled}
                        />
                      }
                      label="Require Email Verification for New Users"
                    />
                    
                    <Divider sx={{ my: 1 }} />
                    
                    <Typography variant="subtitle2" gutterBottom>Test Email Configuration</Typography>
                    <TextField
                      label="Test Email Address"
                      type="email"
                      value={testEmail}
                      onChange={(e) => setTestEmail(e.target.value)}
                      disabled={!emailSettings.enabled || emailTestLoading}
                      fullWidth
                      placeholder="Enter email to test configuration"
                    />
                    
                    <Button
                      variant="outlined"
                      onClick={testEmailConfiguration}
                      disabled={!emailSettings.enabled || emailTestLoading || !testEmail}
                      startIcon={emailTestLoading ? <CircularProgress size={20} /> : <NotificationIcon />}
                    >
                      {emailTestLoading ? 'Sending...' : 'Send Test Email'}
                    </Button>
                  </Box>
                </Grid>
                
                <Grid item xs={12}>
                  <Box sx={{ display: 'flex', gap: 2, alignItems: 'center' }}>
                    <Button
                      variant="contained"
                      onClick={saveEmailSettings}
                      disabled={emailLoading}
                      startIcon={emailLoading ? <CircularProgress size={20} /> : undefined}
                    >
                      {emailLoading ? 'Saving...' : 'Save Email Settings'}
                    </Button>
                    
                    {emailMessage && (
                      <Alert 
                        severity={emailMessage.includes('success') ? 'success' : 'error'}
                        sx={{ flex: 1 }}
                      >
                        {emailMessage}
                      </Alert>
                    )}
                  </Box>
                </Grid>
              </Grid>
            )}
          </CardContent>
        </Card>
      </Grid>
    </Grid>
  );
};

// Authentication Settings Panel
const AuthenticationSettingsPanel: React.FC = () => (
  <Grid container spacing={3}>
    <Grid item xs={12} md={6}>
      <Card>
        <CardContent>
          <Typography variant="h6" gutterBottom>Authentication Settings</Typography>
          <Box sx={{ display: 'flex', flexDirection: 'column', gap: 2 }}>
            <FormControlLabel control={<Switch defaultChecked />} label="Require Email Verification" />
            <FormControlLabel control={<Switch />} label="Enable Two-Factor Authentication" />
            <FormControlLabel control={<Switch defaultChecked />} label="Remember Login" />
            <TextField label="Session Timeout (minutes)" defaultValue="60" type="number" />
            <TextField label="Password Min Length" defaultValue="8" type="number" />
          </Box>
        </CardContent>
      </Card>
    </Grid>
    <Grid item xs={12} md={6}>
      <Card>
        <CardContent>
          <Typography variant="h6" gutterBottom>OAuth Providers</Typography>
          <Box sx={{ display: 'flex', flexDirection: 'column', gap: 2 }}>
            <FormControlLabel control={<Switch />} label="Enable Google OAuth" />
            <FormControlLabel control={<Switch />} label="Enable GitHub OAuth" />
            <FormControlLabel control={<Switch />} label="Enable Discord OAuth" />
            <Button variant="outlined" fullWidth>Configure OAuth Settings</Button>
          </Box>
        </CardContent>
      </Card>
    </Grid>
  </Grid>
);

// Plugins Panel
const PluginsPanel: React.FC = () => (
  <Grid container spacing={3}>
    <Grid item xs={12} md={8}>
      <Card>
        <CardContent>
          <Typography variant="h6" gutterBottom>Installed Plugins</Typography>
          <List>
            {[
              { name: 'Model Viewer', status: 'Active', version: '1.2.3' },
              { name: 'Auto Moderation', status: 'Inactive', version: '2.1.0' },
              { name: 'Analytics Dashboard', status: 'Active', version: '1.0.5' }
            ].map((plugin, index) => (
              <ListItem key={index} divider>
                <ListItemIcon>
                  <ExtensionIcon />
                </ListItemIcon>
                <ListItemText 
                  primary={plugin.name} 
                  secondary={`Version ${plugin.version}`}
                />
                <Chip 
                  label={plugin.status} 
                  color={plugin.status === 'Active' ? 'success' : 'default'} 
                  size="small" 
                />
                <Button size="small" sx={{ ml: 1 }}>
                  {plugin.status === 'Active' ? 'Deactivate' : 'Activate'}
                </Button>
              </ListItem>
            ))}
          </List>
        </CardContent>
      </Card>
    </Grid>
    <Grid item xs={12} md={4}>
      <Card>
        <CardContent>
          <Typography variant="h6" gutterBottom>Plugin Store</Typography>
          <Typography variant="body2" color="text.secondary" sx={{ mb: 2 }}>
            Discover and install new plugins
          </Typography>
          <Button variant="contained" fullWidth sx={{ mb: 1 }}>Browse Plugins</Button>
          <Button variant="outlined" fullWidth>Upload Plugin</Button>
        </CardContent>
      </Card>
    </Grid>
  </Grid>
);

// Analytics Panel
const AnalyticsPanel: React.FC = () => (
  <Grid container spacing={3}>
    <Grid item xs={12} md={3}>
      <Card>
        <CardContent>
          <Typography variant="h6" color="primary">1,234</Typography>
          <Typography variant="body2">Total Users</Typography>
        </CardContent>
      </Card>
    </Grid>
    <Grid item xs={12} md={3}>
      <Card>
        <CardContent>
          <Typography variant="h6" color="primary">5,678</Typography>
          <Typography variant="body2">Total Models</Typography>
        </CardContent>
      </Card>
    </Grid>
    <Grid item xs={12} md={3}>
      <Card>
        <CardContent>
          <Typography variant="h6" color="primary">12,345</Typography>
          <Typography variant="body2">Downloads</Typography>
        </CardContent>
      </Card>
    </Grid>
    <Grid item xs={12} md={3}>
      <Card>
        <CardContent>
          <Typography variant="h6" color="primary">89%</Typography>
          <Typography variant="body2">Uptime</Typography>
        </CardContent>
      </Card>
    </Grid>
    <Grid item xs={12}>
      <Card>
        <CardContent>
          <Typography variant="h6" gutterBottom>Usage Analytics</Typography>
          <Alert severity="info">
            Analytics dashboard will be implemented here. This will include charts for user activity, 
            model uploads over time, popular models, and system performance metrics.
          </Alert>
        </CardContent>
      </Card>
    </Grid>
  </Grid>
);

const ComprehensiveAdminPanel: React.FC = () => {
  const [tabValue, setTabValue] = useState(0);

  const handleTabChange = (event: React.SyntheticEvent, newValue: number) => {
    setTabValue(newValue);
  };

  const tabs = [
    { label: 'Dashboard', icon: <DashboardIcon />, component: <AnalyticsPanel /> },
    { label: 'Users', icon: <PeopleIcon />, component: <UserManagementPanel /> },
    { label: 'Roles', icon: <RoleIcon />, component: <RoleManagementPanel /> },
    { label: 'Models', icon: <ModelIcon />, component: <ModelManagementPanel /> },
    { label: 'System', icon: <SettingsIcon />, component: <SystemSettingsPanel /> },
    { label: 'Auth', icon: <SecurityIcon />, component: <AuthenticationSettingsPanel /> },
    { label: 'Plugins', icon: <ExtensionIcon />, component: <PluginsPanel /> },
  ];

  return (
    <Container maxWidth="xl" sx={{ mt: 4, mb: 4 }}>
      <Paper elevation={3} sx={{ p: 3 }}>
        <Box sx={{ display: 'flex', justifyContent: 'space-between', alignItems: 'center', mb: 3 }}>
          <Typography variant="h4" component="h1">
            Admin Control Panel
          </Typography>
          <Box sx={{ display: 'flex', gap: 1 }}>
            <Badge badgeContent={3} color="warning">
              <NotificationIcon />
            </Badge>
            <Chip label="Development Mode" color="warning" size="small" />
          </Box>
        </Box>
        
        <Alert severity="info" sx={{ mb: 3 }}>
          This is a comprehensive admin panel shell. Each section will be expanded with backend integration 
          and real functionality as development progresses.
        </Alert>
        
        <Box sx={{ borderBottom: 1, borderColor: 'divider', mb: 2 }}>
          <Tabs 
            value={tabValue} 
            onChange={handleTabChange} 
            aria-label="admin panel tabs"
            variant="scrollable"
            scrollButtons="auto"
          >
            {tabs.map((tab, index) => (
              <Tab 
                key={index}
                icon={tab.icon} 
                label={tab.label}
                iconPosition="start"
              />
            ))}
          </Tabs>
        </Box>
        
        {tabs.map((tab, index) => (
          <TabPanel key={index} value={tabValue} index={index}>
            {tab.component}
          </TabPanel>
        ))}
      </Paper>
    </Container>
  );
};

export default ComprehensiveAdminPanel; 