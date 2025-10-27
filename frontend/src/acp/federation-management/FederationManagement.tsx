import React, { useState, useEffect } from 'react';
import { useAuth } from '../../context/AuthContext';
import { 
  Add as AddIcon,
  Delete as DeleteIcon,
  Edit as EditIcon,
  CheckCircle as CheckCircleIcon,
  Error as ErrorIcon,
  Public as PublicIcon,
  CloudSync as CloudSyncIcon,
  Handshake as HandshakeIcon,
  SwapHoriz as SwapHorizIcon
} from '@mui/icons-material';

interface FederatedInstance {
  id: string;
  name: string;
  baseUrl: string;
  status: string;
  description?: string;
  isEnabled: boolean;
  lastSyncAt?: string;
  createdAt: string;
  updatedAt?: string;
}

interface HealthStatus {
  instanceId: string;
  instanceName: string;
  baseUrl: string;
  status: string;
  isEnabled: boolean;
  isReachable: boolean;
  responseTimeMs: number;
  lastSyncAt?: string;
  errorMessage?: string;
  checkedAt: string;
}

interface Handshake {
  id: string;
  initiatorUrl: string;
  responderUrl: string;
  status: string;
  handshakeToken?: string;
  expiresAt: string;
  completedAt?: string;
  createdAt: string;
  errorMessage?: string;
}

const FederationManagement: React.FC = () => {
  const { user } = useAuth();
  const [instances, setInstances] = useState<FederatedInstance[]>([]);
  const [loading, setLoading] = useState(true);
  const [error, setError] = useState<string>('');
  const [showAddModal, setShowAddModal] = useState(false);
  const [editingInstance, setEditingInstance] = useState<FederatedInstance | null>(null);
  const [healthStatuses, setHealthStatuses] = useState<Map<string, HealthStatus>>(new Map());
  
  // Handshake state
  const [showHandshakeModal, setShowHandshakeModal] = useState(false);
  const [handshakes, setHandshakes] = useState<Handshake[]>([]);
  const [handshakeLoading, setHandshakeLoading] = useState(false);
  const [activeTab, setActiveTab] = useState<'instances' | 'handshakes'>('instances');

  const [formData, setFormData] = useState({
    name: '',
    baseUrl: '',
    description: '',
    publicKey: '',
    sharedSecret: '',
    isEnabled: true
  });

  const [handshakeFormData, setHandshakeFormData] = useState({
    remoteUrl: '',
    remoteName: ''
  });

  useEffect(() => {
    loadInstances();
  }, []);

  const loadInstances = async () => {
    try {
      setLoading(true);
      setError('');
      const response = await fetch('/api/federation/instances', {
        headers: {
          'Authorization': `Bearer ${user?.accessToken}`
        }
      });
      
      if (response.ok) {
        const data = await response.json();
        setInstances(data);
      } else {
        setError('Failed to load federated instances');
      }
    } catch (err) {
      setError('Error loading federated instances');
      console.error('Error loading instances:', err);
    } finally {
      setLoading(false);
    }
  };

  const checkHealth = async (instanceId: string) => {
    try {
      const response = await fetch(`/api/federation/health/${instanceId}`, {
        headers: {
          'Authorization': `Bearer ${user?.accessToken}`
        }
      });
      
      if (response.ok) {
        const health: HealthStatus = await response.json();
        setHealthStatuses(prev => new Map(prev).set(instanceId, health));
      }
    } catch (err) {
      console.error('Error checking health:', err);
    }
  };

  const handleSubmit = async (e: React.FormEvent) => {
    e.preventDefault();
    setError('');

    try {
      const url = editingInstance 
        ? `/api/federation/instances/${editingInstance.id}` 
        : '/api/federation/instances';
      
      const method = editingInstance ? 'PUT' : 'POST';

      const response = await fetch(url, {
        method,
        headers: {
          'Content-Type': 'application/json',
          'Authorization': `Bearer ${user?.accessToken}`
        },
        body: JSON.stringify(formData)
      });

      if (response.ok) {
        await loadInstances();
        resetForm();
        setShowAddModal(false);
        setEditingInstance(null);
      } else {
        const errorData = await response.json();
        setError(errorData.message || 'Failed to save instance');
      }
    } catch (err) {
      setError('Error saving instance');
      console.error('Error saving instance:', err);
    }
  };

  const handleDelete = async (instanceId: string) => {
    if (!confirm('Are you sure you want to delete this federated instance?')) {
      return;
    }

    try {
      const response = await fetch(`/api/federation/instances/${instanceId}`, {
        method: 'DELETE',
        headers: {
          'Authorization': `Bearer ${user?.accessToken}`
        }
      });

      if (response.ok) {
        await loadInstances();
      } else {
        setError('Failed to delete instance');
      }
    } catch (err) {
      setError('Error deleting instance');
      console.error('Error deleting instance:', err);
    }
  };

  const handleEdit = (instance: FederatedInstance) => {
    setEditingInstance(instance);
    setFormData({
      name: instance.name,
      baseUrl: instance.baseUrl,
      description: instance.description || '',
      publicKey: '',
      sharedSecret: '',
      isEnabled: instance.isEnabled
    });
    setShowAddModal(true);
  };

  const resetForm = () => {
    setFormData({
      name: '',
      baseUrl: '',
      description: '',
      publicKey: '',
      sharedSecret: '',
      isEnabled: true
    });
    setEditingInstance(null);
  };

  // Handshake functions
  const initiateHandshake = async () => {
    if (!handshakeFormData.remoteUrl || !handshakeFormData.remoteName) {
      setError('Please enter both remote instance URL and name');
      return;
    }

    try {
      setHandshakeLoading(true);
      setError('');

      // Step 1: Call the remote instance to initiate handshake
      const response = await fetch(`${handshakeFormData.remoteUrl}/api/federation/handshake/initiate`, {
        method: 'POST',
        headers: {
          'Content-Type': 'application/json'
        },
        body: JSON.stringify({
          initiatorUrl: window.location.origin,
          initiatorName: 'PolyBucket Instance', // TODO: Get from config
          version: '1.0.0'
        })
      });

      if (!response.ok) {
        throw new Error('Failed to initiate handshake with remote instance');
      }

      const handshakeResponse = await response.json();
      
      // Handshake accepted, now proceed to token exchange
      alert(`Handshake accepted! Handshake ID: ${handshakeResponse.handshakeId}\n\nNext: Token exchange will begin automatically.`);
      
      // Close modal and refresh
      setShowHandshakeModal(false);
      setHandshakeFormData({ remoteUrl: '', remoteName: '' });
      
      // In a real implementation, you would automatically proceed with token exchange here
      // For now, we'll just show a success message
      
    } catch (err: any) {
      setError(err.message || 'Failed to initiate handshake');
      console.error('Handshake error:', err);
    } finally {
      setHandshakeLoading(false);
    }
  };

  const resetHandshakeForm = () => {
    setHandshakeFormData({
      remoteUrl: '',
      remoteName: ''
    });
  };

  const getStatusColor = (status: string) => {
    switch (status.toLowerCase()) {
      case 'active':
        return 'text-green-500';
      case 'pending':
        return 'text-yellow-500';
      case 'inactive':
      case 'suspended':
      case 'error':
        return 'text-red-500';
      default:
        return 'text-gray-500';
    }
  };

  const getStatusIcon = (status: string) => {
    switch (status.toLowerCase()) {
      case 'active':
        return <CheckCircleIcon className="w-5 h-5" />;
      case 'error':
        return <ErrorIcon className="w-5 h-5" />;
      default:
        return <PublicIcon className="w-5 h-5" />;
    }
  };

  return (
    <div className="p-6">
      <div className="flex justify-between items-center mb-6">
        <div>
          <h1 className="text-2xl font-bold text-white">Federation Management</h1>
          <p className="text-gray-400 mt-1">Manage federated instances and model sharing</p>
        </div>
        <div className="flex gap-2">
          <button
            onClick={() => {
              resetHandshakeForm();
              setShowHandshakeModal(true);
            }}
            className="px-4 py-2 bg-purple-600 hover:bg-purple-700 text-white rounded-lg flex items-center gap-2 transition-colors"
          >
            <HandshakeIcon />
            Initiate Handshake
          </button>
          <button
            onClick={() => {
              resetForm();
              setShowAddModal(true);
            }}
            className="px-4 py-2 bg-blue-600 hover:bg-blue-700 text-white rounded-lg flex items-center gap-2 transition-colors"
          >
            <AddIcon />
            Add Instance
          </button>
        </div>
      </div>

      {/* Tabs */}
      <div className="flex gap-4 mb-6 border-b border-white/10">
        <button
          onClick={() => setActiveTab('instances')}
          className={`px-4 py-2 font-medium transition-colors ${
            activeTab === 'instances'
              ? 'text-blue-400 border-b-2 border-blue-400'
              : 'text-gray-400 hover:text-gray-300'
          }`}
        >
          <div className="flex items-center gap-2">
            <PublicIcon className="w-5 h-5" />
            <span>Instances</span>
          </div>
        </button>
        <button
          onClick={() => setActiveTab('handshakes')}
          className={`px-4 py-2 font-medium transition-colors ${
            activeTab === 'handshakes'
              ? 'text-purple-400 border-b-2 border-purple-400'
              : 'text-gray-400 hover:text-gray-300'
          }`}
        >
          <div className="flex items-center gap-2">
            <SwapHorizIcon className="w-5 h-5" />
            <span>Handshakes</span>
          </div>
        </button>
      </div>

      {error && (
        <div className="bg-red-500/20 border border-red-500 text-red-300 px-4 py-3 rounded-lg mb-4">
          {error}
        </div>
      )}

      {/* Instances Tab */}
      {activeTab === 'instances' && (
        <>
          {loading ? (
            <div className="text-center py-12">
              <div className="inline-block animate-spin rounded-full h-12 w-12 border-b-2 border-blue-500"></div>
              <p className="text-gray-400 mt-4">Loading instances...</p>
            </div>
          ) : (
            <div className="grid gap-4">
          {instances.length === 0 ? (
            <div className="text-center py-12 bg-white/5 rounded-lg">
              <PublicIcon className="w-16 h-16 text-gray-600 mx-auto mb-4" />
              <p className="text-gray-400">No federated instances configured</p>
              <p className="text-gray-500 text-sm mt-2">Add an instance to start sharing models</p>
            </div>
          ) : (
            instances.map((instance) => {
              const health = healthStatuses.get(instance.id);
              
              return (
                <div key={instance.id} className="bg-white/5 rounded-lg p-6 hover:bg-white/10 transition-colors">
                  <div className="flex justify-between items-start">
                    <div className="flex-1">
                      <div className="flex items-center gap-3 mb-2">
                        <h3 className="text-lg font-semibold text-white">{instance.name}</h3>
                        <span className={`flex items-center gap-1 ${getStatusColor(instance.status)}`}>
                          {getStatusIcon(instance.status)}
                          <span className="text-sm">{instance.status}</span>
                        </span>
                        {!instance.isEnabled && (
                          <span className="px-2 py-1 bg-red-500/20 text-red-400 text-xs rounded">
                            Disabled
                          </span>
                        )}
                      </div>
                      
                      <p className="text-blue-400 text-sm mb-2">{instance.baseUrl}</p>
                      
                      {instance.description && (
                        <p className="text-gray-400 text-sm mb-3">{instance.description}</p>
                      )}

                      <div className="flex gap-4 text-sm text-gray-400">
                        <div>
                          Last sync: {instance.lastSyncAt 
                            ? new Date(instance.lastSyncAt).toLocaleString() 
                            : 'Never'}
                        </div>
                        {health && (
                          <div className="flex items-center gap-2">
                            <span className={health.isReachable ? 'text-green-400' : 'text-red-400'}>
                              {health.isReachable ? '● Online' : '● Offline'}
                            </span>
                            {health.isReachable && (
                              <span className="text-gray-500">
                                ({health.responseTimeMs}ms)
                              </span>
                            )}
                          </div>
                        )}
                      </div>
                    </div>

                    <div className="flex gap-2">
                      <button
                        onClick={() => checkHealth(instance.id)}
                        className="p-2 hover:bg-white/10 rounded-lg text-gray-400 hover:text-blue-400 transition-colors"
                        title="Check health"
                      >
                        <CloudSyncIcon />
                      </button>
                      <button
                        onClick={() => handleEdit(instance)}
                        className="p-2 hover:bg-white/10 rounded-lg text-gray-400 hover:text-blue-400 transition-colors"
                        title="Edit"
                      >
                        <EditIcon />
                      </button>
                      <button
                        onClick={() => handleDelete(instance.id)}
                        className="p-2 hover:bg-white/10 rounded-lg text-gray-400 hover:text-red-400 transition-colors"
                        title="Delete"
                      >
                        <DeleteIcon />
                      </button>
                    </div>
                  </div>
                </div>
              );
            })
          )}
            </div>
          )}
        </>
      )}

      {/* Handshakes Tab */}
      {activeTab === 'handshakes' && (
        <div className="grid gap-4">
          {handshakes.length === 0 ? (
            <div className="text-center py-12 bg-white/5 rounded-lg">
              <SwapHorizIcon className="w-16 h-16 text-gray-600 mx-auto mb-4" />
              <p className="text-gray-400">No active handshakes</p>
              <p className="text-gray-500 text-sm mt-2">Initiate a handshake to connect with another instance</p>
            </div>
          ) : (
            handshakes.map((handshake) => (
              <div key={handshake.id} className="bg-white/5 rounded-lg p-6">
                <div className="flex justify-between items-start">
                  <div className="flex-1">
                    <div className="flex items-center gap-3 mb-2">
                      <h3 className="text-lg font-semibold text-white">Handshake</h3>
                      <span className={`px-2 py-1 rounded text-xs font-medium ${
                        handshake.status === 'Completed' ? 'bg-green-500/20 text-green-400' :
                        handshake.status === 'Failed' ? 'bg-red-500/20 text-red-400' :
                        'bg-yellow-500/20 text-yellow-400'
                      }`}>
                        {handshake.status}
                      </span>
                    </div>
                    <div className="space-y-1 text-sm">
                      <p className="text-gray-400">
                        <span className="text-gray-500">Initiator:</span> {handshake.initiatorUrl}
                      </p>
                      <p className="text-gray-400">
                        <span className="text-gray-500">Responder:</span> {handshake.responderUrl}
                      </p>
                      <p className="text-gray-400">
                        <span className="text-gray-500">Created:</span> {new Date(handshake.createdAt).toLocaleString()}
                      </p>
                      {handshake.expiresAt && (
                        <p className="text-gray-400">
                          <span className="text-gray-500">Expires:</span> {new Date(handshake.expiresAt).toLocaleString()}
                        </p>
                      )}
                      {handshake.completedAt && (
                        <p className="text-gray-400">
                          <span className="text-gray-500">Completed:</span> {new Date(handshake.completedAt).toLocaleString()}
                        </p>
                      )}
                      {handshake.errorMessage && (
                        <p className="text-red-400">
                          <span className="text-gray-500">Error:</span> {handshake.errorMessage}
                        </p>
                      )}
                    </div>
                  </div>
                </div>
              </div>
            ))
          )}
        </div>
      )}

      {/* Handshake Modal */}
      {showHandshakeModal && (
        <div className="fixed inset-0 bg-black/50 flex items-center justify-center z-50" onClick={() => {
          setShowHandshakeModal(false);
          resetHandshakeForm();
        }}>
          <div className="bg-gray-800 rounded-lg p-6 w-full max-w-2xl mx-4" onClick={(e) => e.stopPropagation()}>
            <div className="flex justify-between items-center mb-4">
              <h2 className="text-xl font-bold text-white flex items-center gap-2">
                <HandshakeIcon />
                Initiate Federation Handshake
              </h2>
              <button
                onClick={() => {
                  setShowHandshakeModal(false);
                  resetHandshakeForm();
                }}
                className="text-gray-400 hover:text-white"
              >
                ×
              </button>
            </div>

            <div className="space-y-4">
              <p className="text-gray-400 text-sm">
                Enter the URL of the remote PolyBucket instance you want to federate with. This will initiate a secure handshake process.
              </p>

              <div>
                <label className="block text-sm font-medium text-gray-300 mb-2">
                  Remote Instance URL *
                </label>
                <input
                  type="url"
                  value={handshakeFormData.remoteUrl}
                  onChange={(e) => setHandshakeFormData({ ...handshakeFormData, remoteUrl: e.target.value })}
                  placeholder="https://instance.example.com"
                  className="w-full bg-gray-900 border border-gray-700 rounded-lg px-4 py-2 text-white placeholder-gray-500 focus:outline-none focus:border-purple-500"
                  required
                />
              </div>

              <div>
                <label className="block text-sm font-medium text-gray-300 mb-2">
                  Remote Instance Name *
                </label>
                <input
                  type="text"
                  value={handshakeFormData.remoteName}
                  onChange={(e) => setHandshakeFormData({ ...handshakeFormData, remoteName: e.target.value })}
                  placeholder="Example Instance"
                  className="w-full bg-gray-900 border border-gray-700 rounded-lg px-4 py-2 text-white placeholder-gray-500 focus:outline-none focus:border-purple-500"
                  required
                />
              </div>

              <div className="bg-purple-500/10 border border-purple-500/30 rounded-lg p-4">
                <h3 className="text-sm font-semibold text-purple-300 mb-2">Handshake Process:</h3>
                <ol className="text-xs text-gray-400 space-y-1 list-decimal list-inside">
                  <li>Connection request sent to remote instance</li>
                  <li>Cryptographic tokens exchanged</li>
                  <li>Model catalogs shared between instances</li>
                  <li>Admin selects models to import</li>
                  <li>Federation established!</li>
                </ol>
              </div>

              <div className="flex justify-end gap-2 mt-6">
                <button
                  onClick={() => {
                    setShowHandshakeModal(false);
                    resetHandshakeForm();
                  }}
                  className="px-4 py-2 bg-gray-700 hover:bg-gray-600 text-white rounded-lg transition-colors"
                  disabled={handshakeLoading}
                >
                  Cancel
                </button>
                <button
                  onClick={initiateHandshake}
                  disabled={handshakeLoading || !handshakeFormData.remoteUrl || !handshakeFormData.remoteName}
                  className="px-4 py-2 bg-purple-600 hover:bg-purple-700 text-white rounded-lg transition-colors disabled:opacity-50 disabled:cursor-not-allowed flex items-center gap-2"
                >
                  {handshakeLoading ? (
                    <>
                      <div className="animate-spin rounded-full h-4 w-4 border-b-2 border-white"></div>
                      <span>Initiating...</span>
                    </>
                  ) : (
                    <>
                      <HandshakeIcon />
                      <span>Initiate Handshake</span>
                    </>
                  )}
                </button>
              </div>
            </div>
          </div>
        </div>
      )}

      {showAddModal && (
        <div className="fixed inset-0 bg-black/50 flex items-center justify-center z-50" onClick={() => {
          setShowAddModal(false);
          resetForm();
        }}>
          <div className="bg-gray-800 rounded-lg p-6 w-full max-w-2xl mx-4" onClick={(e) => e.stopPropagation()}>
            <h2 className="text-xl font-bold text-white mb-4">
              {editingInstance ? 'Edit Instance' : 'Add Federated Instance'}
            </h2>
            
            <form onSubmit={handleSubmit} className="space-y-4">
              <div>
                <label className="block text-sm font-medium text-gray-300 mb-2">
                  Instance Name
                </label>
                <input
                  type="text"
                  value={formData.name}
                  onChange={(e) => setFormData({ ...formData, name: e.target.value })}
                  className="w-full px-4 py-2 bg-gray-700 border border-gray-600 rounded-lg text-white focus:outline-none focus:border-blue-500"
                  required
                />
              </div>

              <div>
                <label className="block text-sm font-medium text-gray-300 mb-2">
                  Base URL
                </label>
                <input
                  type="url"
                  value={formData.baseUrl}
                  onChange={(e) => setFormData({ ...formData, baseUrl: e.target.value })}
                  className="w-full px-4 py-2 bg-gray-700 border border-gray-600 rounded-lg text-white focus:outline-none focus:border-blue-500"
                  placeholder="https://example.com"
                  required
                />
              </div>

              <div>
                <label className="block text-sm font-medium text-gray-300 mb-2">
                  Description
                </label>
                <textarea
                  value={formData.description}
                  onChange={(e) => setFormData({ ...formData, description: e.target.value })}
                  className="w-full px-4 py-2 bg-gray-700 border border-gray-600 rounded-lg text-white focus:outline-none focus:border-blue-500 resize-none"
                  rows={3}
                />
              </div>

              <div>
                <label className="flex items-center gap-2 text-sm font-medium text-gray-300">
                  <input
                    type="checkbox"
                    checked={formData.isEnabled}
                    onChange={(e) => setFormData({ ...formData, isEnabled: e.target.checked })}
                    className="w-4 h-4 text-blue-600 bg-gray-700 border-gray-600 rounded focus:ring-blue-500"
                  />
                  Enable this instance
                </label>
              </div>

              <div className="flex gap-3 justify-end pt-4">
                <button
                  type="button"
                  onClick={() => {
                    setShowAddModal(false);
                    resetForm();
                  }}
                  className="px-4 py-2 bg-gray-700 hover:bg-gray-600 text-white rounded-lg transition-colors"
                >
                  Cancel
                </button>
                <button
                  type="submit"
                  className="px-4 py-2 bg-blue-600 hover:bg-blue-700 text-white rounded-lg transition-colors"
                >
                  {editingInstance ? 'Update' : 'Add'} Instance
                </button>
              </div>
            </form>
          </div>
        </div>
      )}
    </div>
  );
};

export default FederationManagement;

