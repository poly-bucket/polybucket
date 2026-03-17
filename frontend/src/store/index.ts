// #region agent log
fetch('http://127.0.0.1:7242/ingest/fbd6cafe-c55d-4c1d-9edb-12a6a38267bf',{method:'POST',headers:{'Content-Type':'application/json'},body:JSON.stringify({location:'store/index.ts:1',message:'Store module loading - starting imports',data:{timestamp:Date.now()},timestamp:Date.now(),runId:'run2',hypothesisId:'E'})}).catch(()=>{});
// #endregion
import { configureStore } from '@reduxjs/toolkit';
// #region agent log
fetch('http://127.0.0.1:7242/ingest/fbd6cafe-c55d-4c1d-9edb-12a6a38267bf',{method:'POST',headers:{'Content-Type':'application/json'},body:JSON.stringify({location:'store/index.ts:2',message:'Store - @reduxjs/toolkit imported',data:{timestamp:Date.now()},timestamp:Date.now(),runId:'run2',hypothesisId:'E'})}).catch(()=>{});
// #endregion
import { persistStore, persistReducer } from 'redux-persist';
// #region agent log
fetch('http://127.0.0.1:7242/ingest/fbd6cafe-c55d-4c1d-9edb-12a6a38267bf',{method:'POST',headers:{'Content-Type':'application/json'},body:JSON.stringify({location:'store/index.ts:3',message:'Store - redux-persist imported',data:{timestamp:Date.now()},timestamp:Date.now(),runId:'run2',hypothesisId:'E'})}).catch(()=>{});
// #endregion
import storage from 'redux-persist/lib/storage';
// #region agent log
fetch('http://127.0.0.1:7242/ingest/fbd6cafe-c55d-4c1d-9edb-12a6a38267bf',{method:'POST',headers:{'Content-Type':'application/json'},body:JSON.stringify({location:'store/index.ts:4',message:'Store - about to import react-redux',data:{timestamp:Date.now()},timestamp:Date.now(),runId:'run2',hypothesisId:'E'})}).catch(()=>{});
// #endregion
import { TypedUseSelectorHook, useDispatch, useSelector } from 'react-redux';
// #region agent log
fetch('http://127.0.0.1:7242/ingest/fbd6cafe-c55d-4c1d-9edb-12a6a38267bf',{method:'POST',headers:{'Content-Type':'application/json'},body:JSON.stringify({location:'store/index.ts:5',message:'Store - react-redux imported successfully',data:{timestamp:Date.now()},timestamp:Date.now(),runId:'run2',hypothesisId:'E'})}).catch(()=>{});
// #endregion
import authReducer from './slices/authSlice';
import roleReducer from './slices/roleSlice';
import fileTypeSettingsReducer from './slices/fileTypeSettingsSlice';

const authPersistConfig = {
  key: 'auth',
  storage,
  whitelist: ['isInitialized', 'user'] // Persist both isInitialized and user data
};

const persistedAuthReducer = persistReducer(authPersistConfig, authReducer);

// #region agent log
fetch('http://127.0.0.1:7242/ingest/fbd6cafe-c55d-4c1d-9edb-12a6a38267bf',{method:'POST',headers:{'Content-Type':'application/json'},body:JSON.stringify({location:'store/index.ts:6',message:'Store - about to configure store',data:{timestamp:Date.now()},timestamp:Date.now(),runId:'run2',hypothesisId:'E'})}).catch(()=>{});
// #endregion
const store = configureStore({
  reducer: {
    auth: persistedAuthReducer,
    roles: roleReducer,
    fileTypeSettings: fileTypeSettingsReducer,
  },
  middleware: (getDefaultMiddleware: any) =>
    getDefaultMiddleware({
      serializableCheck: {
        ignoredActions: ['persist/PERSIST', 'persist/REHYDRATE'],
      },
    }),
  devTools: import.meta.env.MODE !== 'production',
});
// #region agent log
fetch('http://127.0.0.1:7242/ingest/fbd6cafe-c55d-4c1d-9edb-12a6a38267bf',{method:'POST',headers:{'Content-Type':'application/json'},body:JSON.stringify({location:'store/index.ts:7',message:'Store - store configured successfully',data:{timestamp:Date.now()},timestamp:Date.now(),runId:'run2',hypothesisId:'E'})}).catch(()=>{});
// #endregion

// #region agent log
fetch('http://127.0.0.1:7242/ingest/fbd6cafe-c55d-4c1d-9edb-12a6a38267bf',{method:'POST',headers:{'Content-Type':'application/json'},body:JSON.stringify({location:'store/index.ts:8',message:'Store - about to create persistor',data:{timestamp:Date.now()},timestamp:Date.now(),runId:'run2',hypothesisId:'E'})}).catch(()=>{});
// #endregion
export const persistor = persistStore(store);
// #region agent log
fetch('http://127.0.0.1:7242/ingest/fbd6cafe-c55d-4c1d-9edb-12a6a38267bf',{method:'POST',headers:{'Content-Type':'application/json'},body:JSON.stringify({location:'store/index.ts:9',message:'Store - persistor created, module complete',data:{timestamp:Date.now()},timestamp:Date.now(),runId:'run2',hypothesisId:'E'})}).catch(()=>{});
// #endregion

// Export types for hooks
export type RootState = ReturnType<typeof store.getState>;
export type AppDispatch = typeof store.dispatch;

// Export typed hooks
export const useAppDispatch = () => useDispatch<AppDispatch>();
export const useAppSelector: TypedUseSelectorHook<RootState> = useSelector;

export default store; 