import { createAsyncThunk } from '@reduxjs/toolkit';
import fileTypeSettingsService, { FileTypeSettingsData, GetFileSettingsResponse } from '../../services/fileTypeSettingsService';

export interface FetchFileSettingsResponse {
  fileTypes: FileTypeSettingsData[];
}

export const fetchFileSettings = createAsyncThunk(
  'fileTypeSettings/fetch',
  async (_, { rejectWithValue }) => {
    try {
      const response: GetFileSettingsResponse = await fileTypeSettingsService.getFileSettings();
      
      if (!response.success || !response.fileTypes) {
        return rejectWithValue(response.message || 'Failed to fetch file type settings');
      }
      
      return {
        fileTypes: response.fileTypes
      } as FetchFileSettingsResponse;
    } catch (error: any) {
      const errorMessage = error.message || 'Failed to fetch file type settings';
      return rejectWithValue(errorMessage);
    }
  }
);

