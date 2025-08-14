import React, { useState, useEffect } from 'react';
import { useNavigate } from 'react-router-dom';
import { 
  ShieldCheckIcon, 
  FlagIcon, 
  UserMinusIcon, 
  ClockIcon,
  ArrowLeftIcon
} from '@heroicons/react/24/outline';
import { usePermissions } from '../../hooks/usePermissions';
import { useSimplePermissions } from '../../hooks/useSimplePermissions';
import { useAppSelector } from '../../utils/hooks';
import NavigationBar from '../common/NavigationBar';

import { 
  ReportsDashboard, 
  BannedUsersTab, 
  AuditLogsTab 
} from './tabs';

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
        <div>
          {children}
        </div>
      )}
    </div>
  );
}

export const ModerationDashboard: React.FC = () => {
  const navigate = useNavigate();
  const [tabValue, setTabValue] = useState(0);
  const { permissions, hasPermission, hasMinimumRole, isAdmin, isModerator, loading } = usePermissions();
  const { isAdmin: simpleIsAdmin, isModerator: simpleIsModerator } = useSimplePermissions();
  const { user } = useAppSelector((state) => state.auth);

  const handleTabChange = (event: React.SyntheticEvent, newValue: number) => {
    setTabValue(newValue);
  };

  const handleBackToDashboard = () => {
    navigate('/dashboard');
  };

  if (loading) {
    return (
      <div className="lg-container min-h-screen">
        <div className="py-6">
          <div className="max-w-7xl mx-auto px-4 sm:px-6 lg:px-8">
            <div className="lg-card p-6">
              <div className="flex items-center justify-center">
                <div className="lg-spinner h-8 w-8"></div>
                <span className="ml-3 text-lg text-white">Loading moderation dashboard...</span>
              </div>
            </div>
          </div>
        </div>
      </div>
    );
  }

  const isAdminUser = simpleIsAdmin() || (user?.roles?.includes('Admin') || false);
  const canViewReports = hasPermission('moderation.view.reports') || isAdmin() || isAdminUser;
  const canHandleReports = hasPermission('moderation.handle.reports') || isAdmin() || isAdminUser;
  const canBanUsers = hasPermission('admin.ban.users') || isAdmin() || isAdminUser;
  const canViewAuditLogs = hasPermission('moderation.view.audit_log') || isAdmin() || isAdminUser;

  console.log('ModerationDashboard - User:', user);
  console.log('ModerationDashboard - Permissions:', permissions);
  console.log('ModerationDashboard - isAdmin():', isAdmin());
  console.log('ModerationDashboard - simpleIsAdmin():', simpleIsAdmin());
  console.log('ModerationDashboard - hasPermission moderation.view.reports:', hasPermission('moderation.view.reports'));
  console.log('ModerationDashboard - canViewReports:', canViewReports);
  console.log('ModerationDashboard - canHandleReports:', canHandleReports);
  console.log('ModerationDashboard - canBanUsers:', canBanUsers);
  console.log('ModerationDashboard - canViewAuditLogs:', canViewAuditLogs);

  const hasAnyModerationPermission = canViewReports || canBanUsers || canViewAuditLogs;
  
  if (!hasAnyModerationPermission && !isAdminUser) {
    return (
      <div className="lg-container min-h-screen">
        <div className="py-6">
          <div className="max-w-7xl mx-auto px-4 sm:px-6 lg:px-8">
            <div className="lg-card p-6">
              <div className="lg-badge lg-badge-error p-4">
                <span className="text-lg text-white">You do not have permission to access the moderation dashboard.</span>
              </div>
            </div>
          </div>
        </div>
      </div>
    );
  }

  const tabs = [
    ...(canViewReports ? [{
      label: 'Reports',
      icon: <FlagIcon className="w-5 h-5" />,
      component: <ReportsDashboard canHandleReports={canHandleReports} />
    }] : []),
    ...(canBanUsers ? [{
      label: 'Banned Users',
      icon: <UserMinusIcon className="w-5 h-5" />,
      component: <BannedUsersTab />
    }] : []),
    ...(canViewAuditLogs ? [{
      label: 'Audit Logs',
      icon: <ClockIcon className="w-5 h-5" />,
      component: <AuditLogsTab />
    }] : [])
  ];

  return (
    <div className="lg-container min-h-screen">
      {/* Navigation Bar */}
      <NavigationBar
        title="Moderation Dashboard"
        showSearch={false}
        showUploadButton={false}
        showHomeLink={true}
      />

      {/* Navigation Tabs */}
      <div className="lg-tabs">
        <div className="max-w-7xl mx-auto px-4 sm:px-6 lg:px-8">
          <nav>
            {tabs.map((tab, index) => (
              <button
                key={index}
                onClick={() => setTabValue(index)}
                className={`lg-tab-button ${tabValue === index ? 'active' : ''}`}
              >
                {tab.icon}
                {tab.label}
              </button>
            ))}
          </nav>
        </div>
      </div>
      
      {/* Main Content Area */}
      <div className="max-w-7xl mx-auto px-4 sm:px-6 lg:px-8">
        {tabs.map((tab, index) => (
          <TabPanel key={index} value={tabValue} index={index}>
            <div className="lg-tab-panel">
              {tab.component}
            </div>
          </TabPanel>
        ))}
      </div>
    </div>
  );
}; 