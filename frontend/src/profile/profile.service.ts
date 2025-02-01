import api from '../services/api';
import { AxiosError } from 'axios';

export interface UserProfile {
    id: string;
    username: string;
    email: string;
    role: string;
    createdAt: string;
    // Add any additional fields we want to display
    bio?: string;
    avatarUrl?: string;
    displayName?: string;
}

export const getUserProfile = async (userId: string): Promise<UserProfile> => {
    try {
        const response = await api.get<UserProfile>(`/Users/${userId}`);
        return response.data;
    } catch (error) {
        const axiosError = error as AxiosError<{ message: string }>;
        if (axiosError.response?.status === 404) {
            throw new Error('User not found');
        }
        throw new Error(axiosError.response?.data?.message || 'Failed to fetch user profile');
    }
};

export const getCurrentUserProfile = async (): Promise<UserProfile> => {
    const token = localStorage.getItem('token');
    if (!token) {
        throw new Error('No authentication token found');
    }

    try {
        const response = await api.get<UserProfile>(
            `/Users/me`,
            {
                headers: {
                    'Authorization': `Bearer ${token}`,
                },
            }
        );
        return response.data;
    } catch (error) {
        if (axios.isAxiosError(error)) {
            console.error('Profile fetch error:', error.response?.data);
            throw new Error(error.response?.data?.message || 'Failed to fetch current user profile');
        }
        throw new Error('An unexpected error occurred');
    }
}; 