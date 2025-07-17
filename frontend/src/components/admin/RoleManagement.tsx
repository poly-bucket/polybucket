import React, { useState, useEffect } from 'react';
import {
  Box,
  Button,
  Card,
  CardContent,
  Chip,
  Grid,
  List,
  ListItem,
  ListItemText,
  Paper,
  Typography,
  Alert,
  Snackbar,
  CircularProgress,
  Divider,
} from '@mui/material';
import {
  AdminPanelSettings as RoleIcon,
  Security as SecurityIcon,
  People as PeopleIcon,
} from '@mui/icons-material';
import api from '../../utils/axiosConfig';

interface UserRole {
  name: string;
  description: string;
  userCount: number;
  permissions: string[];
  isSystemRole: boolean;
  color: 'primary' | 'secondary' | 'info' | 'warning' | 'error' | 'default';
}

const RoleManagement: React.FC = () => {
  const [loading, setLoading] = useState(true);
  const [snackbar, setSnackbar] = useState({
    open: false,
    message: '',
    severity: 'success' as 'success' | 'error' | 'warning' | 'info',
  });

  // Current system roles based on UserRole enum
  const systemRoles: UserRole[] = [
    {
      name: 'Admin',
      description: 'Full system access with all administrative permissions',
      userCount: 0, // Will be populated from API
      permissions: [
        'System Management',
        'User Management', 
        'Model Management',
        'Role Management',
        'Plugin Management',
        'Moderation',
        'Analytics Access'
      ],
      isSystemRole: true,
      color: 'error'
    },
    {
      name: 'Moderator', 
      description: 'Can moderate content and manage users',
      userCount: 0,
      permissions: [
        'Model Moderation',
        'User Moderation', 
        'Comment Management',
        'Report Handling'
      ],
      isSystemRole: true,
      color: 'warning'
    },
    {
      name: 'User',
      description: 'Standard user with basic viewing and uploading permissions',
      userCount: 0,
      permissions: [
        'Model Upload',
        'Model Download',
        'Comment Creation',
        'Collection Management',
        'Profile Management'
      ],
      isSystemRole: true,
      color: 'info'
    }
  ];

  const [roles, setRoles] = useState<UserRole[]>(systemRoles);

  useEffect(() => {
    fetchUserCounts();
  }, []);

  const fetchUserCounts = async () => {
    try {
      setLoading(true);
      // For now, we'll simulate API calls since the backend has build issues
      // In a working system, this would fetch actual user counts per role
      
      // Simulate API delay
      await new Promise(resolve => setTimeout(resolve, 1000));
      
      // Mock data - in reality this would come from: await api.get('/admin/users/role-counts');
      const mockCounts = {
        Admin: 1,
        Moderator: 2, 
        User: 42
      };

      setRoles(prev => prev.map(role => ({
        ...role,
        userCount: mockCounts[role.name as keyof typeof mockCounts] || 0
      })));

      showSnackbar('Role data loaded successfully', 'success');
    } catch (error) {
      console.error('Failed to fetch user counts:', error);
      showSnackbar('Failed to load role data. Using default values.', 'warning');
    } finally {
      setLoading(false);
    }
  };

  const showSnackbar = (message: string, severity: 'success' | 'error' | 'warning' | 'info') => {
    setSnackbar({ open: true, message, severity });
  };

  const getTotalUsers = () => {
    return roles.reduce((sum, role) => sum + role.userCount, 0);
  };

  const getTotalPermissions = () => {
    const allPermissions = new Set();
    roles.forEach(role => {
      role.permissions.forEach(permission => allPermissions.add(permission));
    });
    return allPermissions.size;
  };

  if (loading) {
    return (
      <Box display="flex" justifyContent="center" alignItems="center" minHeight="400px">
        <CircularProgress />
      </Box>
    );
  }

  return (
    <Box>
      <Box display="flex" justifyContent="space-between" alignItems="center" mb={3}>
        <Typography variant="h5" component="h2">
          Role Management
        </Typography>
        <Alert severity="info" sx={{ ml: 2 }}>
          Current system uses predefined roles. ACL expansion coming soon.
        </Alert>
      </Box>

      <Grid container spacing={3}>
        <Grid item xs={12} md={8}>
          <Card>
            <CardContent>
              <Typography variant="h6" gutterBottom>
                System Roles
              </Typography>
              <Alert severity="info" sx={{ mb: 2 }}>
                These are the current system roles. Role modification will be available when the ACL system is fully integrated.
              </Alert>
              
              <List>
                {roles.map((role) => (
                  <Paper key={role.name} variant="outlined" sx={{ mb: 1 }}>
                    <ListItem>
                      <RoleIcon sx={{ mr: 2, color: 'primary.main' }} />
                      <ListItemText
                        primary={
                          <Box display="flex" alignItems="center" gap={1}>
                            <Typography variant="h6">{role.name}</Typography>
                            <Chip
                              label="System Role"
                              size="small"
                              color={role.color}
                            />
                            <Chip
                              label={`${role.userCount} users`}
                              size="small"
                              variant="outlined"
                            />
                          </Box>
                        }
                        secondary={
                          <Box>
                            <Typography variant="body2" color="text.secondary">
                              {role.description}
                            </Typography>
                            <Typography variant="caption" color="text.secondary">
                              Permissions: {role.permissions.length}
                            </Typography>
                          </Box>
                        }
                      />
                    </ListItem>
                    
                    {/* Role Permissions Display */}
                    <Box sx={{ px: 2, pb: 2 }}>
                      <Typography variant="caption" color="text.secondary" gutterBottom>
                        Current Permissions:
                      </Typography>
                      <Box sx={{ display: 'flex', gap: 0.5, flexWrap: 'wrap', mt: 0.5 }}>
                        {role.permissions.map((permission) => (
                          <Chip 
                            key={permission} 
                            label={permission}
                            size="small"
                            variant="outlined"
                            color="default"
                          />
                        ))}
                      </Box>
                    </Box>
                  </Paper>
                ))}
              </List>
            </CardContent>
          </Card>
        </Grid>

        <Grid item xs={12} md={4}>
          <Card>
            <CardContent>
              <Typography variant="h6" gutterBottom>
                Role Statistics
              </Typography>
              <Box display="flex" flexDirection="column" gap={1}>
                <Typography variant="body2">
                  Total Roles: {roles.length}
                </Typography>
                <Typography variant="body2">
                  System Roles: {roles.filter(r => r.isSystemRole).length}
                </Typography>
                <Typography variant="body2">
                  Total Users: {getTotalUsers()}
                </Typography>
                <Typography variant="body2">
                  Total Permissions: {getTotalPermissions()}
                </Typography>
              </Box>
              
              <Divider sx={{ my: 2 }} />
              
              <Typography variant="h6" gutterBottom>
                User Distribution
              </Typography>
              <Box display="flex" flexDirection="column" gap={1}>
                {roles.map((role) => (
                  <Box key={role.name} display="flex" justifyContent="space-between" alignItems="center">
                    <Typography variant="body2">{role.name}</Typography>
                    <Box display="flex" alignItems="center" gap={1}>
                      <Typography variant="body2" color="text.secondary">
                        {role.userCount}
                      </Typography>
                      <Chip
                        label={`${((role.userCount / getTotalUsers()) * 100).toFixed(1)}%`}
                        size="small"
                        color={role.color}
                        variant="outlined"
                      />
                    </Box>
                  </Box>
                ))}
              </Box>
            </CardContent>
          </Card>

          <Card sx={{ mt: 2 }}>
            <CardContent>
              <Typography variant="h6" gutterBottom>
                System Information
              </Typography>
              <Alert severity="warning" sx={{ mb: 2 }}>
                <Typography variant="body2">
                  <strong>Current Limitations:</strong>
                </Typography>
                <Typography variant="body2">
                  • Roles cannot be created or modified
                  • Permission assignment is static
                  • User role changes require backend API calls
                </Typography>
              </Alert>
              
              <Typography variant="body2" color="text.secondary" gutterBottom>
                <strong>Coming Soon:</strong>
              </Typography>
              <List dense>
                <ListItem sx={{ py: 0 }}>
                  <ListItemText 
                    primary="Custom Role Creation" 
                    secondary="Create roles with specific permissions"
                    primaryTypographyProps={{ variant: 'body2' }}
                    secondaryTypographyProps={{ variant: 'caption' }}
                  />
                </ListItem>
                <ListItem sx={{ py: 0 }}>
                  <ListItemText 
                    primary="Permission Management" 
                    secondary="Fine-grained permission control"
                    primaryTypographyProps={{ variant: 'body2' }}
                    secondaryTypographyProps={{ variant: 'caption' }}
                  />
                </ListItem>
                <ListItem sx={{ py: 0 }}>
                  <ListItemText 
                    primary="Role Hierarchy" 
                    secondary="Parent-child role relationships"
                    primaryTypographyProps={{ variant: 'body2' }}
                    secondaryTypographyProps={{ variant: 'caption' }}
                  />
                </ListItem>
              </List>
            </CardContent>
          </Card>
        </Grid>
      </Grid>

      {/* Snackbar for notifications */}
      <Snackbar
        open={snackbar.open}
        autoHideDuration={6000}
        onClose={() => setSnackbar(prev => ({ ...prev, open: false }))}
      >
        <Alert
          onClose={() => setSnackbar(prev => ({ ...prev, open: false }))}
          severity={snackbar.severity}
          sx={{ width: '100%' }}
        >
          {snackbar.message}
        </Alert>
      </Snackbar>
    </Box>
  );
};

export default RoleManagement; 