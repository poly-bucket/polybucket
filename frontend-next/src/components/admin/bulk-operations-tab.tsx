"use client";

import { SettingsSection } from "@/components/settings/settings-section";
import { BulkDeleteOperationRow } from "@/components/admin/admin-bulk-delete";
import {
  deleteAllCategories,
  deleteAllCollections,
  deleteAllFileSettings,
  deleteAllModels,
} from "@/lib/services/adminService";
import { DeleteAllModelsRequest } from "@/lib/api/client";
import { toast } from "sonner";

const confirmPhraseDescription = (target: string) =>
  `This will permanently delete ${target}. Type DELETE ALL to continue.`;

export function BulkOperationsTab() {
  const runDelete = async (
    action: (request: DeleteAllModelsRequest) => Promise<unknown>,
    successMessage: string,
    errorMessage: string,
    password: string
  ) => {
    try {
      await action(new DeleteAllModelsRequest({ adminPassword: password }));
      toast.success(successMessage);
    } catch {
      toast.error(errorMessage);
      throw new Error(errorMessage);
    }
  };

  return (
    <SettingsSection
      title="Bulk Operations"
      description="Dangerous operations that affect site-wide content for all users"
    >
      <BulkDeleteOperationRow
        title="Delete all models"
        description="Permanently delete every model on the site. This cannot be undone."
        buttonLabel="Delete All Models"
        dialogTitle="Delete All Models"
        dialogStepOneDescription={confirmPhraseDescription("all models")}
        onConfirm={(password) =>
          runDelete(
            deleteAllModels,
            "All models deleted",
            "Failed to delete models",
            password
          )
        }
      />
      <BulkDeleteOperationRow
        title="Delete all file types"
        description="Remove every file type setting. Upload rules will reset until types are configured again."
        buttonLabel="Delete All File Types"
        dialogTitle="Delete All File Types"
        dialogStepOneDescription={confirmPhraseDescription(
          "all file type settings"
        )}
        onConfirm={(password) =>
          runDelete(
            deleteAllFileSettings,
            "All file types deleted",
            "Failed to delete file types",
            password
          )
        }
      />
      <BulkDeleteOperationRow
        title="Delete all categories"
        description="Remove every category and detach them from all models. This cannot be undone."
        buttonLabel="Delete All Categories"
        dialogTitle="Delete All Categories"
        dialogStepOneDescription={confirmPhraseDescription("all categories")}
        onConfirm={(password) =>
          runDelete(
            deleteAllCategories,
            "All categories deleted",
            "Failed to delete categories",
            password
          )
        }
      />
      <BulkDeleteOperationRow
        title="Delete all collections"
        description="Permanently delete every user collection on the site. This cannot be undone."
        buttonLabel="Delete All Collections"
        dialogTitle="Delete All Collections"
        dialogStepOneDescription={confirmPhraseDescription("all collections")}
        onConfirm={(password) =>
          runDelete(
            deleteAllCollections,
            "All collections deleted",
            "Failed to delete collections",
            password
          )
        }
      />
    </SettingsSection>
  );
}
