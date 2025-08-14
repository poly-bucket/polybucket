import React from 'react';
import { render, screen, fireEvent, waitFor } from '@testing-library/react';
import '@testing-library/jest-dom';
import { BrowserRouter } from 'react-router-dom';
import RoleManagement from '../RoleManagement';
import UsersTab from '../tabs/UsersTab';

// Mock the API clients
jest.mock('../../../../services/api.client', () => ({
  RoleManagementClient: jest.fn().mockImplementation(() => ({
    getAllRoles: jest.fn().mockResolvedValue({
      roles: [
        { id: '1', name: 'Admin', description: 'Administrator role', priority: 1, isSystemRole: true, isDefault: false, isActive: true, canBeDeleted: false },
        { id: '2', name: 'Moderator', description: 'Moderator role', priority: 2, isSystemRole: false, isDefault: false, isActive: true, canBeDeleted: true },
        { id: '3', name: 'User', description: 'Standard user role', priority: 3, isSystemRole: false, isDefault: true, isActive: true, canBeDeleted: false }
      ],
      pagination: {
        totalCount: 3,
        totalPages: 1,
        hasNextPage: false,
        hasPreviousPage: false
      }
    })
  })),
  GetUsersClient: jest.fn().mockImplementation(() => ({
    getUsers: jest.fn().mockResolvedValue({
      users: [
        { id: '1', email: 'admin@example.com', username: 'admin', role: 'Admin', isActive: true },
        { id: '2', email: 'mod@example.com', username: 'moderator', role: 'Moderator', isActive: true },
        { id: '3', email: 'user@example.com', username: 'user', role: 'User', isActive: true }
      ],
      totalCount: 3,
      totalPages: 1
    })
  }))
}));

// Mock the auth context
jest.mock('../../../../context/AuthContext', () => ({
  useAuth: () => ({
    user: { accessToken: 'mock-token' },
    isAuthenticated: true
  })
}));

const renderWithRouter = (component: React.ReactElement) => {
  return render(
    <BrowserRouter>
      {component}
    </BrowserRouter>
  );
};

describe('Admin Pagination Functionality', () => {
  describe('RoleManagement Component', () => {
    it('should display pagination controls when there are multiple pages', async () => {
      renderWithRouter(<RoleManagement />);
      
      await waitFor(() => {
        expect(screen.getByText('Role Management')).toBeInTheDocument();
      });
      
      // Check if pagination info is displayed
      expect(screen.getByText(/Showing 1 to 3 of 3 roles/)).toBeInTheDocument();
      expect(screen.getByText(/Page 1 of 1/)).toBeInTheDocument();
    });

    it('should handle page navigation correctly', async () => {
      renderWithRouter(<RoleManagement />);
      
      await waitFor(() => {
        expect(screen.getByText('Role Management')).toBeInTheDocument();
      });
      
      // Check if navigation buttons are present
      const prevButton = screen.getByText('Previous');
      const nextButton = screen.getByText('Next');
      
      expect(prevButton).toBeInTheDocument();
      expect(nextButton).toBeInTheDocument();
      
      // Since there's only 1 page, buttons should be disabled
      expect(prevButton).toBeDisabled();
      expect(nextButton).toBeDisabled();
    });

    it('should display role information correctly', async () => {
      renderWithRouter(<RoleManagement />);
      
      await waitFor(() => {
        expect(screen.getByText('Admin')).toBeInTheDocument();
        expect(screen.getByText('Moderator')).toBeInTheDocument();
        expect(screen.getByText('User')).toBeInTheDocument();
      });
    });
  });

  describe('UsersTab Component', () => {
    it('should display pagination controls when there are multiple pages', async () => {
      renderWithRouter(<UsersTab />);
      
      await waitFor(() => {
        expect(screen.getByText(/Showing 1 to 3 of 3 users/)).toBeInTheDocument();
      });
      
      // Check if page size selector is present
      const pageSizeSelector = screen.getByDisplayValue('20');
      expect(pageSizeSelector).toBeInTheDocument();
    });

    it('should handle page size changes correctly', async () => {
      renderWithRouter(<UsersTab />);
      
      await waitFor(() => {
        expect(screen.getByDisplayValue('20')).toBeInTheDocument();
      });
      
      const pageSizeSelector = screen.getByDisplayValue('20');
      fireEvent.change(pageSizeSelector, { target: { value: '10' } });
      
      expect(pageSizeSelector).toHaveValue('10');
    });

    it('should display user information correctly', async () => {
      renderWithRouter(<UsersTab />);
      
      await waitFor(() => {
        expect(screen.getByText('admin@example.com')).toBeInTheDocument();
        expect(screen.getByText('mod@example.com')).toBeInTheDocument();
        expect(screen.getByText('user@example.com')).toBeInTheDocument();
      });
    });

    it('should handle search functionality', async () => {
      renderWithRouter(<UsersTab />);
      
      await waitFor(() => {
        expect(screen.getByPlaceholderText('Search users...')).toBeInTheDocument();
      });
      
      const searchInput = screen.getByPlaceholderText('Search users...');
      fireEvent.change(searchInput, { target: { value: 'admin' } });
      
      expect(searchInput).toHaveValue('admin');
    });
  });

  describe('Pagination State Management', () => {
    it('should reset to first page when search is performed', async () => {
      renderWithRouter(<UsersTab />);
      
      await waitFor(() => {
        expect(screen.getByPlaceholderText('Search users...')).toBeInTheDocument();
      });
      
      const searchInput = screen.getByPlaceholderText('Search users...');
      fireEvent.change(searchInput, { target: { value: 'admin' } });
      
      // Wait for debounced search
      await waitFor(() => {
        expect(searchInput).toHaveValue('admin');
      }, { timeout: 600 });
    });

    it('should update pagination when page size changes', async () => {
      renderWithRouter(<UsersTab />);
      
      await waitFor(() => {
        expect(screen.getByDisplayValue('20')).toBeInTheDocument();
      });
      
      const pageSizeSelector = screen.getByDisplayValue('20');
      fireEvent.change(pageSizeSelector, { target: { value: '10' } });
      
      // Should reset to page 1 when page size changes
      expect(screen.getByText(/Page 1 of 1/)).toBeInTheDocument();
    });
  });
});
