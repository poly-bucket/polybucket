import { ApiClient } from './client';
import { getApiConfig } from './config';
import api from '../utils/axiosConfig';

const apiClient = new ApiClient(getApiConfig().baseUrl, api);

export class ApiClientFactory {
  static getApiClient = () => apiClient;
}

export default ApiClientFactory;
