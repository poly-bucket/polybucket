import React, { useState, useEffect } from 'react';
import { 
  Box,
  Typography,
  Paper,
  Button,
  Table,
  TableBody,
  TableCell,
  TableContainer,
  TableHead,
  TableRow,
  CircularProgress,
  Alert
} from '@mui/material';
import { useAuth } from '../../context/AuthContext';
import axios from 'axios';

interface Role {
  id: string;
  name: string;
  description: string;
}

const RoleManagement: React.FC = () => {
  const { token } = useAuth();
  const [roles, setRoles] = useState<Role[]>([]);
  const [loading, setLoading] = useState(true);
  const [error, setError] = useState('');
  const [success, setSuccess] = useState('');

  useEffect(() => {
    const fetchRoles = async () => {
      try {
        setLoading(true);
        const response = await axios.get('/api/roles', {
          headers: { Authorization: `Bearer ${token}` }
        });
        
        setRoles(response.data);
        setError('');
      } catch (err) {
        setError('Failed to load roles. Please try again.');
        console.error('Error fetching roles:', err);
      } finally {
        setLoading(false);
      }
    };

    if (token) {
      fetchRoles();
    }
  }, [token]);

  const handleConfigureComplete = async () => {
    try {
      await axios.post('/api/admin/settings/role-configuration', 
        { isRoleConfigured: true },
        { headers: { Authorization: `Bearer ${token}` } }
      );
      
      setSuccess('Role configuration marked as complete!');
      
      // Clear success message after 3 seconds
      setTimeout(() => {
        setSuccess('');
      }, 3000);
    } catch (err) {
      setError('Failed to save configuration status.');
      console.error('Error saving configuration status:', err);
    }
  };

  if (loading) {
    return <CircularProgress />;
  }

  return (
    <Box>
      <Typography variant="h5" gutterBottom>
        Role Management
      </Typography>
      
      {error && <Alert severity="error" sx={{ mb: 2 }}>{error}</Alert>}
      {success && <Alert severity="success" sx={{ mb: 2 }}>{success}</Alert>}
      
      <Paper elevation={2} sx={{ p: 2, mb: 3 }}>
        <Typography variant="body1" paragraph>
          Assign appropriate roles to users to control access to features. 
          Configure which roles have moderation abilities in the Moderation Settings tab.
        </Typography>
        
        <TableContainer>
          <Table>
            <TableHead>
              <TableRow>
                <TableCell>Role Name</TableCell>
                <TableCell>Description</TableCell>
                <TableCell>Actions</TableCell>
              </TableRow>
            </TableHead>
            <TableBody>
              {roles.length === 0 ? (
                <TableRow>
                  <TableCell colSpan={3}>
                    <Typography align="center">No roles found. Create some roles to get started.</Typography>
                  </TableCell>
                </TableRow>
              ) : (
                roles.map((role) => (
                  <TableRow key={role.id}>
                    <TableCell>{role.name}</TableCell>
                    <TableCell>{role.description}</TableCell>
                    <TableCell>
                      <Button size="small" color="primary">Edit</Button>
                    </TableCell>
                  </TableRow>
                ))
              )}
            </TableBody>
          </Table>
        </TableContainer>
        
        <Box sx={{ mt: 2, display: 'flex', justifyContent: 'space-between' }}>
          <Button variant="contained" color="primary">
            Add New Role
          </Button>
          <Button 
            variant="contained" 
            color="success"
            onClick={handleConfigureComplete}
          >
            Mark Role Setup Complete
          </Button>
        </Box>
      </Paper>
    </Box>
  );
};

export default RoleManagement; 