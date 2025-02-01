import { useState } from 'react';
import { ReportReason, ReportType, reportsService } from '../../services/reports.service';

interface ReportDialogProps {
  isOpen: boolean;
  onClose: () => void;
  type: ReportType;
  targetId: string;
  targetName: string;
}

export const ReportDialog: React.FC<ReportDialogProps> = ({
  isOpen,
  onClose,
  type,
  targetId,
  targetName,
}) => {
  const [reason, setReason] = useState<ReportReason>(ReportReason.Inappropriate);
  const [description, setDescription] = useState('');
  const [isSubmitting, setIsSubmitting] = useState(false);
  const [error, setError] = useState<string | null>(null);

  if (!isOpen) return null;

  const handleSubmit = async (e: React.FormEvent) => {
    e.preventDefault();
    setIsSubmitting(true);
    setError(null);

    try {
      await reportsService.submitReport({
        type,
        targetId,
        reason,
        description,
      });
      onClose();
    } catch (err) {
      setError('Failed to submit report. Please try again.');
      console.error('Error submitting report:', err);
    } finally {
      setIsSubmitting(false);
    }
  };

  return (
    <div className="fixed inset-0 z-50 flex items-center justify-center bg-black bg-opacity-50">
      <div className="w-full max-w-md p-6 bg-gray-900 rounded-lg">
        <h2 className="text-xl font-semibold text-green-400 mb-4">
          Report {type === ReportType.Model ? 'Model' : 'Comment'}
        </h2>
        <p className="text-green-300/80 mb-4">
          You are reporting: {targetName}
        </p>

        <form onSubmit={handleSubmit} className="space-y-4">
          <div>
            <label htmlFor="report-reason" className="block text-green-400 mb-2">Reason</label>
            <select
              id="report-reason"
              value={reason}
              onChange={(e) => setReason(Number(e.target.value) as ReportReason)}
              className="w-full p-2 bg-gray-800 border rounded-md border-green-500/30 text-green-300"
            >
              <option value={ReportReason.Inappropriate}>Inappropriate Content</option>
              <option value={ReportReason.Spam}>Spam</option>
              <option value={ReportReason.Copyright}>Copyright Violation</option>
              <option value={ReportReason.Other}>Other</option>
            </select>
          </div>

          <div>
            <label className="block text-green-400 mb-2">Description</label>
            <textarea
              value={description}
              onChange={(e) => setDescription(e.target.value)}
              className="w-full h-32 p-2 bg-gray-800 border rounded-md border-green-500/30 text-green-300"
              placeholder="Please provide details about your report..."
              required
            />
          </div>

          {error && (
            <div className="text-red-500">{error}</div>
          )}

          <div className="flex justify-end space-x-4">
            <button
              type="button"
              onClick={onClose}
              className="px-4 py-2 text-green-400 border rounded-md border-green-500/30 hover:bg-gray-800"
              disabled={isSubmitting}
            >
              Cancel
            </button>
            <button
              type="submit"
              className="px-4 py-2 bg-green-600 text-white rounded-md hover:bg-green-700 disabled:opacity-50"
              disabled={isSubmitting}
            >
              {isSubmitting ? 'Submitting...' : 'Submit Report'}
            </button>
          </div>
        </form>
      </div>
    </div>
  );
}; 