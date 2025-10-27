import React, { useState, useEffect } from 'react';
import {
  AdminPanelSettings as RoleIcon,
  Security as SecurityIcon,
  People as PeopleIcon,
  Add as AddIcon,
  Edit as EditIcon,
  Delete as DeleteIcon,
  Visibility as ViewIcon,
  Close as CloseIcon,
  Save as SaveIcon,
  Cancel as CancelIcon,
  Search as SearchIcon,
  FirstPage as FirstPageIcon,
  LastPage as LastPageIcon,
  NavigateNext as NextPageIcon,
  NavigateBefore as PrevPageIcon,
  ExpandMore as ExpandMoreIcon,
  ExpandLess as ExpandLessIcon,
  Shield as ShieldIcon,
  Settings as SettingsIcon,
  Storage as StorageIcon,
  Comment as CommentIcon,
  Report as ReportIcon,
  Category as CategoryIcon,
  Api as ApiIcon,

} from '@mui/icons-material';
import { RoleDto, CreateRoleRequest, UpdateRoleRequest, PermissionDto } from '../../services/api.client';
import { RoleManagementClient } from '../../services/api.client';
import { defaultAxiosClient } from '../../api/axiosAdapter';

interface PaginationInfo {
  page: number;
  pageSize: number;
  totalCount: number;
  totalPages: number;
  hasNextPage: boolean;
  hasPreviousPage: boolean;
}

interface PermissionCategory {
  name: string;
  icon: string;
  color: string;
  permissions: string[];
}

interface CreateRoleFormData {
  name: string;
  description: string;
  priority: number;
  isSystemRole: boolean;
  isDefault: boolean;
  parentRoleId?: string;
  initialPermissions: string[];
  color: string;
}

const RoleManagement: React.FC = () => {
  const [loading, setLoading] = useState(true);
  const [allRoles, setAllRoles] = useState<RoleDto[]>([]);
  const [displayedRoles, setDisplayedRoles] = useState<RoleDto[]>([]);
  const [permissions, setPermissions] = useState<PermissionDto[]>([]);
  const [selectedRole, setSelectedRole] = useState<RoleDto | null>(null);
  const [isCreateModalOpen, setIsCreateModalOpen] = useState(false);
  const [isEditModalOpen, setIsEditModalOpen] = useState(false);
  const [isViewModalOpen, setIsViewModalOpen] = useState(false);
  const [expandedRoles, setExpandedRoles] = useState<Set<string>>(new Set());
  const [snackbar, setSnackbar] = useState({
    open: false,
    message: '',
    severity: 'success' as 'success' | 'error' | 'warning' | 'info',
  });

  // Pagination state
  const [pagination, setPagination] = useState<PaginationInfo>({
    page: 1,
    pageSize: 10,
    totalCount: 0,
    totalPages: 0,
    hasNextPage: false,
    hasPreviousPage: false
  });

  // Search and filter state
  const [searchTerm, setSearchTerm] = useState('');
  const [sortBy, setSortBy] = useState('priority');
  const [sortDescending, setSortDescending] = useState(true);

  const [createForm, setCreateForm] = useState<CreateRoleFormData>({
    name: '',
    description: '',
    priority: 100,
    isSystemRole: false,
    isDefault: false,
    parentRoleId: undefined,
    initialPermissions: [],
    color: '#3B82F6', // Default blue color
  });

  const [editForm, setEditForm] = useState<UpdateRoleRequest>(() => {
    const form = new UpdateRoleRequest();
    form.name = '';
    form.description = '';
    form.priority = 100;
    form.isDefault = false;
    form.isActive = true;
    return form;
  });

  const [editFormColor, setEditFormColor] = useState<string>('#3B82F6');

  const roleManagementClient = new RoleManagementClient(undefined, defaultAxiosClient);

  useEffect(() => {
    fetchData();
    fetchPermissions();
  }, []);

  useEffect(() => {
    applyFiltersAndPagination();
  }, [allRoles, searchTerm, sortBy, sortDescending, pagination.page, pagination.pageSize]);

  const fetchData = async () => {
    try {
      setLoading(true);
      
      // Fetch roles with pagination
      const response = await roleManagementClient.getAllRoles(
        pagination.page,
        pagination.pageSize,
        searchTerm || undefined,
        sortBy || undefined,
        sortDescending
      );
      
      console.log('Fetched roles:', response.roles);
      setAllRoles(response.roles || []);
      setDisplayedRoles(response.roles || []);
      
      // Update pagination info from response
      setPagination(prev => ({
        ...prev,
        totalCount: response.pagination?.totalCount || 0,
        totalPages: response.pagination?.totalPages || 0,
        hasNextPage: response.pagination?.hasNextPage || false,
        hasPreviousPage: response.pagination?.hasPreviousPage || false
      }));
      
    } catch (error) {
      console.error('Failed to fetch data:', error);
      showSnackbar('Failed to load data', 'error');
    } finally {
      setLoading(false);
    }
  };

  const fetchPermissions = async () => {
    try {
      // Fetch all available permissions
      const permissionsResponse = await roleManagementClient.getAllPermissions();
      setPermissions(permissionsResponse || []);
    } catch (error) {
      console.error('Failed to fetch permissions:', error);
      // Don't show error snackbar for permissions as it's not critical for basic functionality
    }
  };

  const applyFiltersAndPagination = () => {
    let filteredRoles = [...allRoles];

    // Apply search filter
    if (searchTerm) {
      filteredRoles = filteredRoles.filter(role => 
        role.name?.toLowerCase().includes(searchTerm.toLowerCase()) ||
        role.description?.toLowerCase().includes(searchTerm.toLowerCase())
      );
    }

    // Apply sorting
    filteredRoles.sort((a, b) => {
      let comparison = 0;
      switch (sortBy) {
        case 'name':
          comparison = (a.name || '').localeCompare(b.name || '');
          break;
        case 'priority':
          comparison = (a.priority || 0) - (b.priority || 0);
          break;
        case 'users':
          comparison = (a.userCount || 0) - (b.userCount || 0);
          break;
        default:
          comparison = (a.priority || 0) - (b.priority || 0);
          break;
      }
      return sortDescending ? -comparison : comparison;
    });

    // Apply pagination
    const startIndex = (pagination.page - 1) * pagination.pageSize;
    const endIndex = startIndex + pagination.pageSize;
    const pagedRoles = filteredRoles.slice(startIndex, endIndex);

    setDisplayedRoles(pagedRoles);
    
    // Update pagination info
    setPagination(prev => ({
      ...prev,
      totalCount: filteredRoles.length,
      totalPages: Math.ceil(filteredRoles.length / prev.pageSize),
      hasNextPage: endIndex < filteredRoles.length,
      hasPreviousPage: pagination.page > 1
    }));
  };

  const handleCreateRole = async () => {
    try {
      // Convert local form data to CreateRoleRequest
      const createRoleRequest = new CreateRoleRequest();
      createRoleRequest.name = createForm.name;
      createRoleRequest.description = createForm.description;
      createRoleRequest.priority = createForm.priority;
      createRoleRequest.isSystemRole = createForm.isSystemRole;
      createRoleRequest.isDefault = createForm.isDefault;
      createRoleRequest.parentRoleId = createForm.parentRoleId;
      createRoleRequest.initialPermissions = createForm.initialPermissions;
      createRoleRequest.color = createForm.color;
      
      const newRole = await roleManagementClient.createRole(createRoleRequest);
      setAllRoles([newRole, ...allRoles]);
      setIsCreateModalOpen(false);
      resetCreateForm();
      showSnackbar('Role created successfully', 'success');
    } catch (error) {
      console.error('Failed to create role:', error);
      showSnackbar('Failed to create role', 'error');
    }
  };

  const handleUpdateRole = async () => {
    if (!selectedRole?.id) return;
    
    try {
      // Include the color in the update request
      editForm.color = editFormColor;
      
      const updatedRole = await roleManagementClient.updateRole(selectedRole.id, editForm);
      
      // Update the role with the new color
      const roleWithUpdatedColor = new RoleDto(updatedRole);
      roleWithUpdatedColor.color = editFormColor;
      
      setAllRoles(allRoles => allRoles.map(r => r.id === updatedRole.id ? roleWithUpdatedColor : r));
      setIsEditModalOpen(false);
      setSelectedRole(null);
      showSnackbar('Role updated successfully', 'success');
    } catch (error) {
      console.error('Failed to update role:', error);
      showSnackbar('Failed to update role', 'error');
    }
  };

  const handleDeleteRole = async (roleId: string) => {
    if (!confirm('Are you sure you want to delete this role?')) return;
    
    try {
      await roleManagementClient.deleteRole(roleId);
      setAllRoles(roles => roles.filter(r => r.id !== roleId));
      showSnackbar('Role deleted successfully', 'success');
    } catch (error) {
      console.error('Failed to delete role:', error);
      showSnackbar('Failed to delete role', 'error');
    }
  };

  const openCreateModal = () => {
    setIsCreateModalOpen(true);
  };

  const openEditModal = (role: RoleDto) => {
    setSelectedRole(role);
    const form = new UpdateRoleRequest();
    form.name = role.name || '';
    form.description = role.description || '';
    form.priority = role.priority || 100;
    form.isDefault = role.isDefault || false;
    form.isActive = role.isActive || true;
    form.color = role.color || '#3B82F6';
    setEditForm(form);
    setEditFormColor(role.color || '#3B82F6');
    setIsEditModalOpen(true);
  };

  const openViewModal = (role: RoleDto) => {
    setSelectedRole(role);
    setIsViewModalOpen(true);
  };

  const duplicateRole = (role: RoleDto) => {
    setCreateForm({
      name: `${role.name} Copy`,
      description: `Copy of ${role.description}`,
      priority: (role.priority || 100) + 1,
      isSystemRole: false,
      isDefault: false,
      parentRoleId: undefined,
      initialPermissions: role.permissions || [],
      color: role.color || '#3B82F6', // Copy the original role's color or use default
    });
    setIsCreateModalOpen(true);
  };

  const resetCreateForm = () => {
    setCreateForm({
      name: '',
      description: '',
      priority: 100,
      isSystemRole: false,
      isDefault: false,
      parentRoleId: undefined,
      initialPermissions: [],
      color: '#3B82F6', // Default blue color
    });
  };

  const handleSearch = () => {
    setPagination(prev => ({ ...prev, page: 1 }));
  };

  const handlePageChange = (newPage: number) => {
    setPagination(prev => ({ ...prev, page: newPage }));
  };

  const handlePageSizeChange = (newPageSize: number) => {
    setPagination(prev => ({ ...prev, page: 1, pageSize: newPageSize }));
  };

  const handleSort = (newSortBy: string) => {
    if (sortBy === newSortBy) {
      setSortDescending(!sortDescending);
    } else {
      setSortBy(newSortBy);
      setSortDescending(true);
    }
    setPagination(prev => ({ ...prev, page: 1 }));
  };

  const showSnackbar = (message: string, severity: 'success' | 'error' | 'warning' | 'info') => {
    setSnackbar({ open: true, message, severity });
  };

  const getTotalUsers = () => {
    return allRoles.reduce((total, role) => total + (role.userCount || 0), 0);
  };

  const getTotalPermissions = () => {
    return allRoles.reduce((total, role) => total + (role.permissions?.length || 0), 0);
  };

  const toggleRoleExpansion = (roleId: string) => {
    const newExpanded = new Set(expandedRoles);
    if (newExpanded.has(roleId)) {
      newExpanded.delete(roleId);
    } else {
      newExpanded.add(roleId);
    }
    setExpandedRoles(newExpanded);
  };

  const groupPermissionsByCategory = (permissions: string[]): PermissionCategory[] => {
    const categories: { [key: string]: PermissionCategory } = {
      'Administration': {
        name: 'Administration',
        icon: 'shield',
        color: '#EF4444',
        permissions: []
      },
      'User Management': {
        name: 'User Management',
        icon: 'people',
        color: '#3B82F6',
        permissions: []
      },
      'Model Management': {
        name: 'Model Management',
        icon: 'category',
        color: '#10B981',
        permissions: []
      },
      'Moderation': {
        name: 'Moderation',
        icon: 'security',
        color: '#F59E0B',
        permissions: []
      },
      'Collections': {
        name: 'Collections',
        icon: 'storage',
        color: '#8B5CF6',
        permissions: []
      },
      'Comments': {
        name: 'Comments',
        icon: 'comment',
        color: '#EC4899',
        permissions: []
      },
      'Reports': {
        name: 'Reports',
        icon: 'report',
        color: '#F97316',
        permissions: []
      },
      'Plugins': {
        name: 'Plugins',
        icon: 'settings',
        color: '#6366F1',
        permissions: []
      },
      'API Access': {
        name: 'API Access',
        icon: 'api',
        color: '#14B8A6',
        permissions: []
      },
      'Storage & Files': {
        name: 'Storage & Files',
        icon: 'storage',
        color: '#6B7280',
        permissions: []
      }
    };

    permissions.forEach(permission => {
      const category = permission.split('.')[0];
      const categoryKey = Object.keys(categories).find(key => 
        key.toLowerCase().includes(category) || 
        category.toLowerCase().includes(key.toLowerCase())
      ) || 'Other';
      
      if (categories[categoryKey]) {
        categories[categoryKey].permissions.push(permission);
      } else if (!categories['Other']) {
        categories['Other'] = {
          name: 'Other',
          icon: 'settings',
          color: '#6B7280',
          permissions: []
        };
      }
      if (categories['Other']) {
        categories['Other'].permissions.push(permission);
      }
    });

    return Object.values(categories).filter(cat => cat.permissions.length > 0);
  };

  const getRolePriorityColor = (priority: number) => {
    if (priority >= 1000) return 'bg-red-600';
    if (priority >= 500) return 'bg-orange-600';
    if (priority >= 200) return 'bg-yellow-600';
    return 'bg-blue-600';
  };

  const getPermissionCategoryColor = (category: string) => {
    const colors: { [key: string]: string } = {
      'Administration': 'bg-red-500',
      'User Management': 'bg-blue-500',
      'Model Management': 'bg-green-500',
      'Moderation': 'bg-yellow-500',
      'Collections': 'bg-purple-500',
      'Comments': 'bg-pink-500',
      'Reports': 'bg-orange-500',
      'Plugins': 'bg-indigo-500',
      'API Access': 'bg-teal-500',
      'Storage & Files': 'bg-gray-500'
    };
    return colors[category] || 'bg-gray-500';
  };

  const renderPermissionIcon = (iconName: string) => {
    switch (iconName) {
      case 'shield': return <ShieldIcon className="w-4 h-4" />;
      case 'people': return <PeopleIcon className="w-4 h-4" />;
      case 'category': return <CategoryIcon className="w-4 h-4" />;
      case 'security': return <SecurityIcon className="w-4 h-4" />;
      case 'storage': return <StorageIcon className="w-4 h-4" />;
      case 'comment': return <CommentIcon className="w-4 h-4" />;
      case 'report': return <ReportIcon className="w-4 h-4" />;
      case 'api': return <ApiIcon className="w-4 h-4" />;
      case 'settings': return <SettingsIcon className="w-4 h-4" />;
      default: return <SettingsIcon className="w-4 h-4" />;
    }
  };

  const getPermissionDescription = (permissionName: string): string => {
    const descriptions: { [key: string]: string } = {
      // User Management
      'user.delete.account': 'Delete user account',
      'user.view.profile': 'View user profiles',
      'user.edit.profile': 'Edit user profiles',
      'user.change.password': 'Change user passwords',
      'user.manage.settings': 'Manage user settings',
      'user.view.private_models': 'View private models',
      
      // Model Management
      'model.create': 'Create new models',
      'model.view.public': 'View public models',
      'model.view.private': 'View private models',
      'model.edit.own': 'Edit own models',
      'model.edit.any': 'Edit any models',
      'model.delete.own': 'Delete own models',
      'model.delete.any': 'Delete any models',
      'model.upload.files': 'Upload model files',
      'model.download': 'Download models',
      'model.like': 'Like models',
      'model.feature': 'Feature models',
      'model.view.analytics': 'View model analytics',
      
      // Collections
      'collection.view.public': 'View public collections',
      'collection.view.private': 'View private collections',
      'collection.create': 'Create collections',
      'collection.edit.own': 'Edit own collections',
      'collection.edit.any': 'Edit any collections',
      'collection.add.models': 'Add models to collections',
      'collection.delete.own': 'Delete own collections',
      'collection.delete.any': 'Delete any collections',
      'collection.remove.models': 'Remove models from collections',
      
      // Comments
      'comment.create': 'Create comments',
      'comment.view': 'View comments',
      'comment.edit.own': 'Edit own comments',
      'comment.edit.any': 'Edit any comments',
      'comment.delete.own': 'Delete own comments',
      'comment.delete.any': 'Delete any comments',
      'comment.report': 'Report comments',
      
      // Moderation
      'moderation.approve.models': 'Approve models for publication',
      'moderation.view.queue': 'View moderation queue',
      'moderation.reject.models': 'Reject models',
      'moderation.flag.content': 'Flag inappropriate content',
      'moderation.edit.models': 'Edit models during moderation',
      'moderation.handle.reports': 'Handle user reports',
      'moderation.view.reports': 'View all reports',
      'moderation.moderate.comments': 'Moderate comments',
      'moderation.view.audit_log': 'View moderation audit logs',
      'moderation.moderate.users': 'Moderate user accounts',
      
      // Reports
      'report.view.own': 'View own reports',
      'report.view.all': 'View all reports',
      'report.create': 'Create reports',
      'report.handle': 'Handle reports',
      'report.delete': 'Delete reports',
      
      // API Access
      'api.read.access': 'Read API access',
      'api.write.access': 'Write API access',
      'api.admin.access': 'Admin API access',
      'api.rate_limit.override': 'Override API rate limits',
      
      // Storage & Files
      'storage.upload': 'Upload files',
      'storage.delete.own': 'Delete own files',
      'storage.delete.any': 'Delete any files',
      'storage.view.usage': 'View storage usage',
      'storage.manage.quotas': 'Manage storage quotas',
      
      // Plugins
      'plugin.install': 'Install plugins',
      'plugin.view.logs': 'View plugin logs',
      'plugin.configure': 'Configure plugins',
      'plugin.manage': 'Manage plugins',
      
      // Administration
      'admin.system.settings': 'Manage system settings',
      'admin.view.audit_logs': 'View system audit logs',
      'admin.manage.roles': 'Manage user roles',
      'admin.manage.permissions': 'Manage permissions',
      'admin.manage.users': 'Manage all users',
      'admin.delete.any_user': 'Delete any user account',
      'admin.ban.users': 'Ban user accounts',
      'admin.view.user_details': 'View detailed user information',
      'admin.manage.categories': 'Manage content categories'
    };
    
    return descriptions[permissionName] || permissionName;
  };

  if (loading) {
    return (
      <div className="flex items-center justify-center py-12">
        <div className="lg-spinner"></div>
        <span className="ml-3 text-lg text-white">Loading role data...</span>
      </div>
    );
  }

  return (
    <div className="space-y-6">
      <div className="flex justify-between items-center">
        <h2 className="text-2xl font-bold text-white">System Roles</h2>
        <button
          onClick={openCreateModal}
          className="lg-button lg-button-primary flex items-center"
        >
          <AddIcon className="w-4 h-4 mr-2" />
          Create Role
        </button>
      </div>

      {/* Search and Filter Controls */}
      <div className="lg-card p-4">
        <div className="flex flex-col md:flex-row gap-4 items-center">
          <div className="flex-1">
            <div className="relative">
              <SearchIcon className="absolute left-3 top-1/2 transform -translate-y-1/2 text-white/40 w-4 h-4" />
              <input
                type="text"
                value={searchTerm}
                onChange={(e) => setSearchTerm(e.target.value)}
                onKeyPress={(e) => e.key === 'Enter' && handleSearch()}
                placeholder="Search roles by name or description..."
                className="w-full pl-10 pr-4 py-2 bg-gray-700 border border-gray-600 rounded-md text-white focus:outline-none focus:ring-2 focus:ring-blue-500"
              />
            </div>
          </div>
          
          <div className="flex items-center space-x-4">
            <select
              value={sortBy}
              onChange={(e) => handleSort(e.target.value)}
              className="px-3 py-2 bg-gray-700 border border-gray-600 rounded-md text-white focus:outline-none focus:ring-2 focus:ring-blue-500"
            >
              <option value="priority">Sort by Priority</option>
              <option value="name">Sort by Name</option>
              <option value="users">Sort by Users</option>
            </select>
            
            <button
              onClick={() => handleSort(sortBy)}
              className={`px-3 py-2 rounded-md ${
                sortDescending 
                  ? 'bg-blue-600 hover:bg-blue-700' 
                  : 'bg-gray-600 hover:bg-gray-700'
              } text-white`}
            >
              {sortDescending ? '↓' : '↑'}
            </button>
            
            <select
              value={pagination.pageSize}
              onChange={(e) => handlePageSizeChange(parseInt(e.target.value))}
              className="px-3 py-2 bg-gray-700 border border-gray-600 rounded-md text-white focus:outline-none focus:ring-2 focus:ring-blue-500"
            >
              <option value={5}>5 per page</option>
              <option value={10}>10 per page</option>
              <option value={25}>25 per page</option>
              <option value={50}>50 per page</option>
            </select>
          </div>
        </div>
      </div>

      {/* Statistics Cards */}
      <div className="grid grid-cols-1 md:grid-cols-3 gap-4">
        <div className="lg-card p-4">
          <div className="flex items-center">
            <div className="w-12 h-12 bg-indigo-500 rounded-lg flex items-center justify-center mr-3">
              <RoleIcon className="w-6 h-6 text-white" />
            </div>
            <div>
              <div className="text-2xl font-bold text-white">{pagination.totalCount}</div>
              <div className="text-sm text-white/60">Total Roles</div>
            </div>
          </div>
        </div>
        
        <div className="lg-card p-4">
          <div className="flex items-center">
            <div className="w-12 h-12 bg-green-500 rounded-lg flex items-center justify-center mr-3">
              <PeopleIcon className="w-6 h-6 text-white" />
            </div>
            <div>
              <div className="text-2xl font-bold text-white">{getTotalUsers()}</div>
              <div className="text-sm text-white/60">Total Users</div>
            </div>
          </div>
        </div>
        
        <div className="lg-card p-4">
          <div className="flex items-center">
            <div className="w-12 h-12 bg-yellow-500 rounded-lg flex items-center justify-center mr-3">
              <SecurityIcon className="w-6 h-4 text-white" />
            </div>
            <div>
              <div className="text-2xl font-bold text-white">{getTotalPermissions()}</div>
              <div className="text-sm text-white/60">Total Permissions</div>
            </div>
          </div>
        </div>
      </div>

      {/* Roles List */}
      <div className="lg-card p-6">
        <h3 className="text-lg font-medium text-white mb-4">System Roles</h3>
        
        <div className="space-y-4">
          {displayedRoles.map((role) => {
            const permissionCategories = role.permissions ? groupPermissionsByCategory(role.permissions) : [];
            const isExpanded = expandedRoles.has(role.id || '');
            
            return (
              <div key={role.id} className="border border-white/10 rounded-lg overflow-hidden">
                {/* Role Header */}
                <div className="bg-gray-800/50 p-4">
                  <div className="flex items-center justify-between">
                    <div className="flex items-center space-x-3">
                      <span 
                        className="px-3 py-1 rounded-full text-sm font-medium text-white"
                        style={{ 
                          backgroundColor: role.color || '#3B82F6',
                          border: `2px solid ${role.color || '#3B82F6'}`,
                          boxShadow: `0 0 10px ${(role.color || '#3B82F6')}40`
                        }}
                        title={`Color: ${role.color || 'default'}`}
                        data-color={role.color || 'default'}
                      >
                        {role.name}
                      </span>
                      <span className="text-sm text-white/60">
                        {role.userCount || 0} users
                      </span>
                      <span className="text-xs text-white/40 bg-white/10 px-2 py-1 rounded">
                        Priority: {role.priority}
                      </span>
                      {role.isSystemRole && (
                        <span className="text-xs text-white/40 bg-white/10 px-2 py-1 rounded">
                          System Role
                        </span>
                      )}
                    </div>
                    
                    <div className="flex items-center space-x-2">
                      <button
                        onClick={() => toggleRoleExpansion(role.id || '')}
                        className="text-white/60 hover:text-white p-1"
                        title={isExpanded ? "Collapse" : "Expand"}
                      >
                        {isExpanded ? <ExpandLessIcon className="w-4 h-4" /> : <ExpandMoreIcon className="w-4 h-4" />}
                      </button>
                      <button
                        onClick={() => openViewModal(role)}
                        className="text-white/60 hover:text-white p-1"
                        title="View Role"
                      >
                        <ViewIcon className="w-4 h-4" />
                      </button>
                                        <button
                    onClick={() => duplicateRole(role)}
                    className="text-white/60 hover:text-white p-1"
                    title="Duplicate Role"
                  >
                    <AddIcon className="w-4 h-4" />
                  </button>
                  {role.canBeDeleted && (
                    <button
                      onClick={() => openEditModal(role)}
                      className="text-white/60 hover:text-white p-1"
                      title="Edit Role"
                    >
                      <EditIcon className="w-4 h-4" />
                    </button>
                  )}
                  {!role.canBeDeleted && (
                    <button
                      onClick={() => openEditModal(role)}
                      className="text-blue-400 hover:text-blue-300 p-1"
                      title="Edit Role (Limited - Admin Role)"
                    >
                      <EditIcon className="w-4 h-4" />
                    </button>
                  )}
                  {role.canBeDeleted && (
                    <button
                      onClick={() => role.id && handleDeleteRole(role.id)}
                      className="text-red-400 hover:text-red-300 p-1"
                      title="Delete Role"
                    >
                      <DeleteIcon className="w-4 h-4" />
                    </button>
                  )}
                    </div>
                  </div>
                  
                  <p className="text-white/80 mt-2">{role.description}</p>
                </div>
                
                {/* Permissions Section */}
                {isExpanded && role.permissions && role.permissions.length > 0 && (
                  <div className="p-4 bg-gray-900/30">
                    <h4 className="text-sm font-medium text-white mb-3 flex items-center">
                      <SecurityIcon className="w-4 h-4 mr-2" />
                      Permissions ({role.permissions.length})
                    </h4>
                    
                    <div className="grid grid-cols-1 lg:grid-cols-2 gap-4">
                      {permissionCategories.map((category) => (
                        <div key={category.name} className="space-y-2">
                                                   <div className="flex items-center space-x-2">
                           <div 
                           className="w-3 h-3 rounded-full" 
                           style={{ backgroundColor: category.color }}
                           title={`Category: ${category.name}, Color: ${category.color}`}
                           data-category-color={category.color}
                         ></div>
                           {renderPermissionIcon(category.icon)}
                           <span className="text-sm font-medium text-white/80">{category.name}</span>
                           <span className="text-xs text-white/40">({category.permissions.length})</span>
                         </div>
                          <div className="space-y-1">
                            {category.permissions.map((permission) => (
                              <div
                                key={permission}
                                className="text-xs bg-white/5 px-2 py-1 rounded"
                              >
                                <div className="font-medium text-white/80">{permission}</div>
                                <div className="text-white/60 text-xs mt-1">
                                  {getPermissionDescription(permission)}
                                </div>
                              </div>
                            ))}
                          </div>
                        </div>
                      ))}
                    </div>
                  </div>
                )}
              </div>
            );
          })}
        </div>

        {/* Pagination Controls */}
        {pagination.totalPages > 1 && (
          <div className="flex items-center justify-between mt-6 pt-4 border-t border-white/10">
            <div className="text-sm text-white/60">
              Showing {((pagination.page - 1) * pagination.pageSize) + 1} to {Math.min(pagination.page * pagination.pageSize, pagination.totalCount)} of {pagination.totalCount} roles
            </div>
            
            <div className="flex items-center space-x-2">
              <button
                onClick={() => handlePageChange(1)}
                disabled={!pagination.hasPreviousPage}
                className="p-2 rounded-md bg-gray-700 hover:bg-gray-600 disabled:opacity-50 disabled:cursor-not-allowed text-white"
              >
                <FirstPageIcon className="w-4 h-4" />
              </button>
              
              <button
                onClick={() => handlePageChange(pagination.page - 1)}
                disabled={!pagination.hasPreviousPage}
                className="p-2 rounded-md bg-gray-700 hover:bg-gray-600 disabled:opacity-50 disabled:cursor-not-allowed text-white"
              >
                <PrevPageIcon className="w-4 h-4" />
              </button>
              
              <span className="px-3 py-2 text-white">
                Page {pagination.page} of {pagination.totalPages}
              </span>
              
              <button
                onClick={() => handlePageChange(pagination.page + 1)}
                disabled={!pagination.hasNextPage}
                className="p-2 rounded-md bg-gray-700 hover:bg-gray-600 disabled:opacity-50 disabled:cursor-not-allowed text-white"
              >
                <NextPageIcon className="w-4 h-4" />
              </button>
              
              <button
                onClick={() => handlePageChange(pagination.totalPages)}
                disabled={!pagination.hasNextPage}
                className="p-2 rounded-md bg-gray-700 hover:bg-gray-600 disabled:opacity-50 disabled:cursor-not-allowed text-white"
              >
                <LastPageIcon className="w-4 h-4" />
              </button>
            </div>
          </div>
        )}
      </div>

      {/* Create Role Modal */}
      {isCreateModalOpen && (
        <div className="fixed inset-0 bg-black/50 flex items-center justify-center z-50">
          <div className="bg-gray-800 rounded-lg p-6 w-full max-w-2xl max-h-[90vh] overflow-y-auto">
            <div className="flex justify-between items-center mb-4">
              <h3 className="text-lg font-medium text-white">Create New Role</h3>
              <button
                onClick={() => setIsCreateModalOpen(false)}
                className="text-white/60 hover:text-white"
              >
                <CloseIcon />
              </button>
            </div>
            
            <div className="space-y-4">
              <div>
                <label className="block text-sm font-medium text-white mb-2">Name</label>
                <input
                  type="text"
                  value={createForm.name}
                  onChange={(e) => {
                    const newForm = new CreateRoleRequest();
                    newForm.name = e.target.value;
                    newForm.description = createForm.description;
                    newForm.priority = createForm.priority;
                    newForm.isSystemRole = createForm.isSystemRole;
                    newForm.isDefault = createForm.isDefault;
                    newForm.parentRoleId = createForm.parentRoleId;
                    newForm.initialPermissions = createForm.initialPermissions || [];
                    newForm.color = createForm.color;
                    setCreateForm({
                      name: e.target.value,
                      description: createForm.description,
                      priority: createForm.priority,
                      isSystemRole: createForm.isSystemRole,
                      isDefault: createForm.isDefault,
                      parentRoleId: createForm.parentRoleId,
                      initialPermissions: createForm.initialPermissions || [],
                      color: createForm.color,
                    });
                  }}
                  className="w-full px-3 py-2 bg-gray-700 border border-gray-600 rounded-md text-white focus:outline-none focus:ring-2 focus:ring-blue-500"
                  placeholder="Enter role name"
                />
              </div>
              
              <div>
                <label className="block text-sm font-medium text-white mb-2">Description</label>
                <textarea
                  value={createForm.description}
                  onChange={(e) => {
                    const newForm = new CreateRoleRequest();
                    newForm.name = createForm.name;
                    newForm.description = e.target.value;
                    newForm.priority = createForm.priority;
                    newForm.isSystemRole = createForm.isSystemRole;
                    newForm.isDefault = createForm.isDefault;
                    newForm.parentRoleId = createForm.parentRoleId;
                    newForm.initialPermissions = createForm.initialPermissions || [];
                    newForm.color = createForm.color;
                    setCreateForm({
                      name: createForm.name,
                      description: e.target.value,
                      priority: createForm.priority,
                      isSystemRole: createForm.isSystemRole,
                      isDefault: createForm.isDefault,
                      parentRoleId: createForm.parentRoleId,
                      initialPermissions: createForm.initialPermissions || [],
                      color: createForm.color,
                    });
                  }}
                  className="w-full px-3 py-2 bg-gray-700 border border-gray-600 rounded-md text-white focus:outline-none focus:ring-2 focus:ring-blue-500"
                  rows={3}
                  placeholder="Enter role description"
                />
              </div>
              
              <div>
                <label className="block text-sm font-medium text-white mb-2">Priority</label>
                <input
                  type="number"
                  value={createForm.priority}
                  onChange={(e) => {
                    const newForm = new CreateRoleRequest();
                    newForm.name = createForm.name;
                    newForm.description = createForm.description;
                    newForm.priority = parseInt(e.target.value) || 100;
                    newForm.isSystemRole = createForm.isSystemRole;
                    newForm.isDefault = createForm.isDefault;
                    newForm.parentRoleId = createForm.parentRoleId;
                    newForm.initialPermissions = createForm.initialPermissions || [];
                    newForm.color = createForm.color;
                    setCreateForm({
                      name: createForm.name,
                      description: createForm.description,
                      priority: parseInt(e.target.value) || 100,
                      isSystemRole: createForm.isSystemRole,
                      isDefault: createForm.isDefault,
                      parentRoleId: createForm.parentRoleId,
                      initialPermissions: createForm.initialPermissions || [],
                      color: createForm.color,
                    });
                  }}
                  className="w-full px-3 py-2 bg-gray-700 border border-gray-600 rounded-md text-white focus:outline-none focus:ring-2 focus:ring-blue-500"
                  min="1"
                  max="999"
                />
              </div>

              <div>
                <label className="block text-sm font-medium text-white mb-2">Role Color</label>
                <div className="flex items-center space-x-3">
                  <div className="relative">
                    <input
                      type="color"
                      value={createForm.color || '#3B82F6'}
                      onChange={(e) => {
                        const newForm = new CreateRoleRequest();
                        newForm.name = createForm.name;
                        newForm.description = createForm.description;
                        newForm.priority = createForm.priority;
                        newForm.isSystemRole = createForm.isSystemRole;
                        newForm.isDefault = createForm.isDefault;
                        newForm.parentRoleId = createForm.parentRoleId;
                        newForm.initialPermissions = createForm.initialPermissions || [];
                        newForm.color = e.target.value;
                        setCreateForm({
                          name: createForm.name,
                          description: createForm.description,
                          priority: createForm.priority,
                          isSystemRole: createForm.isSystemRole,
                          isDefault: createForm.isDefault,
                          parentRoleId: createForm.parentRoleId,
                          initialPermissions: createForm.initialPermissions || [],
                          color: e.target.value,
                        });
                      }}
                      className="w-12 h-12 rounded-lg border-2 border-gray-600 cursor-pointer bg-transparent"
                      title="Choose role color"
                    />
                  </div>
                  <div className="flex-1">
                    <input
                      type="text"
                      value={createForm.color || '#3B82F6'}
                      onChange={(e) => {
                        const newForm = new CreateRoleRequest();
                        newForm.name = createForm.name;
                        newForm.description = createForm.description;
                        newForm.priority = createForm.priority;
                        newForm.isSystemRole = createForm.isSystemRole;
                        newForm.isDefault = createForm.isDefault;
                        newForm.parentRoleId = createForm.parentRoleId;
                        newForm.initialPermissions = createForm.initialPermissions || [];
                        newForm.color = e.target.value;
                        setCreateForm({
                          name: createForm.name,
                          description: createForm.description,
                          priority: createForm.priority,
                          isSystemRole: createForm.isSystemRole,
                          isDefault: createForm.isDefault,
                          parentRoleId: createForm.parentRoleId,
                          initialPermissions: createForm.initialPermissions || [],
                          color: e.target.value,
                        });
                      }}
                      placeholder="#3B82F6"
                      className="w-full px-3 py-2 bg-gray-700 border border-gray-600 rounded-md text-white focus:outline-none focus:ring-2 focus:ring-blue-500 font-mono text-sm"
                    />
                  </div>
                  <div className="flex space-x-2">
                    <button
                      type="button"
                      onClick={() => {
                        const newForm = new CreateRoleRequest();
                        newForm.name = createForm.name;
                        newForm.description = createForm.description;
                        newForm.priority = createForm.priority;
                        newForm.isSystemRole = createForm.isSystemRole;
                        newForm.isDefault = createForm.isDefault;
                        newForm.parentRoleId = createForm.parentRoleId;
                        newForm.initialPermissions = createForm.initialPermissions || [];
                        newForm.color = '#EF4444'; // Red
                        setCreateForm({
                          name: createForm.name,
                          description: createForm.description,
                          priority: createForm.priority,
                          isSystemRole: createForm.isSystemRole,
                          isDefault: createForm.isDefault,
                          parentRoleId: createForm.parentRoleId,
                          initialPermissions: createForm.initialPermissions || [],
                          color: '#EF4444',
                        });
                      }}
                      className="w-8 h-8 rounded-full bg-red-500 hover:bg-red-600 border-2 border-white/20 hover:border-white/40 transition-colors"
                      title="Red"
                    />
                    <button
                      type="button"
                      onClick={() => {
                        const newForm = new CreateRoleRequest();
                        newForm.name = createForm.name;
                        newForm.description = createForm.description;
                        newForm.priority = createForm.priority;
                        newForm.isSystemRole = createForm.isSystemRole;
                        newForm.isDefault = createForm.isDefault;
                        newForm.parentRoleId = createForm.parentRoleId;
                        newForm.initialPermissions = createForm.initialPermissions || [];
                        newForm.color = '#F59E0B'; // Orange
                        setCreateForm({
                          name: createForm.name,
                          description: createForm.description,
                          priority: createForm.priority,
                          isSystemRole: createForm.isSystemRole,
                          isDefault: createForm.isDefault,
                          parentRoleId: createForm.parentRoleId,
                          initialPermissions: createForm.initialPermissions || [],
                          color: '#F59E0B',
                        });
                      }}
                      className="w-8 h-8 rounded-full bg-orange-500 hover:bg-orange-600 border-2 border-white/20 hover:border-white/40 transition-colors"
                      title="Orange"
                    />
                    <button
                      type="button"
                      onClick={() => {
                        const newForm = new CreateRoleRequest();
                        newForm.name = createForm.name;
                        newForm.description = createForm.description;
                        newForm.priority = createForm.priority;
                        newForm.isSystemRole = createForm.isSystemRole;
                        newForm.isDefault = createForm.isDefault;
                        newForm.parentRoleId = createForm.parentRoleId;
                        newForm.initialPermissions = createForm.initialPermissions || [];
                        newForm.color = '#10B981'; // Green
                        setCreateForm({
                          name: createForm.name,
                          description: createForm.description,
                          priority: createForm.priority,
                          isSystemRole: createForm.isSystemRole,
                          isDefault: createForm.isDefault,
                          parentRoleId: createForm.parentRoleId,
                          initialPermissions: createForm.initialPermissions || [],
                          color: '#10B981',
                        });
                      }}
                      className="w-8 h-8 rounded-full bg-green-500 hover:bg-green-600 border-2 border-white/20 hover:border-white/40 transition-colors"
                      title="Green"
                    />
                    <button
                      type="button"
                      onClick={() => {
                        const newForm = new CreateRoleRequest();
                        newForm.name = createForm.name;
                        newForm.description = createForm.description;
                        newForm.priority = createForm.priority;
                        newForm.isSystemRole = createForm.isSystemRole;
                        newForm.isDefault = createForm.isDefault;
                        newForm.parentRoleId = createForm.parentRoleId;
                        newForm.initialPermissions = createForm.initialPermissions || [];
                        newForm.color = '#3B82F6'; // Blue
                        setCreateForm({
                          name: createForm.name,
                          description: createForm.description,
                          priority: createForm.priority,
                          isSystemRole: createForm.isSystemRole,
                          isDefault: createForm.isDefault,
                          parentRoleId: createForm.parentRoleId,
                          initialPermissions: createForm.initialPermissions || [],
                          color: '#3B82F6',
                        });
                      }}
                      className="w-8 h-8 rounded-full bg-blue-500 hover:bg-blue-600 border-2 border-white/20 hover:border-white/40 transition-colors"
                      title="Blue"
                    />
                    <button
                      type="button"
                      onClick={() => {
                        const newForm = new CreateRoleRequest();
                        newForm.name = createForm.name;
                        newForm.description = createForm.description;
                        newForm.priority = createForm.priority;
                        newForm.isSystemRole = createForm.isSystemRole;
                        newForm.isDefault = createForm.isDefault;
                        newForm.parentRoleId = createForm.parentRoleId;
                        newForm.initialPermissions = createForm.initialPermissions || [];
                        newForm.color = '#8B5CF6'; // Purple
                        setCreateForm({
                          name: createForm.name,
                          description: createForm.description,
                          priority: createForm.priority,
                          isSystemRole: createForm.isSystemRole,
                          isDefault: createForm.isDefault,
                          parentRoleId: createForm.parentRoleId,
                          initialPermissions: createForm.initialPermissions || [],
                          color: '#8B5CF6',
                        });
                      }}
                      className="w-8 h-8 rounded-full bg-purple-500 hover:bg-purple-600 border-2 border-white/20 hover:border-white/40 transition-colors"
                      title="Purple"
                    />
                  </div>
                </div>
                <p className="text-xs text-white/60 mt-1">
                  Choose a color to help identify this role visually
                </p>
              </div>
              
              <div className="flex items-center space-x-4">
                <label className="flex items-center">
                  <input
                    type="checkbox"
                    checked={createForm.isSystemRole}
                    onChange={(e) => {
                      const newForm = new CreateRoleRequest();
                      newForm.name = createForm.name;
                      newForm.description = createForm.description;
                      newForm.priority = createForm.priority;
                      newForm.isSystemRole = e.target.checked;
                      newForm.isDefault = createForm.isDefault;
                      newForm.parentRoleId = createForm.parentRoleId;
                      newForm.initialPermissions = createForm.initialPermissions || [];
                      newForm.color = createForm.color;
                      setCreateForm({
                        name: createForm.name,
                        description: createForm.description,
                        priority: createForm.priority,
                        isSystemRole: e.target.checked,
                        isDefault: createForm.isDefault,
                        parentRoleId: createForm.parentRoleId,
                        initialPermissions: createForm.initialPermissions || [],
                        color: createForm.color,
                      });
                    }}
                    className="mr-2"
                  />
                  <span className="text-sm text-white">System Role</span>
                </label>
                
                <label className="flex items-center">
                  <input
                    type="checkbox"
                    checked={createForm.isDefault}
                    onChange={(e) => {
                      const newForm = new CreateRoleRequest();
                      newForm.name = createForm.name;
                      newForm.description = createForm.description;
                      newForm.priority = createForm.priority;
                      newForm.isSystemRole = createForm.isSystemRole;
                      newForm.isDefault = e.target.checked;
                      newForm.parentRoleId = createForm.parentRoleId;
                      newForm.initialPermissions = createForm.initialPermissions || [];
                      newForm.color = createForm.color;
                      setCreateForm({
                        name: createForm.name,
                        description: createForm.description,
                        priority: createForm.priority,
                        isSystemRole: createForm.isSystemRole,
                        isDefault: e.target.checked,
                        parentRoleId: createForm.parentRoleId,
                        initialPermissions: createForm.initialPermissions || [],
                        color: createForm.color,
                      });
                    }}
                    className="mr-2"
                  />
                  <span className="text-white">Default Role</span>
                </label>
              </div>

              <div>
                <label className="block text-sm font-medium text-white mb-2">Copy Permissions from Existing Role</label>
                <select
                  onChange={(e) => {
                    const selectedRole = allRoles.find(r => r.id === e.target.value);
                    if (selectedRole && selectedRole.permissions) {
                      setCreateForm({
                        name: createForm.name,
                        description: createForm.description,
                        priority: createForm.priority,
                        isSystemRole: createForm.isSystemRole,
                        isDefault: createForm.isDefault,
                        parentRoleId: createForm.parentRoleId,
                        initialPermissions: [...selectedRole.permissions],
                        color: createForm.color,
                      });
                    }
                  }}
                  className="w-full px-3 py-2 bg-gray-700 border border-gray-600 rounded-md text-white focus:outline-none focus:ring-2 focus:ring-blue-500"
                  defaultValue=""
                >
                  <option value="">-- Select a role to copy permissions from --</option>
                  {allRoles.map((role) => (
                    <option key={role.id} value={role.id}>
                      {role.name} ({role.permissions?.length || 0} permissions)
                    </option>
                  ))}
                </select>
                <p className="text-xs text-white/60 mt-1">
                  This will replace any currently selected permissions
                </p>
              </div>
              
              <div>
                <label className="block text-sm font-medium text-white mb-2">Initial Permissions</label>
                  <div className="max-h-96 overflow-y-auto border border-gray-600 rounded-md p-4 bg-gray-900/30">
                    {/* Permissions List - Show when permissions are available */}
                    {permissions.length > 0 ? (
                      <div className="grid grid-cols-1 lg:grid-cols-2 gap-6">
                        {groupPermissionsByCategory(permissions.map(p => p.name || '')).map((category) => (
                          <div key={category.name} className="space-y-3">
                            <div className="flex items-center space-x-2">
                              <div className={`w-3 h-3 rounded-full ${category.color}`}></div>
                              {renderPermissionIcon(category.icon)}
                              <span className="text-sm font-medium text-white/80">{category.name}</span>
                              <span className="text-xs text-white/40">({category.permissions.length})</span>
                            </div>
                            
                            <div className="space-y-2">
                              {category.permissions.map((permissionName) => {
                                const permission = permissions.find(p => p.name === permissionName);
                                const isChecked = (createForm.initialPermissions || []).includes(permissionName);
                                
                                return (
                                  <label key={permissionName} className="flex items-start space-x-2 cursor-pointer group">
                                    <input
                                      type="checkbox"
                                      checked={isChecked}
                                      onChange={(e) => {
                                        const current = createForm.initialPermissions || [];
                                        if (e.target.checked) {
                                          const newForm = new CreateRoleRequest();
                                          newForm.name = createForm.name;
                                          newForm.description = createForm.description;
                                          newForm.priority = createForm.priority;
                                          newForm.isSystemRole = createForm.isSystemRole;
                                          newForm.isDefault = createForm.isDefault;
                                          newForm.parentRoleId = createForm.parentRoleId;
                                          newForm.initialPermissions = [...current, permissionName];
                                          newForm.color = createForm.color;
                                          setCreateForm({
                                            name: createForm.name,
                                            description: createForm.description,
                                            priority: createForm.priority,
                                            isSystemRole: createForm.isSystemRole,
                                            isDefault: createForm.isDefault,
                                            parentRoleId: createForm.parentRoleId,
                                            initialPermissions: [...current, permissionName],
                                            color: createForm.color,
                                          });
                                        } else {
                                          const newForm = new CreateRoleRequest();
                                          newForm.name = createForm.name;
                                          newForm.description = createForm.description;
                                          newForm.priority = createForm.priority;
                                          newForm.isSystemRole = createForm.isSystemRole;
                                          newForm.isDefault = createForm.isDefault;
                                          newForm.parentRoleId = createForm.parentRoleId;
                                          newForm.initialPermissions = current.filter(p => p !== permissionName);
                                          newForm.color = createForm.color;
                                          setCreateForm({
                                            name: createForm.name,
                                            description: createForm.description,
                                            priority: createForm.priority,
                                            isSystemRole: createForm.isSystemRole,
                                            isDefault: createForm.isDefault,
                                            parentRoleId: createForm.parentRoleId,
                                            initialPermissions: current.filter(p => p !== permissionName),
                                            color: createForm.color,
                                          });
                                        }
                                      }}
                                      className="mt-1 mr-2"
                                    />
                                    <div className="flex-1">
                                      <div className="text-sm text-white group-hover:text-blue-300 transition-colors">
                                        {permissionName}
                                      </div>
                                      <div className="text-xs text-white/60 mt-1">
                                        {getPermissionDescription(permissionName)}
                                      </div>
                                    </div>
                                  </label>
                                );
                              })}
                            </div>
                          </div>
                        ))}
                      </div>
                    ) : (
                      <div className="text-center py-8">
                        <SecurityIcon className="w-12 h-12 text-white/40 mx-auto mb-3" />
                        <p className="text-white/60">No permissions available</p>
                        <p className="text-sm text-white/40">Permissions will be loaded from the server</p>
                      </div>
                    )}
                    
                    {/* Quick Selection Controls */}
                    <div className="mt-6 pt-4 border-t border-white/10">
                      <div className="flex flex-wrap gap-2">
                        <button
                          type="button"
                          onClick={() => {
                            const allPermissionNames = permissions.map(p => p.name || '').filter(Boolean);
                            const newForm = new CreateRoleRequest();
                            newForm.name = createForm.name;
                            newForm.description = createForm.description;
                            newForm.priority = createForm.priority;
                            newForm.isSystemRole = createForm.isSystemRole;
                            newForm.isDefault = createForm.isDefault;
                            newForm.parentRoleId = createForm.parentRoleId;
                            newForm.initialPermissions = allPermissionNames;
                            newForm.color = createForm.color;
                            setCreateForm({
                              name: createForm.name,
                              description: createForm.description,
                              priority: createForm.priority,
                              isSystemRole: createForm.isSystemRole,
                              isDefault: createForm.isDefault,
                              parentRoleId: createForm.parentRoleId,
                              initialPermissions: allPermissionNames,
                              color: createForm.color,
                            });
                          }}
                          className="px-3 py-1 text-xs bg-blue-600 hover:bg-blue-700 text-white rounded-md transition-colors"
                        >
                          Select All
                        </button>
                        <button
                          type="button"
                          onClick={() => {
                            const newForm = new CreateRoleRequest();
                            newForm.name = createForm.name;
                            newForm.description = createForm.description;
                            newForm.priority = createForm.priority;
                            newForm.isSystemRole = createForm.isSystemRole;
                            newForm.isDefault = createForm.isDefault;
                            newForm.parentRoleId = createForm.parentRoleId;
                            newForm.initialPermissions = [];
                            newForm.color = createForm.color;
                            setCreateForm({
                              name: createForm.name,
                              description: createForm.description,
                              priority: createForm.priority,
                              isSystemRole: createForm.isSystemRole,
                              isDefault: createForm.isDefault,
                              parentRoleId: createForm.parentRoleId,
                              initialPermissions: [],
                              color: createForm.color,
                            });
                          }}
                          className="px-3 py-1 text-xs bg-gray-600 hover:bg-gray-700 text-white rounded-md transition-colors"
                        >
                          Clear All
                        </button>
                        <button
                          type="button"
                          onClick={() => {
                            // Select only basic user permissions
                            const basicPermissions = permissions
                              .filter(p => p.name && (
                                p.name.startsWith('user.') ||
                                p.name.startsWith('model.view.') ||
                                p.name.startsWith('collection.view.')
                              ))
                              .map(p => p.name || '');
                            const newForm = new CreateRoleRequest();
                            newForm.name = createForm.name;
                            newForm.description = createForm.description;
                            newForm.priority = createForm.priority;
                            newForm.isSystemRole = createForm.isSystemRole;
                            newForm.isDefault = createForm.isDefault;
                            newForm.parentRoleId = createForm.parentRoleId;
                            newForm.initialPermissions = basicPermissions;
                            newForm.color = createForm.color;
                            setCreateForm({
                              name: createForm.name,
                              description: createForm.description,
                              priority: createForm.priority,
                              isSystemRole: createForm.isSystemRole,
                              isDefault: createForm.isDefault,
                              parentRoleId: createForm.parentRoleId,
                              initialPermissions: basicPermissions,
                              color: createForm.color,
                            });
                          }}
                          className="px-3 py-1 text-xs bg-green-600 hover:bg-green-700 text-white rounded-md transition-colors"
                        >
                          Basic User
                        </button>
                        <button
                          type="button"
                          onClick={() => {
                            // Select moderator permissions
                            const moderatorPermissions = permissions
                              .filter(p => p.name && (
                                p.name.startsWith('moderation.') ||
                                p.name.startsWith('comment.edit.any') ||
                                p.name.startsWith('report.handle')
                              ))
                              .map(p => p.name || '');
                            const newForm = new CreateRoleRequest();
                            newForm.name = createForm.name;
                            newForm.description = createForm.description;
                            newForm.priority = createForm.priority;
                            newForm.isSystemRole = createForm.isSystemRole;
                            newForm.isDefault = createForm.isDefault;
                            newForm.parentRoleId = createForm.parentRoleId;
                            newForm.initialPermissions = moderatorPermissions;
                            newForm.color = createForm.color;
                            setCreateForm({
                              name: createForm.name,
                              description: createForm.description,
                              priority: createForm.priority,
                              isSystemRole: createForm.isSystemRole,
                              isDefault: createForm.isDefault,
                              parentRoleId: createForm.parentRoleId,
                              initialPermissions: moderatorPermissions,
                              color: createForm.color,
                            });
                          }}
                          className="px-3 py-1 text-xs bg-yellow-600 hover:bg-yellow-700 text-white rounded-md transition-colors"
                        >
                          Moderator
                        </button>
                      </div>
                      <div className="mt-2 text-xs text-white/40">
                        Selected: {createForm.initialPermissions?.length || 0} permissions
                      </div>
                    </div>
                  </div>
              </div>
            </div>
            
            <div className="flex justify-end space-x-3 mt-6">
              <button
                onClick={() => setIsCreateModalOpen(false)}
                className="lg-button lg-button-secondary"
              >
                <CancelIcon className="w-4 h-4 mr-2" />
                Cancel
              </button>
              <button
                onClick={handleCreateRole}
                disabled={!createForm.name || !createForm.description}
                className="lg-button lg-button-primary"
              >
                <SaveIcon className="w-4 h-4 mr-2" />
                Create Role
              </button>
            </div>
          </div>
        </div>
      )}

      {/* Edit Role Modal */}
      {isEditModalOpen && selectedRole && (
        <div className="fixed inset-0 bg-black/50 flex items-center justify-center z-50">
          <div className="bg-gray-800 rounded-lg p-6 w-full max-w-2xl">
            <div className="flex justify-between items-center mb-4">
              <h3 className="text-lg font-medium text-white">Edit Role: {selectedRole.name}</h3>
              <button
                onClick={() => setIsEditModalOpen(false)}
                className="text-white/60 hover:text-white"
              >
                <CloseIcon />
              </button>
            </div>
            
            {!selectedRole.canBeDeleted && (
              <div className="mb-4 p-3 bg-blue-900/20 border border-blue-500/30 rounded-md">
                <div className="flex items-center text-blue-300">
                  <ShieldIcon className="w-4 h-4 mr-2" />
                  <span className="text-sm font-medium">Admin Role - Limited Editing</span>
                </div>
                <p className="text-xs text-blue-200/80 mt-1">
                  This is an admin role. You can modify the name, description, priority, and color, but permissions cannot be changed.
                </p>
              </div>
            )}
            
            <div className="space-y-4">
              <div>
                <label className="block text-sm font-medium text-white mb-2">Name</label>
                <input
                  type="text"
                  value={editForm.name}
                  onChange={(e) => { editForm.name = e.target.value; setEditForm(editForm); }}
                  className="w-full px-3 py-2 bg-gray-700 border border-gray-600 rounded-md text-white focus:outline-none focus:ring-2 focus:ring-blue-500"
                />
              </div>
              
              <div>
                <label className="block text-sm font-medium text-white mb-2">Description</label>
                <textarea
                  value={editForm.description}
                  onChange={(e) => { editForm.description = e.target.value; setEditForm(editForm); }}
                  className="w-full px-3 py-2 bg-gray-700 border border-gray-600 rounded-md text-white focus:outline-none focus:ring-2 focus:ring-blue-500"
                  rows={3}
                />
              </div>
              
              <div>
                <label className="block text-sm font-medium text-white mb-2">Priority</label>
                <input
                  type="number"
                  value={editForm.priority}
                  onChange={(e) => { editForm.priority = parseInt(e.target.value) || 100; setEditForm(editForm); }}
                  className="w-full px-3 py-2 bg-gray-700 border border-gray-600 rounded-md text-white focus:outline-none focus:ring-2 focus:ring-blue-500"
                  min="1"
                  max="999"
                />
              </div>

              <div>
                <label className="block text-sm font-medium text-white mb-2">Role Color</label>
                <div className="flex items-center space-x-3">
                  <div className="relative">
                    <input
                      type="color"
                      value={editFormColor}
                      onChange={(e) => setEditFormColor(e.target.value)}
                      className="w-12 h-12 rounded-lg border-2 border-gray-600 cursor-pointer bg-transparent"
                      title="Choose role color"
                    />
                  </div>
                  <div className="flex-1">
                    <input
                      type="text"
                      value={editFormColor}
                      onChange={(e) => setEditFormColor(e.target.value)}
                      placeholder="#3B82F6"
                      className="w-full px-3 py-2 bg-gray-700 border border-gray-600 rounded-md text-white focus:outline-none focus:ring-2 focus:ring-blue-500 font-mono text-sm"
                    />
                  </div>
                  <div className="flex space-x-2">
                    <button
                      type="button"
                      onClick={() => setEditFormColor('#EF4444')}
                      className="w-8 h-8 rounded-full bg-red-500 hover:bg-red-600 border-2 border-white/20 hover:border-white/40 transition-colors"
                      title="Red"
                    />
                    <button
                      type="button"
                      onClick={() => setEditFormColor('#F59E0B')}
                      className="w-8 h-8 rounded-full bg-orange-500 hover:bg-orange-600 border-2 border-white/20 hover:border-white/40 transition-colors"
                      title="Orange"
                    />
                    <button
                      type="button"
                      onClick={() => setEditFormColor('#10B981')}
                      className="w-8 h-8 rounded-full bg-green-500 hover:bg-green-600 border-2 border-white/20 hover:border-white/40 transition-colors"
                      title="Green"
                    />
                    <button
                      type="button"
                      onClick={() => setEditFormColor('#3B82F6')}
                      className="w-8 h-8 rounded-full bg-blue-500 hover:bg-blue-600 border-2 border-white/20 hover:border-white/40 transition-colors"
                      title="Blue"
                    />
                    <button
                      type="button"
                      onClick={() => setEditFormColor('#8B5CF6')}
                      className="w-8 h-8 rounded-full bg-purple-500 hover:bg-purple-600 border-2 border-white/20 hover:border-white/40 transition-colors"
                      title="Purple"
                    />
                  </div>
                </div>
                <p className="text-xs text-white/60 mt-1">
                  Choose a color to help identify this role visually
                </p>
              </div>
              
              <div className="flex items-center space-x-4">
                <label className="flex items-center">
                  <input
                    type="checkbox"
                    checked={editForm.isDefault}
                    onChange={(e) => { editForm.isDefault = e.target.checked; setEditForm(editForm); }}
                    className="mr-2"
                  />
                  <span className="text-sm text-white">Default Role</span>
                </label>
                
                <label className="flex items-center">
                  <input
                    type="checkbox"
                    checked={editForm.isActive}
                    onChange={(e) => { editForm.isActive = e.target.checked; setEditForm(editForm); }}
                    className="mr-2"
                  />
                  <span className="text-sm text-white">Active</span>
                </label>
              </div>

              {selectedRole.canBeDeleted && (
                <div>
                  <label className="block text-sm font-medium text-white mb-2">Permissions</label>
                  <div className="p-3 bg-gray-900/30 border border-gray-600 rounded-md">
                    <p className="text-sm text-white/60 mb-2">
                      Current permissions: {selectedRole.permissions?.length || 0}
                    </p>
                    <p className="text-xs text-white/40">
                      To modify permissions, use the permissions management section or create a new role with the desired permissions.
                    </p>
                  </div>
                </div>
              )}

              {!selectedRole.canBeDeleted && (
                <div>
                  <label className="block text-sm font-medium text-white mb-2">Permissions</label>
                  <div className="p-3 bg-blue-900/20 border border-blue-500/30 rounded-md">
                    <div className="flex items-center text-blue-300 mb-2">
                      <ShieldIcon className="w-4 h-4 mr-2" />
                      <span className="text-sm font-medium">Admin Role Permissions</span>
                    </div>
                    <p className="text-sm text-blue-200/80 mb-2">
                      Current permissions: {selectedRole.permissions?.length || 0}
                    </p>
                    <p className="text-xs text-blue-200/60">
                      Admin role permissions are protected and cannot be modified. This ensures system security and stability.
                    </p>
                  </div>
                </div>
              )}
            </div>
            
            <div className="flex justify-end space-x-3 mt-6">
              <button
                onClick={() => setIsEditModalOpen(false)}
                className="lg-button lg-button-secondary"
              >
                <CancelIcon className="w-4 h-4 mr-2" />
                Cancel
              </button>
              <button
                onClick={handleUpdateRole}
                className="lg-button lg-button-primary"
              >
                <SaveIcon className="w-4 h-4 mr-2" />
                Update Role
              </button>
            </div>
          </div>
        </div>
      )}

      {/* View Role Modal */}
      {isViewModalOpen && selectedRole && (
        <div className="fixed inset-0 bg-black/50 flex items-center justify-center z-50">
          <div className="bg-gray-800 rounded-lg p-6 w-full max-w-2xl max-h-[90vh] overflow-y-auto">
            <div className="flex justify-between items-center mb-4">
              <h3 className="text-lg font-medium text-white">Role Details: {selectedRole.name}</h3>
              <button
                onClick={() => setIsViewModalOpen(false)}
                className="text-white/60 hover:text-white"
              >
                <CloseIcon />
              </button>
            </div>
            
            <div className="space-y-4">
              <div className="grid grid-cols-2 gap-4">
                <div>
                  <label className="block text-sm font-medium text-white/60 mb-1">Name</label>
                  <p className="text-white">{selectedRole.name}</p>
                </div>
                <div>
                  <label className="block text-sm font-medium text-white/60 mb-1">Priority</label>
                  <p className="text-white">{selectedRole.priority}</p>
                </div>
                <div>
                  <label className="block text-sm font-medium text-white/60 mb-1">System Role</label>
                  <p className="text-white">{selectedRole.isSystemRole ? 'Yes' : 'No'}</p>
                </div>
                <div>
                  <label className="block text-sm font-medium text-white/60 mb-1">Default Role</label>
                  <p className="text-white">{selectedRole.isDefault ? 'Yes' : 'No'}</p>
                </div>
                <div>
                  <label className="block text-sm font-medium text-white/60 mb-1">Can Be Deleted</label>
                  <p className="text-white">{selectedRole.canBeDeleted ? 'Yes' : 'No'}</p>
                </div>
                <div>
                  <label className="block text-sm font-medium text-white/60 mb-1">Active</label>
                  <p className="text-white">{selectedRole.isActive ? 'Yes' : 'No'}</p>
                </div>
              </div>
              
              <div>
                <label className="block text-sm font-medium text-white/60 mb-1">Description</label>
                <p className="text-white">{selectedRole.description}</p>
              </div>
              
              {selectedRole.permissions && selectedRole.permissions.length > 0 && (
                <div>
                  <label className="block text-sm font-medium text-white/60 mb-2">Permissions ({selectedRole.permissions.length})</label>
                  <div className="grid grid-cols-1 lg:grid-cols-2 gap-4">
                    {groupPermissionsByCategory(selectedRole.permissions).map((category) => (
                      <div key={category.name} className="space-y-2">
                        <div className="flex items-center space-x-2">
                          <div className={`w-3 h-3 rounded-full ${category.color}`}></div>
                          {renderPermissionIcon(category.icon)}
                          <span className="text-sm font-medium text-white/80">{category.name}</span>
                          <span className="text-xs text-white/40">({category.permissions.length})</span>
                        </div>
                        
                        <div className="space-y-1">
                          {category.permissions.map((permission) => (
                            <div
                              key={permission}
                              className="text-xs bg-white/10 px-2 py-1 rounded"
                            >
                              <div className="font-medium text-white/80">{permission}</div>
                              <div className="text-white/60 text-xs mt-1">
                                {getPermissionDescription(permission)}
                              </div>
                            </div>
                          ))}
                        </div>
                      </div>
                    ))}
                  </div>
                </div>
              )}
            </div>
            
            <div className="flex justify-end mt-6">
              <button
                onClick={() => setIsViewModalOpen(false)}
                className="lg-button lg-button-secondary"
              >
                Close
              </button>
            </div>
          </div>
        </div>
      )}

      {/* Snackbar */}
      {snackbar.open && (
        <div className="fixed bottom-4 right-4 z-50">
          <div className={`lg-badge p-4 ${
            snackbar.severity === 'error' ? 'lg-badge-error' :
            snackbar.severity === 'warning' ? 'lg-badge-warning' :
            snackbar.severity === 'info' ? 'lg-badge-info' :
            'lg-badge-success'
          }`}>
            <span className="text-white">{snackbar.message}</span>
            <button
              onClick={() => setSnackbar({ ...snackbar, open: false })}
              className="ml-3 text-white/80 hover:text-white"
            >
              ×
            </button>
          </div>
        </div>
      )}
    </div>
  );
};

export default RoleManagement; 