import React, { useState, useEffect } from 'react';
import {
  Box,
  Typography,
  Button,
  Table,
  TableBody,
  TableCell,
  TableContainer,
  TableHead,
  TableRow,
  Paper,
  TablePagination,
  Stack,
  Dialog,
  DialogTitle,
  DialogContent,
  DialogActions,
  TextField,
  Alert,
  CircularProgress,
  Chip,
  IconButton,
  Tooltip
} from '@mui/material';
import { 
  UserCircleIcon, 
  CheckCircleIcon, 
  EyeIcon,
  CalendarIcon,
  UserIcon,
  UserMinusIcon
} from '@heroicons/react/24/outline';
import { moderationService, BannedUser, BannedUsersResponse, BanUserRequest } from '../../../services/moderationService';

export const BannedUsersTab: React.FC = () => {
  const [bannedUsers, setBannedUsers] = useState<BannedUser[]>([]);
  const [loading, setLoading] = useState(true);
  const [error, setError] = useState<string | null>(null);
  const [page, setPage] = useState(0);
  const [rowsPerPage, setRowsPerPage] = useState(10);
  const [totalCount, setTotalCount] = useState(0);
  
  // Unban dialog state
  const [selectedUser, setSelectedUser] = useState<BannedUser | null>(null);
  const [unbanDialogOpen, setUnbanDialogOpen] = useState(false);
  const [unbanning, setUnbanning] = useState(false);

  const fetchBannedUsers = async () => {
    try {
      setLoading(true);
      setError(null);
      
      const response: BannedUsersResponse = await moderationService.getBannedUsers(
        page + 1,
        rowsPerPage
      );
      
      setBannedUsers(response.users);
      setTotalCount(response.totalCount);
    } catch (err) {
      setError('Failed to fetch banned users');
      console.error('Error fetching banned users:', err);
    } finally {
      setLoading(false);
    }
  };

  useEffect(() => {
    fetchBannedUsers();
  }, [page, rowsPerPage]);

  const handlePageChange = (event: unknown, newPage: number) => {
    setPage(newPage);
  };

  const handleRowsPerPageChange = (event: React.ChangeEvent<HTMLInputElement>) => {
    setRowsPerPage(parseInt(event.target.value, 10));
    setPage(0);
  };

  const openUnbanDialog = (user: BannedUser) => {
    setSelectedUser(user);
    setUnbanDialogOpen(true);
  };

  const closeUnbanDialog = () => {
    setSelectedUser(null);
    setUnbanDialogOpen(false);
  };

  const handleUnbanUser = async () => {
    if (!selectedUser) return;

    try {
      setUnbanning(true);
      await moderationService.unbanUser(selectedUser.id);
      closeUnbanDialog();
      fetchBannedUsers(); // Refresh the list
    } catch (err) {
      console.error('Error unbanning user:', err);
      // TODO: Show error toast
    } finally {
      setUnbanning(false);
    }
  };

  const formatDate = (dateString?: string) => {
    if (!dateString) return '-';
    return new Date(dateString).toLocaleString();
  };

  const getBanStatusChip = (user: BannedUser) => {
    if (!user.banExpiresAt) {
      return <Chip label="Permanent" color="error" size="small" />;
    }

    const expiresAt = new Date(user.banExpiresAt);
    const now = new Date();
    const isExpired = expiresAt < now;

    return (
      <Chip
        label={isExpired ? 'Expired' : 'Temporary'}
        color={isExpired ? 'warning' : 'error'}
        size="small"
      />
    );
  };

  const getRoleColor = (roleName: string) => {
    switch (roleName.toLowerCase()) {
      case 'admin': return 'error';
      case 'moderator': return 'warning';
      case 'user': return 'default';
      default: return 'default';
    }
  };

  if (loading && bannedUsers.length === 0) {
    return (
      <Box display="flex" justifyContent="center" alignItems="center" minHeight="200px">
        <CircularProgress />
      </Box>
    );
  }

  return (
    <Box>
      <Typography variant="h5" gutterBottom sx={{ display: 'flex', alignItems: 'center', gap: 1 }}>
        <UserMinusIcon className="w-6 h-6" />
        Banned Users Management
      </Typography>

      {error && (
        <Alert severity="error" sx={{ mb: 2 }}>
          {error}
        </Alert>
      )}

      <Box sx={{ mb: 3 }}>
        <Typography variant="body2" color="text.secondary">
          Total banned users: {totalCount}
        </Typography>
      </Box>

      <TableContainer component={Paper}>
        <Table>
          <TableHead>
            <TableRow>
              <TableCell>User</TableCell>
              <TableCell>Role</TableCell>
              <TableCell>Ban Status</TableCell>
              <TableCell>Banned By</TableCell>
              <TableCell>Banned Date</TableCell>
              <TableCell>Expires</TableCell>
              <TableCell>Reason</TableCell>
              <TableCell>Actions</TableCell>
            </TableRow>
          </TableHead>
          <TableBody>
            {bannedUsers.map((user) => (
              <TableRow key={user.id}>
                <TableCell>
                  <Box>
                    <Typography variant="body2" fontWeight="medium">
                      {user.username}
                    </Typography>
                    <Typography variant="caption" color="text.secondary">
                      {user.email}
                    </Typography>
                  </Box>
                </TableCell>
                <TableCell>
                  <Chip
                    label={user.roleName}
                    color={getRoleColor(user.roleName) as any}
                    size="small"
                  />
                </TableCell>
                <TableCell>
                  {getBanStatusChip(user)}
                </TableCell>
                <TableCell>
                  <Typography variant="body2">
                    {user.bannedByUsername || 'System'}
                  </Typography>
                </TableCell>
                <TableCell>
                  <Typography variant="body2">
                    {formatDate(user.bannedAt)}
                  </Typography>
                </TableCell>
                <TableCell>
                  <Typography variant="body2">
                    {user.banExpiresAt ? formatDate(user.banExpiresAt) : 'Never'}
                  </Typography>
                </TableCell>
                <TableCell>
                  <Tooltip title={user.banReason || 'No reason provided'}>
                    <Typography variant="body2" sx={{ maxWidth: 150, overflow: 'hidden', textOverflow: 'ellipsis' }}>
                      {user.banReason || 'No reason provided'}
                    </Typography>
                  </Tooltip>
                </TableCell>
                <TableCell>
                  <Stack direction="row" spacing={1}>
                    <Tooltip title="View user details">
                      <IconButton
                        size="small"
                        onClick={() => {
                          // TODO: Open user details dialog
                        }}
                      >
                        <EyeIcon className="w-4 h-4" />
                      </IconButton>
                    </Tooltip>
                    <Button
                      size="small"
                      variant="outlined"
                      color="success"
                      startIcon={<CheckCircleIcon className="w-4 h-4" />}
                      onClick={() => openUnbanDialog(user)}
                    >
                      Unban
                    </Button>
                  </Stack>
                </TableCell>
              </TableRow>
            ))}
            {bannedUsers.length === 0 && !loading && (
              <TableRow>
                <TableCell colSpan={8} align="center">
                  <Box sx={{ py: 4 }}>
                    <CheckCircleIcon className="w-12 h-12" style={{ opacity: 0.5, marginBottom: 16 }} />
                    <Typography variant="h6" color="text.secondary">
                      No banned users found
                    </Typography>
                    <Typography variant="body2" color="text.secondary">
                      All users are currently in good standing
                    </Typography>
                  </Box>
                </TableCell>
              </TableRow>
            )}
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

      {/* Unban Confirmation Dialog */}
      <Dialog open={unbanDialogOpen} onClose={closeUnbanDialog} maxWidth="sm" fullWidth>
        <DialogTitle>Confirm User Unban</DialogTitle>
        <DialogContent>
          {selectedUser && (
            <Box>
              <Typography variant="body1" gutterBottom>
                Are you sure you want to unban the following user?
              </Typography>
              <Box sx={{ mt: 2, p: 2, bgcolor: 'grey.100', borderRadius: 1 }}>
                <Typography variant="subtitle2">
                  <strong>Username:</strong> {selectedUser.username}
                </Typography>
                <Typography variant="body2" color="text.secondary">
                  <strong>Email:</strong> {selectedUser.email}
                </Typography>
                <Typography variant="body2" color="text.secondary">
                  <strong>Banned:</strong> {formatDate(selectedUser.bannedAt)}
                </Typography>
                <Typography variant="body2" color="text.secondary">
                  <strong>Reason:</strong> {selectedUser.banReason || 'No reason provided'}
                </Typography>
              </Box>
              <Alert severity="info" sx={{ mt: 2 }}>
                This action will immediately restore the user's access to the platform.
              </Alert>
            </Box>
          )}
        </DialogContent>
        <DialogActions>
          <Button onClick={closeUnbanDialog}>Cancel</Button>
          <Button
            onClick={handleUnbanUser}
            variant="contained"
            color="success"
            disabled={unbanning}
            startIcon={unbanning ? <CircularProgress size={16} /> : <CheckCircleIcon className="w-4 h-4" />}
          >
            {unbanning ? 'Unbanning...' : 'Unban User'}
          </Button>
        </DialogActions>
      </Dialog>
    </Box>
  );
}; 