import { createBrowserRouter } from "react-router-dom";
import { LoginPage } from "../authentication/login/login";
import { routes } from "./routes";
import { AppLayout } from "./app-layout";

export const router = createBrowserRouter([
  {
    element: <AppLayout />,
    path: routes.default,
    children: [
      {
        path: routes.login,
        element: <LoginPage />,
      },
    ],
  },
]);