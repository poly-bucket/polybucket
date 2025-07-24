import React from 'react';
import { useNavigate } from 'react-router-dom';
import { useAppSelector } from '../../utils/hooks';
import NavigationBar from '../../components/common/NavigationBar';
import ComprehensiveAdminPanel from '../../components/admin/ComprehensiveAdminPanel';

const AdminControlPanel: React.FC = () => {
  const navigate = useNavigate();
  const { user } = useAppSelector((state) => state.auth);

  const handleBackToDashboard = () => {
    navigate('/dashboard');
  };

  return (
    <div className="min-h-screen bg-gray-100">
      {/* Navigation Bar */}
      <NavigationBar
        title="Admin Control Panel"
        showSearch={false}
        showUploadButton={false}
        showHomeLink={true}
      />

      {/* Main Content */}
      <div className="py-6">
        <ComprehensiveAdminPanel />
      </div>
    </div>
  );
};

export default AdminControlPanel; 