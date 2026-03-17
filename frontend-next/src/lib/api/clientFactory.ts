import { ApiClient } from "./client";
import { getApiConfig } from "./config";
import axiosInstance from "./axiosConfig";

const apiClient = new ApiClient(getApiConfig().baseUrl, axiosInstance);

export class ApiClientFactory {
  static getApiClient = (): ApiClient => apiClient;
}

export default ApiClientFactory;
