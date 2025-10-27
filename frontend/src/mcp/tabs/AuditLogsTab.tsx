import React, { useState, useEffect } from 'react';
import {
  Box,
  Typography,
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
  Alert,
  CircularProgress,
  Chip,
  TextField
} from '@mui/material';
import { 
  ClockIcon,
  UserIcon,
  DocumentTextIcon,
  ExclamationTriangleIcon
} from '@heroicons/react/24/outline';
import { moderationService } from '../../services/moderationService';

interface AuditLog {
  id: string;
  modelId: string;
  performedByUserId: string;
  performedByUser: {
    username: string;
  };
  action: string;
  previousValues?: string;
  newValues?: string;
  moderationNotes?: string;
  performedAt: string;
  ipAddress?: string;
  userAgent?: string;
}

interface AuditLogsResponse {
  auditLogs: AuditLog[];
  totalCount: number;
  page: number;
  pageSize: number;
  totalPages: number;
}

export const AuditLogsTab: React.FC = () => {
  const [auditLogs, setAuditLogs] = useState<AuditLog[]>([]);
  const [loading, setLoading] = useState(true);
  const [error, setError] = useState<string | null>(null);
  const [page, setPage] = useState(0);
  const [rowsPerPage, setRowsPerPage] = useState(10);
  const [totalCount, setTotalCount] = useState(0);
  const [actionFilter, setActionFilter] = useState<string>('all');
  const [userFilter, setUserFilter] = useState<string>('');
  const [modelFilter, setModelFilter] = useState<string>('');

  const fetchAuditLogs = async () => {
    try {
      setLoading(true);
      setError(null);
      
      const action = actionFilter === 'all' ? undefined : actionFilter;
      const userId = userFilter.trim() || undefined;
      const modelId = modelFilter.trim() || undefined;
      
      const response: AuditLogsResponse = await moderationService.getModerationAuditLogs(
        page + 1,
        rowsPerPage,
        action,
        userId,
        modelId
      );
      
      setAuditLogs(response.auditLogs);
      setTotalCount(response.totalCount);
    } catch (err) {
      setError('Failed to fetch audit logs');
      console.error('Error fetching audit logs:', err);
    } finally {
      setLoading(false);
    }
  };

  useEffect(() => {
    fetchAuditLogs();
  }, [page, rowsPerPage, actionFilter, userFilter, modelFilter]);

  const handlePageChange = (event: unknown, newPage: number) => {
    setPage(newPage);
  };

  const handleRowsPerPageChange = (event: React.ChangeEvent<HTMLInputElement>) => {
    setRowsPerPage(parseInt(event.target.value, 10));
    setPage(0);
  };

  const handleActionFilterChange = (event: any) => {
    setActionFilter(event.target.value);
    setPage(0);
  };

  const getActionColor = (action: string) => {
    switch (action.toLowerCase()) {
      case 'edit': return 'info';
      case 'approvewithchanges': return 'success';
      case 'flagforreview': return 'warning';
      case 'featuremodel': return 'primary';
      case 'unfeaturemodel': return 'default';
      default: return 'default';
    }
  };

  const formatDate = (dateString: string) => {
    return new Date(dateString).toLocaleString();
  };

  const formatAction = (action: string) => {
    // Convert camelCase to readable format
    return action.replace(/([A-Z])/g, ' $1').replace(/^./, str => str.toUpperCase());
  };

  if (loading && auditLogs.length === 0) {
    return (
      <Box display="flex" justifyContent="center" alignItems="center" minHeight="200px">
        <CircularProgress />
      </Box>
    );
  }

  return (
    <Box>
      <Typography variant="h5" gutterBottom sx={{ display: 'flex', alignItems: 'center', gap: 1 }}>
        <ClockIcon className="w-6 h-6" />
        Moderation Audit Logs
      </Typography>

      {error && (
        <Alert severity="error" sx={{ mb: 2 }}>
          {error}
        </Alert>
      )}

      <Stack direction="row" spacing={2} sx={{ mb: 3 }}>
        <FormControl size="small" sx={{ minWidth: 150 }}>
          <InputLabel>Action</InputLabel>
          <Select value={actionFilter} label="Action" onChange={handleActionFilterChange}>
            <MenuItem value="all">All Actions</MenuItem>
            <MenuItem value="Edit">Edit</MenuItem>
            <MenuItem value="ApproveWithChanges">Approve with Changes</MenuItem>
            <MenuItem value="FlagForReview">Flag for Review</MenuItem>
            <MenuItem value="FeatureModel">Feature Model</MenuItem>
            <MenuItem value="UnfeatureModel">Unfeature Model</MenuItem>
          </Select>
        </FormControl>

        <TextField
          size="small"
          label="User ID"
          value={userFilter}
          onChange={(e) => setUserFilter(e.target.value)}
          placeholder="Filter by user ID..."
          sx={{ width: 200 }}
        />

        <TextField
          size="small"
          label="Model ID"
          value={modelFilter}
          onChange={(e) => setModelFilter(e.target.value)}
          placeholder="Filter by model ID..."
          sx={{ width: 200 }}
        />
      </Stack>

      <Box sx={{ mb: 2 }}>
        <Typography variant="body2" color="text.secondary">
          Total audit logs: {totalCount}
        </Typography>
      </Box>

      <TableContainer component={Paper}>
        <Table>
          <TableHead>
            <TableRow>
              <TableCell>Date</TableCell>
              <TableCell>User</TableCell>
              <TableCell>Action</TableCell>
              <TableCell>Model ID</TableCell>
              <TableCell>Notes</TableCell>
              <TableCell>IP Address</TableCell>
            </TableRow>
          </TableHead>
          <TableBody>
            {auditLogs.map((log) => (
              <TableRow key={log.id}>
                <TableCell>
                  <Typography variant="body2">
                    {formatDate(log.performedAt)}
                  </Typography>
                </TableCell>
                <TableCell>
                  <Box sx={{ display: 'flex', alignItems: 'center', gap: 1 }}>
                    <UserIcon className="w-4 h-4" />
                    <Typography variant="body2">
                      {log.performedByUser.username}
                    </Typography>
                  </Box>
                </TableCell>
                <TableCell>
                  <Chip
                    label={formatAction(log.action)}
                    color={getActionColor(log.action) as any}
                    size="small"
                  />
                </TableCell>
                <TableCell>
                  <Typography variant="body2" fontFamily="monospace">
                    {log.modelId}
                  </Typography>
                </TableCell>
                <TableCell>
                  <Typography variant="body2" sx={{ maxWidth: 200 }}>
                    {log.moderationNotes ? (
                      log.moderationNotes.length > 100
                        ? `${log.moderationNotes.substring(0, 100)}...`
                        : log.moderationNotes
                    ) : (
                      '-'
                    )}
                  </Typography>
                </TableCell>
                <TableCell>
                  <Typography variant="body2" fontFamily="monospace">
                    {log.ipAddress || '-'}
                  </Typography>
                </TableCell>
              </TableRow>
            ))}
            {auditLogs.length === 0 && !loading && (
              <TableRow>
                <TableCell colSpan={6} align="center">
                  <Box sx={{ py: 4 }}>
                    <DocumentTextIcon className="w-12 h-12" style={{ opacity: 0.5, marginBottom: 16 }} />
                    <Typography variant="h6" color="text.secondary">
                      No audit logs found
                    </Typography>
                    <Typography variant="body2" color="text.secondary">
                      No moderation actions have been logged yet
                    </Typography>
                  </Box>
                </TableCell>
              </TableRow>
            )}
          </TableBody>
        </Table>
      </TableContainer>

      <TablePagination
        rowsPerPageOptions={[5, 10, 25, 50]}
        component="div"
        count={totalCount}
        rowsPerPage={rowsPerPage}
        page={page}
        onPageChange={handlePageChange}
        onRowsPerPageChange={handleRowsPerPageChange}
      />
    </Box>
  );
}; 