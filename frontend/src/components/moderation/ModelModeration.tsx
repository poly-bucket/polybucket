import React, { useState, useEffect } from 'react';
import { 
  Box, 
  Card, 
  CardContent, 
  Typography, 
  Button, 
  TextField, 
  Dialog, 
  DialogTitle, 
  DialogContent, 
  DialogActions,
  CircularProgress,
  Alert,
  Pagination
} from '@mui/material';
import { useAuth } from '../../context/AuthContext';
import axios from 'axios';
import ModeratorEditForm, { ModeratorEditRequest } from './ModeratorEditForm';
import { Model } from '../../services/api.client';

interface PendingModel {
  id: string;
  name: string;
  description: string;
  thumbnailUrl: string;
  userName: string;
  fileFormat: string;
  createdAt: string;
}

const ModelModeration: React.FC = () => {
  const { token } = useAuth();
  const [loading, setLoading] = useState(true);
  const [pendingModels, setPendingModels] = useState<PendingModel[]>([]);
  const [page, setPage] = useState(1);
  const [totalPages, setTotalPages] = useState(1);
  const [rejectDialogOpen, setRejectDialogOpen] = useState(false);
  const [editDialogOpen, setEditDialogOpen] = useState(false);
  const [selectedModel, setSelectedModel] = useState<PendingModel | null>(null);
  const [selectedModelForEdit, setSelectedModelForEdit] = useState<Model | null>(null);
  const [rejectReason, setRejectReason] = useState('');
  const [actionLoading, setActionLoading] = useState(false);
  const [error, setError] = useState('');
  const [successMessage, setSuccessMessage] = useState('');

  const fetchPendingModels = async () => {
    try {
      setLoading(true);
      const response = await axios.get(`/api/moderation/models?page=${page}&pageSize=10`, {
        headers: {
          Authorization: `Bearer ${token}`
        }
      });
      
      setPendingModels(response.data);
      // Assuming pagination info is in headers or response data
      setTotalPages(response.data.totalPages || 1);
      setError('');
    } catch (err) {
      setError('Failed to load pending models. Please try again.');
      console.error('Error fetching pending models:', err);
    } finally {
      setLoading(false);
    }
  };

  useEffect(() => {
    if (token) {
      fetchPendingModels();
    }
  }, [token, page]);

  const handlePageChange = (event: React.ChangeEvent<unknown>, value: number) => {
    setPage(value);
  };

  const handleApproveModel = async (modelId: string) => {
    try {
      setActionLoading(true);
      await axios.post(`/api/moderation/models/${modelId}/approve`, {}, {
        headers: {
          Authorization: `Bearer ${token}`
        }
      });
      
      setSuccessMessage('Model approved successfully!');
      // Update the UI by removing the approved model
      setPendingModels(prev => prev.filter(model => model.id !== modelId));
      
      // Clear success message after 3 seconds
      setTimeout(() => {
        setSuccessMessage('');
      }, 3000);
    } catch (err) {
      setError('Failed to approve model. Please try again.');
      console.error('Error approving model:', err);
    } finally {
      setActionLoading(false);
    }
  };

  const openRejectDialog = (model: PendingModel) => {
    setSelectedModel(model);
    setRejectDialogOpen(true);
  };

  const openEditDialog = async (model: PendingModel) => {
    try {
      setActionLoading(true);
      // Fetch full model details for editing
      const response = await axios.get(`/api/moderation/models/${model.id}`, {
        headers: {
          Authorization: `Bearer ${token}`
        }
      });
      
      setSelectedModelForEdit(response.data);
      setEditDialogOpen(true);
    } catch (err) {
      setError('Failed to load model details for editing. Please try again.');
      console.error('Error loading model for edit:', err);
    } finally {
      setActionLoading(false);
    }
  };

  const handleEditModel = async (editRequest: ModeratorEditRequest) => {
    if (!selectedModelForEdit) return;
    
    try {
      setActionLoading(true);
      await axios.put(`/api/moderation/models/${selectedModelForEdit.id}`, editRequest, {
        headers: {
          Authorization: `Bearer ${token}`,
          'Content-Type': 'application/json'
        }
      });
      
      setSuccessMessage('Model updated successfully!');
      setEditDialogOpen(false);
      
      // Refresh the pending models list
      await fetchPendingModels();
      
      // Clear success message after 3 seconds
      setTimeout(() => {
        setSuccessMessage('');
      }, 3000);
    } catch (err) {
      setError('Failed to update model. Please try again.');
      console.error('Error updating model:', err);
    } finally {
      setActionLoading(false);
    }
  };

  const handleRejectModel = async () => {
    if (!selectedModel) return;
    
    try {
      setActionLoading(true);
      await axios.post(`/api/moderation/models/${selectedModel.id}/reject`, {
        reason: rejectReason
      }, {
        headers: {
          Authorization: `Bearer ${token}`
        }
      });
      
      setSuccessMessage('Model rejected successfully!');
      // Update the UI by removing the rejected model
      setPendingModels(prev => prev.filter(model => model.id !== selectedModel.id));
      setRejectDialogOpen(false);
      setRejectReason('');
      
      // Clear success message after 3 seconds
      setTimeout(() => {
        setSuccessMessage('');
      }, 3000);
    } catch (err) {
      setError('Failed to reject model. Please try again.');
      console.error('Error rejecting model:', err);
    } finally {
      setActionLoading(false);
    }
  };

  if (loading) {
    return (
      <Box sx={{ display: 'flex', justifyContent: 'center', mt: 4 }}>
        <CircularProgress />
      </Box>
    );
  }

  return (
    <Box sx={{ p: 3 }}>
      <Typography variant="h4" component="h1" gutterBottom>
        Models Pending Moderation
      </Typography>
      
      {error && <Alert severity="error" sx={{ mb: 2 }}>{error}</Alert>}
      {successMessage && <Alert severity="success" sx={{ mb: 2 }}>{successMessage}</Alert>}
      
      {pendingModels.length === 0 ? (
        <Alert severity="info">No models are currently pending moderation.</Alert>
      ) : (
        <>
          <div style={{ display: 'flex', flexWrap: 'wrap', margin: -12 }}>
            {pendingModels.map(model => (
              <div key={model.id} style={{ width: '33.333%', padding: 12, boxSizing: 'border-box' }}>
                <Card>
                  <Box sx={{ position: 'relative', paddingTop: '75%', overflow: 'hidden' }}>
                    <Box 
                      component="img" 
                      src={model.thumbnailUrl || '/placeholder.png'}
                      alt={model.name}
                      sx={{ 
                        position: 'absolute', 
                        top: 0, 
                        left: 0, 
                        width: '100%', 
                        height: '100%', 
                        objectFit: 'cover' 
                      }}
                    />
                  </Box>
                  <CardContent>
                    <Typography variant="h6" component="h2" gutterBottom>
                      {model.name}
                    </Typography>
                    <Typography variant="body2" color="text.secondary" gutterBottom>
                      By {model.userName} • {new Date(model.createdAt).toLocaleDateString()}
                    </Typography>
                    <Typography variant="body2" sx={{ mb: 2 }}>
                      {model.description.length > 100 
                        ? `${model.description.substring(0, 100)}...` 
                        : model.description}
                    </Typography>
                    <Box sx={{ display: 'flex', justifyContent: 'space-between', mt: 2, gap: 1 }}>
                      <Button 
                        variant="contained" 
                        color="primary"
                        disabled={actionLoading}
                        onClick={() => handleApproveModel(model.id)}
                        size="small"
                      >
                        Approve
                      </Button>
                      <Button 
                        variant="outlined" 
                        color="secondary"
                        disabled={actionLoading}
                        onClick={() => openEditDialog(model)}
                        size="small"
                      >
                        Edit
                      </Button>
                      <Button 
                        variant="outlined" 
                        color="error"
                        disabled={actionLoading}
                        onClick={() => openRejectDialog(model)}
                        size="small"
                      >
                        Reject
                      </Button>
                    </Box>
                  </CardContent>
                </Card>
              </div>
            ))}
          </div>
          
          <Box sx={{ display: 'flex', justifyContent: 'center', mt: 4 }}>
            <Pagination 
              count={totalPages} 
              page={page} 
              onChange={handlePageChange} 
              color="primary" 
            />
          </Box>
        </>
      )}
      
      {/* Reject Dialog */}
      <Dialog open={rejectDialogOpen} onClose={() => setRejectDialogOpen(false)} maxWidth="sm" fullWidth>
        <DialogTitle>Reject Model: {selectedModel?.name}</DialogTitle>
        <DialogContent>
          <Typography variant="body2" gutterBottom sx={{ mb: 2 }}>
            Please provide a reason for rejecting this model. This will be shown to the uploader.
          </Typography>
          <TextField
            autoFocus
            label="Reason for rejection"
            fullWidth
            multiline
            rows={4}
            value={rejectReason}
            onChange={(e: React.ChangeEvent<HTMLInputElement>) => setRejectReason(e.target.value)}
            required
          />
        </DialogContent>
        <DialogActions>
          <Button onClick={() => setRejectDialogOpen(false)}>Cancel</Button>
          <Button 
            onClick={handleRejectModel} 
            color="error" 
            variant="contained"
            disabled={!rejectReason.trim() || actionLoading}
          >
            {actionLoading ? <CircularProgress size={24} /> : 'Reject Model'}
          </Button>
        </DialogActions>
      </Dialog>

      {/* Edit Model Dialog */}
      {selectedModelForEdit && (
        <ModeratorEditForm
          model={selectedModelForEdit}
          isOpen={editDialogOpen}
          onClose={() => {
            setEditDialogOpen(false);
            setSelectedModelForEdit(null);
          }}
          onSave={handleEditModel}
          loading={actionLoading}
        />
      )}
    </Box>
  );
};

export default ModelModeration;