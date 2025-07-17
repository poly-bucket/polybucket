import React from 'react';
import { BrowserRouter as Router, Routes, Route, Navigate } from 'react-router-dom';
import FirstTimeSetup from './components/auth/FirstTimeSetup';
import CustomRoleSetup from './components/auth/CustomRoleSetup';
import LoginForm from './components/auth/LoginForm';
import RegisterForm from './components/auth/RegisterForm';
import Dashboard from './components/Dashboard';
import RootRedirect from './components/RootRedirect';
import ProtectedRoute from './components/ProtectedRoute';
import AdminDashboard from './pages/admin/AdminDashboard';
import AdminControlPanel from './pages/admin/AdminControlPanel';
import ModelModeration from './components/moderation/ModelModeration';
import ModelUploadSettings from './components/admin/ModelUploadSettings';
import ModerationSettings from './components/admin/ModerationSettings';
import RoleManagement from './pages/admin/RoleManagement';
import ModelUploadSettingsPage from './pages/admin/ModelUploadSettings';
import ModelUpload from './pages/ModelUpload';
import ModelDetails from './pages/ModelDetails';
import UserSettings from './pages/UserSettings';
import Collections from './pages/Collections';
import AvatarDemo from './components/AvatarDemo';

// Component to test localStorage functionality
const LocalStorageTest: React.FC = () => {
  React.useEffect(() => {
    try {
      console.log('=== Testing localStorage functionality ===');
      
      // Test basic storage
      localStorage.setItem('test-key', 'test-value');
      const testValue = localStorage.getItem('test-key');
      console.log('Basic localStorage test:', testValue === 'test-value' ? 'SUCCESS' : 'FAILED');
      
      // Test object storage
      const testObj = { 
        id: 'test-id', 
        token: 'test-token-' + Date.now(),
        nestedData: { foo: 'bar' }
      };
      localStorage.setItem('test-obj', JSON.stringify(testObj));
      const retrievedObj = localStorage.getItem('test-obj');
      console.log('Object storage test - raw:', retrievedObj);
      
      if (retrievedObj) {
        const parsedObj = JSON.parse(retrievedObj);
        console.log('Object storage test - parsed:', parsedObj);
        console.log('Token value exists:', !!parsedObj.token);
      }
      
      // Clean up
      localStorage.removeItem('test-key');
      localStorage.removeItem('test-obj');
      
      console.log('=== localStorage test complete ===');
    } catch (error) {
      console.error('localStorage test error:', error);
    }
  }, []);

  return null; // This component doesn't render anything
};

const App: React.FC = () => {
  return (
    <>
      <LocalStorageTest />
      <Router>
        <Routes>
          {/* Root route - determines where to go */}
          <Route path="/" element={<RootRedirect />} />
          
          {/* First-time setup routes - accessible without auth */}
          <Route path="/admin-setup" element={<FirstTimeSetup />} />
          <Route path="/custom-role-setup" element={<CustomRoleSetup />} />
          <Route path="/model-upload-settings" element={<ModelUploadSettings />} />
          <Route path="/moderation-settings" element={<ModerationSettings />} />
          
          {/* Auth routes */}
          <Route path="/login" element={<LoginForm />} />
          <Route path="/register" element={<RegisterForm />} />
          
          {/* Demo routes (development only) */}
          <Route path="/avatar-demo" element={<AvatarDemo />} />
          
          {/* Protected routes */}
          <Route 
            path="/dashboard" 
            element={
              <ProtectedRoute>
                <Dashboard />
              </ProtectedRoute>
            } 
          />
          
          <Route 
            path="/upload-model" 
            element={
              <ProtectedRoute>
                <ModelUpload />
              </ProtectedRoute>
            } 
          />
          
          <Route 
            path="/models/:id" 
            element={
              <ProtectedRoute>
                <ModelDetails />
              </ProtectedRoute>
            } 
          />
          
          <Route 
            path="/settings" 
            element={
              <ProtectedRoute>
                <UserSettings />
              </ProtectedRoute>
            } 
          />
          
          <Route 
            path="/collections" 
            element={
              <ProtectedRoute>
                <Collections />
              </ProtectedRoute>
            } 
          />
          
          {/* Admin Control Panel - Accessible for development purposes */}
          <Route 
            path="/admin-panel" 
            element={<AdminControlPanel />}
          />
          
          {/* Admin routes - Use the new AdminDashboard and nested routes */}
          <Route 
            path="/admin" 
            element={
              <ProtectedRoute requiredRole="admin">
                <AdminDashboard />
              </ProtectedRoute>
            } 
          >
            {/* Nested routes within the Admin Dashboard */}
            <Route 
              path="roles" 
              element={
                <ProtectedRoute requiredRole="admin">
                  <RoleManagement />
                </ProtectedRoute>
              } 
            />
            <Route 
              path="model-settings" 
              element={
                <ProtectedRoute requiredRole="admin">
                  <ModelUploadSettingsPage />
                </ProtectedRoute>
              } 
            />
          </Route>
          
          {/* Moderation routes */}
          <Route 
            path="/moderation" 
            element={
              <ProtectedRoute>
                <ModelModeration />
              </ProtectedRoute>
            } 
          />
          
          {/* Fallback route */}
          <Route path="*" element={<Navigate to="/" replace />} />
        </Routes>
      </Router>
    </>
  );
};

export default App;
