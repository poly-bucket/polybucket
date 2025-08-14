import React from 'react';
import { Edit as EditIcon, Delete as DeleteIcon } from '@mui/icons-material';
import { CategoryDto } from '../../../../services/categoryService';

interface CategoriesTableProps {
  categories: CategoryDto[];
  loading: boolean;
  searchTerm: string;
  editingCategory: CategoryDto | null;
  editCategoryName: string;
  setEditCategoryName: (name: string) => void;
  setEditingCategory: (category: CategoryDto | null) => void;
  updateCategory: () => void;
  deleteCategory: (id: string) => void;
  formatDate: (date: string) => string;
}

const CategoriesTable: React.FC<CategoriesTableProps> = ({
  categories,
  loading,
  searchTerm,
  editingCategory,
  editCategoryName,
  setEditCategoryName,
  setEditingCategory,
  updateCategory,
  deleteCategory,
  formatDate
}) => {
  if (loading) {
    return <div className="text-center text-white/60 py-8">Loading categories...</div>;
  }

  if (categories.length === 0) {
    return (
      <div className="text-center text-white/60 py-8">
        {searchTerm ? 'No categories match your search' : 'No categories found'}
      </div>
    );
  }

  return (
    <div className="overflow-x-auto">
      <table className="w-full">
        <thead>
          <tr className="border-b border-white/20">
            <th className="text-left py-3 px-4 text-white/80 font-medium">Name</th>
            <th className="text-left py-3 px-4 text-white/80 font-medium">Created At</th>
            <th className="text-left py-3 px-4 text-white/80 font-medium">Updated At</th>
            <th className="text-right py-3 px-4 text-white/80 font-medium">Actions</th>
          </tr>
        </thead>
        <tbody>
          {categories.map((category) => (
            <tr key={category.id} className="border-b border-white/10 hover:bg-white/5">
              <td className="py-3 px-4">
                {editingCategory?.id === category.id ? (
                  <input
                    type="text"
                    value={editCategoryName}
                    onChange={(e) => setEditCategoryName(e.target.value)}
                    className="w-full px-2 py-1 bg-white/10 border border-white/20 rounded text-white focus:outline-none focus:border-blue-400"
                    onKeyPress={(e) => e.key === 'Enter' && updateCategory()}
                  />
                ) : (
                  <span className="text-white font-medium">{category.name}</span>
                )}
              </td>
              <td className="py-3 px-4 text-white/70 text-sm">
                {formatDate(category.createdAt)}
              </td>
              <td className="py-3 px-4 text-white/70 text-sm">
                {formatDate(category.updatedAt)}
              </td>
              <td className="py-3 px-4 text-right">
                {editingCategory?.id === category.id ? (
                  <div className="flex space-x-2 justify-end">
                    <button
                      onClick={updateCategory}
                      disabled={!editCategoryName.trim()}
                      className="px-3 py-1 bg-green-500 hover:bg-green-600 disabled:bg-gray-500 text-white rounded text-sm transition-colors"
                      title="Save changes"
                    >
                      Save
                    </button>
                    <button
                      onClick={() => {
                        setEditingCategory(null);
                        setEditCategoryName('');
                      }}
                      className="px-3 py-1 bg-gray-500 hover:bg-gray-600 text-white rounded text-sm transition-colors"
                      title="Cancel editing"
                    >
                      Cancel
                    </button>
                  </div>
                ) : (
                  <div className="flex space-x-2 justify-end">
                    <button
                      onClick={() => {
                        setEditingCategory(category);
                        setEditCategoryName(category.name);
                      }}
                      className="p-2 text-blue-400 hover:text-blue-300 hover:bg-white/10 rounded transition-colors"
                      title="Edit category"
                    >
                      <EditIcon className="w-4 h-4" />
                    </button>
                    <button
                      onClick={() => deleteCategory(category.id)}
                      className="p-2 text-red-400 hover:text-red-300 hover:bg-white/10 rounded transition-colors"
                      title="Delete category"
                    >
                      <DeleteIcon className="w-4 h-4" />
                    </button>
                  </div>
                )}
              </td>
            </tr>
          ))}
        </tbody>
      </table>
    </div>
  );
};

export default CategoriesTable;
