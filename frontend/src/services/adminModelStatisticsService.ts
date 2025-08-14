import { GetAdminModelStatisticsResponse } from '../api';
import { useAuth } from '../context/AuthContext';
import { API_CONFIG } from '../api/config';
import axios from 'axios';

export class AdminModelStatisticsService {
    private static instance: AdminModelStatisticsService;

    private constructor() {}

    public static getInstance(): AdminModelStatisticsService {
        if (!AdminModelStatisticsService.instance) {
            AdminModelStatisticsService.instance = new AdminModelStatisticsService();
        }
        return AdminModelStatisticsService.instance;
    }

    public async getAdminModelStatistics(accessToken: string): Promise<GetAdminModelStatisticsResponse> {
        try {
            const response = await axios.get(`${API_CONFIG.baseUrl}/api/admin/models/statistics`, {
                headers: {
                    'Authorization': `Bearer ${accessToken}`,
                    'Accept': 'application/json'
                }
            });
            
            return response.data;
        } catch (error) {
            console.error('Error fetching admin model statistics:', error);
            throw error;
        }
    }
}

// Hook to use the service with authentication
export const useAdminModelStatistics = () => {
    const { user } = useAuth();
    
    const getAdminModelStatistics = async (): Promise<GetAdminModelStatisticsResponse> => {
        if (!user?.accessToken) {
            throw new Error('User not authenticated');
        }
        
        const service = AdminModelStatisticsService.getInstance();
        return await service.getAdminModelStatistics(user.accessToken);
    };
    
    return { getAdminModelStatistics };
};

export default AdminModelStatisticsService;
