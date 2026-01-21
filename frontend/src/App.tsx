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
import ProtectedRoute from './components/ProtectedRoute';

import AdminControlPanel from './acp/AdminControlPanel';
import ModelModeration from './mcp/ModelModeration';
import { ModerationDashboard } from './mcp/ModerationDashboard';

import ModelUpload from './models/ModelUpload';
import ModelDetails from './models/ModelDetails';
import { UserControlPanel } from './ucp';
import Collections from './collections/Collections';
import FirstTimeSetup from './setup/FirstTimeSetup';
import PublicDashboard from './dashboard/PublicDashboard';
import ThemeProvider from './context/ThemeContext';
import { UserSettingsProvider } from './context/UserSettingsContext';
import CreateCollection from './collections/CreateCollection';
import CollectionDetails from './collections/CollectionDetails';
import EditCollection from './collections/EditCollection';
import UserProfile from './ucp/UserProfile';
import MyModels from './ucp/MyModels';

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
                {/* Root route - redirect to dashboard */}
                <Route path="/" element={<Navigate to="/dashboard" replace />} />
                
                {/* Auth routes */}
                <Route path="/login" element={<LoginForm />} />
                <Route path="/register" element={<RegisterForm />} />
                <Route path="/setup" element={<FirstTimeSetup />} />
                
                {/* Public routes */}
                <Route path="/profile/:id" element={<UserProfile />} />
                
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
                      <UserControlPanel />
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
                  path="/my-models" 
                  element={
                    <ProtectedRoute>
                      <MyModels />
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
                
                {/* Admin routes - Use the new AdminControlPanel */}
                <Route 
                  path="/admin" 
                  element={
                    <ProtectedRoute requiredRole="admin">
                      <AdminControlPanel />
                    </ProtectedRoute>
                  } 
                />
                
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
                <Route path="*" element={<Navigate to="/dashboard" replace />} />
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
