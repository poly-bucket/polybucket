import React, { useState, useEffect } from 'react';
import {
  Box,
  Typography,
  Button,
  Chip,
  Table,
  TableBody,
  TableCell,
  TableContainer,
  TableHead,
  TableRow,
  Paper,
  TablePagination,
  FormControl,
  InputLabel,
  Select,
  MenuItem,
  Stack,
  Dialog,
  DialogTitle,
  DialogContent,
  DialogActions,
  TextField,
  Alert,
  CircularProgress
} from '@mui/material';
import { 
  FlagIcon, 
  ClockIcon, 
  CheckCircleIcon, 
  EyeIcon 
} from '@heroicons/react/24/outline';
import { moderationService, Report, ReportsResponse } from '../../services/moderationService';

interface ReportsTabProps {
  canHandleReports: boolean;
}

export const ReportsTab: React.FC<ReportsTabProps> = ({ canHandleReports }) => {
  const [reports, setReports] = useState<Report[]>([]);
  const [loading, setLoading] = useState(true);
  const [error, setError] = useState<string | null>(null);
  const [page, setPage] = useState(0);
  const [rowsPerPage, setRowsPerPage] = useState(10);
  const [totalCount, setTotalCount] = useState(0);
  const [filter, setFilter] = useState<'all' | 'unresolved' | 'resolved'>('unresolved');
  const [typeFilter, setTypeFilter] = useState<string>('all');
  
  // Resolution dialog state
  const [selectedReport, setSelectedReport] = useState<Report | null>(null);
  const [resolutionDialogOpen, setResolutionDialogOpen] = useState(false);
  const [resolution, setResolution] = useState('');
  const [resolving, setResolving] = useState(false);

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
    fetchReports();
  }, [page, rowsPerPage, filter, typeFilter]);

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
      fetchReports(); // Refresh the list
    } catch (err) {
      console.error('Error resolving report:', err);
      // TODO: Show error toast
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

  if (loading && reports.length === 0) {
    return (
      <Box display="flex" justifyContent="center" alignItems="center" minHeight="200px">
        <CircularProgress />
      </Box>
    );
  }

  return (
    <Box>
      <Typography variant="h5" gutterBottom sx={{ display: 'flex', alignItems: 'center', gap: 1 }}>
        <FlagIcon className="w-6 h-6" />
        Reports Management
      </Typography>

      {error && (
        <Alert severity="error" sx={{ mb: 2 }}>
          {error}
        </Alert>
      )}

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
                    <Button
                      size="small"
                      startIcon={<EyeIcon className="w-4 h-4" />}
                      onClick={() => {
                        // TODO: Open report details dialog
                      }}
                    >
                      View
                    </Button>
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

      <TablePagination
        rowsPerPageOptions={[5, 10, 25]}
        component="div"
        count={totalCount}
        rowsPerPage={rowsPerPage}
        page={page}
        onPageChange={handlePageChange}
        onRowsPerPageChange={handleRowsPerPageChange}
      />

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