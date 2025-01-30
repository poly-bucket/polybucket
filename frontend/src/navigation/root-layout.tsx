import { AuthProvider } from '../authentication/auth-context';
import { AppLayout } from './app-layout';
import { Outlet } from 'react-router-dom';

export const RootLayout = () => {
  return (
    <AuthProvider>
      <AppLayout>
        <Outlet />
      </AppLayout>
    </AuthProvider>
  );
}; 