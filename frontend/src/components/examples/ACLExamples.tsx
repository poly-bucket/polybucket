import React from 'react';
import {
  Box,
  Card,
  CardContent,
  Typography,
  Button,
  Alert,
  Divider,
  Chip,
  Grid
} from '@mui/material';
import { usePermissions, PERMISSIONS } from '../../hooks/usePermissions';
import PermissionGate, {
  AdminOnly,
  ModeratorOnly,
  RequirePermission,
  RequireAnyPermission,
  RequireMinimumRole
} from '../common/PermissionGate';

const ACLExamples: React.FC = () => {
  const { permissions, hasPermission, isAdmin, isModerator } = usePermissions();

  // Example model data
  const exampleModel = {
    id: '123e4567-e89b-12d3-a456-426614174000',
    name: 'Example Model',
    authorId: 'user-123',
    isPublic: true
  };

  const currentUserId = permissions?.role?.id || 'current-user-id';

  return (
    <Box sx={{ p: 3 }}>
      <Typography variant="h4" gutterBottom>
        ACL System Examples
      </Typography>
      
      <Typography variant="body1" paragraph>
        This page demonstrates how to use the Access Control List (ACL) system for fine-grained permission control.
      </Typography>

      <Grid container spacing={3}>
        {/* User Info */}
        <Grid item xs={12}>
          <Card>
            <CardContent>
              <Typography variant="h6" gutterBottom>
                Current User Information
              </Typography>
              {permissions ? (
                <Box>
                  <Typography><strong>Role:</strong> {permissions.role.name} (Priority: {permissions.role.priority})</Typography>
                  <Typography><strong>Is Admin:</strong> {isAdmin() ? 'Yes' : 'No'}</Typography>
                  <Typography><strong>Is Moderator:</strong> {isModerator() ? 'Yes' : 'No'}</Typography>
                  <Typography><strong>Total Permissions:</strong> {permissions.effectivePermissions?.length || 0}</Typography>
                  
                  <Box sx={{ mt: 2 }}>
                    <Typography variant="subtitle2">Sample Permissions:</Typography>
                    {permissions.effectivePermissions?.slice(0, 5).map(permission => (
                      <Chip key={permission} label={permission} size="small" sx={{ mr: 1, mt: 1 }} />
                    ))}
                    {permissions.effectivePermissions && permissions.effectivePermissions.length > 5 && (
                      <Chip label={`+${permissions.effectivePermissions.length - 5} more`} size="small" sx={{ mt: 1 }} />
                    )}
                  </Box>
                </Box>
              ) : (
                <Alert severity="warning">User permissions not loaded</Alert>
              )}
            </CardContent>
          </Card>
        </Grid>

        {/* Admin Only Content */}
        <Grid item xs={12} md={6}>
          <Card>
            <CardContent>
              <Typography variant="h6" gutterBottom>
                Admin Only Content
              </Typography>
              <AdminOnly fallback={<Alert severity="info">You need admin privileges to see this content.</Alert>}>
                <Alert severity="success">
                  Welcome, Administrator! You can see this because you have admin privileges.
                </Alert>
                <Button variant="contained" color="primary" sx={{ mt: 2 }}>
                  Admin Action
                </Button>
              </AdminOnly>
            </CardContent>
          </Card>
        </Grid>

        {/* Moderator Only Content */}
        <Grid item xs={12} md={6}>
          <Card>
            <CardContent>
              <Typography variant="h6" gutterBottom>
                Moderator+ Content
              </Typography>
              <ModeratorOnly fallback={<Alert severity="info">You need moderator or admin privileges to see this content.</Alert>}>
                <Alert severity="success">
                  You have moderation privileges! This includes both moderators and admins.
                </Alert>
                <Button variant="contained" color="secondary" sx={{ mt: 2 }}>
                  Moderate Content
                </Button>
              </ModeratorOnly>
            </CardContent>
          </Card>
        </Grid>

        {/* Specific Permission Check */}
        <Grid item xs={12} md={6}>
          <Card>
            <CardContent>
              <Typography variant="h6" gutterBottom>
                Model Creation
              </Typography>
              <RequirePermission 
                permission={PERMISSIONS.MODEL_CREATE}
                fallback={<Alert severity="warning">You don't have permission to create models.</Alert>}
              >
                <Alert severity="success">
                  You can create models!
                </Alert>
                <Button variant="contained" sx={{ mt: 2 }}>
                  Create New Model
                </Button>
              </RequirePermission>
            </CardContent>
          </Card>
        </Grid>

        {/* Any Permission Check */}
        <Grid item xs={12} md={6}>
          <Card>
            <CardContent>
              <Typography variant="h6" gutterBottom>
                Model Management
              </Typography>
              <RequireAnyPermission 
                permissions={[PERMISSIONS.MODEL_EDIT_ANY, PERMISSIONS.MODEL_DELETE_ANY]}
                fallback={<Alert severity="warning">You don't have any model management permissions.</Alert>}
              >
                <Alert severity="success">
                  You can manage models (edit or delete)!
                </Alert>
                <Button variant="contained" sx={{ mt: 2 }}>
                  Manage Models
                </Button>
              </RequireAnyPermission>
            </CardContent>
          </Card>
        </Grid>

        {/* Resource Ownership */}
        <Grid item xs={12}>
          <Card>
            <CardContent>
              <Typography variant="h6" gutterBottom>
                Resource Ownership Example
              </Typography>
              <Typography variant="body2" paragraph>
                This demonstrates how "own" vs "any" permissions work with resource ownership.
              </Typography>
              
              <Box sx={{ display: 'flex', gap: 2, flexWrap: 'wrap' }}>
                {/* Edit Own Model */}
                <RequirePermission 
                  permission={PERMISSIONS.MODEL_EDIT_OWN}
                  resourceOwnerId={exampleModel.authorId}
                  currentUserId={currentUserId}
                  fallback={<Chip label="Can't Edit Own Model" color="error" />}
                >
                  <Chip label="Can Edit Own Model" color="success" />
                </RequirePermission>

                {/* Edit Any Model */}
                <RequirePermission 
                  permission={PERMISSIONS.MODEL_EDIT_ANY}
                  fallback={<Chip label="Can't Edit Any Model" color="error" />}
                >
                  <Chip label="Can Edit Any Model" color="success" />
                </RequirePermission>

                {/* Delete Own Model */}
                <RequirePermission 
                  permission={PERMISSIONS.MODEL_DELETE_OWN}
                  resourceOwnerId={exampleModel.authorId}
                  currentUserId={currentUserId}
                  fallback={<Chip label="Can't Delete Own Model" color="error" />}
                >
                  <Chip label="Can Delete Own Model" color="success" />
                </RequirePermission>

                {/* Delete Any Model */}
                <RequirePermission 
                  permission={PERMISSIONS.MODEL_DELETE_ANY}
                  fallback={<Chip label="Can't Delete Any Model" color="error" />}
                >
                  <Chip label="Can Delete Any Model" color="success" />
                </RequirePermission>
              </Box>
            </CardContent>
          </Card>
        </Grid>

        {/* Role Hierarchy */}
        <Grid item xs={12}>
          <Card>
            <CardContent>
              <Typography variant="h6" gutterBottom>
                Role Hierarchy Examples
              </Typography>
              
              <Box sx={{ display: 'flex', gap: 2, flexWrap: 'wrap' }}>
                <RequireMinimumRole 
                  minimumRole="user"
                  fallback={<Chip label="Not User+" color="error" />}
                >
                  <Chip label="User+" color="primary" />
                </RequireMinimumRole>

                <RequireMinimumRole 
                  minimumRole="moderator"
                  fallback={<Chip label="Not Moderator+" color="error" />}
                >
                  <Chip label="Moderator+" color="secondary" />
                </RequireMinimumRole>

                <RequireMinimumRole 
                  minimumRole="admin"
                  fallback={<Chip label="Not Admin" color="error" />}
                >
                  <Chip label="Admin" color="success" />
                </RequireMinimumRole>
              </Box>
            </CardContent>
          </Card>
        </Grid>

        {/* Complex Permission Logic */}
        <Grid item xs={12}>
          <Card>
            <CardContent>
              <Typography variant="h6" gutterBottom>
                Complex Permission Logic
              </Typography>
              
              <PermissionGate
                customCheck={(userPermissions) => {
                  // Custom logic: Can moderate if user is admin OR has moderation permissions
                  const isUserAdmin = userPermissions.role.priority >= 1000;
                  const hasModerationPerms = userPermissions.effectivePermissions.some(p => 
                    p.startsWith('moderation.')
                  );
                  return isUserAdmin || hasModerationPerms;
                }}
                fallback={<Alert severity="info">You don't have sufficient privileges for moderation.</Alert>}
              >
                <Alert severity="success">
                  You have moderation capabilities through either admin role or specific moderation permissions!
                </Alert>
                <Button variant="contained" color="warning" sx={{ mt: 2 }}>
                  Access Moderation Tools
                </Button>
              </PermissionGate>
            </CardContent>
          </Card>
        </Grid>

        {/* Permission Details */}
        <Grid item xs={12}>
          <Card>
            <CardContent>
              <Typography variant="h6" gutterBottom>
                Permission Testing
              </Typography>
              
              <Box sx={{ display: 'grid', gridTemplateColumns: 'repeat(auto-fit, minmax(300px, 1fr))', gap: 2 }}>
                {Object.entries(PERMISSIONS).slice(0, 12).map(([key, permission]) => (
                  <Box key={key} sx={{ p: 2, border: '1px solid #e0e0e0', borderRadius: 1 }}>
                    <Typography variant="subtitle2">{key}</Typography>
                    <Typography variant="body2" color="text.secondary" sx={{ fontSize: '0.8em' }}>
                      {permission}
                    </Typography>
                    <Chip 
                      label={hasPermission(permission) ? 'Granted' : 'Denied'} 
                      color={hasPermission(permission) ? 'success' : 'error'}
                      size="small"
                      sx={{ mt: 1 }}
                    />
                  </Box>
                ))}
              </Box>
            </CardContent>
          </Card>
        </Grid>
      </Grid>
    </Box>
  );
};

export default ACLExamples; 