import { 
  GetModelConfigurationSettingsClient, 
  UpdateModelConfigurationSettingsClient, 
  UpdateModelConfigurationSettingsCommand,
  GetModelConfigurationSettingsResponse,
  UpdateModelConfigurationSettingsResponse,
  ModelConfigurationSettingsData
} from './api.client';

export type ModelConfigurationSettings = ModelConfigurationSettingsData;

class ModelConfigurationSettingsService {
  private getModelConfigurationSettingsClient(): GetModelConfigurationSettingsClient {
    return new GetModelConfigurationSettingsClient();
  }

  private getUpdateModelConfigurationSettingsClient(): UpdateModelConfigurationSettingsClient {
    return new UpdateModelConfigurationSettingsClient();
  }

  async getSettings(): Promise<GetModelConfigurationSettingsResponse> {
    try {
      const client = this.getModelConfigurationSettingsClient();
      return await client.getModelConfigurationSettings();
    } catch (error) {
      console.error('Error fetching model configuration settings:', error);
      throw error;
    }
  }

  async updateSettings(settings: ModelConfigurationSettings): Promise<UpdateModelConfigurationSettingsResponse> {
    try {
      const client = this.getUpdateModelConfigurationSettingsClient();
      const command = new UpdateModelConfigurationSettingsCommand();
      
      // Map the settings to the command
      command.allowAnonUploads = settings.allowAnonUploads;
      command.requireUploadModeration = settings.requireUploadModeration;
      command.defaultPrivacySetting = settings.defaultPrivacySetting;
      command.allowAnonDownloads = settings.allowAnonDownloads;
      command.enableModelVersioning = settings.enableModelVersioning;
      command.limitTotalModels = settings.limitTotalModels;
      command.allowNSFWContent = settings.allowNSFWContent;
      command.allowAIGeneratedContent = settings.allowAIGeneratedContent;
      command.requireModelDescription = settings.requireModelDescription;
      command.requireModelTags = settings.requireModelTags;
      command.minDescriptionLength = settings.minDescriptionLength;
      command.maxDescriptionLength = settings.maxDescriptionLength;
      command.maxTagsPerModel = settings.maxTagsPerModel;
      command.autoApproveVerifiedUsers = settings.autoApproveVerifiedUsers;
      command.requireThumbnail = settings.requireThumbnail;
      command.allowModelRemixing = settings.allowModelRemixing;
      command.requireRemixAttribution = settings.requireRemixAttribution;
      command.maxModelsPerUser = settings.maxModelsPerUser;
      command.enableModelComments = settings.enableModelComments;
      command.enableModelLikes = settings.enableModelLikes;
      command.enableModelDownloads = settings.enableModelDownloads;
      command.requireLicenseSelection = settings.requireLicenseSelection;
      command.allowCustomLicenses = settings.allowCustomLicenses;
      command.enableModelCollections = settings.enableModelCollections;
      command.requireCategorySelection = settings.requireCategorySelection;
      command.maxCategoriesPerModel = settings.maxCategoriesPerModel;
      command.enableModelSharing = settings.enableModelSharing;
      command.enableModelEmbedding = settings.enableModelEmbedding;
      command.requireModelPreview = settings.requireModelPreview;
      command.autoGenerateModelPreviews = settings.autoGenerateModelPreviews;
      command.enableModelAnalytics = settings.enableModelAnalytics;
      command.requireUserAgreement = settings.requireUserAgreement;
      command.userAgreementText = settings.userAgreementText;
      command.enableModelExport = settings.enableModelExport;
      command.enableModelImport = settings.enableModelImport;
      command.requireModelValidation = settings.requireModelValidation;
      command.enableModelBackup = settings.enableModelBackup;
      command.modelBackupRetentionDays = settings.modelBackupRetentionDays;
      command.enableModelArchiving = settings.enableModelArchiving;
      command.modelArchiveThresholdDays = settings.modelArchiveThresholdDays;
      command.requireModeratorApproval = settings.requireModeratorApproval;
      command.enableAutoModeration = settings.enableAutoModeration;
      command.requireContentRating = settings.requireContentRating;
      command.enableModelFlagging = settings.enableModelFlagging;
      command.requireFlagReason = settings.requireFlagReason;
      command.enableModelReporting = settings.enableModelReporting;
      command.requireReportDetails = settings.requireReportDetails;
      command.enableModelBlocking = settings.enableModelBlocking;
      command.requireBlockReason = settings.requireBlockReason;
      command.enableModelWhitelisting = settings.enableModelWhitelisting;
      command.enableModelBlacklisting = settings.enableModelBlacklisting;
      command.requireModelApproval = settings.requireModelApproval;
      command.enableModelRejection = settings.enableModelRejection;
      command.requireRejectionReason = settings.requireRejectionReason;
      command.enableModelAppeals = settings.enableModelAppeals;
      command.requireAppealDetails = settings.requireAppealDetails;
      command.enableModelLocking = settings.enableModelLocking;
      command.requireLockReason = settings.requireLockReason;
      command.enableModelUnlocking = settings.enableModelUnlocking;
      command.requireUnlockApproval = settings.requireUnlockApproval;
      command.enableModelDeletion = settings.enableModelDeletion;
      command.requireDeletionApproval = settings.requireDeletionApproval;
      command.requireDeletionReason = settings.requireDeletionReason;
      command.enableModelRestoration = settings.enableModelRestoration;
      command.requireRestorationApproval = settings.requireRestorationApproval;

      return await client.updateModelConfigurationSettings(command);
    } catch (error) {
      console.error('Error updating model configuration settings:', error);
      throw error;
    }
  }
}

const modelConfigurationSettingsService = new ModelConfigurationSettingsService();
export default modelConfigurationSettingsService;
