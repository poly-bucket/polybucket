import { useState, useEffect } from 'react';
import { Report, reportsService } from '../../services/reports.service';
import { ReportCard } from '../../components/report-card/report-card';

export const ReportsPage: React.FC = () => {
  const [reports, setReports] = useState<Report[]>([]);
  const [isLoading, setIsLoading] = useState(true);
  const [error, setError] = useState<string | null>(null);

  useEffect(() => {
    loadReports();
  }, []);

  const loadReports = async () => {
    try {
      setIsLoading(true);
      const fetchedReports = await reportsService.getUnresolvedReports();
      setReports(fetchedReports);
      setError(null);
    } catch (err) {
      setError('Failed to load reports');
      console.error('Error loading reports:', err);
    } finally {
      setIsLoading(false);
    }
  };

  const handleReportResolved = (resolvedReportId: string) => {
    setReports(reports.filter(report => report.id !== resolvedReportId));
  };

  if (isLoading) {
    return <div className="text-green-400">Loading reports...</div>;
  }

  if (error) {
    return <div className="text-red-500">{error}</div>;
  }

  return (
    <div className="container mx-auto px-4 py-8">
      <div className="mb-8">
        <h1 className="text-3xl font-bold text-green-400">Reports</h1>
        <p className="text-green-300/80 mt-2">
          {reports.length} unresolved {reports.length === 1 ? 'report' : 'reports'}
        </p>
      </div>

      <div className="space-y-6">
        {reports.length === 0 ? (
          <div className="text-center py-12 text-green-300/60">
            <p>No unresolved reports</p>
          </div>
        ) : (
          reports.map((report) => (
            <ReportCard
              key={report.id}
              report={report}
              onResolved={() => handleReportResolved(report.id)}
            />
          ))
        )}
      </div>
    </div>
  );
}; 