import React, { Suspense, lazy } from 'react';
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
import ThemeProvider from './context/ThemeContext';
import { UserSettingsProvider } from './context/UserSettingsContext';

// Lazy load route components for code splitting
const AdminControlPanel = lazy(() => import('./acp/AdminControlPanel'));
const ModelModeration = lazy(() => import('./mcp/ModelModeration'));
const ModerationDashboard = lazy(() => import('./mcp/ModerationDashboard').then(module => ({ default: module.ModerationDashboard })));
const ModelUpload = lazy(() => import('./models/ModelUpload'));
const ModelDetails = lazy(() => import('./models/ModelDetails'));
const UserControlPanel = lazy(() => import('./ucp').then(module => ({ default: module.UserControlPanel })));
const Collections = lazy(() => import('./collections/Collections'));
const FirstTimeSetup = lazy(() => import('./setup/FirstTimeSetup'));
const PublicDashboard = lazy(() => import('./dashboard/PublicDashboard'));
const CreateCollection = lazy(() => import('./collections/CreateCollection'));
const CollectionDetails = lazy(() => import('./collections/CollectionDetails'));
const EditCollection = lazy(() => import('./collections/EditCollection'));
const UserProfile = lazy(() => import('./ucp/UserProfile'));
const MyModels = lazy(() => import('./ucp/MyModels'));

// Loading component for Suspense fallback
const RouteLoadingFallback: React.FC = () => (
  <div className="min-h-screen flex items-center justify-center">
    <div className="text-center">
      <div className="lg-spinner h-12 w-12 mx-auto mb-4"></div>
      <p className="text-white/60">Loading...</p>
    </div>
  </div>
);

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
                <Route 
                  path="/setup" 
                  element={
                    <Suspense fallback={<RouteLoadingFallback />}>
                      <FirstTimeSetup />
                    </Suspense>
                  } 
                />
                
                {/* Public routes */}
                <Route 
                  path="/profile/:id" 
                  element={
                    <Suspense fallback={<RouteLoadingFallback />}>
                      <UserProfile />
                    </Suspense>
                  } 
                />
                
                {/* Protected routes */}
                <Route 
                  path="/dashboard" 
                  element={
                    <Suspense fallback={<RouteLoadingFallback />}>
                      <PublicDashboard />
                    </Suspense>
                  }
                />
                
                <Route 
                  path="/upload-model" 
                  element={
                    <ProtectedRoute>
                      <Suspense fallback={<RouteLoadingFallback />}>
                        <ModelUpload />
                      </Suspense>
                    </ProtectedRoute>
                  } 
                />
                
                <Route 
                  path="/models/:id" 
                  element={
                    <ProtectedRoute>
                      <Suspense fallback={<RouteLoadingFallback />}>
                        <ModelDetails />
                      </Suspense>
                    </ProtectedRoute>
                  } 
                />
                
                <Route 
                  path="/settings" 
                  element={
                    <ProtectedRoute>
                      <Suspense fallback={<RouteLoadingFallback />}>
                        <UserControlPanel />
                      </Suspense>
                    </ProtectedRoute>
                  } 
                />
                
                <Route 
                  path="/my-collections" 
                  element={
                    <ProtectedRoute>
                      <Suspense fallback={<RouteLoadingFallback />}>
                        <Collections />
                      </Suspense>
                    </ProtectedRoute>
                  } 
                />
                
                <Route 
                  path="/my-models" 
                  element={
                    <ProtectedRoute>
                      <Suspense fallback={<RouteLoadingFallback />}>
                        <MyModels />
                      </Suspense>
                    </ProtectedRoute>
                  } 
                />
                
                <Route 
                  path="/my-collections/create" 
                  element={
                    <ProtectedRoute>
                      <Suspense fallback={<RouteLoadingFallback />}>
                        <CreateCollection />
                      </Suspense>
                    </ProtectedRoute>
                  } 
                />
                
                <Route 
                  path="/my-collections/:id" 
                  element={
                    <ProtectedRoute>
                      <Suspense fallback={<RouteLoadingFallback />}>
                        <CollectionDetails />
                      </Suspense>
                    </ProtectedRoute>
                  } 
                />
                
                <Route 
                  path="/my-collections/:id/edit" 
                  element={
                    <ProtectedRoute>
                      <Suspense fallback={<RouteLoadingFallback />}>
                        <EditCollection />
                      </Suspense>
                    </ProtectedRoute>
                  } 
                />
                
                {/* Admin routes - Use the new AdminControlPanel */}
                <Route 
                  path="/admin" 
                  element={
                    <ProtectedRoute requiredRole="admin">
                      <Suspense fallback={<RouteLoadingFallback />}>
                        <AdminControlPanel />
                      </Suspense>
                    </ProtectedRoute>
                  } 
                />
                
                {/* Moderation routes */}
                <Route 
                  path="/moderation" 
                  element={
                    <ProtectedRoute>
                      <Suspense fallback={<RouteLoadingFallback />}>
                        <ModelModeration />
                      </Suspense>
                    </ProtectedRoute>
                  } 
                />
                
                <Route 
                  path="/moderation-dashboard" 
                  element={
                    <ProtectedRoute>
                      <Suspense fallback={<RouteLoadingFallback />}>
                        <ModerationDashboard />
                      </Suspense>
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
