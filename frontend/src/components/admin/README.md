# Admin Panel Documentation

## Overview

The `ComprehensiveAdminPanel` component provides a tabbed interface for managing various aspects of the PolyBucket application. It's designed as an expandable shell that can be integrated with backend APIs as development progresses.

## Features

### Current Tabs

1. **Dashboard** - Analytics and overview metrics
2. **Users** - User management with clickable users and moderation management
3. **Roles** - Role management with permissions and hierarchy control
4. **Models** - Model management and bulk operations
5. **System** - General site and upload settings
6. **Auth** - Authentication and OAuth configuration
7. **Plugins** - Plugin management and store

### Key Components

- **Material-UI based** - Uses MUI components for consistent styling
- **Responsive design** - Works on desktop and tablet devices
- **Tabbed navigation** - Easy to navigate between different management areas
- **Mock data** - Currently displays sample data for development
- **Expandable structure** - Easy to add new tabs and functionality
- **Interactive user management** - Clickable users with expandable action panels
- **Role-based filtering** - Separate tabs for all users vs moderators only

## Integration

The admin panel is now available as a dedicated page at `/admin-panel`. It can be accessed via the "Admin Panel" link in the navigation bar on the dashboard. This allows viewing the admin interface even when not logged in (for development purposes). The page includes its own navigation header with a "Back to Dashboard" button for easy navigation.

## Enhanced User Management Features

### Interactive User Management
- **Clickable Users**: Click on any user in the list to reveal quick action buttons
- **Action Panel**: Expandable panel showing user-specific management options
- **Search Functionality**: Real-time search across usernames and email addresses

### User Actions Available
- **Edit Profile**: Modify user information
- **Ban/Unban User**: Toggle user access to the platform
- **Change Role**: Modify user permissions and roles
- **Make/Remove Moderator**: Promote users to moderator or demote them
- **Reset Password**: Force password reset for users
- **View Activity**: Check user activity logs and history

### Moderation Management Tab
- **Dedicated Moderator View**: Separate tab showing only users with moderator privileges
- **Moderator Statistics**: Special metrics for moderation team
- **Moderator Permissions**: Overview of what moderators can do
- **Add New Moderator**: Quick action to promote users to moderator role

### Visual Indicators
- **Role Badges**: Color-coded chips showing Admin, Moderator, or User roles
- **Status Indicators**: Active, Inactive, Online status display
- **Selection Highlighting**: Selected users are visually highlighted
- **Responsive Actions**: Different actions available based on user role and status

## Role Management Features

### Interactive Role Management
- **Clickable Roles**: Click on any role to reveal management actions and permissions
- **Default vs Custom Roles**: Visual distinction between system-provided and custom roles
- **Hierarchy Display**: Shows role hierarchy levels for permission inheritance
- **Permission Visualization**: Expandable view of all permissions for each role

### Role Actions Available
- **Edit Permissions**: Modify what actions users with this role can perform
- **View Users**: See all users assigned to this specific role
- **Edit Details**: Change role name, description, and hierarchy
- **Delete Role**: Remove custom roles (default roles are protected)
- **Duplicate Role**: Create a copy of an existing role as starting point
- **Change Hierarchy**: Modify the role's position in the permission hierarchy

### Default Role System
The system includes these default roles (typically fetched from API):
- **Administrator**: Full system access with all permissions (hierarchy 1)
- **Moderator**: Content and user moderation capabilities (hierarchy 2)
- **Contributor**: Can upload and manage own content (hierarchy 3)
- **User**: Basic viewing and downloading permissions (hierarchy 4)
- **Custom Roles**: Additional roles like VIP Member can be created

### Role Statistics & Management
- **Role Metrics**: Shows total roles, default vs custom breakdown
- **User Distribution**: See how many users are assigned to each role
- **Permission Overview**: Quick reference for available system permissions
- **Bulk Actions**: Create new roles, import role configurations

## Expanding the Admin Panel

### Adding New Tabs

1. Create a new panel component following the existing pattern:
```tsx
const NewManagementPanel: React.FC = () => (
  <Grid container spacing={3}>
    {/* Your content here */}
  </Grid>
);
```

2. Add the new tab to the `tabs` array in `ComprehensiveAdminPanel`:
```tsx
const tabs = [
  // existing tabs...
  { label: 'New Feature', icon: <NewIcon />, component: <NewManagementPanel /> },
];
```

### Connecting to Backend APIs

Replace the mock data with actual API calls:

1. **User Management**: Connect to `/api/admin/users` endpoints
2. **Role Management**: Connect to `/api/admin/roles` endpoints
3. **Model Management**: Connect to `/api/admin/models` endpoints  
4. **System Settings**: Connect to `/api/admin/settings` endpoints
5. **Authentication**: Connect to `/api/admin/auth-config` endpoints
6. **Plugins**: Connect to `/api/admin/plugins` endpoints

### Example API Integration

```tsx
// User Management Example
const [users, setUsers] = useState([]);

useEffect(() => {
  const fetchUsers = async () => {
    try {
      const response = await api.get('/admin/users');
      setUsers(response.data);
    } catch (error) {
      console.error('Failed to fetch users:', error);
    }
  };
  fetchUsers();
}, []);

// Role Management Example
const [roles, setRoles] = useState([]);

useEffect(() => {
  const fetchRoles = async () => {
    try {
      const response = await api.get('/admin/roles');
      setRoles(response.data);
    } catch (error) {
      console.error('Failed to fetch roles:', error);
    }
  };
  fetchRoles();
}, []);

// Role Action Example
const handleRoleAction = async (action: string, roleId: string) => {
  try {
    switch (action) {
      case 'edit-permissions':
        const permissions = await api.get(`/admin/roles/${roleId}/permissions`);
        // Open permissions editor with current permissions
        break;
      case 'view-users':
        const users = await api.get(`/admin/roles/${roleId}/users`);
        // Show users with this role
        break;
      case 'delete':
        await api.delete(`/admin/roles/${roleId}`);
        // Refresh role list
        break;
    }
  } catch (error) {
    console.error('Role action failed:', error);
  }
};
```

## Security Considerations

**Important**: In production, this admin panel should be:

1. **Protected by authentication** - Only accessible to authenticated admin users
2. **Role-based access controlled** - Different tabs restricted based on user permissions
3. **Rate limited** - Prevent abuse of admin endpoints
4. **Audit logged** - Track all admin actions for security

## File Structure

```
frontend/src/
├── components/admin/
│   ├── ComprehensiveAdminPanel.tsx  # Main admin panel component
│   ├── AdminDashboard.tsx           # Legacy admin dashboard (Material-UI)
│   ├── ModerationSettings.tsx       # Model moderation settings
│   ├── RoleManagement.tsx          # User role management
│   ├── ModelUploadSettings.tsx     # Model upload configuration
│   └── README.md                   # This documentation
└── pages/admin/
    ├── AdminControlPanel.tsx       # Dedicated admin panel page
    ├── AdminDashboard.tsx          # Legacy admin dashboard page
    ├── ModelUploadSettings.tsx     # Model upload settings page
    └── RoleManagement.tsx          # Role management page
```

## Development Workflow

1. **Design the UI** - Create the tab panel with mock data
2. **Define API contracts** - Plan the backend endpoints needed
3. **Implement backend APIs** - Create the corresponding backend controllers
4. **Connect frontend to backend** - Replace mock data with API calls
5. **Add validation and error handling** - Ensure robust user experience
6. **Implement role-based access** - Secure different admin functions
7. **Add testing** - Unit and integration tests for admin functionality

## Future Enhancements

- **Real-time updates** - WebSocket integration for live admin data
- **Advanced analytics** - Charts and graphs for usage metrics
- **Bulk operations** - Mass user/model management actions
- **Audit trail** - Complete admin action logging
- **Dark mode support** - Theme switching capability
- **Mobile responsive** - Better mobile admin experience 