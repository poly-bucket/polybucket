import axios from 'axios';

// Base URL for API requests
const API_URL = import.meta.env.VITE_API_URL ? `${import.meta.env.VITE_API_URL}/api/file` : 'http://localhost:11666/api/file';

// Types for file configuration
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

/**
 * Gets all supported file extensions
 */
const getSupportedExtensions = async (): Promise<string[]> => {
  const response = await axios.get(`${API_URL}/extensions`);
  return response.data;
};

/**
 * Gets supported file extensions for a specific file type
 * @param fileType The type of file (numeric ID or string name)
 */
const getSupportedExtensionsForType = async (fileType: number | string): Promise<string[]> => {
  const response = await axios.get(`${API_URL}/extensions/by-type/${fileType}`);
  return response.data;
};

/**
 * Gets complete file configuration including extensions, types, and rules
 */
const getFileConfiguration = async (): Promise<FileUploadConfiguration> => {
  const response = await axios.get(`${API_URL}/config`);
  return response.data;
};

/**
 * Updates the file upload configuration settings
 * @param config The updated configuration
 */
const updateFileConfiguration = async (config: Partial<FileUploadConfiguration>): Promise<void> => {
  await axios.post(`${API_URL}/config`, config);
};

/**
 * Formats a file size in bytes to a human-readable string
 * @param bytes File size in bytes
 */
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

/**
 * Checks if a file's extension is supported
 * @param fileName The file name to check
 * @param supportedExtensions List of supported extensions
 */
const isFileExtensionSupported = (
  fileName: string, 
  supportedExtensions: string[]
): boolean => {
  // Extract extension from filename (including the dot)
  const extension = fileName.substring(fileName.lastIndexOf('.')).toLowerCase();
  
  // Check if it's in the supported list
  return supportedExtensions.some(ext => 
    ext.toLowerCase() === extension
  );
};

/**
 * Extracts the file extension from a filename
 * @param fileName File name
 */
const getFileExtension = (fileName: string): string => {
  return fileName.substring(fileName.lastIndexOf('.')).toLowerCase();
};

/**
 * Gets the appropriate file type for a given file extension
 * @param fileName File name
 * @param extensionsByType Map of file types to allowed extensions
 */
const suggestFileType = (
  fileName: string, 
  extensionsByType: Record<string, string[]>
): string => {
  const extension = getFileExtension(fileName);
  
  // Look through each file type's allowed extensions
  for (const [fileType, extensions] of Object.entries(extensionsByType)) {
    if (extensions.some(ext => ext.toLowerCase() === extension)) {
      return fileType;
    }
  }
  
  // Default
  return 'Other';
};

const fileService = {
  getSupportedExtensions,
  getSupportedExtensionsForType,
  getFileConfiguration,
  updateFileConfiguration,
  formatFileSize,
  isFileExtensionSupported,
  getFileExtension,
  suggestFileType
};

export default fileService; 