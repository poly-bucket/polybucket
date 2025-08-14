// Export configuration
export * from './config';

// Export the client factory
export { default as ApiClientFactory } from './clientFactory';

// Export all generated types and classes
export * from './client';

// Re-export commonly used types
export type {
  GetUserByIdResponse,
  GetUserSettingsResponse,
  GetModelByIdResponse,
  GetModelsResponse,
  GetAdminModelStatisticsResponse,
  LoginCommand,
  AddCommentRequest,
  CreateFilamentCommand,
  UpdateFilamentCommand,
  SubmitReportRequest,
  ResolveReportRequest,
  Model,
  User,
  UserSettings,
  Comment,
  Category,
  Tag,
  ModelFile,
  ModelVersion,
  Like,
  PluginInfo,
  ProblemDetails,
  FileResponse,
  ApiException
} from './client'; 