import { createBrowserRouter, Navigate } from "react-router-dom";
import { routes } from "./routes";
import { RootLayout } from "./root-layout";
import { LoginPage } from "../authentication/login/login";
import { Dashboard } from "../dashboard/dashboard";
import { ModelDetails } from "../pages/model-details/model-details";
import { UserProfile } from '../pages/user-profile/user-profile';
import { ProtectedRoute } from "./protected-route";
import { Profile } from '../profile/profile';

export const router = createBrowserRouter([
  {
    element: <RootLayout />,
    children: [
      {
        path: routes.default,
        element: <Navigate to={routes.dashboard} replace />
      },
      {
        path: routes.dashboard,
        element: (
          <ProtectedRoute>
            <Dashboard />
          </ProtectedRoute>
        ),
      },
      {
        path: routes.login,
        element: <LoginPage />,
      },
      {
        path: routes.modelDetail,
        element: (
          <ProtectedRoute>
            <ModelDetails />
          </ProtectedRoute>
        ),
      },
      {
        path: "/users/:username",
        element: (
          <ProtectedRoute>
            <UserProfile />
          </ProtectedRoute>
        ),
      },
      {
        path: routes.profile,
        element: <Profile />
      },
    ],
  },
]);