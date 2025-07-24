import React, { useState, useEffect } from 'react';
import { useNavigate } from 'react-router-dom';
import { 
  ShieldCheckIcon, 
  FlagIcon, 
  UserMinusIcon, 
  ClockIcon 
} from '@heroicons/react/24/outline';
import { usePermissions } from '../../hooks/usePermissions';
import { useSimplePermissions } from '../../hooks/useSimplePermissions';
import { useAppSelector } from '../../utils/hooks';
import NavigationBar from '../common/NavigationBar';

import { ReportsDashboard } from './ReportsDashboard';
import { BannedUsersTab } from './BannedUsersTab';
import { AuditLogsTab } from './AuditLogsTab';

interface TabPanelProps {
  children?: React.ReactNode;
  index: number;
  value: number;
}

function TabPanel(props: TabPanelProps) {
  const { children, value, index, ...other } = props;

  return (
    <div
      role="tabpanel"
      hidden={value !== index}
      id={`moderation-tabpanel-${index}`}
      aria-labelledby={`moderation-tab-${index}`}
      {...other}
    >
      {value === index && (
        <div className="p-6">
          {children}
        </div>
      )}
    </div>
  );
}

export const ModerationDashboard: React.FC = () => {
  const navigate = useNavigate();
  const [activeTab, setActiveTab] = useState(0);
  const { permissions, hasPermission, hasMinimumRole, isAdmin, isModerator, loading } = usePermissions();
  const { isAdmin: simpleIsAdmin, isModerator: simpleIsModerator } = useSimplePermissions();
  const { user } = useAppSelector((state) => state.auth);

  const handleTabChange = (event: React.SyntheticEvent, newValue: number) => {
    setActiveTab(newValue);
  };

  const handleBackToDashboard = () => {
    navigate('/dashboard');
  };

  if (loading) {
    return (
      <div className="lg-container min-h-screen">
        {/* Navigation Bar */}
        <NavigationBar
          title="Moderation Dashboard"
          showSearch={false}
          showUploadButton={false}
          showHomeLink={true}
        />
        <div className="py-6">
          <div className="max-w-7xl mx-auto px-4 sm:px-6 lg:px-8">
            <div className="lg-card p-6">
              <div className="flex items-center justify-center">
                <div className="lg-spinner"></div>
                <span className="ml-3 text-lg text-white">Loading moderation dashboard...</span>
              </div>
            </div>
          </div>
        </div>
      </div>
    );
  }

  // Check if user has moderation permissions
  // Roles above moderator (Admin) should be able to see reports dashboard
  // Use fallback to simple permissions if detailed permissions are not loaded
  const isAdminUser = simpleIsAdmin() || (user?.roles?.includes('Admin') || false);
  const canViewReports = hasPermission('moderation.view.reports') || isAdmin() || isAdminUser;
  const canHandleReports = hasPermission('moderation.handle.reports') || isAdmin() || isAdminUser;
  const canBanUsers = hasPermission('admin.ban.users') || isAdmin() || isAdminUser;
  const canViewAuditLogs = hasPermission('moderation.view.audit_log') || isAdmin() || isAdminUser;

  // Debug logging
  console.log('ModerationDashboard - User:', user);
  console.log('ModerationDashboard - Permissions:', permissions);
  console.log('ModerationDashboard - isAdmin():', isAdmin());
  console.log('ModerationDashboard - simpleIsAdmin():', simpleIsAdmin());
  console.log('ModerationDashboard - hasPermission moderation.view.reports:', hasPermission('moderation.view.reports'));
  console.log('ModerationDashboard - canViewReports:', canViewReports);
  console.log('ModerationDashboard - canHandleReports:', canHandleReports);
  console.log('ModerationDashboard - canBanUsers:', canBanUsers);
  console.log('ModerationDashboard - canViewAuditLogs:', canViewAuditLogs);

  // Check if user has any moderation permissions at all
  // Allow admin users to access even if permissions are not loaded yet
  const hasAnyModerationPermission = canViewReports || canBanUsers || canViewAuditLogs;
  
  if (!hasAnyModerationPermission && !isAdminUser) {
    return (
      <div className="lg-container min-h-screen">
        <NavigationBar
          title="Moderation Dashboard"
          showSearch={false}
          showUploadButton={false}
          showHomeLink={true}
        />
        <div className="py-6">
          <div className="max-w-7xl mx-auto px-4 sm:px-6 lg:px-8">
            <div className="lg-card p-6">
              <div className="lg-badge-error p-4">
                <span className="text-lg">You do not have permission to access the moderation dashboard.</span>
              </div>
            </div>
          </div>
        </div>
      </div>
    );
  }

  return (
    <div className="lg-container min-h-screen">
      {/* Navigation Bar */}
      <NavigationBar
        title="Moderation Dashboard"
        showSearch={false}
        showUploadButton={false}
        showHomeLink={true}
      />

      {/* Main Content */}
      <div className="py-6">
        <div className="max-w-7xl mx-auto px-4 sm:px-6 lg:px-8">
          <div className="mb-6">
            <div className="flex items-center gap-3 mb-2">
              <ShieldCheckIcon className="w-8 h-8 text-indigo-400" />
              <h1 className="text-3xl font-bold text-white">Moderation Dashboard</h1>
            </div>
            <p className="text-lg text-white/80">
              Manage reports, banned users, and review moderation actions
            </p>
          </div>

          <div className="lg-card">
            <div className="border-b border-white/10">
              <nav className="flex space-x-8 px-6">
                {canViewReports && (
                  <button
                    onClick={() => setActiveTab(0)}
                    className={`py-4 px-1 border-b-2 font-medium text-sm capitalize transition-colors duration-200 flex items-center gap-2 ${
                      activeTab === 0
                        ? 'border-indigo-500 text-indigo-400'
                        : 'border-transparent text-white/60 hover:text-white/80 hover:border-white/20'
                    }`}
                  >
                    <FlagIcon className="w-5 h-5" />
                    Reports
                  </button>
                )}
                {canBanUsers && (
                  <button
                    onClick={() => setActiveTab(canViewReports ? 1 : 0)}
                    className={`py-4 px-1 border-b-2 font-medium text-sm capitalize transition-colors duration-200 flex items-center gap-2 ${
                      activeTab === (canViewReports ? 1 : 0)
                        ? 'border-indigo-500 text-indigo-400'
                        : 'border-transparent text-white/60 hover:text-white/80 hover:border-white/20'
                    }`}
                  >
                    <UserMinusIcon className="w-5 h-5" />
                    Banned Users
                  </button>
                )}
                {canViewAuditLogs && (
                  <button
                    onClick={() => setActiveTab((canViewReports ? 1 : 0) + (canBanUsers ? 1 : 0))}
                    className={`py-4 px-1 border-b-2 font-medium text-sm capitalize transition-colors duration-200 flex items-center gap-2 ${
                      activeTab === ((canViewReports ? 1 : 0) + (canBanUsers ? 1 : 0))
                        ? 'border-indigo-500 text-indigo-400'
                        : 'border-transparent text-white/60 hover:text-white/80 hover:border-white/20'
                    }`}
                  >
                    <ClockIcon className="w-5 h-5" />
                    Audit Logs
                  </button>
                )}
              </nav>
            </div>

            {canViewReports && (
              <TabPanel value={activeTab} index={0}>
                <ReportsDashboard canHandleReports={canHandleReports} />
              </TabPanel>
            )}

            {canBanUsers && (
              <TabPanel value={activeTab} index={canViewReports ? 1 : 0}>
                <BannedUsersTab />
              </TabPanel>
            )}

            {canViewAuditLogs && (
              <TabPanel value={activeTab} index={(canViewReports ? 1 : 0) + (canBanUsers ? 1 : 0)}>
                <AuditLogsTab />
              </TabPanel>
            )}
          </div>
        </div>
      </div>
    </div>
  );
}; 