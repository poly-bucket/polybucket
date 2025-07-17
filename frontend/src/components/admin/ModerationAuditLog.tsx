import React, { useState, useEffect } from 'react';
import {
  Box,
  Card,
  CardContent,
  Typography,
  Table,
  TableBody,
  TableCell,
  TableContainer,
  TableHead,
  TableRow,
  Paper,
  Chip,
  Dialog,
  DialogTitle,
  DialogContent,
  DialogActions,
  Button,
  TextField,
  Pagination,
  CircularProgress,
  Alert,
  FormControl,
  InputLabel,
  Select,
  MenuItem
} from '@mui/material';
import { useAuth } from '../../context/AuthContext';
import axios from 'axios';

interface ModerationAuditEntry {
  id: string;
  modelId: string;
  performedByUserId: string;
  performedByUser: {
    username: string;
    email: string;
  };
  action: string;
  previousValues: string;
  newValues: string;
  moderationNotes?: string;
  performedAt: string;
  ipAddress?: string;
  userAgent?: string;
}

const ModerationAuditLog: React.FC = () => {
  const { token } = useAuth();
  const [loading, setLoading] = useState(true);
  const [auditLogs, setAuditLogs] = useState<ModerationAuditEntry[]>([]);
  const [page, setPage] = useState(1);
  const [totalPages, setTotalPages] = useState(1);
  const [selectedEntry, setSelectedEntry] = useState<ModerationAuditEntry | null>(null);
  const [detailsDialogOpen, setDetailsDialogOpen] = useState(false);
  const [error, setError] = useState('');
  const [filterAction, setFilterAction] = useState('');
  const [searchUserId, setSearchUserId] = useState('');

  const fetchAuditLogs = async () => {
    try {
      setLoading(true);
      const params = new URLSearchParams({
        page: page.toString(),
        pageSize: '20'
      });
      
      if (filterAction) {
        params.append('action', filterAction);
      }
      
      if (searchUserId) {
        params.append('userId', searchUserId);
      }

      const response = await axios.get(`/api/admin/moderation/audit-logs?${params.toString()}`, {
        headers: {
          Authorization: `Bearer ${token}`
        }
      });
      
      setAuditLogs(response.data.logs || []);
      setTotalPages(response.data.totalPages || 1);
      setError('');
    } catch (err) {
      setError('Failed to load audit logs. Please try again.');
      console.error('Error fetching audit logs:', err);
    } finally {
      setLoading(false);
    }
  };

  useEffect(() => {
    if (token) {
      fetchAuditLogs();
    }
  }, [token, page, filterAction, searchUserId]);

  const handlePageChange = (event: React.ChangeEvent<unknown>, value: number) => {
    setPage(value);
  };

  const openDetailsDialog = (entry: ModerationAuditEntry) => {
    setSelectedEntry(entry);
    setDetailsDialogOpen(true);
  };

  const getActionColor = (action: string) => {
    switch (action.toLowerCase()) {
      case 'edit':
        return 'primary';
      case 'approvewithchanges':
        return 'success';
      case 'flagforreview':
        return 'warning';
      case 'featuremodel':
        return 'info';
      case 'unfeaturemodel':
        return 'default';
      default:
        return 'default';
    }
  };

  const getActionLabel = (action: string) => {
    switch (action.toLowerCase()) {
      case 'edit':
        return 'Edit';
      case 'approvewithchanges':
        return 'Approve with Changes';
      case 'flagforreview':
        return 'Flag for Review';
      case 'featuremodel':
        return 'Feature Model';
      case 'unfeaturemodel':
        return 'Unfeature Model';
      default:
        return action;
    }
  };

  const formatChanges = (jsonString: string) => {
    try {
      const changes = JSON.parse(jsonString);
      return Object.entries(changes).map(([key, value]) => (
        <div key={key}>
          <strong>{key}:</strong> {String(value)}
        </div>
      ));
    } catch {
      return jsonString;
    }
  };

  if (loading && auditLogs.length === 0) {
    return (
      <Box sx={{ display: 'flex', justifyContent: 'center', mt: 4 }}>
        <CircularProgress />
      </Box>
    );
  }

  return (
    <Box sx={{ p: 3 }}>
      <Typography variant="h4" component="h1" gutterBottom>
        Moderation Audit Log
      </Typography>
      
      <Typography variant="body1" color="text.secondary" gutterBottom>
        Track all moderation actions performed on models for compliance and auditing purposes.
      </Typography>

      {error && <Alert severity="error" sx={{ mb: 2 }}>{error}</Alert>}

      {/* Filters */}
      <Card sx={{ mb: 3 }}>
        <CardContent>
          <Box sx={{ display: 'flex', gap: 2, flexWrap: 'wrap' }}>
            <FormControl sx={{ minWidth: 200 }}>
              <InputLabel>Filter by Action</InputLabel>
              <Select
                value={filterAction}
                onChange={(e) => setFilterAction(e.target.value)}
                label="Filter by Action"
              >
                <MenuItem value="">All Actions</MenuItem>
                <MenuItem value="Edit">Edit</MenuItem>
                <MenuItem value="ApproveWithChanges">Approve with Changes</MenuItem>
                <MenuItem value="FlagForReview">Flag for Review</MenuItem>
                <MenuItem value="FeatureModel">Feature Model</MenuItem>
                <MenuItem value="UnfeatureModel">Unfeature Model</MenuItem>
              </Select>
            </FormControl>
            
            <TextField
              label="Search by User ID"
              value={searchUserId}
              onChange={(e) => setSearchUserId(e.target.value)}
              sx={{ minWidth: 200 }}
            />
          </Box>
        </CardContent>
      </Card>

      {/* Audit Log Table */}
      <TableContainer component={Paper}>
        <Table>
          <TableHead>
            <TableRow>
              <TableCell>Date & Time</TableCell>
              <TableCell>Moderator</TableCell>
              <TableCell>Model ID</TableCell>
              <TableCell>Action</TableCell>
              <TableCell>Notes</TableCell>
              <TableCell>IP Address</TableCell>
              <TableCell>Actions</TableCell>
            </TableRow>
          </TableHead>
          <TableBody>
            {auditLogs.map((entry) => (
              <TableRow key={entry.id}>
                <TableCell>
                  {new Date(entry.performedAt).toLocaleString()}
                </TableCell>
                <TableCell>
                  <div>
                    <Typography variant="body2">
                      {entry.performedByUser.username}
                    </Typography>
                    <Typography variant="caption" color="text.secondary">
                      {entry.performedByUser.email}
                    </Typography>
                  </div>
                </TableCell>
                <TableCell>
                  <Typography variant="body2" sx={{ fontFamily: 'monospace' }}>
                    {entry.modelId.substring(0, 8)}...
                  </Typography>
                </TableCell>
                <TableCell>
                  <Chip
                    label={getActionLabel(entry.action)}
                    color={getActionColor(entry.action) as any}
                    size="small"
                  />
                </TableCell>
                <TableCell>
                  <Typography variant="body2" noWrap sx={{ maxWidth: 200 }}>
                    {entry.moderationNotes || '-'}
                  </Typography>
                </TableCell>
                <TableCell>
                  <Typography variant="body2" sx={{ fontFamily: 'monospace' }}>
                    {entry.ipAddress || '-'}
                  </Typography>
                </TableCell>
                <TableCell>
                  <Button
                    size="small"
                    variant="outlined"
                    onClick={() => openDetailsDialog(entry)}
                  >
                    View Details
                  </Button>
                </TableCell>
              </TableRow>
            ))}
          </TableBody>
        </Table>
      </TableContainer>

      {auditLogs.length === 0 && !loading && (
        <Alert severity="info" sx={{ mt: 2 }}>
          No audit log entries found.
        </Alert>
      )}

      {/* Pagination */}
      {totalPages > 1 && (
        <Box sx={{ display: 'flex', justifyContent: 'center', mt: 4 }}>
          <Pagination 
            count={totalPages} 
            page={page} 
            onChange={handlePageChange} 
            color="primary" 
          />
        </Box>
      )}

      {/* Details Dialog */}
      <Dialog open={detailsDialogOpen} onClose={() => setDetailsDialogOpen(false)} maxWidth="md" fullWidth>
        <DialogTitle>
          Audit Log Details
        </DialogTitle>
        <DialogContent>
          {selectedEntry && (
            <Box sx={{ mt: 2 }}>
              <Typography variant="h6" gutterBottom>
                Action Information
              </Typography>
              <Typography><strong>Action:</strong> {getActionLabel(selectedEntry.action)}</Typography>
              <Typography><strong>Performed By:</strong> {selectedEntry.performedByUser.username} ({selectedEntry.performedByUser.email})</Typography>
              <Typography><strong>Date & Time:</strong> {new Date(selectedEntry.performedAt).toLocaleString()}</Typography>
              <Typography><strong>Model ID:</strong> {selectedEntry.modelId}</Typography>
              <Typography><strong>IP Address:</strong> {selectedEntry.ipAddress || 'N/A'}</Typography>
              <Typography><strong>User Agent:</strong> {selectedEntry.userAgent || 'N/A'}</Typography>
              
              {selectedEntry.moderationNotes && (
                <>
                  <Typography variant="h6" gutterBottom sx={{ mt: 3 }}>
                    Moderation Notes
                  </Typography>
                  <Typography>{selectedEntry.moderationNotes}</Typography>
                </>
              )}

              <Typography variant="h6" gutterBottom sx={{ mt: 3 }}>
                Previous Values
              </Typography>
              <Paper sx={{ p: 2, backgroundColor: '#f5f5f5' }}>
                {formatChanges(selectedEntry.previousValues)}
              </Paper>

              <Typography variant="h6" gutterBottom sx={{ mt: 3 }}>
                New Values
              </Typography>
              <Paper sx={{ p: 2, backgroundColor: '#e8f5e8' }}>
                {formatChanges(selectedEntry.newValues)}
              </Paper>
            </Box>
          )}
        </DialogContent>
        <DialogActions>
          <Button onClick={() => setDetailsDialogOpen(false)}>Close</Button>
        </DialogActions>
      </Dialog>
    </Box>
  );
};

export default ModerationAuditLog; 