import React, { useEffect } from 'react';
import { BrowserRouter as Router, Routes, Route, Navigate } from 'react-router-dom';
import { ThemeProvider as MuiThemeProvider, CssBaseline } from '@mui/material';
import { Provider } from 'react-redux';
import { PersistGate } from 'redux-persist/integration/react';
import { liquidGlassTheme } from './theme/muiTheme';
import store, { persistor } from './store';
import AppInitializer from './components/AppInitializer';
import LoginForm from './components/auth/LoginForm';
import RegisterForm from './components/auth/RegisterForm';
import Dashboard from './components/Dashboard';
import RootRedirect from './components/RootRedirect';
import ProtectedRoute from './components/ProtectedRoute';
import AdminDashboard from './pages/admin/AdminDashboard';
import AdminControlPanel from './pages/admin/AdminControlPanel';
import ModelModeration from './components/moderation/ModelModeration';
import { ModerationDashboard } from './components/moderation/ModerationDashboard';
import ModelUploadSettings from './components/admin/ModelUploadSettings';
import ModerationSettings from './components/admin/ModerationSettings';
import RoleManagement from './pages/admin/RoleManagement';
import ModelUploadSettingsPage from './pages/admin/ModelUploadSettings';
import ModelUpload from './pages/ModelUpload';
import ModelDetails from './pages/ModelDetails';
import UserSettings from './pages/UserSettings';
import Collections from './pages/Collections';
import AvatarDemo from './components/AvatarDemo';
import LiquidGlassDemo from './components/LiquidGlassDemo';
import FirstTimeSetup from './components/setup/FirstTimeSetup';
import TestModels from './components/TestModels';
import PublicDashboard from './components/PublicDashboard';
import ThemeProvider from './context/ThemeContext';
import { UserSettingsProvider } from './context/UserSettingsContext';
import CreateCollection from './pages/CreateCollection';
import CollectionDetails from './pages/CollectionDetails';
import EditCollection from './pages/EditCollection';

const App: React.FC = () => {
  return (
    <Provider store={store}>
      <PersistGate loading={null} persistor={persistor}>
        <MuiThemeProvider theme={liquidGlassTheme}>
          <CssBaseline />
          <ThemeProvider>
            <UserSettingsProvider>
              <div className="lg-container">
                <AppInitializer>
                  <Router>
              <Routes>
                {/* Root route - determines where to go */}
                <Route path="/" element={<RootRedirect />} />
                
                {/* Auth routes */}
                <Route path="/login" element={<LoginForm />} />
                <Route path="/register" element={<RegisterForm />} />
                <Route path="/setup" element={<FirstTimeSetup />} />
                
                {/* Demo routes (development only) */}
                <Route path="/avatar-demo" element={<AvatarDemo />} />
                <Route path="/liquid-glass-demo" element={<LiquidGlassDemo />} />
                <Route path="/test-models" element={<TestModels />} />
                <Route path="/public-dashboard" element={<PublicDashboard />} />
                
                {/* Protected routes */}
                <Route 
                  path="/dashboard" 
                  element={<PublicDashboard />}
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
                  path="/my-collections" 
                  element={
                    <ProtectedRoute>
                      <Collections />
                    </ProtectedRoute>
                  } 
                />
                
                <Route 
                  path="/my-collections/create" 
                  element={
                    <ProtectedRoute>
                      <CreateCollection />
                    </ProtectedRoute>
                  } 
                />
                
                <Route 
                  path="/my-collections/:id" 
                  element={
                    <ProtectedRoute>
                      <CollectionDetails />
                    </ProtectedRoute>
                  } 
                />
                
                <Route 
                  path="/my-collections/:id/edit" 
                  element={
                    <ProtectedRoute>
                      <EditCollection />
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
                  <Route 
                    path="moderation-settings" 
                    element={
                      <ProtectedRoute requiredRole="admin">
                        <ModerationSettings />
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
                
                <Route 
                  path="/moderation-dashboard" 
                  element={
                    <ProtectedRoute>
                      <ModerationDashboard />
                    </ProtectedRoute>
                  } 
                />
                
                {/* Fallback route */}
                <Route path="*" element={<Navigate to="/" replace />} />
              </Routes>
            </Router>
                </AppInitializer>
              </div>
            </UserSettingsProvider>
          </ThemeProvider>
        </MuiThemeProvider>
      </PersistGate>
    </Provider>
  );
};

export default App;
