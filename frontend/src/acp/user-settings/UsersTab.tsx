import React, { useState, useEffect } from 'react';
import api from '../../utils/axiosConfig';
import { GetUsersClient } from '../../services/api.client';
import { defaultAxiosClient } from '../../api/axiosAdapter';
import {
  Add as AddIcon,
  Visibility,
  VisibilityOff,
} from '@mui/icons-material';

const UsersTab: React.FC = () => {
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
    password: '',
    role: 'User'
  });

  // Real API data state
  const [users, setUsers] = useState<any[]>([]);
  const [loading, setLoading] = useState(true);
  const [error, setError] = useState<string | null>(null);
  const [pagination, setPagination] = useState({
    page: 1,
    pageSize: 20,
    totalCount: 0,
    totalPages: 0
  });
  const [filters, setFilters] = useState({
    roleFilter: 'All Roles',
    statusFilter: 'All Status',
    sortBy: 'CreatedAt',
    sortDescending: true
  });

  const handleUserClick = (userEmail: string) => {
    setSelectedUser(userEmail);
  };

  const handleUserAction = (action: string, userEmail: string) => {
    console.log(`${action} user: ${userEmail}`);
    setSnackbar({ open: true, message: `${action} action performed on ${userEmail}`, severity: 'success' });
  };

  const handleCreateUser = async () => {
    setIsCreatingUser(true);
    // Simulate API call
    await new Promise(resolve => setTimeout(resolve, 1000));
    setIsCreatingUser(false);
    setCreateUserOpen(false);
    setSnackbar({ open: true, message: 'User created successfully', severity: 'success' });
    // Reset form
    setNewUserForm({ email: '', username: '', password: '', role: 'User' });
  };

  const handleFormChange = (field: string, value: string) => {
    setNewUserForm(prev => ({ ...prev, [field]: value }));
  };

  const handleCloseCreateDialog = () => {
    setCreateUserOpen(false);
    setNewUserForm({ email: '', username: '', password: '', role: 'User' });
  };

  const copyPasswordToClipboard = () => {
    navigator.clipboard.writeText(newUserForm.password);
    setSnackbar({ open: true, message: 'Password copied to clipboard', severity: 'success' });
  };

  // Fetch users from API
  const fetchUsers = async () => {
    try {
      setLoading(true);
      setError(null);
      
      const client = new GetUsersClient(undefined, defaultAxiosClient);
      const response = await client.getUsers(
        pagination.page,
        pagination.pageSize,
        searchQuery || undefined,
        filters.roleFilter === 'All Roles' ? undefined : filters.roleFilter,
        filters.statusFilter === 'All Status' ? undefined : filters.statusFilter,
        filters.sortBy,
        filters.sortDescending
      );
      
      setUsers(response.users || []);
      setPagination(prev => ({
        ...prev,
        totalCount: response.totalCount || 0,
        totalPages: response.totalPages || 0
      }));
      
      if (response.users && response.users.length > 0) {
        setSnackbar({ open: true, message: `Loaded ${response.users.length} users`, severity: 'success' });
      }
    } catch (err: any) {
      console.error('Failed to fetch users:', err);
      setError(err.message || 'Failed to fetch users');
      setSnackbar({ open: true, message: 'Failed to fetch users', severity: 'error' });
    } finally {
      setLoading(false);
    }
  };

  // Fetch users when component mounts or filters change
  useEffect(() => {
    fetchUsers();
  }, [pagination.page, pagination.pageSize, filters.roleFilter, filters.statusFilter, filters.sortBy, filters.sortDescending]);

  // Debounced search effect
  useEffect(() => {
    const timer = setTimeout(() => {
      if (searchQuery !== '') {
        setPagination(prev => ({ ...prev, page: 1 })); // Reset to first page when searching
        fetchUsers();
      }
    }, 500);

    return () => clearTimeout(timer);
  }, [searchQuery]);

  return (
    <div className="space-y-8">
      {/* Header Section */}
      <div className="bg-gray-800 rounded-lg p-6 border border-gray-700">
        <div className="flex justify-between items-center">
          <div>
            <h2 className="text-3xl font-bold text-white mb-2">User Management</h2>
            <p className="text-gray-300">Manage system users, roles, and permissions</p>
          </div>
          <div className="flex space-x-3">
            <button
              onClick={fetchUsers}
              disabled={loading}
              className="lg-button lg-button-secondary"
            >
              {loading ? 'Loading...' : 'Refresh'}
            </button>
            <button
              onClick={() => setCreateUserOpen(true)}
              className="lg-button lg-button-primary"
            >
              <AddIcon className="w-5 h-5" />
              Create User
            </button>
          </div>
        </div>
      </div>

      {/* Search and Filters */}
      <div className="bg-gray-800 rounded-lg p-6 border border-gray-700">
        <div className="flex gap-4">
          <div className="flex-1 relative">
            <input
              type="text"
              placeholder="Search users by username, email, first name, or last name..."
              value={searchQuery}
              onChange={(e) => setSearchQuery(e.target.value)}
              className="lg-input pr-10"
            />
            {searchQuery && (
              <button
                onClick={() => setSearchQuery('')}
                className="absolute right-3 top-1/2 transform -translate-y-1/2 text-white/60 hover:text-white"
              >
                ×
              </button>
            )}
          </div>
          <select 
            className="lg-input w-48"
            value={filters.roleFilter}
            onChange={(e) => setFilters(prev => ({ ...prev, roleFilter: e.target.value }))}
          >
            <option>All Roles</option>
            <option>Admin</option>
            <option>Moderator</option>
            <option>User</option>
          </select>
          <select 
            className="lg-input w-48"
            value={filters.statusFilter}
            onChange={(e) => setFilters(prev => ({ ...prev, statusFilter: e.target.value }))}
          >
            <option>All Status</option>
            <option>Active</option>
            <option>Banned</option>
            <option>Inactive</option>
          </select>
          <select 
            className="lg-input w-48"
            value={filters.sortBy}
            onChange={(e) => setFilters(prev => ({ ...prev, sortBy: e.target.value }))}
          >
            <option value="CreatedAt">Created Date</option>
            <option value="Username">Username</option>
            <option value="Email">Email</option>
            <option value="Role">Role</option>
            <option value="LastLogin">Last Login</option>
          </select>
          <button
            onClick={() => setFilters(prev => ({ ...prev, sortDescending: !prev.sortDescending }))}
            className="lg-button lg-button-secondary w-12"
            title={filters.sortDescending ? 'Sort Descending' : 'Sort Ascending'}
          >
            {filters.sortDescending ? '↓' : '↑'}
          </button>
        </div>
      </div>

      {/* Loading and Error States */}
      {loading && (
        <div className="lg-card p-12 text-center border border-white/10">
          <div className="lg-spinner h-12 w-12 mx-auto mb-6"></div>
          <div className="text-2xl text-white/60">Loading users...</div>
        </div>
      )}

      {error && (
        <div className="lg-card p-6 border border-red-500/30">
          <div className="text-red-400 text-center text-lg">{error}</div>
          <button 
            onClick={fetchUsers}
            className="mt-4 lg-button lg-button-secondary mx-auto block"
          >
            Retry
          </button>
        </div>
      )}

      {/* Users Table */}
      {!loading && !error && (
        <div className="lg-card border border-white/10 overflow-hidden">
          <div className="overflow-x-auto">
            <table className="lg-table w-full">
              <thead>
                <tr className="border-b border-white/10">
                  <th className="px-6 py-4 text-left text-xs font-medium text-white/80 uppercase tracking-wider">User</th>
                  <th className="px-6 py-4 text-left text-xs font-medium text-white/80 uppercase tracking-wider">Role</th>
                  <th className="px-6 py-4 text-left text-xs font-medium text-white/80 uppercase tracking-wider">Status</th>
                  <th className="px-6 py-4 text-left text-xs font-medium text-white/80 uppercase tracking-wider">Last Login</th>
                  <th className="px-6 py-4 text-left text-xs font-medium text-white/80 uppercase tracking-wider">Created</th>
                  <th className="px-6 py-4 text-left text-xs font-medium text-white/80 uppercase tracking-wider">Actions</th>
                </tr>
              </thead>
              <tbody>
                {users.length === 0 ? (
                  <tr>
                    <td colSpan={6} className="px-6 py-12 text-center text-white/60 text-lg">
                      No users found
                    </td>
                  </tr>
                ) : (
                  users.map((user) => (
                    <tr key={user.id} className="cursor-pointer hover:bg-white/5 border-b border-white/5 transition-colors duration-150">
                      <td className="px-6 py-4">
                        <div className="flex items-center space-x-3">
                          <div className="w-10 h-10 bg-indigo-500 rounded-full flex items-center justify-center text-white text-sm font-medium">
                            {user.username?.charAt(0)?.toUpperCase() || '?'}
                          </div>
                          <div>
                            <div className="font-medium text-white text-lg">{user.username || 'Unknown'}</div>
                            <div className="text-sm text-white/60">{user.email || 'No email'}</div>
                          </div>
                        </div>
                      </td>
                      <td className="px-6 py-4">
                        <span className={`lg-badge ${
                          user.roleName === 'Admin' ? 'lg-badge-error' :
                          user.roleName === 'Moderator' ? 'lg-badge-warning' :
                          'lg-badge-info'
                        }`}>
                          {user.roleName || 'No Role'}
                        </span>
                      </td>
                      <td className="px-6 py-4">
                        <span className={`lg-badge ${
                          user.isBanned ? 'lg-badge-error' : 'lg-badge-success'
                        }`}>
                          {user.isBanned ? 'Banned' : 'Active'}
                        </span>
                      </td>
                      <td className="px-6 py-4 text-white/60">
                        {user.lastLoginAt ? new Date(user.lastLoginAt).toLocaleDateString() : 'Never'}
                      </td>
                      <td className="px-6 py-4 text-white/60">
                        {user.createdAt ? new Date(user.createdAt).toLocaleDateString() : 'Unknown'}
                      </td>
                      <td className="px-6 py-4">
                        <div className="flex space-x-2">
                          <button
                            onClick={() => handleUserAction('Edit', user.email || user.id)}
                            className="lg-button text-sm"
                          >
                            Edit
                          </button>
                          <button
                            onClick={() => handleUserAction('Ban', user.email || user.id)}
                            className="lg-button lg-button-secondary text-sm"
                          >
                            {user.isBanned ? 'Unban' : 'Ban'}
                          </button>
                        </div>
                      </td>
                    </tr>
                  ))
                )}
              </tbody>
            </table>
          </div>
          
          {/* Pagination */}
          {pagination.totalPages > 1 && (
            <div className="px-6 py-4 border-t border-gray-700 bg-gray-750">
              <div className="flex justify-between items-center">
                <div className="text-gray-300 text-sm">
                  Showing {((pagination.page - 1) * pagination.pageSize) + 1} to {Math.min(pagination.page * pagination.pageSize, pagination.totalCount)} of {pagination.totalCount} users
                </div>
                <div className="flex items-center space-x-6">
                  <div className="flex items-center space-x-3">
                    <span className="text-gray-300 text-sm font-medium">Page size:</span>
                    <select
                      value={pagination.pageSize}
                      onChange={(e) => {
                        setPagination(prev => ({ ...prev, pageSize: parseInt(e.target.value), page: 1 }));
                      }}
                      className="lg-input w-24 text-sm bg-gray-700 border-gray-600 text-white"
                    >
                      <option value={10}>10</option>
                      <option value={20}>20</option>
                      <option value={50}>50</option>
                      <option value={100}>100</option>
                    </select>
                  </div>
                  <div className="flex items-center space-x-3">
                    <button
                      onClick={() => setPagination(prev => ({ ...prev, page: prev.page - 1 }))}
                      disabled={pagination.page <= 1}
                      className="lg-button disabled:opacity-50"
                    >
                      Previous
                    </button>
                    <span className="text-gray-300 px-4 py-2 font-medium">
                      Page {pagination.page} of {pagination.totalPages}
                    </span>
                    <button
                      onClick={() => setPagination(prev => ({ ...prev, page: prev.page + 1 }))}
                      disabled={pagination.page >= pagination.totalPages}
                      className="lg-button disabled:opacity-50"
                    >
                      Next
                    </button>
                  </div>
                </div>
              </div>
            </div>
          )}
        </div>
      )}

      {/* Create User Dialog */}
      {createUserOpen && (
        <div className="lg-modal-overlay">
          <div className="lg-modal p-6 max-w-md w-full">
            <h3 className="text-xl font-bold text-white mb-4">Create New User</h3>
            
            <div className="space-y-4">
              <div>
                <label className="block text-sm font-medium text-white/80 mb-2">Email</label>
                <input
                  type="email"
                  value={newUserForm.email}
                  onChange={(e) => handleFormChange('email', e.target.value)}
                  className="lg-input"
                  placeholder="user@example.com"
                />
              </div>
              
              <div>
                <label className="block text-sm font-medium text-white/80 mb-2">Username</label>
                <input
                  type="text"
                  value={newUserForm.username}
                  onChange={(e) => handleFormChange('username', e.target.value)}
                  className="lg-input"
                  placeholder="username"
                />
              </div>
              
              <div>
                <label className="block text-sm font-medium text-white/80 mb-2">Password</label>
                <div className="relative">
                  <input
                    type={showPassword ? 'text' : 'password'}
                    value={newUserForm.password}
                    onChange={(e) => handleFormChange('password', e.target.value)}
                    className="lg-input pr-12"
                    placeholder="Generate password"
                  />
                  <button
                    type="button"
                    onClick={() => setShowPassword(!showPassword)}
                    className="absolute right-2 top-1/2 transform -translate-y-1/2 text-white/60 hover:text-white"
                  >
                    {showPassword ? <VisibilityOff className="w-5 h-5" /> : <Visibility className="w-5 h-5" />}
                  </button>
                </div>
                <button
                  onClick={() => handleFormChange('password', Math.random().toString(36).slice(-8))}
                  className="text-sm text-indigo-400 hover:text-indigo-300 mt-1"
                >
                  Generate Password
                </button>
              </div>
              
              <div>
                <label className="block text-sm font-medium text-white/80 mb-2">Role</label>
                <select
                  value={newUserForm.role}
                  onChange={(e) => handleFormChange('role', e.target.value)}
                  className="lg-input"
                >
                  <option value="User">User</option>
                  <option value="Moderator">Moderator</option>
                  <option value="Admin">Admin</option>
                </select>
              </div>
            </div>
            
            <div className="flex justify-end space-x-3 mt-6">
              <button
                onClick={handleCloseCreateDialog}
                className="lg-button"
              >
                Cancel
              </button>
              <button
                onClick={handleCreateUser}
                disabled={isCreatingUser}
                className="lg-button lg-button-primary"
              >
                {isCreatingUser ? 'Creating...' : 'Create User'}
              </button>
            </div>
          </div>
        </div>
      )}

      {/* Snackbar */}
      {snackbar.open && (
        <div className={`fixed bottom-4 right-4 p-4 rounded-lg shadow-lg z-50 ${
          snackbar.severity === 'success' ? 'bg-green-600' : 'bg-red-600'
        } text-white`}>
          <div className="flex items-center space-x-2">
            <span>{snackbar.message}</span>
            <button
              onClick={() => setSnackbar(prev => ({ ...prev, open: false }))}
              className="ml-2 text-white/80 hover:text-white"
            >
              ×
            </button>
          </div>
        </div>
      )}
    </div>
  );
};

export default UsersTab;
