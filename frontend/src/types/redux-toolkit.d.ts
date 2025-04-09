declare module '@reduxjs/toolkit' {
  export const createSlice: any;
  export const createAsyncThunk: any;
  export const configureStore: any;
  export type PayloadAction<T = any> = {
    payload: T;
    type: string;
  };
  export type ActionReducerMapBuilder<T> = any;
} 