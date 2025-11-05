import { createSlice, PayloadAction, ActionReducerMapBuilder } from '@reduxjs/toolkit';
import { fetchFileSettings, FetchFileSettingsResponse } from '../thunks/fileTypeSettingsThunks';
import { FileTypeSettingsData } from '../../services/fileTypeSettingsService';

interface FileTypeSettingsState {
  fileTypes: FileTypeSettingsData[];
  isLoading: boolean;
  isError: boolean;
  errorMessage: string;
  lastFetched: number | null;
}

const initialState: FileTypeSettingsState = {
  fileTypes: [],
  isLoading: false,
  isError: false,
  errorMessage: '',
  lastFetched: null
};

const fileTypeSettingsSlice = createSlice({
  name: 'fileTypeSettings',
  initialState,
  reducers: {
    reset: (state: FileTypeSettingsState) => {
      state.isLoading = false;
      state.isError = false;
      state.errorMessage = '';
    },
    clearFileTypeSettings: (state: FileTypeSettingsState) => {
      state.fileTypes = [];
      state.isLoading = false;
      state.isError = false;
      state.errorMessage = '';
      state.lastFetched = null;
    },
    updateFileType: (state: FileTypeSettingsState, action: PayloadAction<FileTypeSettingsData>) => {
      const index = state.fileTypes.findIndex(ft => ft.id === action.payload.id);
      if (index !== -1) {
        state.fileTypes[index] = action.payload;
      }
    }
  },
  extraReducers: (builder: ActionReducerMapBuilder<FileTypeSettingsState>) => {
    builder
      .addCase(fetchFileSettings.pending, (state: FileTypeSettingsState) => {
        state.isLoading = true;
        state.isError = false;
        state.errorMessage = '';
      })
      .addCase(fetchFileSettings.fulfilled, (state: FileTypeSettingsState, action: PayloadAction<FetchFileSettingsResponse>) => {
        state.isLoading = false;
        state.fileTypes = action.payload.fileTypes;
        state.isError = false;
        state.errorMessage = '';
        state.lastFetched = Date.now();
      })
      .addCase(fetchFileSettings.rejected, (state: FileTypeSettingsState, action: any) => {
        state.isLoading = false;
        state.isError = true;
        state.errorMessage = action.payload || 'Failed to fetch file type settings';
      });
  },
});

export const { reset, clearFileTypeSettings, updateFileType } = fileTypeSettingsSlice.actions;
export default fileTypeSettingsSlice.reducer;

