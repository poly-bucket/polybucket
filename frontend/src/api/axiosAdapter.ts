import axios, { AxiosInstance, AxiosRequestConfig, AxiosResponse } from 'axios';
import { API_CONFIG } from './config';
import api from '../utils/axiosConfig';

// Axios HTTP client wrapper that mimics the fetch interface
export class AxiosHttpClient {
    private axiosInstance: AxiosInstance;

    constructor(baseUrl?: string, config?: AxiosRequestConfig) {
        const finalBaseUrl = baseUrl || API_CONFIG.baseUrl;
        
        // Use the configured axios instance with interceptors if the base URL matches
        if (finalBaseUrl === API_CONFIG.baseUrl) {
            this.axiosInstance = api;
        } else {
            // Create a new instance for different base URLs
            this.axiosInstance = axios.create({
                baseURL: finalBaseUrl,
                timeout: API_CONFIG.timeout,
                withCredentials: API_CONFIG.withCredentials,
                ...config
            });
        }
    }

    async fetch(url: string, init?: RequestInit): Promise<Response> {
        try {
            const method = init?.method || 'GET';
            const headers = init?.headers || {};
            const body = init?.body;

            const axiosConfig: AxiosRequestConfig = {
                method: method.toLowerCase() as any,
                headers: headers as any,
                data: body,
                url: url
            };

            const response = await this.axiosInstance.request(axiosConfig);
            return this.createResponse(response);
        } catch (error: any) {
            if (error.response) {
                // Create a response object from the error
                const response = this.createResponse(error.response);
                return response;
            }
            throw error;
        }
    }

    private createResponse(axiosResponse: AxiosResponse): Response {
        const responseText = JSON.stringify(axiosResponse.data);
        const response = new Response(responseText, {
            status: axiosResponse.status,
            statusText: axiosResponse.statusText,
            headers: new Headers(axiosResponse.headers as any)
        });
        return response;
    }
}

// Export a default instance
export const defaultAxiosClient = new AxiosHttpClient(); 