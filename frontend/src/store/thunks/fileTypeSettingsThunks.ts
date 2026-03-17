import { createAsyncThunk } from '@reduxjs/toolkit';
import fileTypeSettingsService, { FileTypeSettingsData, GetFileSettingsResponse } from '../../services/fileTypeSettingsService';

export interface FetchFileSettingsResponse {
  fileTypes: FileTypeSettingsData[];
}

export const fetchFileSettings = createAsyncThunk(
  'fileTypeSettings/fetch',
  async (_: any, { rejectWithValue }: { rejectWithValue: (value: string) => void }) => {
    try {
      const response: GetFileSettingsResponse = await fileTypeSettingsService.getFileSettings();
      
      if (!response.success || !response.fileTypes) {
        console.error('Failed to fetch file type settings:', response.message);
        return rejectWithValue(response.message || 'Failed to fetch file type settings');
      }
      
      return {
        fileTypes: response.fileTypes
      } as FetchFileSettingsResponse;
    } catch (error: any) {
      const errorMessage = error.message || 'Failed to fetch file type settings';
      console.error('Failed to fetch file type settings:', errorMessage);
      return rejectWithValue(errorMessage);
    }
  }
);

