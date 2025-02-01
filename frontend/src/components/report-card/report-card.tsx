import { useState } from 'react';
import { formatDistanceToNow } from 'date-fns';
import { Report, ReportReason, ReportType, reportsService } from '../../services/reports.service';

interface ReportCardProps {
  report: Report;
  onResolved: () => void;
}

export const ReportCard: React.FC<ReportCardProps> = ({ report, onResolved }) => {
  const [isResolving, setIsResolving] = useState(false);
  const [resolution, setResolution] = useState('');
  const [isResolutionOpen, setIsResolutionOpen] = useState(false);
  const [error, setError] = useState<string | null>(null);

  const getReasonText = (reason: ReportReason) => {
    switch (reason) {
      case ReportReason.Inappropriate:
        return 'Inappropriate Content';
      case ReportReason.Spam:
        return 'Spam';
      case ReportReason.Copyright:
        return 'Copyright Violation';
      case ReportReason.Other:
        return 'Other';
    }
  };

  const handleResolve = async () => {
    if (!resolution.trim()) {
      setError('Please provide a resolution');
      return;
    }

    try {
      setIsResolving(true);
      setError(null);
      await reportsService.resolveReport(report.id, resolution);
      onResolved();
    } catch (err) {
      setError('Failed to resolve report');
      console.error('Error resolving report:', err);
    } finally {
      setIsResolving(false);
    }
  };

  return (
    <div className="p-6 bg-gray-900 border rounded-lg border-green-500/30">
      <div className="flex justify-between items-start mb-4">
        <div>
          <h3 className="text-lg font-semibold text-green-400">
            {report.type === ReportType.Model ? 'Model Report' : 'Comment Report'}
          </h3>
          <p className="text-sm text-green-300/60">
            Reported {formatDistanceToNow(new Date(report.createdAt), { addSuffix: true })}
          </p>
        </div>
        <span className="px-3 py-1 text-sm rounded-full bg-red-500/20 text-red-400">
          {getReasonText(report.reason)}
        </span>
      </div>

      <div className="space-y-4">
        <div>
          <h4 className="font-medium text-green-400 mb-1">Description</h4>
          <p className="text-green-300/80">{report.description}</p>
        </div>

        {!report.isResolved && (
          <div>
            {isResolutionOpen ? (
              <div className="space-y-4">
                <div>
                  <label className="block text-green-400 mb-2">Resolution</label>
                  <textarea
                    value={resolution}
                    onChange={(e) => setResolution(e.target.value)}
                    className="w-full h-32 p-2 bg-gray-800 border rounded-md border-green-500/30 text-green-300"
                    placeholder="Describe how this report was resolved..."
                    required
                  />
                </div>

                {error && (
                  <div className="text-red-500">{error}</div>
                )}

                <div className="flex justify-end space-x-4">
                  <button
                    type="button"
                    onClick={() => setIsResolutionOpen(false)}
                    className="px-4 py-2 text-green-400 border rounded-md border-green-500/30 hover:bg-gray-800"
                    disabled={isResolving}
                  >
                    Cancel
                  </button>
                  <button
                    type="button"
                    onClick={handleResolve}
                    className="px-4 py-2 bg-green-600 text-white rounded-md hover:bg-green-700 disabled:opacity-50"
                    disabled={isResolving}
                  >
                    {isResolving ? 'Resolving...' : 'Resolve Report'}
                  </button>
                </div>
              </div>
            ) : (
              <button
                onClick={() => setIsResolutionOpen(true)}
                className="px-4 py-2 bg-green-600 text-white rounded-md hover:bg-green-700"
              >
                Resolve
              </button>
            )}
          </div>
        )}

        {report.isResolved && (
          <div>
            <h4 className="font-medium text-green-400 mb-1">Resolution</h4>
            <p className="text-green-300/80">{report.resolution}</p>
            <p className="text-sm text-green-300/60 mt-2">
              Resolved {report.resolvedAt && formatDistanceToNow(new Date(report.resolvedAt), { addSuffix: true })}
            </p>
          </div>
        )}
      </div>
    </div>
  );
}; 