import React, { useState } from 'react';
import { useAuth } from '../../context/AuthContext';
import RoleManagement from './RoleManagement';
import ModelUploadSettings from './ModelUploadSettings';
import ModerationSettings from './ModerationSettings';

const AdminDashboard: React.FC = () => {
  const { user } = useAuth();
  const [tabValue, setTabValue] = useState(0);
  const [error, setError] = useState('');

  const handleTabChange = (event: React.SyntheticEvent, newValue: number) => {
    setTabValue(newValue);
  };

  if (!user) {
    return (
      <div className="lg-container min-h-screen">
        <div className="max-w-7xl mx-auto px-4 sm:px-6 lg:px-8 py-8">
          <div className="lg-card p-6">
            <div className="text-center">
              <div className="text-lg text-white">Please log in to access the admin dashboard.</div>
            </div>
          </div>
        </div>
      </div>
    );
  }

  return (
    <div className="lg-container min-h-screen">
      <div className="max-w-7xl mx-auto px-4 sm:px-6 lg:px-8 py-8">
        <div className="lg-card p-6">
          {error && (
            <div className="lg-badge-error p-4 mb-6">
              <span className="text-lg">{error}</span>
            </div>
          )}

          <div className="border-b border-white/10 mb-6">
            <nav className="flex space-x-8">
              <button
                onClick={() => setTabValue(0)}
                className={`py-4 px-1 border-b-2 font-medium text-sm capitalize transition-colors duration-200 ${
                  tabValue === 0
                    ? 'border-indigo-500 text-indigo-400'
                    : 'border-transparent text-white/60 hover:text-white/80 hover:border-white/20'
                }`}
              >
                Role Management
              </button>
              <button
                onClick={() => setTabValue(1)}
                className={`py-4 px-1 border-b-2 font-medium text-sm capitalize transition-colors duration-200 ${
                  tabValue === 1
                    ? 'border-indigo-500 text-indigo-400'
                    : 'border-transparent text-white/60 hover:text-white/80 hover:border-white/20'
                }`}
              >
                Model Upload Settings
              </button>
              <button
                onClick={() => setTabValue(2)}
                className={`py-4 px-1 border-b-2 font-medium text-sm capitalize transition-colors duration-200 ${
                  tabValue === 2
                    ? 'border-indigo-500 text-indigo-400'
                    : 'border-transparent text-white/60 hover:text-white/80 hover:border-white/20'
                }`}
              >
                Moderation Settings
              </button>
            </nav>
          </div>

          <div className="p-6">
            {tabValue === 0 && <RoleManagement />}
            {tabValue === 1 && <ModelUploadSettings />}
            {tabValue === 2 && <ModerationSettings />}
          </div>
        </div>
      </div>
    </div>
  );
};

export default AdminDashboard; 