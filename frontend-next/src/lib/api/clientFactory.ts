import { ApiClient } from "./client";
import axiosInstance from "./axiosConfig";

const apiClient = new ApiClient("", axiosInstance);

export class ApiClientFactory {
  static getApiClient = (): ApiClient => apiClient;
}

export default ApiClientFactory;
