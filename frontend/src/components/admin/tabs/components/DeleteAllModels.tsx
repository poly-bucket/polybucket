import React, { useState } from 'react';
import {
  DeleteForever as DeleteIcon,
  Warning as WarningIcon,
  CheckCircle as CheckIcon,
  Cancel as CancelIcon,
  Security as SecurityIcon
} from '@mui/icons-material';
import { deleteAllModelsService } from '../../../../services/deleteAllModelsService';

const DeleteAllModels: React.FC = () => {
  const [confirmationStep, setConfirmationStep] = useState(0);
  const [isDeleting, setIsDeleting] = useState(false);
  const [message, setMessage] = useState<string | null>(null);
  const [adminPassword, setAdminPassword] = useState('');
  const [confirmText, setConfirmText] = useState('');

  const confirmationSteps = [
    {
      title: "⚠️ DANGER: Delete All Models",
      description: "This action will permanently delete ALL models from the website. This cannot be undone.",
      action: "Type 'DELETE ALL MODELS' to confirm",
      placeholder: "DELETE ALL MODELS",
      expectedText: "DELETE ALL MODELS"
    },
    {
      title: "🔒 Admin Password Required",
      description: "Enter your admin password to confirm this destructive action.",
      action: "Enter your admin password",
      placeholder: "Enter admin password",
      expectedText: null
    },
    {
      title: "🚨 Final Confirmation",
      description: "This is your last chance to cancel. Are you absolutely sure you want to delete ALL models?",
      action: "Type 'I UNDERSTAND THE CONSEQUENCES' to proceed",
      placeholder: "I UNDERSTAND THE CONSEQUENCES",
      expectedText: "I UNDERSTAND THE CONSEQUENCES"
    }
  ];

  const handleNextStep = () => {
    const currentStep = confirmationSteps[confirmationStep];
    
    if (confirmationStep === 0) {
      if (confirmText === currentStep.expectedText) {
        setConfirmationStep(1);
        setConfirmText('');
      } else {
        setMessage('Please type the exact text to confirm');
      }
    } else if (confirmationStep === 1) {
      if (adminPassword.length > 0) {
        setConfirmationStep(2);
        setConfirmText('');
      } else {
        setMessage('Please enter your admin password');
      }
    } else if (confirmationStep === 2) {
      if (confirmText === currentStep.expectedText) {
        executeDeleteAllModels();
      } else {
        setMessage('Please type the exact text to confirm');
      }
    }
  };

  const executeDeleteAllModels = async () => {
    setIsDeleting(true);
    setMessage('Deleting all models... This may take a while.');
    
    try {
      const response = await deleteAllModelsService.deleteAllModels({
        adminPassword: adminPassword
      });

      if (response.success) {
        setMessage(`✅ ${response.message} (${response.deletedCount} models deleted)`);
        setConfirmationStep(0);
        setAdminPassword('');
        setConfirmText('');
      } else {
        setMessage(`❌ Error: ${response.message}`);
      }
    } catch (error: any) {
      setMessage(`❌ ${error.message || 'Network error occurred while deleting models'}`);
      console.error('Error deleting all models:', error);
    } finally {
      setIsDeleting(false);
    }
  };

  const handleCancel = () => {
    setConfirmationStep(0);
    setAdminPassword('');
    setConfirmText('');
    setMessage(null);
  };

  const currentStep = confirmationSteps[confirmationStep];

  return (
    <div className="space-y-6">
      <div className="lg-card p-6">
        <div className="flex justify-between items-center mb-6">
          <h3 className="text-lg font-medium text-white flex items-center">
            <DeleteIcon className="w-5 h-5 mr-2 text-red-400" />
            Delete All Models
          </h3>
          <div className="flex items-center gap-2 text-red-400">
            <SecurityIcon />
            <span className="text-sm">Admin Only</span>
          </div>
        </div>

        <div className="bg-red-900/20 border border-red-500/50 rounded-lg p-4 mb-6">
          <div className="flex items-start gap-3">
            <WarningIcon className="w-6 h-6 text-red-400 mt-1 flex-shrink-0" />
            <div>
              <h4 className="text-red-300 font-medium mb-2">⚠️ DANGEROUS OPERATION</h4>
              <p className="text-red-200 text-sm">
                This feature will permanently delete ALL models from the website. 
                This action cannot be undone and will affect all users. 
                Use this feature only for testing purposes or when you need to completely reset the model database.
              </p>
            </div>
          </div>
        </div>

        {message && (
          <div className={`mb-4 p-3 rounded ${
            message.includes('✅') 
              ? 'bg-green-500/20 border border-green-500/50 text-green-300' 
              : message.includes('❌')
              ? 'bg-red-500/20 border border-red-500/50 text-red-300'
              : 'bg-yellow-500/20 border border-yellow-500/50 text-yellow-300'
          }`}>
            {message}
          </div>
        )}

        {confirmationStep < confirmationSteps.length && (
          <div className="bg-gray-800 rounded-lg p-6">
            <h4 className="text-xl font-medium text-white mb-4">{currentStep.title}</h4>
            <p className="text-white/80 mb-6">{currentStep.description}</p>
            
            <div className="space-y-4">
              {confirmationStep === 1 && (
                <div>
                  <label className="block text-sm font-medium text-white/80 mb-2">
                    Admin Password
                  </label>
                  <input
                    type="password"
                    value={adminPassword}
                    onChange={(e) => setAdminPassword(e.target.value)}
                    placeholder={currentStep.placeholder}
                    className="lg-input w-full"
                    disabled={isDeleting}
                  />
                </div>
              )}
              
              <div>
                <label className="block text-sm font-medium text-white/80 mb-2">
                  {currentStep.action}
                </label>
                <input
                  type="text"
                  value={confirmText}
                  onChange={(e) => setConfirmText(e.target.value)}
                  placeholder={currentStep.placeholder}
                  className="lg-input w-full"
                  disabled={isDeleting}
                />
              </div>
            </div>

            <div className="flex gap-3 mt-6">
              <button
                onClick={handleNextStep}
                disabled={isDeleting || (confirmationStep === 0 && confirmText !== currentStep.expectedText) || 
                         (confirmationStep === 1 && adminPassword.length === 0) ||
                         (confirmationStep === 2 && confirmText !== currentStep.expectedText)}
                className="lg-button lg-button-danger"
              >
                {isDeleting ? (
                  <>
                    <div className="animate-spin rounded-full h-4 w-4 border-b-2 border-white mr-2"></div>
                    Deleting...
                  </>
                ) : (
                  <>
                    <DeleteIcon className="w-4 h-4 mr-2" />
                    {confirmationStep === 2 ? 'DELETE ALL MODELS' : 'Continue'}
                  </>
                )}
              </button>
              
              <button
                onClick={handleCancel}
                disabled={isDeleting}
                className="lg-button lg-button-secondary"
              >
                <CancelIcon className="w-4 h-4 mr-2" />
                Cancel
              </button>
            </div>

            <div className="mt-4 text-sm text-white/60">
              Step {confirmationStep + 1} of {confirmationSteps.length}
            </div>
          </div>
        )}

        <div className="mt-6 p-4 bg-gray-800 rounded-lg">
          <h5 className="text-white font-medium mb-2">What this action will do:</h5>
          <ul className="text-white/70 text-sm space-y-1">
            <li>• Permanently delete ALL model files from storage</li>
            <li>• Remove ALL model records from the database</li>
            <li>• Delete ALL associated metadata (tags, categories, etc.)</li>
            <li>• Remove ALL user uploads and contributions</li>
            <li>• This action CANNOT be undone</li>
          </ul>
        </div>
      </div>
    </div>
  );
};

export default DeleteAllModels;
