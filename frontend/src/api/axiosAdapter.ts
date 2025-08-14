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

            // Check if this is a binary request (like download)
            const headersObj = headers as Record<string, string>;
            const acceptHeader = headersObj['Accept'] || headersObj['accept'];
            const isBinaryRequest = acceptHeader === 'application/octet-stream';

            const axiosConfig: AxiosRequestConfig = {
                method: method.toLowerCase() as any,
                headers: headers as any,
                data: body,
                url: url,
                responseType: isBinaryRequest ? 'blob' : 'json'
            };

            const response = await this.axiosInstance.request(axiosConfig);
            return this.createResponse(response, isBinaryRequest);
        } catch (error: any) {
            if (error.response) {
                // Create a response object from the error
                const response = this.createResponse(error.response, false);
                return response;
            }
            throw error;
        }
    }

    private createResponse(axiosResponse: AxiosResponse, isBinary: boolean = false): Response {
        let responseText: string;
        let responseBody: BodyInit;

        if (isBinary && axiosResponse.data instanceof Blob) {
            // For binary responses, use the blob directly
            responseBody = axiosResponse.data;
            responseText = ''; // Binary data doesn't have text representation
        } else {
            // For JSON/text responses, convert to string
            responseText = JSON.stringify(axiosResponse.data);
            responseBody = responseText;
        }

        const response = new Response(responseBody, {
            status: axiosResponse.status,
            statusText: axiosResponse.statusText,
            headers: new Headers(axiosResponse.headers as any)
        });

        // Store the original axios response data for binary handling
        // This is crucial for ZIP file downloads
        if (isBinary && axiosResponse.data instanceof Blob) {
            (response as any)._axiosData = axiosResponse.data;
            console.log('Binary response created with blob data:', {
                size: axiosResponse.data.size,
                type: axiosResponse.data.type,
                hasAxiosData: !!(response as any)._axiosData
            });
        }

        return response;
    }
}

// Export a default instance
export const defaultAxiosClient = new AxiosHttpClient(); 