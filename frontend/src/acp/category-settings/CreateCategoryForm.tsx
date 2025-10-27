import React from 'react';

interface CreateCategoryFormProps {
  showCreateForm: boolean;
  newCategoryName: string;
  setNewCategoryName: (name: string) => void;
  setShowCreateForm: (show: boolean) => void;
  createCategory: () => void;
}

const CreateCategoryForm: React.FC<CreateCategoryFormProps> = ({
  showCreateForm,
  newCategoryName,
  setNewCategoryName,
  setShowCreateForm,
  createCategory
}) => {
  if (!showCreateForm) return null;

  return (
    <div className="mb-6 p-4 bg-white/5 rounded-lg">
      <h4 className="text-md font-medium text-white mb-3">Create New Category</h4>
      <div className="flex space-x-3">
        <input
          type="text"
          value={newCategoryName}
          onChange={(e) => setNewCategoryName(e.target.value)}
          placeholder="Category name"
          className="flex-1 px-3 py-2 bg-white/10 border border-white/20 rounded-lg text-white placeholder-white/50 focus:outline-none focus:border-blue-400"
          onKeyPress={(e) => e.key === 'Enter' && createCategory()}
        />
        <button
          onClick={createCategory}
          disabled={!newCategoryName.trim()}
          className="px-4 py-2 bg-blue-500 hover:bg-blue-600 disabled:bg-gray-500 text-white rounded-lg transition-colors"
        >
          Create
        </button>
        <button
          onClick={() => {
            setShowCreateForm(false);
            setNewCategoryName('');
          }}
          className="px-4 py-2 bg-gray-500 hover:bg-gray-600 text-white rounded-lg transition-colors"
        >
          Cancel
        </button>
      </div>
    </div>
  );
};

export default CreateCategoryForm;
