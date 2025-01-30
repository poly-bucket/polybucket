import { createBrowserRouter } from "react-router-dom";
import { LoginPage } from "../authentication/login/login";
import { routes } from "./routes";
import { AppLayout } from "./app-layout";
import { Dashboard } from "../dashboard/dashboard";
import { ModelDetail } from "../pages/model-detail/model-detail";
import { UserProfile } from '../pages/user-profile/user-profile';

export const router = createBrowserRouter([
  {
    element: <AppLayout />,
    path: routes.default,
    children: [
      {
        path: routes.login,
        element: <LoginPage />,
      },
      {
        path: routes.dashboard,
        element: <Dashboard />,
      },
      {
        path: routes.modelDetail,
        element: <ModelDetail />,
      },
      {
        path: "/users/:username",
        element: <UserProfile />,
      }
    ],
  },
]);