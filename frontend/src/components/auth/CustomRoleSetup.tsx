import React, { useState, useEffect } from 'react';
import { useNavigate } from 'react-router-dom';
import { useAppDispatch, useAppSelector } from '../../utils/hooks';
import { getAllRoles, createRole, reset } from '../../store/slices/roleSlice';
import { setRoleConfigured, getSetupStatus } from '../../store/slices/authSlice';
import { PlusIcon, XMarkIcon } from '@heroicons/react/24/outline';
import { RoleDto, CreateRoleRequest } from '../../services/roleService';

interface Role {
  name: string;
  description: string;
}

interface RoleFormErrors {
  name?: string;
  description?: string;
}

const CustomRoleSetup: React.FC = () => {
  // Default suggested roles
  const [suggestedRoles, setSuggestedRoles] = useState<Role[]>([
    { name: 'Editor', description: 'Can edit and publish content' },
    { name: 'Viewer', description: 'Can view content only' }
  ]);
  // Custom roles added by the user during this session
  const [customRoles, setCustomRoles] = useState<Role[]>([]);
  // Current role being created
  const [currentRole, setCurrentRole] = useState<Role>({ name: '', description: '' });
  const [errors, setErrors] = useState<RoleFormErrors>({});
  const [isSubmitting, setIsSubmitting] = useState(false);
  // Track whether we've loaded existing roles
  const [existingRolesLoaded, setExistingRolesLoaded] = useState(false);

  const navigate = useNavigate();
  const dispatch = useAppDispatch();
  const { roles, isLoading, isSuccess, isError, errorMessage } = useAppSelector(state => state.roles);
  const { user, setupStatus } = useAppSelector(state => state.auth);

  // Check if setup status is available, if not fetch it
  useEffect(() => {
    if (!setupStatus && user) {
      dispatch(getSetupStatus());
    }
  }, [setupStatus, user, dispatch]);

  // Check if user is authenticated and redirect if needed
  useEffect(() => {
    if (!user) {
      navigate('/login');
    } else if (setupStatus) {
      // If admin isn't configured, redirect to admin setup
      if (!setupStatus.isAdminConfigured) {
        navigate('/admin-setup');
      } 
      // If roles are already configured, redirect to dashboard
      else if (setupStatus.isRoleConfigured) {
        navigate('/dashboard');
      }
    }
  }, [user, setupStatus, navigate]);

  // Fetch existing roles when component mounts
  useEffect(() => {
    if (user && user.accessToken) {
      // Ensure we have a valid auth token before making the API call
      console.log('Fetching roles with token:', user.accessToken);
      dispatch(getAllRoles());
    }
  }, [dispatch, user]);

  // Add error handling UI
  useEffect(() => {
    if (isError) {
      console.error('Error fetching or creating roles:', errorMessage);
    }
  }, [isError, errorMessage]);

  // Process loaded roles to avoid duplicates
  useEffect(() => {
    if (roles.length > 0 && !existingRolesLoaded) {
      // Extract existing system and custom roles
      const existingRoleNames = roles.map((role: RoleDto) => role.name.toLowerCase());
      
      // Filter out suggested roles that already exist
      const filteredSuggestedRoles = suggestedRoles.filter(
        role => !existingRoleNames.includes(role.name.toLowerCase())
      );
      
      setSuggestedRoles(filteredSuggestedRoles);
      setExistingRolesLoaded(true);
    }
  }, [roles, existingRolesLoaded, suggestedRoles]);

  useEffect(() => {
    return () => {
      dispatch(reset());
    };
  }, [dispatch]);

  const validateRole = (role: Role): RoleFormErrors => {
    const newErrors: RoleFormErrors = {};
    
    if (!role.name.trim()) {
      newErrors.name = 'Role name is required';
    } else if (role.name.length < 3) {
      newErrors.name = 'Role name must be at least 3 characters';
    } else if (role.name.length > 50) {
      newErrors.name = 'Role name must be less than 50 characters';
    } else {
      // Check for duplicates in existing roles
      const existingNames = [
        ...roles.map((r: RoleDto) => r.name.toLowerCase()),
        ...customRoles.map(r => r.name.toLowerCase()),
        ...suggestedRoles.map(r => r.name.toLowerCase())
      ];
      
      if (existingNames.includes(role.name.toLowerCase())) {
        newErrors.name = 'A role with this name already exists';
      }
    }
    
    if (role.description && role.description.length > 255) {
      newErrors.description = 'Description must be less than 255 characters';
    }
    
    return newErrors;
  };

  const handleAddRole = () => {
    const validationErrors = validateRole(currentRole);
    
    if (Object.keys(validationErrors).length === 0) {
      setCustomRoles([...customRoles, { ...currentRole }]);
      setCurrentRole({ name: '', description: '' });
      setErrors({});
    } else {
      setErrors(validationErrors);
    }
  };

  const handleRemoveSuggestedRole = (index: number) => {
    const updatedRoles = [...suggestedRoles];
    updatedRoles.splice(index, 1);
    setSuggestedRoles(updatedRoles);
  };

  const handleRemoveCustomRole = (index: number) => {
    const updatedRoles = [...customRoles];
    updatedRoles.splice(index, 1);
    setCustomRoles(updatedRoles);
  };

  const handleInputChange = (e: React.ChangeEvent<HTMLInputElement | HTMLTextAreaElement>) => {
    const { name, value } = e.target;
    setCurrentRole({
      ...currentRole,
      [name]: value
    });
    
    // Clear error when user types
    if (errors[name as keyof RoleFormErrors]) {
      setErrors({
        ...errors,
        [name]: undefined
      });
    }
  };

  const handleSubmit = async (e: React.FormEvent) => {
    e.preventDefault();
    setIsSubmitting(true);
    
    try {
      // Create suggested roles
      if (suggestedRoles.length > 0) {
        console.log('Creating suggested roles:', suggestedRoles);
        // @ts-ignore - Role type is inferred from suggestedRoles array
        for (const role of suggestedRoles) {
          await dispatch(createRole(role)).unwrap();
        }
      }
      
      // Create custom roles
      if (customRoles.length > 0) {
        console.log('Creating custom roles:', customRoles);
        // @ts-ignore - Role type is inferred from customRoles array
        for (const role of customRoles) {
          await dispatch(createRole(role)).unwrap();
        }
      }
      
      // Mark role setup as configured
      console.log('Marking role setup as configured');
      await dispatch(setRoleConfigured(true)).unwrap();
      console.log('Role setup marked as configured successfully');
      
      // All roles created successfully, navigate to model upload settings page
      navigate('/model-upload-settings');
    } catch (error: any) {
      console.error('Error creating roles:', error);
      // If no roles were created, show a generic error message
      if (typeof error === 'string') {
        // Error message is already a string
      } else if (error.message) {
        console.error('Error message:', error.message);
      }
    } finally {
      setIsSubmitting(false);
    }
  };

  const handleSkip = () => {
    setIsSubmitting(true);
    // Even if we skip, still mark roles as configured
    console.log('Skipping role creation, marking as configured');
    dispatch(setRoleConfigured(true))
      .unwrap()
      .then(() => {
        console.log('Role setup marked as configured (skipped)');
        navigate('/model-upload-settings');
      })
      .catch((err: any) => {
        console.error('Failed to mark roles as configured:', err);
        // Try to navigate to model upload settings anyway
        navigate('/model-upload-settings');
      })
      .finally(() => {
        setIsSubmitting(false);
      });
  };

  // Add a retry function for role fetching
  const handleRetryFetchRoles = () => {
    if (user && user.accessToken) {
      dispatch(getAllRoles());
    } else {
      // If we don't have a token, redirect to login
      navigate('/login');
    }
  };

  // Combine all roles for display - separate system roles from custom roles
  // @ts-ignore - RoleDto type is inferred from roles array
  const systemRoles = roles.filter(role => role.isSystemRole);
  // @ts-ignore - RoleDto type is inferred from roles array
  const existingCustomRoles = roles.filter(role => !role.isSystemRole);
  const pendingRoles = [...suggestedRoles, ...customRoles];

  // Cards to display pending roles
  const pendingRoleCards = pendingRoles.map((role: Role, index: number) => (
    <div key={index} className="border rounded p-3 mb-2">
      <h5>{role.name}</h5>
      <p className="text-muted">{role.description || 'No description'}</p>
      <button
        onClick={() => handleRemoveCustomRole(index)}
        className="btn btn-sm btn-outline-danger"
      >
        Remove
      </button>
    </div>
  ));

  // Cards to display existing system roles
  const systemRoleCards = systemRoles.map((role: RoleDto, index: number) => (
    <div key={index} className="border rounded p-3 mb-2">
      <h5>{role.name}</h5>
      <p className="text-muted">{role.description || 'No description'}</p>
    </div>
  ));

  // Cards to display existing custom roles
  const existingCustomRoleCards = existingCustomRoles.map((role: RoleDto, index: number) => (
    <div key={index} className="border rounded p-3 mb-2">
      <h5>{role.name}</h5>
      <p className="text-muted">{role.description || 'No description'}</p>
    </div>
  ));

  return (
    <div className="flex flex-col items-center justify-center min-h-screen bg-gray-50 px-4">
      <div className="w-full max-w-2xl">
        <div className="text-center mb-10">
          <h1 className="text-3xl font-bold text-gray-900">Create Custom Roles</h1>
          <p className="mt-2 text-lg text-gray-600">
            Define custom roles for your organization members
          </p>
        </div>

        {isError && (
          <div className="bg-red-100 border border-red-400 text-red-700 px-4 py-3 rounded relative mb-6">
            <strong className="font-bold">Error: </strong>
            <span className="block sm:inline">{errorMessage || 'Failed to fetch roles'}</span>
            <div className="mt-2">
              <button
                onClick={handleRetryFetchRoles}
                className="bg-red-500 hover:bg-red-700 text-white font-bold py-2 px-4 rounded"
              >
                Retry
              </button>
            </div>
          </div>
        )}

        <div className="bg-white shadow-md rounded-lg p-6 mb-6">
          {/* Existing System Roles Section */}
          {systemRoles.length > 0 && (
            <div className="mb-6">
              <h2 className="text-xl font-semibold text-gray-800 mb-4">System Roles</h2>
              <ul className="space-y-3">
                {systemRoleCards}
              </ul>
            </div>
          )}

          {/* Existing Custom Roles Section */}
          {existingCustomRoles.length > 0 && (
            <div className="mb-6">
              <h2 className="text-xl font-semibold text-gray-800 mb-4">Existing Custom Roles</h2>
              <ul className="space-y-3">
                {existingCustomRoleCards}
              </ul>
            </div>
          )}

          {/* Roles to add section */}
          <div className="mb-6">
            <h2 className="text-xl font-semibold text-gray-800 mb-4">Roles to Add</h2>
            
            {pendingRoles.length === 0 ? (
              <p className="text-gray-500 italic">No roles pending to be added</p>
            ) : (
              <ul className="space-y-3">
                {pendingRoleCards}
              </ul>
            )}
          </div>

          <div className="border-t border-gray-200 pt-6">
            <h2 className="text-xl font-semibold text-gray-800 mb-4">Add New Role</h2>
            
            <div className="space-y-4">
              <div>
                <label htmlFor="name" className="block text-sm font-medium text-gray-700">
                  Role Name
                </label>
                <input
                  type="text"
                  id="name"
                  name="name"
                  value={currentRole.name}
                  onChange={handleInputChange}
                  className={`mt-1 block w-full rounded-md shadow-sm focus:ring-indigo-500 focus:border-indigo-500 sm:text-sm ${
                    errors.name ? 'border-red-300' : 'border-gray-300'
                  }`}
                  placeholder="Enter role name"
                />
                {errors.name && (
                  <p className="mt-1 text-sm text-red-600">{errors.name}</p>
                )}
              </div>
              
              <div>
                <label htmlFor="description" className="block text-sm font-medium text-gray-700">
                  Description
                </label>
                <textarea
                  id="description"
                  name="description"
                  value={currentRole.description}
                  onChange={handleInputChange}
                  rows={3}
                  className={`mt-1 block w-full rounded-md shadow-sm focus:ring-indigo-500 focus:border-indigo-500 sm:text-sm ${
                    errors.description ? 'border-red-300' : 'border-gray-300'
                  }`}
                  placeholder="Describe what this role can do"
                />
                {errors.description && (
                  <p className="mt-1 text-sm text-red-600">{errors.description}</p>
                )}
              </div>
              
              <div className="flex justify-end">
                <button
                  type="button"
                  onClick={handleAddRole}
                  disabled={!currentRole.name.trim()}
                  className="inline-flex items-center px-4 py-2 border border-transparent rounded-md shadow-sm text-sm font-medium text-white bg-indigo-600 hover:bg-indigo-700 focus:outline-none focus:ring-2 focus:ring-offset-2 focus:ring-indigo-500 disabled:opacity-50 disabled:cursor-not-allowed"
                >
                  <PlusIcon className="-ml-1 mr-2 h-5 w-5" />
                  Add Role
                </button>
              </div>
            </div>
          </div>
        </div>

        <div className="flex justify-between space-x-4">
          <button
            type="button"
            onClick={handleSkip}
            className="inline-flex justify-center py-2 px-4 border border-gray-300 shadow-sm text-sm font-medium rounded-md text-gray-700 bg-white hover:bg-gray-50 focus:outline-none focus:ring-2 focus:ring-offset-2 focus:ring-indigo-500"
          >
            Skip
          </button>
          
          <button
            type="button"
            onClick={handleSubmit}
            disabled={isLoading || isSubmitting}
            className="inline-flex justify-center py-2 px-4 border border-transparent shadow-sm text-sm font-medium rounded-md text-white bg-indigo-600 hover:bg-indigo-700 focus:outline-none focus:ring-2 focus:ring-offset-2 focus:ring-indigo-500 disabled:opacity-50 disabled:cursor-not-allowed"
          >
            {isLoading || isSubmitting ? 'Creating Roles...' : 'Continue to Dashboard'}
          </button>
        </div>
      </div>
    </div>
  );
};

export default CustomRoleSetup;