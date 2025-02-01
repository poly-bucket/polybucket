import axios from 'axios';
import { Model, PaginatedModelsResponse } from '../types/models';

const API_URL = 'https://localhost:44378/api';

export const ModelsService = {
    getModels: async (page: number = 1, take: number = 20): Promise<PaginatedModelsResponse> => {
        try {
            const response = await axios.get<PaginatedModelsResponse>(
                `${API_URL}/Models`,
                {
                    params: { page, take }
                }
            );
            return response.data;
        } catch (error) {
            console.error('Error fetching models:', error);
            throw error;
        }
    },

    getModelById: async (id: string): Promise<Model> => {
        try {
            const response = await axios.get<{ model: Model }>(
                `${API_URL}/Models/${id}`
            );
            return response.data.model;
        } catch (error) {
            console.error('Error fetching model:', error);
            throw error;
        }
    }
}; 