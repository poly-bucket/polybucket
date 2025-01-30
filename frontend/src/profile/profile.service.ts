import axios from 'axios';

const API_URL = 'https://localhost:44378/api';

export interface UserProfile {
    id: string;
    username: string;
    email: string;
    role: string;
    createdAt: string;
}

export const getUserProfile = async (userId: string): Promise<UserProfile> => {
    const token = localStorage.getItem('auth');
    if (!token) {
        throw new Error('No authentication token found');
    }

    try {
        const response = await axios.get<UserProfile>(
            `${API_URL}/User/${userId}`,
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
            if (error.response?.status === 404) {
                throw new Error('User not found');
            }
            throw new Error(error.response?.data?.message || 'Failed to fetch user profile');
        }
        throw new Error('An unexpected error occurred');
    }
};

export const getCurrentUserProfile = async (): Promise<UserProfile> => {
    const token = localStorage.getItem('token');
    if (!token) {
        throw new Error('No authentication token found');
    }

    try {
        const response = await axios.get<UserProfile>(
            `${API_URL}/User/me`,
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