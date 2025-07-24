import React from 'react';
import { Outlet, Link } from 'react-router-dom';
import NavigationBar from '../../components/common/NavigationBar';

const AdminDashboard: React.FC = () => {
  return (
    <div className="min-h-screen bg-gray-50">
      {/* Navigation Bar */}
      <NavigationBar
        title="Admin Dashboard"
        showSearch={false}
        showUploadButton={false}
        showHomeLink={true}
      />
      
      {/* Main Content */}
      <div className="max-w-7xl mx-auto px-4 sm:px-6 lg:px-8 py-8">
        <h1 className="text-2xl font-bold text-gray-900 mb-6">Admin Control Panel</h1>
        <nav className="mb-6">
          <ul className="flex space-x-6">
            <li>
              <Link to="roles" className="text-indigo-600 hover:text-indigo-800 font-medium">
                Role Management
              </Link>
            </li>
            <li>
              <Link to="model-settings" className="text-indigo-600 hover:text-indigo-800 font-medium">
                Model Upload Settings
              </Link>
            </li>
            <li>
              <Link to="moderation-settings" className="text-indigo-600 hover:text-indigo-800 font-medium">
                Moderation Settings
              </Link>
            </li>
            {/* Add links to other admin sections here */}
          </ul>
        </nav>
        <hr className="mb-6" />
        <Outlet /> {/* This will render the nested admin routes */}
      </div>
    </div>
  );
};

export default AdminDashboard; 