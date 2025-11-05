import { API_CONFIG } from './config';
import { AxiosHttpClient } from './axiosAdapter';
import {
  GetUserByIdClient,
  GetUserSettingsClient,
  UpdateUserSettingsClient,
  SystemSetupClient,
  GetReportClient,
  GetReportsForTargetClient,
  GetUnresolvedReportsClient,
  ResolveReportClient,
  SubmitReportClient,
  GetPrintersClient,
  GetPluginsClient,
  ReloadPluginsClient,
  GetModelByIdClient,
  DeleteModelClient,
  UpdateModelClient,
  GetModelsClient,
  CreateModelClient,
  AddCategoryToModelClient,
  RemoveCategoryFromModelClient,
  AddModelToCollectionClient,
  RemoveModelFromCollectionClient,
  AddTagToModelClient,
  CreateModelVersionClient,
  GetModelVersionsClient,
  DownloadModelClient,
  GetModelByUserIdClient,
  LikeModelClient,
  RemoveTagFromModelClient,
  SearchClient,
  ApproveModelClient,
  GetModelsAwaitingModerationClient,
  GetModerationSettingsClient,
  UpdateModerationSettingsClient,
  RejectModelClient,
  GetFileConfigClient,
  GetSupportedExtensionsByTypeClient,
  GetSupportedExtensionsClient,
  CreateFilamentClient,
  GetAllFilamentsClient,
  DeleteFilamentClient,
  GetFilamentByIdClient,
  UpdateFilamentClient,
  GetCommentsForModelClient,
  AddCommentClient,
  EnhancedCommentsClient,
  LoginClient,
  GetEmailSettingsClient,
  RegisterClient,
  RefreshTokenClient,
  CreateCollectionClient,
  UpdateCollectionClient,
  GetCollectionByIdClient,
  DeleteCollectionClient,
  GetUserCollectionsClient,
  GetFavoriteCollectionsClient,
  FavoriteCollectionClient,
  AccessCollectionClient,
  GetCategoriesClient,
  CreateCategoryClient,
  UpdateCategoryClient,
  DeleteCategoryClient,
  GetTwoFactorAuthStatusClient,
  InitializeTwoFactorAuthClient,
  EnableTwoFactorAuthClient,
  DisableTwoFactorAuthClient,
  RegenerateAvatarClient,
  GetFileSettingsClient,
  UpdateFileSettingsClient,
  GetPluginDetailsClient,
  GetAllReportsClient,
  GetReportsAnalyticsClient,
  GetModerationAuditLogsClient,
  RoleManagementClient,
  BanUserClient
} from '../services/api.client';

// Create a shared HTTP client instance
const sharedHttpClient = new AxiosHttpClient(API_CONFIG.baseUrl);

// API Client Factory
export class ApiClientFactory {
  // User-related clients
  static getUserByIdClient = () => new GetUserByIdClient(API_CONFIG.baseUrl, sharedHttpClient);
  static getUserSettingsClient = () => new GetUserSettingsClient(API_CONFIG.baseUrl, sharedHttpClient);
  static getUpdateUserSettingsClient = () => new UpdateUserSettingsClient(API_CONFIG.baseUrl, sharedHttpClient);

  // Admin-related clients
  static getAdminSetupClient = () => new SystemSetupClient(API_CONFIG.baseUrl, sharedHttpClient);
  static getSetupStatusClient = () => new SystemSetupClient(API_CONFIG.baseUrl, sharedHttpClient);

  // Report-related clients
  static getReportClient = () => new GetReportClient(API_CONFIG.baseUrl, sharedHttpClient);
  static getReportsForTargetClient = () => new GetReportsForTargetClient(API_CONFIG.baseUrl, sharedHttpClient);
  static getUnresolvedReportsClient = () => new GetUnresolvedReportsClient(API_CONFIG.baseUrl, sharedHttpClient);
  static getResolveReportClient = () => new ResolveReportClient(API_CONFIG.baseUrl, sharedHttpClient);
  static getSubmitReportClient = () => new SubmitReportClient(API_CONFIG.baseUrl, sharedHttpClient);

  // Printer-related clients
  static getPrintersClient = () => new GetPrintersClient(API_CONFIG.baseUrl, sharedHttpClient);

  // Plugin-related clients
  static getPluginsClient = () => new GetPluginsClient(API_CONFIG.baseUrl, sharedHttpClient);
  static getReloadPluginsClient = () => new ReloadPluginsClient(API_CONFIG.baseUrl, sharedHttpClient);

  // Model-related clients
  static getModelByIdClient = () => new GetModelByIdClient(API_CONFIG.baseUrl, sharedHttpClient);
  static getDeleteModelClient = () => new DeleteModelClient(API_CONFIG.baseUrl, sharedHttpClient);
  static getUpdateModelClient = () => new UpdateModelClient(API_CONFIG.baseUrl, sharedHttpClient);
  static getModelsClient = () => new GetModelsClient(API_CONFIG.baseUrl, sharedHttpClient);
  static getUploadModelClient = () => new CreateModelClient(API_CONFIG.baseUrl, sharedHttpClient);
  static getAddCategoryToModelClient = () => new AddCategoryToModelClient(API_CONFIG.baseUrl, sharedHttpClient);
  static getRemoveCategoryFromModelClient = () => new RemoveCategoryFromModelClient(API_CONFIG.baseUrl, sharedHttpClient);
  static getAddModelToCollectionClient = () => new AddModelToCollectionClient(API_CONFIG.baseUrl, sharedHttpClient);
  static getRemoveModelFromCollectionClient = () => new RemoveModelFromCollectionClient(API_CONFIG.baseUrl, sharedHttpClient);
  static getAddTagToModelClient = () => new AddTagToModelClient(API_CONFIG.baseUrl, sharedHttpClient);
  static getCreateModelVersionClient = () => new CreateModelVersionClient(API_CONFIG.baseUrl, sharedHttpClient);
  static getModelVersionsClient = () => new GetModelVersionsClient(API_CONFIG.baseUrl, sharedHttpClient);
  static getDownloadModelClient = () => new DownloadModelClient(API_CONFIG.baseUrl, sharedHttpClient);
  static getModelsByUserClient = () => new GetModelByUserIdClient(API_CONFIG.baseUrl, sharedHttpClient);
  static getLikeModelClient = () => new LikeModelClient(API_CONFIG.baseUrl, sharedHttpClient);
  static getRemoveTagFromModelClient = () => new RemoveTagFromModelClient(API_CONFIG.baseUrl, sharedHttpClient);
  static getSearchModelsClient = () => new SearchClient(API_CONFIG.baseUrl, sharedHttpClient);
  static getApproveModelClient = () => new ApproveModelClient(API_CONFIG.baseUrl, sharedHttpClient);
  static getModelsAwaitingModerationClient = () => new GetModelsAwaitingModerationClient(API_CONFIG.baseUrl, sharedHttpClient);
  static getModerationSettingsClient = () => new GetModerationSettingsClient(API_CONFIG.baseUrl, sharedHttpClient);
  static getUpdateModerationSettingsClient = () => new UpdateModerationSettingsClient(API_CONFIG.baseUrl, sharedHttpClient);
  static getRejectModelClient = () => new RejectModelClient(API_CONFIG.baseUrl, sharedHttpClient);

  // File-related clients
  static getFileConfigClient = () => new GetFileConfigClient(API_CONFIG.baseUrl, sharedHttpClient);
  static getSupportedExtensionsByTypeClient = () => new GetSupportedExtensionsByTypeClient(API_CONFIG.baseUrl, sharedHttpClient);
  static getSupportedExtensionsClient = () => new GetSupportedExtensionsClient(API_CONFIG.baseUrl, sharedHttpClient);

  // Filament-related clients
  static getCreateFilamentClient = () => new CreateFilamentClient(API_CONFIG.baseUrl, sharedHttpClient);
  static getAllFilamentsClient = () => new GetAllFilamentsClient(API_CONFIG.baseUrl, sharedHttpClient);
  static getDeleteFilamentClient = () => new DeleteFilamentClient(API_CONFIG.baseUrl, sharedHttpClient);
  static getFilamentByIdClient = () => new GetFilamentByIdClient(API_CONFIG.baseUrl, sharedHttpClient);
  static getUpdateFilamentClient = () => new UpdateFilamentClient(API_CONFIG.baseUrl, sharedHttpClient);

  // Comment-related clients
  static getCommentsForModelClient = () => new GetCommentsForModelClient(API_CONFIG.baseUrl, sharedHttpClient);
  static getAddCommentClient = () => new AddCommentClient(API_CONFIG.baseUrl, sharedHttpClient);
  static getDeleteCommentClient = () => new EnhancedCommentsClient(API_CONFIG.baseUrl, sharedHttpClient);
  static getDislikeCommentClient = () => new EnhancedCommentsClient(API_CONFIG.baseUrl, sharedHttpClient);
  static getLikeCommentClient = () => new EnhancedCommentsClient(API_CONFIG.baseUrl, sharedHttpClient);

  // Authentication-related clients
  static getLoginClient = () => new LoginClient(API_CONFIG.baseUrl, sharedHttpClient);
  static getRegisterClient = () => new RegisterClient(API_CONFIG.baseUrl, sharedHttpClient);
  static getRefreshTokenClient = () => new RefreshTokenClient(API_CONFIG.baseUrl, sharedHttpClient);

  // Collection-related clients
  static getCreateCollectionClient = () => new CreateCollectionClient(API_CONFIG.baseUrl, sharedHttpClient);
  static getUpdateCollectionClient = () => new UpdateCollectionClient(API_CONFIG.baseUrl, sharedHttpClient);
  static getGetCollectionByIdClient = () => new GetCollectionByIdClient(API_CONFIG.baseUrl, sharedHttpClient);
  static getDeleteCollectionClient = () => new DeleteCollectionClient(API_CONFIG.baseUrl, sharedHttpClient);
  static getGetUserCollectionsClient = () => new GetUserCollectionsClient(API_CONFIG.baseUrl, sharedHttpClient);
  static getGetFavoriteCollectionsClient = () => new GetFavoriteCollectionsClient(API_CONFIG.baseUrl, sharedHttpClient);
  static getFavoriteCollectionClient = () => new FavoriteCollectionClient(API_CONFIG.baseUrl, sharedHttpClient);
  static getAccessCollectionClient = () => new AccessCollectionClient(API_CONFIG.baseUrl, sharedHttpClient);

  // Category-related clients
  static getGetCategoriesClient = () => new GetCategoriesClient(API_CONFIG.baseUrl, sharedHttpClient);
  static getCreateCategoryClient = () => new CreateCategoryClient(API_CONFIG.baseUrl, sharedHttpClient);
  static getUpdateCategoryClient = () => new UpdateCategoryClient(API_CONFIG.baseUrl, sharedHttpClient);
  static getDeleteCategoryClient = () => new DeleteCategoryClient(API_CONFIG.baseUrl, sharedHttpClient);

  // Two-Factor Auth clients
  static getGetTwoFactorAuthStatusClient = () => new GetTwoFactorAuthStatusClient(API_CONFIG.baseUrl, sharedHttpClient);
  static getInitializeTwoFactorAuthClient = () => new InitializeTwoFactorAuthClient(API_CONFIG.baseUrl, sharedHttpClient);
  static getEnableTwoFactorAuthClient = () => new EnableTwoFactorAuthClient(API_CONFIG.baseUrl, sharedHttpClient);
  static getDisableTwoFactorAuthClient = () => new DisableTwoFactorAuthClient(API_CONFIG.baseUrl, sharedHttpClient);

  // Avatar client
  static getRegenerateAvatarClient = () => new RegenerateAvatarClient(API_CONFIG.baseUrl, sharedHttpClient);

  // File Settings clients
  static getGetFileSettingsClient = () => new GetFileSettingsClient(API_CONFIG.baseUrl, sharedHttpClient);
  static getUpdateFileSettingsClient = () => new UpdateFileSettingsClient(API_CONFIG.baseUrl, sharedHttpClient);

  // File clients
  static getGetSupportedExtensionsClient = () => new GetSupportedExtensionsClient(API_CONFIG.baseUrl, sharedHttpClient);
  static getGetSupportedExtensionsByTypeClient = () => new GetSupportedExtensionsByTypeClient(API_CONFIG.baseUrl, sharedHttpClient);

  // Plugin clients
  static getGetPluginDetailsClient = () => new GetPluginDetailsClient(API_CONFIG.baseUrl, sharedHttpClient);

  // Moderation clients
  static getGetAllReportsClient = () => new GetAllReportsClient(API_CONFIG.baseUrl, sharedHttpClient);
  static getGetReportsAnalyticsClient = () => new GetReportsAnalyticsClient(API_CONFIG.baseUrl, sharedHttpClient);
  static getGetModerationAuditLogsClient = () => new GetModerationAuditLogsClient(API_CONFIG.baseUrl, sharedHttpClient);

  // Role management client
  static getRoleManagementClient = () => new RoleManagementClient(API_CONFIG.baseUrl, sharedHttpClient);

  // Search-related clients
  static getSearchClient = () => new SearchClient(API_CONFIG.baseUrl, sharedHttpClient);

  // System settings clients
  static getSystemSettingsClient = () => new GetEmailSettingsClient(API_CONFIG.baseUrl, sharedHttpClient);
}

// Export the factory as default
export default ApiClientFactory; 