import React, { useState } from 'react';

export interface BillOfMaterial {
  id: string;
  name: string;
  quantity: number;
  price?: number;
  url?: string;
  required: boolean;
  notes?: string;
}

interface BillOfMaterialsManagerProps {
  billOfMaterials: BillOfMaterial[];
  onUpdate: (bom: BillOfMaterial[]) => void;
  readonly?: boolean;
}

const BillOfMaterialsManager: React.FC<BillOfMaterialsManagerProps> = ({
  billOfMaterials,
  onUpdate,
  readonly = false
}) => {
  const [editingItem, setEditingItem] = useState<BillOfMaterial | null>(null);
  const [showAddForm, setShowAddForm] = useState(false);

  const [newItem, setNewItem] = useState<Partial<BillOfMaterial>>({
    name: '',
    quantity: 1,
    price: undefined,
    url: '',
    required: true,
    notes: ''
  });

  const handleAddItem = () => {
    if (!newItem.name?.trim()) return;

    const item: BillOfMaterial = {
      id: Math.random().toString(36).substr(2, 9),
      name: newItem.name.trim(),
      quantity: newItem.quantity || 1,
      price: newItem.price,
      url: newItem.url?.trim() || undefined,
      required: newItem.required || false,
      notes: newItem.notes?.trim() || undefined
    };

    onUpdate([...billOfMaterials, item]);
    setNewItem({
      name: '',
      quantity: 1,
      price: undefined,
      url: '',
      required: true,
      notes: ''
    });
    setShowAddForm(false);
  };

  const handleUpdateItem = (updatedItem: BillOfMaterial) => {
    const updated = billOfMaterials.map(item => 
      item.id === updatedItem.id ? updatedItem : item
    );
    onUpdate(updated);
    setEditingItem(null);
  };

  const handleDeleteItem = (id: string) => {
    const updated = billOfMaterials.filter(item => item.id !== id);
    onUpdate(updated);
  };

  const formatPrice = (price: number | undefined) => {
    if (!price) return 'N/A';
    return `$${price.toFixed(2)}`;
  };

  const getTotalCost = () => {
    return billOfMaterials.reduce((total, item) => {
      return total + (item.price ? item.price * item.quantity : 0);
    }, 0);
  };

  return (
    <div className="space-y-4">
      <div className="flex items-center justify-between">
        <h3 className="text-lg font-semibold text-gray-800">Bill of Materials</h3>
        {!readonly && (
          <button
            onClick={() => setShowAddForm(true)}
            className="px-4 py-2 bg-green-600 text-white rounded-md hover:bg-green-700 focus:outline-none focus:ring-2 focus:ring-green-500 text-sm"
          >
            Add Item
          </button>
        )}
      </div>

      {/* Summary */}
      <div className="bg-gray-50 p-4 rounded-lg border">
        <div className="grid grid-cols-3 gap-4 text-sm">
          <div>
            <span className="text-gray-600">Total Items:</span>
            <span className="ml-2 font-semibold">{billOfMaterials.length}</span>
          </div>
          <div>
            <span className="text-gray-600">Required Items:</span>
            <span className="ml-2 font-semibold">{billOfMaterials.filter(item => item.required).length}</span>
          </div>
          <div>
            <span className="text-gray-600">Total Cost:</span>
            <span className="ml-2 font-semibold">{formatPrice(getTotalCost())}</span>
          </div>
        </div>
      </div>

      {/* Add Form */}
      {showAddForm && !readonly && (
        <div className="border border-gray-200 rounded-lg p-4 bg-white">
          <h4 className="text-md font-medium text-gray-800 mb-4">Add New Item</h4>
          <div className="grid grid-cols-1 md:grid-cols-2 gap-4">
            <div>
              <label className="block text-sm font-medium text-gray-700 mb-1">
                Item Name *
              </label>
              <input
                type="text"
                value={newItem.name || ''}
                onChange={(e) => setNewItem(prev => ({ ...prev, name: e.target.value }))}
                className="w-full px-3 py-2 border border-gray-300 rounded-md focus:outline-none focus:ring-2 focus:ring-green-500 focus:border-transparent"
                placeholder="e.g., PLA Filament"
              />
            </div>

            <div>
              <label className="block text-sm font-medium text-gray-700 mb-1">
                Quantity
              </label>
              <input
                type="number"
                min="1"
                value={newItem.quantity || 1}
                onChange={(e) => setNewItem(prev => ({ ...prev, quantity: parseInt(e.target.value) || 1 }))}
                className="w-full px-3 py-2 border border-gray-300 rounded-md focus:outline-none focus:ring-2 focus:ring-green-500 focus:border-transparent"
              />
            </div>

            <div>
              <label className="block text-sm font-medium text-gray-700 mb-1">
                Price (per unit)
              </label>
              <input
                type="number"
                step="0.01"
                min="0"
                value={newItem.price || ''}
                onChange={(e) => setNewItem(prev => ({ ...prev, price: parseFloat(e.target.value) || undefined }))}
                className="w-full px-3 py-2 border border-gray-300 rounded-md focus:outline-none focus:ring-2 focus:ring-green-500 focus:border-transparent"
                placeholder="0.00"
              />
            </div>

            <div>
              <label className="block text-sm font-medium text-gray-700 mb-1">
                Purchase URL
              </label>
              <input
                type="url"
                value={newItem.url || ''}
                onChange={(e) => setNewItem(prev => ({ ...prev, url: e.target.value }))}
                className="w-full px-3 py-2 border border-gray-300 rounded-md focus:outline-none focus:ring-2 focus:ring-green-500 focus:border-transparent"
                placeholder="https://..."
              />
            </div>

            <div className="md:col-span-2">
              <label className="block text-sm font-medium text-gray-700 mb-1">
                Notes
              </label>
              <input
                type="text"
                value={newItem.notes || ''}
                onChange={(e) => setNewItem(prev => ({ ...prev, notes: e.target.value }))}
                className="w-full px-3 py-2 border border-gray-300 rounded-md focus:outline-none focus:ring-2 focus:ring-green-500 focus:border-transparent"
                placeholder="Optional notes..."
              />
            </div>

            <div className="md:col-span-2">
              <label className="flex items-center">
                <input
                  type="checkbox"
                  checked={newItem.required || false}
                  onChange={(e) => setNewItem(prev => ({ ...prev, required: e.target.checked }))}
                  className="mr-2 text-green-600 bg-gray-100 border-gray-300 rounded focus:ring-green-500"
                />
                <span className="text-sm text-gray-700">Required item</span>
              </label>
            </div>
          </div>

          <div className="flex justify-end space-x-3 mt-4">
            <button
              onClick={() => setShowAddForm(false)}
              className="px-4 py-2 text-gray-600 bg-gray-100 rounded-md hover:bg-gray-200 focus:outline-none focus:ring-2 focus:ring-gray-500"
            >
              Cancel
            </button>
            <button
              onClick={handleAddItem}
              disabled={!newItem.name?.trim()}
              className="px-4 py-2 bg-green-600 text-white rounded-md hover:bg-green-700 focus:outline-none focus:ring-2 focus:ring-green-500 disabled:opacity-50 disabled:cursor-not-allowed"
            >
              Add Item
            </button>
          </div>
        </div>
      )}

      {/* Items List */}
      <div className="space-y-2">
        {billOfMaterials.map((item) => (
          <div key={item.id} className="border border-gray-200 rounded-lg p-4 bg-white">
            {editingItem?.id === item.id ? (
              <EditItemForm
                item={editingItem}
                onSave={handleUpdateItem}
                onCancel={() => setEditingItem(null)}
              />
            ) : (
              <div className="flex items-center justify-between">
                <div className="flex-1">
                  <div className="flex items-center space-x-3">
                    <h4 className="font-medium text-gray-900">{item.name}</h4>
                    {item.required && (
                      <span className="px-2 py-1 text-xs bg-red-100 text-red-700 rounded-full">
                        Required
                      </span>
                    )}
                  </div>
                  <div className="mt-1 text-sm text-gray-600">
                    <span>Quantity: {item.quantity}</span>
                    {item.price && (
                      <span className="ml-4">
                        Unit Price: {formatPrice(item.price)} • Total: {formatPrice(item.price * item.quantity)}
                      </span>
                    )}
                  </div>
                  {item.notes && (
                    <p className="mt-1 text-sm text-gray-500">{item.notes}</p>
                  )}
                </div>
                
                <div className="flex items-center space-x-2">
                  {item.url && (
                    <a
                      href={item.url}
                      target="_blank"
                      rel="noopener noreferrer"
                      className="px-3 py-1 text-sm bg-blue-600 text-white rounded hover:bg-blue-700"
                    >
                      Buy
                    </a>
                  )}
                  {!readonly && (
                    <>
                      <button
                        onClick={() => setEditingItem(item)}
                        className="p-1 text-gray-400 hover:text-gray-600"
                      >
                        <svg className="w-4 h-4" fill="none" stroke="currentColor" viewBox="0 0 24 24">
                          <path strokeLinecap="round" strokeLinejoin="round" strokeWidth={2} d="M11 5H6a2 2 0 00-2 2v11a2 2 0 002 2h11a2 2 0 002-2v-5m-1.414-9.414a2 2 0 112.828 2.828L11.828 15H9v-2.828l8.586-8.586z" />
                        </svg>
                      </button>
                      <button
                        onClick={() => handleDeleteItem(item.id)}
                        className="p-1 text-red-400 hover:text-red-600"
                      >
                        <svg className="w-4 h-4" fill="none" stroke="currentColor" viewBox="0 0 24 24">
                          <path strokeLinecap="round" strokeLinejoin="round" strokeWidth={2} d="M19 7l-.867 12.142A2 2 0 0116.138 21H7.862a2 2 0 01-1.995-1.858L5 7m5 4v6m4-6v6m1-10V4a1 1 0 00-1-1h-4a1 1 0 00-1 1v3M4 7h16" />
                        </svg>
                      </button>
                    </>
                  )}
                </div>
              </div>
            )}
          </div>
        ))}
      </div>

      {billOfMaterials.length === 0 && (
        <div className="text-center py-8 text-gray-500">
          <svg className="mx-auto h-12 w-12 text-gray-400 mb-4" fill="none" stroke="currentColor" viewBox="0 0 24 24">
            <path strokeLinecap="round" strokeLinejoin="round" strokeWidth={2} d="M9 5H7a2 2 0 00-2 2v10a2 2 0 002 2h8a2 2 0 002-2V7a2 2 0 00-2-2h-2M9 5a2 2 0 002 2h2a2 2 0 002-2M9 5a2 2 0 012-2h2a2 2 0 012 2" />
          </svg>
          <p>No materials added yet</p>
          {!readonly && (
            <p className="text-sm">Click "Add Item" to start building your bill of materials</p>
          )}
        </div>
      )}
    </div>
  );
};

// Edit Item Form Component
interface EditItemFormProps {
  item: BillOfMaterial;
  onSave: (item: BillOfMaterial) => void;
  onCancel: () => void;
}

const EditItemForm: React.FC<EditItemFormProps> = ({ item, onSave, onCancel }) => {
  const [editedItem, setEditedItem] = useState<BillOfMaterial>(item);

  const handleSave = () => {
    if (!editedItem.name.trim()) return;
    onSave(editedItem);
  };

  return (
    <div className="space-y-4">
      <div className="grid grid-cols-1 md:grid-cols-2 gap-4">
        <div>
          <label className="block text-sm font-medium text-gray-700 mb-1">
            Item Name *
          </label>
          <input
            type="text"
            value={editedItem.name}
            onChange={(e) => setEditedItem(prev => ({ ...prev, name: e.target.value }))}
            className="w-full px-3 py-2 border border-gray-300 rounded-md focus:outline-none focus:ring-2 focus:ring-green-500 focus:border-transparent"
          />
        </div>

        <div>
          <label className="block text-sm font-medium text-gray-700 mb-1">
            Quantity
          </label>
          <input
            type="number"
            min="1"
            value={editedItem.quantity}
            onChange={(e) => setEditedItem(prev => ({ ...prev, quantity: parseInt(e.target.value) || 1 }))}
            className="w-full px-3 py-2 border border-gray-300 rounded-md focus:outline-none focus:ring-2 focus:ring-green-500 focus:border-transparent"
          />
        </div>

        <div>
          <label className="block text-sm font-medium text-gray-700 mb-1">
            Price (per unit)
          </label>
          <input
            type="number"
            step="0.01"
            min="0"
            value={editedItem.price || ''}
            onChange={(e) => setEditedItem(prev => ({ ...prev, price: parseFloat(e.target.value) || undefined }))}
            className="w-full px-3 py-2 border border-gray-300 rounded-md focus:outline-none focus:ring-2 focus:ring-green-500 focus:border-transparent"
          />
        </div>

        <div>
          <label className="block text-sm font-medium text-gray-700 mb-1">
            Purchase URL
          </label>
          <input
            type="url"
            value={editedItem.url || ''}
            onChange={(e) => setEditedItem(prev => ({ ...prev, url: e.target.value || undefined }))}
            className="w-full px-3 py-2 border border-gray-300 rounded-md focus:outline-none focus:ring-2 focus:ring-green-500 focus:border-transparent"
          />
        </div>

        <div className="md:col-span-2">
          <label className="block text-sm font-medium text-gray-700 mb-1">
            Notes
          </label>
          <input
            type="text"
            value={editedItem.notes || ''}
            onChange={(e) => setEditedItem(prev => ({ ...prev, notes: e.target.value || undefined }))}
            className="w-full px-3 py-2 border border-gray-300 rounded-md focus:outline-none focus:ring-2 focus:ring-green-500 focus:border-transparent"
          />
        </div>

        <div className="md:col-span-2">
          <label className="flex items-center">
            <input
              type="checkbox"
              checked={editedItem.required}
              onChange={(e) => setEditedItem(prev => ({ ...prev, required: e.target.checked }))}
              className="mr-2 text-green-600 bg-gray-100 border-gray-300 rounded focus:ring-green-500"
            />
            <span className="text-sm text-gray-700">Required item</span>
          </label>
        </div>
      </div>

      <div className="flex justify-end space-x-3">
        <button
          onClick={onCancel}
          className="px-4 py-2 text-gray-600 bg-gray-100 rounded-md hover:bg-gray-200 focus:outline-none focus:ring-2 focus:ring-gray-500"
        >
          Cancel
        </button>
        <button
          onClick={handleSave}
          disabled={!editedItem.name.trim()}
          className="px-4 py-2 bg-green-600 text-white rounded-md hover:bg-green-700 focus:outline-none focus:ring-2 focus:ring-green-500 disabled:opacity-50 disabled:cursor-not-allowed"
        >
          Save Changes
        </button>
      </div>
    </div>
  );
};

export default BillOfMaterialsManager; 