import { Outlet } from "react-router-dom";

export const AppLayout: React.FC = () => {
  return (
    <div>
      <h1>App Layout</h1>
      <Outlet />
    </div>
  );
};