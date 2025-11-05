import { API_CONFIG } from '../api/config';
import { AxiosHttpClient } from '../api/axiosAdapter';
import {
  GetSupportedExtensionsClient,
  GetSupportedExtensionsByTypeClient,
  GetFileConfigClient
} from './api.client';

const sharedHttpClient = new AxiosHttpClient(API_CONFIG.baseUrl);

export interface FileExtensionInfo {
  id: number;
  name: string;
  extension: string;
  mimeType: string;
  category: string;
}

export interface FileTypeInfo {
  id: number;
  name: string;
  description: string;
}

export interface FileUploadConfiguration {
  maxFileSizeBytes: number;
  perCategoryFileSizeLimits: Record<string, number>;
  supportedExtensions: FileExtensionInfo[];
  fileTypes: FileTypeInfo[];
  extensionsByType: Record<string, string[]>;
}

const getSupportedExtensions = async (): Promise<string[]> => {
  const client = new GetSupportedExtensionsClient(API_CONFIG.baseUrl, sharedHttpClient);
  const response = await client.getSupportedExtensions();
  return response as any as string[];
};

const getSupportedExtensionsForType = async (fileType: number | string): Promise<string[]> => {
  const client = new GetSupportedExtensionsByTypeClient(API_CONFIG.baseUrl, sharedHttpClient);
  const response = await client.getSupportedExtensionsByType(fileType.toString());
  return response as any as string[];
};

const getFileConfiguration = async (): Promise<FileUploadConfiguration> => {
  const client = new GetFileConfigClient(API_CONFIG.baseUrl, sharedHttpClient);
  const response = await client.getFileConfig();
  return response as any as FileUploadConfiguration;
};

// SKIPPED: No update endpoint found in generated client
const updateFileConfiguration = async (config: Partial<FileUploadConfiguration>): Promise<void> => {
  // SKIPPED: Update endpoint not available in generated client
  throw new Error('Update file configuration endpoint not available in generated client');
};

const formatFileSize = (bytes: number): string => {
  if (bytes >= 1073741824) {
    return `${(bytes / 1073741824).toFixed(2)} GB`;
  } else if (bytes >= 1048576) {
    return `${(bytes / 1048576).toFixed(2)} MB`;
  } else if (bytes >= 1024) {
    return `${(bytes / 1024).toFixed(2)} KB`;
  } else {
    return `${bytes} bytes`;
  }
};

const isFileExtensionSupported = (
  fileName: string, 
  supportedExtensions: string[]
): boolean => {
  const extension = fileName.substring(fileName.lastIndexOf('.')).toLowerCase();
  return supportedExtensions.some(ext => 
    ext.toLowerCase() === extension
  );
};

const getFileExtension = (fileName: string): string => {
  const lastDot = fileName.lastIndexOf('.');
  return lastDot >= 0 ? fileName.substring(lastDot + 1).toLowerCase() : '';
};

const fileService = {
  getSupportedExtensions,
  getSupportedExtensionsForType,
  getFileConfiguration,
  updateFileConfiguration,
  formatFileSize,
  isFileExtensionSupported,
  getFileExtension
};

export default fileService;
