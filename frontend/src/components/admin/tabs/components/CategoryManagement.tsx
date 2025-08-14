import React, { useState, useEffect } from 'react';
import { Add as AddIcon } from '@mui/icons-material';
import { usePermissions, PERMISSIONS } from '../../../../hooks/usePermissions';
import categoryService, { CategoryDto } from '../../../../services/categoryService';
import CreateCategoryForm from './CreateCategoryForm';
import CategoriesTable from './CategoriesTable';

const CategoryManagement: React.FC = () => {
  const { hasPermission, loading: permissionsLoading, permissions } = usePermissions();
  const [categories, setCategories] = useState<CategoryDto[]>([]);
  const [loading, setLoading] = useState(false);
  const [error, setError] = useState<string | null>(null);
  const [showCreateForm, setShowCreateForm] = useState(false);
  const [editingCategory, setEditingCategory] = useState<CategoryDto | null>(null);
  const [newCategoryName, setNewCategoryName] = useState('');
  const [editCategoryName, setEditCategoryName] = useState('');
  const [searchTerm, setSearchTerm] = useState('');
  const [currentPage, setCurrentPage] = useState(1);
  const [pageSize, setPageSize] = useState(20);
  const [totalPages, setTotalPages] = useState(1);
  const [totalCount, setTotalCount] = useState(0);
  const [hasNextPage, setHasNextPage] = useState(false);
  const [hasPreviousPage, setHasPreviousPage] = useState(false);

  const fetchCategories = async (page: number = currentPage, search: string = searchTerm) => {
    try {
      setLoading(true);
      setError(null);
      const data = await categoryService.getCategories({
        page,
        pageSize,
        searchTerm: search || undefined
      });
      setCategories(data.categories || []);
      setCurrentPage(data.page);
      setTotalPages(data.totalPages);
      setTotalCount(data.totalCount);
      setHasNextPage(data.hasNextPage);
      setHasPreviousPage(data.hasPreviousPage);
    } catch (err) {
      setError('Failed to load categories');
      console.error('Error loading categories:', err);
    } finally {
      setLoading(false);
    }
  };

  const createCategory = async () => {
    if (!newCategoryName.trim()) return;
    
    try {
      await categoryService.createCategory({ name: newCategoryName.trim() });
      setNewCategoryName('');
      setShowCreateForm(false);
      await fetchCategories();
    } catch (err) {
      setError('Failed to create category');
      console.error('Error creating category:', err);
    }
  };

  const updateCategory = async () => {
    if (!editingCategory || !editCategoryName.trim()) return;
    
    try {
      await categoryService.updateCategory({ id: editingCategory.id, name: editCategoryName.trim() });
      setEditingCategory(null);
      setEditCategoryName('');
      await fetchCategories();
    } catch (err) {
      setError('Failed to update category');
      console.error('Error updating category:', err);
    }
  };

  const deleteCategory = async (categoryId: string) => {
    if (!confirm('Are you sure you want to delete this category? This action cannot be undone.')) {
      return;
    }
    
    try {
      await categoryService.deleteCategory(categoryId);
      await fetchCategories();
    } catch (err) {
      setError('Failed to delete category');
      console.error('Error deleting category:', err);
    }
  };

  const handlePageChange = (newPage: number) => {
    setCurrentPage(newPage);
    fetchCategories(newPage, searchTerm);
  };

  const handleSearch = (search: string) => {
    setSearchTerm(search);
    setCurrentPage(1); // Reset to first page when searching
    fetchCategories(1, search);
  };

  const handlePageSizeChange = (newPageSize: number) => {
    setPageSize(newPageSize);
    setCurrentPage(1); // Reset to first page when changing page size
    fetchCategories(1, searchTerm);
  };

  useEffect(() => {
    fetchCategories(1, '');
  }, []);

  // Categories are now filtered server-side, so we use the categories directly

  // Format date for display
  const formatDate = (dateString: string) => {
    if (!dateString || dateString === 'Never') return 'Never';
    try {
      const date = new Date(dateString);
      return date.toLocaleDateString() + ' ' + date.toLocaleTimeString([], { hour: '2-digit', minute: '2-digit' });
    } catch {
      return dateString;
    }
  };

  // Check permission after all hooks are called
  if (permissionsLoading) {
    return (
      <div className="lg-card p-6">
        <div className="text-center text-white/60 py-8">Loading permissions...</div>
      </div>
    );
  }
  
  if (!hasPermission(PERMISSIONS.ADMIN_MANAGE_CATEGORIES)) {
    return null;
  }

  return (
    <div className="lg-card p-6">
      <div className="flex justify-between items-center mb-6">
        <h3 className="text-lg font-medium text-white flex items-center">
          <svg className="w-5 h-5 mr-2 text-blue-400" fill="none" viewBox="0 0 24 24" stroke="currentColor">
            <path strokeLinecap="round" strokeLinejoin="round" strokeWidth={2} d="M19 11H5m14 0a2 2 0 012 2v6a2 2 0 01-2 2H5a2 2 0 01-2-2v-6a2 2 0 012-2m14 0V9a2 2 0 00-2-2M5 11V9a2 2 0 012-2m0 0V5a2 2 0 012-2h6a2 2 0 012 2v2M7 7h10" />
          </svg>
          Category Management
        </h3>
        <button
          onClick={() => setShowCreateForm(true)}
          className="lg-button lg-button-primary"
        >
          <AddIcon className="w-4 h-4 mr-2" />
          Add Category
        </button>
      </div>

      {error && (
        <div className="mb-4 p-3 bg-red-500/20 border border-red-500/30 rounded-lg text-red-400">
          {error}
          <button
            onClick={() => setError(null)}
            className="ml-2 text-red-300 hover:text-red-200 underline"
          >
            Dismiss
          </button>
        </div>
      )}

      <CreateCategoryForm
        showCreateForm={showCreateForm}
        newCategoryName={newCategoryName}
        setNewCategoryName={setNewCategoryName}
        setShowCreateForm={setShowCreateForm}
        createCategory={createCategory}
      />

      {/* Search Bar */}
      <div className="mb-4">
        <div className="flex space-x-3">
          <div className="relative flex-1">
            <input
              type="text"
              value={searchTerm}
              onChange={(e) => setSearchTerm(e.target.value)}
              onKeyPress={(e) => e.key === 'Enter' && handleSearch(searchTerm)}
              placeholder="Search categories..."
              className="w-full px-4 py-2 pl-10 bg-white/10 border border-white/20 rounded-lg text-white placeholder-white/50 focus:outline-none focus:border-blue-400"
            />
            <div className="absolute inset-y-0 left-0 pl-3 flex items-center pointer-events-none">
              <svg className="h-5 w-5 text-white/50" fill="none" viewBox="0 0 24 24" stroke="currentColor">
                <path strokeLinecap="round" strokeLinejoin="round" strokeWidth={2} d="M21 21l-6-6m2-5a7 7 0 11-14 0 7 7 0 0114 0z" />
              </svg>
            </div>
          </div>
          <button
            onClick={() => handleSearch(searchTerm)}
            className="px-4 py-2 bg-blue-500 hover:bg-blue-600 text-white rounded-lg transition-colors"
          >
            Search
          </button>
          {searchTerm && (
            <button
              onClick={() => handleSearch('')}
              className="px-4 py-2 bg-gray-500 hover:bg-gray-600 text-white rounded-lg transition-colors"
            >
              Clear
            </button>
          )}
        </div>
      </div>

      <CategoriesTable
        categories={categories}
        loading={loading}
        searchTerm={searchTerm}
        editingCategory={editingCategory}
        editCategoryName={editCategoryName}
        setEditCategoryName={setEditCategoryName}
        setEditingCategory={setEditingCategory}
        updateCategory={updateCategory}
        deleteCategory={deleteCategory}
        formatDate={formatDate}
      />

      {/* Pagination Controls */}
      {!loading && totalPages > 1 && (
        <div className="mt-6 flex flex-col sm:flex-row justify-between items-center space-y-4 sm:space-y-0">
          <div className="flex items-center space-x-4">
            <div className="flex items-center space-x-2">
              <span className="text-sm text-white/60">Page size:</span>
              <select
                value={pageSize}
                onChange={(e) => handlePageSizeChange(Number(e.target.value))}
                className="px-2 py-1 bg-white/10 border border-white/20 rounded text-white text-sm focus:outline-none focus:border-blue-400"
              >
                <option value={10}>10</option>
                <option value={20}>20</option>
                <option value={50}>50</option>
                <option value={100}>100</option>
              </select>
            </div>
            <div className="text-sm text-white/60">
              Page {currentPage} of {totalPages} ({totalCount} total)
            </div>
          </div>
          
          <div className="flex items-center space-x-2">
            <button
              onClick={() => handlePageChange(currentPage - 1)}
              disabled={!hasPreviousPage}
              className="px-3 py-2 bg-white/10 hover:bg-white/20 disabled:bg-white/5 disabled:text-white/30 text-white rounded transition-colors"
            >
              Previous
            </button>
            
            {/* Page numbers */}
            <div className="flex items-center space-x-1">
              {Array.from({ length: Math.min(5, totalPages) }, (_, i) => {
                let pageNum;
                if (totalPages <= 5) {
                  pageNum = i + 1;
                } else if (currentPage <= 3) {
                  pageNum = i + 1;
                } else if (currentPage >= totalPages - 2) {
                  pageNum = totalPages - 4 + i;
                } else {
                  pageNum = currentPage - 2 + i;
                }
                
                return (
                  <button
                    key={pageNum}
                    onClick={() => handlePageChange(pageNum)}
                    className={`px-3 py-2 rounded transition-colors ${
                      pageNum === currentPage
                        ? 'bg-blue-500 text-white'
                        : 'bg-white/10 hover:bg-white/20 text-white'
                    }`}
                  >
                    {pageNum}
                  </button>
                );
              })}
            </div>
            
            <button
              onClick={() => handlePageChange(currentPage + 1)}
              disabled={!hasNextPage}
              className="px-3 py-2 bg-white/10 hover:bg-white/20 disabled:bg-white/5 disabled:text-white/30 text-white rounded transition-colors"
            >
              Next
            </button>
          </div>
        </div>
      )}

      {/* Results Count */}
      {!loading && categories.length > 0 && (
        <div className="mt-4 text-sm text-white/60">
          Showing {categories.length} of {totalCount} categories
          {searchTerm && ` matching "${searchTerm}"`}
        </div>
      )}
    </div>
  );
};

export default CategoryManagement;
