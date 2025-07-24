import React, { useState, useEffect } from 'react';
import {
  Box,
  Typography,
  Grid,
  Card,
  CardContent,
  Paper,
  Table,
  TableBody,
  TableCell,
  TableContainer,
  TableHead,
  TableRow,
  Chip,
  Button,
  Stack,
  FormControl,
  InputLabel,
  Select,
  MenuItem,
  TextField,
  Alert,
  CircularProgress,
  Tabs,
  Tab,
  Dialog,
  DialogTitle,
  DialogContent,
  DialogActions,
  IconButton,
  Tooltip
} from '@mui/material';
import {
  FlagIcon,
  ChartBarIcon,
  UserGroupIcon,
  DocumentTextIcon,
  ClockIcon,
  CheckCircleIcon,
  XCircleIcon,
  ArchiveBoxIcon,
  EyeIcon,
  ExclamationTriangleIcon,
  InformationCircleIcon
} from '@heroicons/react/24/outline';
import { moderationService, Report, ReportsResponse } from '../../services/moderationService';

interface ReportsDashboardProps {
  canHandleReports: boolean;
}

interface ReportsAnalytics {
  totalReports: number;
  activeReports: number;
  resolvedReports: number;
  dismissedReports: number;
  archivedReports: number;
  lastUpdated: string;
  dailyTrends: ReportTrend[];
  weeklyTrends: ReportTrend[];
  monthlyTrends: ReportTrend[];
  topReportedModels: TopReportedItem[];
  topReportedUsers: TopReportedItem[];
  topReportedComments: TopReportedItem[];
  reasonStatistics: ReportReasonStats[];
  typeStatistics: ReportTypeStats[];
  moderatorActivity: ModeratorActivity[];
}

interface ReportTrend {
  date: string;
  totalReports: number;
  resolvedReports: number;
  activeReports: number;
}

interface TopReportedItem {
  id: string;
  name: string;
  reportCount: number;
  lastReported: string;
  isResolved: boolean;
  resolution?: string;
}

interface ReportReasonStats {
  reason: string;
  count: number;
  percentage: number;
}

interface ReportTypeStats {
  type: string;
  count: number;
  percentage: number;
}

interface ModeratorActivity {
  moderatorId: string;
  moderatorName: string;
  reportsResolved: number;
  reportsDismissed: number;
  reportsArchived: number;
  lastActivity: string;
  averageResolutionTime: number;
}

export const ReportsDashboard: React.FC<ReportsDashboardProps> = ({ canHandleReports }) => {
  const [activeTab, setActiveTab] = useState(0);
  const [analytics, setAnalytics] = useState<ReportsAnalytics | null>(null);
  const [reports, setReports] = useState<Report[]>([]);
  const [loading, setLoading] = useState(true);
  const [analyticsLoading, setAnalyticsLoading] = useState(true);
  const [error, setError] = useState<string | null>(null);
  const [page, setPage] = useState(0);
  const [rowsPerPage, setRowsPerPage] = useState(10);
  const [totalCount, setTotalCount] = useState(0);
  const [filter, setFilter] = useState<'all' | 'unresolved' | 'resolved'>('unresolved');
  const [typeFilter, setTypeFilter] = useState<string>('all');
  const [dateRange, setDateRange] = useState<{ from: string; to: string }>({
    from: new Date(Date.now() - 30 * 24 * 60 * 60 * 1000).toISOString().split('T')[0],
    to: new Date().toISOString().split('T')[0]
  });

  // Resolution dialog state
  const [selectedReport, setSelectedReport] = useState<Report | null>(null);
  const [resolutionDialogOpen, setResolutionDialogOpen] = useState(false);
  const [resolution, setResolution] = useState('');
  const [resolving, setResolving] = useState(false);

  const fetchAnalytics = async () => {
    try {
      setAnalyticsLoading(true);
      const fromDate = dateRange.from ? new Date(dateRange.from) : undefined;
      const toDate = dateRange.to ? new Date(dateRange.to) : undefined;
      
      const analytics = await moderationService.getReportsAnalytics(fromDate, toDate);
      setAnalytics(analytics);
    } catch (err) {
      console.error('Error fetching analytics:', err);
    } finally {
      setAnalyticsLoading(false);
    }
  };

  const fetchReports = async () => {
    try {
      setLoading(true);
      setError(null);
      
      const isResolved = filter === 'all' ? undefined : filter === 'resolved';
      const type = typeFilter === 'all' ? undefined : typeFilter;
      
      const response: ReportsResponse = await moderationService.getAllReports(
        page + 1,
        rowsPerPage,
        isResolved,
        type
      );
      
      setReports(response.reports);
      setTotalCount(response.totalCount);
    } catch (err) {
      setError('Failed to fetch reports');
      console.error('Error fetching reports:', err);
    } finally {
      setLoading(false);
    }
  };

  useEffect(() => {
    fetchAnalytics();
  }, [dateRange]);

  useEffect(() => {
    fetchReports();
  }, [page, rowsPerPage, filter, typeFilter]);

  const handleTabChange = (event: React.SyntheticEvent, newValue: number) => {
    setActiveTab(newValue);
  };

  const handlePageChange = (event: unknown, newPage: number) => {
    setPage(newPage);
  };

  const handleRowsPerPageChange = (event: React.ChangeEvent<HTMLInputElement>) => {
    setRowsPerPage(parseInt(event.target.value, 10));
    setPage(0);
  };

  const handleFilterChange = (newFilter: 'all' | 'unresolved' | 'resolved') => {
    setFilter(newFilter);
    setPage(0);
  };

  const handleTypeFilterChange = (event: any) => {
    setTypeFilter(event.target.value);
    setPage(0);
  };

  const openResolutionDialog = (report: Report) => {
    setSelectedReport(report);
    setResolution('');
    setResolutionDialogOpen(true);
  };

  const closeResolutionDialog = () => {
    setSelectedReport(null);
    setResolutionDialogOpen(false);
    setResolution('');
  };

  const handleResolveReport = async () => {
    if (!selectedReport || !resolution.trim()) return;

    try {
      setResolving(true);
      await moderationService.resolveReport(selectedReport.id, resolution);
      closeResolutionDialog();
      fetchReports();
      fetchAnalytics();
    } catch (err) {
      console.error('Error resolving report:', err);
    } finally {
      setResolving(false);
    }
  };

  const getReasonColor = (reason: string) => {
    switch (reason) {
      case 'Inappropriate': return 'error';
      case 'Spam': return 'warning';
      case 'Copyright': return 'info';
      case 'Malware': return 'error';
      default: return 'default';
    }
  };

  const getTypeColor = (type: string) => {
    switch (type) {
      case 'Model': return 'primary';
      case 'Comment': return 'secondary';
      case 'User': return 'warning';
      case 'Collection': return 'info';
      default: return 'default';
    }
  };

  const formatDate = (dateString: string) => {
    return new Date(dateString).toLocaleString();
  };

  const formatDuration = (hours: number) => {
    if (hours < 1) return '< 1 hour';
    if (hours < 24) return `${Math.round(hours)} hours`;
    return `${Math.round(hours / 24)} days`;
  };

  if (loading && reports.length === 0) {
    return (
      <Box display="flex" justifyContent="center" alignItems="center" minHeight="400px">
        <CircularProgress />
      </Box>
    );
  }

  return (
    <Box>
      <Typography variant="h5" gutterBottom sx={{ display: 'flex', alignItems: 'center', gap: 1 }}>
        <FlagIcon className="w-6 h-6" />
        Reports Dashboard
      </Typography>

      {error && (
        <Alert severity="error" sx={{ mb: 2 }}>
          {error}
        </Alert>
      )}

      {/* Analytics Overview Cards */}
      <Grid container spacing={3} sx={{ mb: 3 }}>
        <Grid item xs={12} sm={6} md={3}>
          <Card>
            <CardContent>
              <Box display="flex" alignItems="center" justifyContent="space-between">
                <Box>
                  <Typography color="textSecondary" gutterBottom>
                    Total Reports
                  </Typography>
                  <Typography variant="h4">
                    {analyticsLoading ? <CircularProgress size={20} /> : analytics?.totalReports || 0}
                  </Typography>
                </Box>
                <FlagIcon className="w-8 h-8 text-blue-500" />
              </Box>
            </CardContent>
          </Card>
        </Grid>
        <Grid item xs={12} sm={6} md={3}>
          <Card>
            <CardContent>
              <Box display="flex" alignItems="center" justifyContent="space-between">
                <Box>
                  <Typography color="textSecondary" gutterBottom>
                    Active Reports
                  </Typography>
                  <Typography variant="h4" color="warning.main">
                    {analyticsLoading ? <CircularProgress size={20} /> : analytics?.activeReports || 0}
                  </Typography>
                </Box>
                <ExclamationTriangleIcon className="w-8 h-8 text-orange-500" />
              </Box>
            </CardContent>
          </Card>
        </Grid>
        <Grid item xs={12} sm={6} md={3}>
          <Card>
            <CardContent>
              <Box display="flex" alignItems="center" justifyContent="space-between">
                <Box>
                  <Typography color="textSecondary" gutterBottom>
                    Resolved
                  </Typography>
                  <Typography variant="h4" color="success.main">
                    {analyticsLoading ? <CircularProgress size={20} /> : analytics?.resolvedReports || 0}
                  </Typography>
                </Box>
                <CheckCircleIcon className="w-8 h-8 text-green-500" />
              </Box>
            </CardContent>
          </Card>
        </Grid>
        <Grid item xs={12} sm={6} md={3}>
          <Card>
            <CardContent>
              <Box display="flex" alignItems="center" justifyContent="space-between">
                <Box>
                  <Typography color="textSecondary" gutterBottom>
                    Dismissed
                  </Typography>
                  <Typography variant="h4" color="info.main">
                    {analyticsLoading ? <CircularProgress size={20} /> : analytics?.dismissedReports || 0}
                  </Typography>
                </Box>
                <XCircleIcon className="w-8 h-8 text-blue-500" />
              </Box>
            </CardContent>
          </Card>
        </Grid>
      </Grid>

      {/* Date Range Filter */}
      <Paper sx={{ p: 2, mb: 3 }}>
        <Stack direction="row" spacing={2} alignItems="center">
          <Typography variant="subtitle2">Date Range:</Typography>
          <TextField
            type="date"
            value={dateRange.from}
            onChange={(e) => setDateRange(prev => ({ ...prev, from: e.target.value }))}
            size="small"
          />
          <Typography>to</Typography>
          <TextField
            type="date"
            value={dateRange.to}
            onChange={(e) => setDateRange(prev => ({ ...prev, to: e.target.value }))}
            size="small"
          />
          <Button variant="outlined" onClick={fetchAnalytics}>
            Update Analytics
          </Button>
        </Stack>
      </Paper>

      {/* Main Content Tabs */}
      <Paper sx={{ width: '100%' }}>
        <Tabs value={activeTab} onChange={handleTabChange} aria-label="reports dashboard tabs">
          <Tab icon={<ChartBarIcon className="w-5 h-5" />} label="Analytics" />
          <Tab icon={<FlagIcon className="w-5 h-5" />} label="Reports" />
          <Tab icon={<UserGroupIcon className="w-5 h-5" />} label="Top Reported" />
          <Tab icon={<DocumentTextIcon className="w-5 h-5" />} label="Moderator Activity" />
        </Tabs>

        {/* Analytics Tab */}
        {activeTab === 0 && (
          <Box sx={{ p: 3 }}>
            <Typography variant="h6" gutterBottom>
              Reports Analytics Overview
            </Typography>
            <Typography variant="body2" color="text.secondary" paragraph>
              Analytics data will be displayed here once the backend implementation is complete.
            </Typography>
            <Box display="flex" alignItems="center" justifyContent="center" height={200}>
              <Typography variant="h6" color="text.secondary">
                Analytics Dashboard Coming Soon
              </Typography>
            </Box>
          </Box>
        )}

        {/* Reports Tab */}
        {activeTab === 1 && (
          <Box sx={{ p: 3 }}>
            <Stack direction="row" spacing={2} sx={{ mb: 3 }}>
              <Button
                variant={filter === 'unresolved' ? 'contained' : 'outlined'}
                onClick={() => handleFilterChange('unresolved')}
                startIcon={<ClockIcon className="w-4 h-4" />}
              >
                Active Reports
              </Button>
              <Button
                variant={filter === 'resolved' ? 'contained' : 'outlined'}
                onClick={() => handleFilterChange('resolved')}
                startIcon={<CheckCircleIcon className="w-4 h-4" />}
              >
                Resolved Reports
              </Button>
              <Button
                variant={filter === 'all' ? 'contained' : 'outlined'}
                onClick={() => handleFilterChange('all')}
              >
                All Reports
              </Button>

              <FormControl size="small" sx={{ minWidth: 120 }}>
                <InputLabel>Type</InputLabel>
                <Select value={typeFilter} label="Type" onChange={handleTypeFilterChange}>
                  <MenuItem value="all">All Types</MenuItem>
                  <MenuItem value="Model">Models</MenuItem>
                  <MenuItem value="Comment">Comments</MenuItem>
                  <MenuItem value="User">Users</MenuItem>
                  <MenuItem value="Collection">Collections</MenuItem>
                </Select>
              </FormControl>
            </Stack>

            <TableContainer component={Paper}>
              <Table>
                <TableHead>
                  <TableRow>
                    <TableCell>Type</TableCell>
                    <TableCell>Reason</TableCell>
                    <TableCell>Description</TableCell>
                    <TableCell>Status</TableCell>
                    <TableCell>Created</TableCell>
                    <TableCell>Resolved</TableCell>
                    <TableCell>Actions</TableCell>
                  </TableRow>
                </TableHead>
                <TableBody>
                  {reports.map((report) => (
                    <TableRow key={report.id}>
                      <TableCell>
                        <Chip
                          label={report.type}
                          color={getTypeColor(report.type) as any}
                          size="small"
                        />
                      </TableCell>
                      <TableCell>
                        <Chip
                          label={report.reason}
                          color={getReasonColor(report.reason) as any}
                          size="small"
                        />
                      </TableCell>
                      <TableCell>
                        <Typography variant="body2" sx={{ maxWidth: 200 }}>
                          {report.description.length > 100
                            ? `${report.description.substring(0, 100)}...`
                            : report.description}
                        </Typography>
                      </TableCell>
                      <TableCell>
                        <Chip
                          label={report.isResolved ? 'Resolved' : 'Active'}
                          color={report.isResolved ? 'success' : 'warning'}
                          size="small"
                        />
                      </TableCell>
                      <TableCell>
                        <Typography variant="body2">
                          {formatDate(report.createdAt)}
                        </Typography>
                      </TableCell>
                      <TableCell>
                        {report.resolvedAt ? (
                          <Typography variant="body2">
                            {formatDate(report.resolvedAt)}
                          </Typography>
                        ) : (
                          '-'
                        )}
                      </TableCell>
                      <TableCell>
                        <Stack direction="row" spacing={1}>
                          <Tooltip title="View Details">
                            <IconButton size="small">
                              <EyeIcon className="w-4 h-4" />
                            </IconButton>
                          </Tooltip>
                          {canHandleReports && !report.isResolved && (
                            <Button
                              size="small"
                              variant="contained"
                              color="primary"
                              onClick={() => openResolutionDialog(report)}
                            >
                              Resolve
                            </Button>
                          )}
                        </Stack>
                      </TableCell>
                    </TableRow>
                  ))}
                </TableBody>
              </Table>
            </TableContainer>
          </Box>
        )}

        {/* Top Reported Tab */}
        {activeTab === 2 && (
          <Box sx={{ p: 3 }}>
            <Typography variant="h6" gutterBottom>
              Top Reported Items
            </Typography>
            <Typography variant="body2" color="text.secondary" paragraph>
              Top reported items will be displayed here once the backend implementation is complete.
            </Typography>
          </Box>
        )}

        {/* Moderator Activity Tab */}
        {activeTab === 3 && (
          <Box sx={{ p: 3 }}>
            <Typography variant="h6" gutterBottom>
              Moderator Activity
            </Typography>
            <Typography variant="body2" color="text.secondary" paragraph>
              Moderator activity statistics will be displayed here once the backend implementation is complete.
            </Typography>
          </Box>
        )}
      </Paper>

      {/* Resolution Dialog */}
      <Dialog open={resolutionDialogOpen} onClose={closeResolutionDialog} maxWidth="md" fullWidth>
        <DialogTitle>Resolve Report</DialogTitle>
        <DialogContent>
          {selectedReport && (
            <Box sx={{ mb: 2 }}>
              <Typography variant="subtitle2" gutterBottom>
                Report Details:
              </Typography>
              <Typography variant="body2" color="text.secondary" paragraph>
                <strong>Type:</strong> {selectedReport.type} | <strong>Reason:</strong> {selectedReport.reason}
              </Typography>
              <Typography variant="body2" color="text.secondary" paragraph>
                <strong>Description:</strong> {selectedReport.description}
              </Typography>
              <Typography variant="body2" color="text.secondary" paragraph>
                <strong>Reported:</strong> {formatDate(selectedReport.createdAt)}
              </Typography>
            </Box>
          )}
          <TextField
            autoFocus
            margin="dense"
            label="Resolution Notes"
            fullWidth
            multiline
            rows={4}
            variant="outlined"
            value={resolution}
            onChange={(e) => setResolution(e.target.value)}
            placeholder="Describe the action taken to resolve this report..."
          />
        </DialogContent>
        <DialogActions>
          <Button onClick={closeResolutionDialog}>Cancel</Button>
          <Button
            onClick={handleResolveReport}
            variant="contained"
            disabled={!resolution.trim() || resolving}
          >
            {resolving ? <CircularProgress size={20} /> : 'Resolve Report'}
          </Button>
        </DialogActions>
      </Dialog>
    </Box>
  );
}; 