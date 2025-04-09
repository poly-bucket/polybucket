import React from 'react';
import { Outlet, Link } from 'react-router-dom';

const AdminDashboard: React.FC = () => {
  return (
    <div>
      <h1>Admin Control Panel</h1>
      <nav>
        <ul>
          <li>
            <Link to="roles">Role Management</Link>
          </li>
          <li>
            <Link to="model-settings">Model Upload Settings</Link>
          </li>
          {/* Add links to other admin sections here */}
        </ul>
      </nav>
      <hr />
      <Outlet /> {/* This will render the nested admin routes */}
    </div>
  );
};

export default AdminDashboard; 