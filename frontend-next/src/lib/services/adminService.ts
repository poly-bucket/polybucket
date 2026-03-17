import { ApiClientFactory } from "@/lib/api/clientFactory";
import type {
  GetAdminModelStatisticsResponse,
  GetUsersResponse,
  PaginatedRolesResponse,
  RoleDto,
  CreateRoleRequest,
  UpdateRoleRequest,
  AssignPermissionsRequest,
  RemovePermissionsRequest,
  PermissionDto,
  BanUserRequest,
  CreateUserCommand,
  CreateUserCommandResponse,
  GetModelConfigurationSettingsResponse,
  UpdateModelConfigurationSettingsCommand,
  UpdateModelConfigurationSettingsResponse,
  GetSiteModelSettingsResponse,
  UpdateSiteModelSettingsCommand,
  UpdateSiteModelSettingsResponse,
  GetFileSettingsResponse,
  UpdateFileSettingsCommand,
  UpdateFileSettingsResponse,
  GetCategoriesResponse,
  CreateCategoryResponse,
  UpdateCategoryCommand,
  UpdateCategoryResponse,
  DeleteCategoryResponse,
  EmailSettingsResponse,
  UpdateEmailSettingsCommand,
  UpdateEmailSettingsResponse,
  TestEmailConfigurationCommand,
  TestEmailConfigurationResponse,
  TokenSettings,
  CheckFirstTimeSetupResponse,
  UpdateSiteSettingsCommand,
  UpdateSiteSettingsResponse,
  DeleteAllModelsRequest,
  DeleteAllModelsResponse,
  AuthenticationSettings,
} from "@/lib/api/client";
import {
  CreateCategoryCommand,
} from "@/lib/api/client";

const client = () => ApiClientFactory.getApiClient();

export async function getAdminModelStatistics(): Promise<GetAdminModelStatisticsResponse> {
  return client().getAdminModelStatistics_GetAdminModelStatistics();
}

export async function getUsers(
  page?: number,
  pageSize?: number,
  searchQuery?: string | null,
  roleFilter?: string | null,
  statusFilter?: string | null,
  sortBy?: string | null,
  sortDescending?: boolean
): Promise<GetUsersResponse> {
  return client().getUsers_GetUsers(
    page,
    pageSize,
    searchQuery ?? undefined,
    roleFilter ?? undefined,
    statusFilter ?? undefined,
    sortBy ?? undefined,
    sortDescending ?? false
  );
}

export async function createUser(command: CreateUserCommand): Promise<CreateUserCommandResponse> {
  return client().createUser_CreateUser(command);
}

export async function banUser(userId: string, request: BanUserRequest): Promise<void> {
  return client().banUser_BanUser(userId, request);
}

export async function unbanUser(userId: string): Promise<void> {
  return client().banUser_UnbanUser(userId);
}

export async function getRoles(
  page?: number,
  pageSize?: number,
  searchTerm?: string | null,
  sortBy?: string | null,
  sortDescending?: boolean
): Promise<PaginatedRolesResponse> {
  return client().roleManagement_GetAllRoles(
    page,
    pageSize,
    searchTerm ?? undefined,
    sortBy ?? undefined,
    sortDescending ?? false
  );
}

export async function getAllRolesUnpaginated(): Promise<RoleDto[]> {
  return client().roleManagement_GetAllRolesUnpaginated();
}

export async function getRole(id: string): Promise<RoleDto> {
  return client().roleManagement_GetRole(id);
}

export async function createRole(request: CreateRoleRequest): Promise<RoleDto> {
  return client().roleManagement_CreateRole(request);
}

export async function updateRole(id: string, request: UpdateRoleRequest): Promise<RoleDto> {
  return client().roleManagement_UpdateRole(id, request);
}

export async function deleteRole(id: string): Promise<void> {
  return client().roleManagement_DeleteRole(id);
}

export async function assignPermissionsToRole(
  id: string,
  request: AssignPermissionsRequest
): Promise<void> {
  return client().roleManagement_AssignPermissionsToRole(id, request);
}

export async function removePermissionsFromRole(
  id: string,
  request: RemovePermissionsRequest
): Promise<void> {
  return client().roleManagement_RemovePermissionsFromRole(id, request);
}

export async function getAllPermissions(): Promise<PermissionDto[]> {
  return client().roleManagement_GetAllPermissions();
}

export async function getModelConfigurationSettings(): Promise<GetModelConfigurationSettingsResponse> {
  return client().getModelConfigurationSettings_GetModelConfigurationSettings();
}

export async function updateModelConfigurationSettings(
  command: UpdateModelConfigurationSettingsCommand
): Promise<UpdateModelConfigurationSettingsResponse> {
  return client().updateModelConfigurationSettings_UpdateModelConfigurationSettings(command);
}

export async function getSiteModelSettings(): Promise<GetSiteModelSettingsResponse> {
  return client().getSiteModelSettings_GetSiteModelSettings();
}

export async function updateSiteModelSettings(
  command: UpdateSiteModelSettingsCommand
): Promise<UpdateSiteModelSettingsResponse> {
  return client().updateSiteModelSettings_UpdateSiteModelSettings(command);
}

export async function getFileSettings(): Promise<GetFileSettingsResponse> {
  return client().getFileSettings_GetFileSettings();
}

export async function updateFileSettings(
  command: UpdateFileSettingsCommand
): Promise<UpdateFileSettingsResponse> {
  return client().updateFileSettings_UpdateFileSettings(command);
}

export async function getEmailSettings(): Promise<EmailSettingsResponse> {
  return client().getEmailSettings_GetEmailSettings();
}

export async function updateEmailSettings(
  command: UpdateEmailSettingsCommand
): Promise<UpdateEmailSettingsResponse> {
  return client().updateEmailSettings_UpdateEmailSettings(command);
}

export async function testEmailConfiguration(
  command: TestEmailConfigurationCommand
): Promise<TestEmailConfigurationResponse> {
  return client().testEmailConfiguration_TestEmailConfiguration(command);
}

export async function getTokenSettings(): Promise<TokenSettings> {
  return client().tokenSettings_GetTokenSettings();
}

export async function updateTokenSettings(settings: TokenSettings): Promise<void> {
  return client().tokenSettings_UpdateTokenSettings(settings);
}

export async function getSetupStatus(): Promise<CheckFirstTimeSetupResponse> {
  return client().systemSetup_GetSetupStatus();
}

export async function updateSiteSettings(
  command: UpdateSiteSettingsCommand
): Promise<UpdateSiteSettingsResponse> {
  return client().systemSetup_UpdateSiteSettings(command);
}

export async function deleteAllModels(
  request: DeleteAllModelsRequest
): Promise<DeleteAllModelsResponse> {
  return client().deleteAllModels_DeleteAllModels(request);
}

export async function getCategories(
  page?: number,
  pageSize?: number,
  searchTerm?: string | null
): Promise<GetCategoriesResponse> {
  return client().getCategories_GetCategories(
    page,
    pageSize,
    searchTerm ?? undefined
  );
}

export async function createCategory(
  command: CreateCategoryCommand
): Promise<CreateCategoryResponse> {
  return client().createCategory_CreateCategory(command);
}

export async function updateCategory(
  id: string,
  command: UpdateCategoryCommand
): Promise<UpdateCategoryResponse> {
  return client().updateCategory_UpdateCategory(id, command);
}

export async function deleteCategory(
  id: string
): Promise<DeleteCategoryResponse> {
  return client().deleteCategory_DeleteCategory(id);
}

export async function getAuthenticationSettings(): Promise<AuthenticationSettings> {
  return client().authenticationSettings_GetAuthenticationSettings();
}

export async function updateAuthenticationSettings(
  settings: AuthenticationSettings
): Promise<void> {
  return client().authenticationSettings_UpdateAuthenticationSettings(settings);
}

export async function getOAuthProviders(): Promise<
  import("@/lib/api/client").OAuthProviderInfo[]
> {
  return client().oAuthPlugin_GetOAuthProviders();
}

export async function getFederatedInstances(): Promise<
  import("@/lib/api/client").FederatedInstanceDto[]
> {
  return client().getFederatedInstances_GetFederatedInstances();
}

export async function getFederatedInstance(
  id: string
): Promise<import("@/lib/api/client").FederatedInstanceDetailDto> {
  return client().getFederatedInstance_GetFederatedInstance(id);
}

export async function createFederatedInstance(
  request: import("@/lib/api/client").CreateFederatedInstanceRequest
): Promise<import("@/lib/api/client").CreateFederatedInstanceResponse> {
  return client().createFederatedInstance_CreateFederatedInstance(request);
}

export async function updateFederatedInstance(
  id: string,
  request: import("@/lib/api/client").UpdateFederatedInstanceRequest
): Promise<import("@/lib/api/client").UpdateFederatedInstanceResponse> {
  return client().updateFederatedInstance_UpdateFederatedInstance(id, request);
}

export async function deleteFederatedInstance(id: string): Promise<void> {
  return client().deleteFederatedInstance_DeleteFederatedInstance(id);
}

export async function getFederationHealth(
  instanceId: string
): Promise<import("@/lib/api/client").FederationHealthResponse> {
  return client().getFederationHealth_GetFederationHealth(instanceId);
}

export async function initiateHandshake(
  request: import("@/lib/api/client").InitiateHandshakeRequest
): Promise<import("@/lib/api/client").InitiateHandshakeResponse> {
  return client().initiateHandshake_InitiateHandshake(request);
}

export async function getAvailableThemes(): Promise<
  import("@/lib/api/client").ThemeDefinition[]
> {
  return client().extensibleTheme_GetAvailableThemes();
}

export async function getActiveTheme(): Promise<
  import("@/lib/api/client").ThemeDefinition
> {
  return client().extensibleTheme_GetActiveTheme();
}

export async function setActiveTheme(themeId: string): Promise<void> {
  return client().extensibleTheme_SetActiveTheme(themeId);
}

export async function getThemeConfiguration(): Promise<
  Record<string, unknown>
> {
  return client().extensibleTheme_GetCurrentConfiguration();
}

export async function updateThemeConfiguration(
  config: Record<string, unknown>
): Promise<import("@/lib/api/client").ThemeApplicationResult> {
  return client().extensibleTheme_UpdateConfiguration(config);
}

export async function resetThemeToDefault(): Promise<
  import("@/lib/api/client").ThemeApplicationResult
> {
  return client().extensibleTheme_ResetToDefault();
}

export async function getFontAwesomeSettings(): Promise<
  import("@/lib/api/client").FontAwesomeSettings
> {
  return client().fontAwesomeSettings_GetSettings();
}

export async function updateFontAwesomeSettings(
  settings: import("@/lib/api/client").FontAwesomeSettings
): Promise<import("@/lib/api/client").FontAwesomeSettings> {
  return client().fontAwesomeSettings_UpdateSettings(settings);
}

export async function testFontAwesomeLicense(
  request: import("@/lib/api/client").TestLicenseRequest
): Promise<import("@/lib/api/client").TestLicenseResponse> {
  return client().fontAwesomeSettings_TestLicense(request);
}
