import React from 'react';
import type { ModelConfigurationSettings } from '../../../../services/modelConfigurationSettingsService';

interface ModerationSettingsProps {
  settings: ModelConfigurationSettings;
  setSettings: (updates: Partial<ModelConfigurationSettings>) => void;
}

const ModerationSettings: React.FC<ModerationSettingsProps> = ({ settings, setSettings }) => {
  return (
    <div className="lg-card p-4">
      <h4 className="text-md font-medium text-white mb-4">Moderation Settings</h4>
      <div className="grid grid-cols-1 md:grid-cols-2 gap-4">
        
        {/* Require Moderator Approval */}
        <div className="flex items-center justify-between">
          <label className="text-sm text-white/80">Require Moderator Approval</label>
          <input
            type="checkbox"
            checked={settings.requireModeratorApproval}
            onChange={(e) => setSettings({ requireModeratorApproval: e.target.checked })}
            className="lg-checkbox"
          />
        </div>

        {/* Enable Auto Moderation */}
        <div className="flex items-center justify-between">
          <label className="text-sm text-white/80">Enable Auto Moderation</label>
          <input
            type="checkbox"
            checked={settings.enableAutoModeration}
            onChange={(e) => setSettings({ enableAutoModeration: e.target.checked })}
            className="lg-checkbox"
          />
        </div>

        {/* Require Content Rating */}
        <div className="flex items-center justify-between">
          <label className="text-sm text-white/80">Require Content Rating</label>
          <input
            type="checkbox"
            checked={settings.requireContentRating}
            onChange={(e) => setSettings({ requireContentRating: e.target.checked })}
            className="lg-checkbox"
          />
        </div>

        {/* Enable Model Flagging */}
        <div className="flex items-center justify-between">
          <label className="text-sm text-white/80">Enable Model Flagging</label>
          <input
            type="checkbox"
            checked={settings.enableModelFlagging}
            onChange={(e) => setSettings({ enableModelFlagging: e.target.checked })}
            className="lg-checkbox"
          />
        </div>

        {/* Require Flag Reason */}
        <div className="flex items-center justify-between">
          <label className="text-sm text-white/80">Require Flag Reason</label>
          <input
            type="checkbox"
            checked={settings.requireFlagReason}
            onChange={(e) => setSettings({ requireFlagReason: e.target.checked })}
            className="lg-checkbox"
          />
        </div>

        {/* Enable Model Reporting */}
        <div className="flex items-center justify-between">
          <label className="text-sm text-white/80">Enable Model Reporting</label>
          <input
            type="checkbox"
            checked={settings.enableModelReporting}
            onChange={(e) => setSettings({ enableModelReporting: e.target.checked })}
            className="lg-checkbox"
          />
        </div>

        {/* Require Report Details */}
        <div className="flex items-center justify-between">
          <label className="text-sm text-white/80">Require Report Details</label>
          <input
            type="checkbox"
            checked={settings.requireReportDetails}
            onChange={(e) => setSettings({ requireReportDetails: e.target.checked })}
            className="lg-checkbox"
          />
        </div>

        {/* Enable Model Blocking */}
        <div className="flex items-center justify-between">
          <label className="text-sm text-white/80">Enable Model Blocking</label>
          <input
            type="checkbox"
            checked={settings.enableModelBlocking}
            onChange={(e) => setSettings({ enableModelBlocking: e.target.checked })}
            className="lg-checkbox"
          />
        </div>

        {/* Require Block Reason */}
        <div className="flex items-center justify-between">
          <label className="text-sm text-white/80">Require Block Reason</label>
          <input
            type="checkbox"
            checked={settings.requireBlockReason}
            onChange={(e) => setSettings({ requireBlockReason: e.target.checked })}
            className="lg-checkbox"
          />
        </div>

        {/* Enable Model Whitelisting */}
        <div className="flex items-center justify-between">
          <label className="text-sm text-white/80">Enable Model Whitelisting</label>
          <input
            type="checkbox"
            checked={settings.enableModelWhitelisting}
            onChange={(e) => setSettings({ enableModelWhitelisting: e.target.checked })}
            className="lg-checkbox"
          />
        </div>

        {/* Enable Model Blacklisting */}
        <div className="flex items-center justify-between">
          <label className="text-sm text-white/80">Enable Model Blacklisting</label>
          <input
            type="checkbox"
            checked={settings.enableModelBlacklisting}
            onChange={(e) => setSettings({ enableModelBlacklisting: e.target.checked })}
            className="lg-checkbox"
          />
        </div>

        {/* Require Model Approval */}
        <div className="flex items-center justify-between">
          <label className="text-sm text-white/80">Require Model Approval</label>
          <input
            type="checkbox"
            checked={settings.requireModelApproval}
            onChange={(e) => setSettings({ requireModelApproval: e.target.checked })}
            className="lg-checkbox"
          />
        </div>

        {/* Enable Model Rejection */}
        <div className="flex items-center justify-between">
          <label className="text-sm text-white/80">Enable Model Rejection</label>
          <input
            type="checkbox"
            checked={settings.enableModelRejection}
            onChange={(e) => setSettings({ enableModelRejection: e.target.checked })}
            className="lg-checkbox"
          />
        </div>

        {/* Require Rejection Reason */}
        <div className="flex items-center justify-between">
          <label className="text-sm text-white/80">Require Rejection Reason</label>
          <input
            type="checkbox"
            checked={settings.requireRejectionReason}
            onChange={(e) => setSettings({ requireRejectionReason: e.target.checked })}
            className="lg-checkbox"
          />
        </div>

        {/* Enable Model Appeals */}
        <div className="flex items-center justify-between">
          <label className="text-sm text-white/80">Enable Model Appeals</label>
          <input
            type="checkbox"
            checked={settings.enableModelAppeals}
            onChange={(e) => setSettings({ enableModelAppeals: e.target.checked })}
            className="lg-checkbox"
          />
        </div>

        {/* Require Appeal Details */}
        <div className="flex items-center justify-between">
          <label className="text-sm text-white/80">Require Appeal Details</label>
          <input
            type="checkbox"
            checked={settings.requireAppealDetails}
            onChange={(e) => setSettings({ requireAppealDetails: e.target.checked })}
            className="lg-checkbox"
          />
        </div>

        {/* Enable Model Locking */}
        <div className="flex items-center justify-between">
          <label className="text-sm text-white/80">Enable Model Locking</label>
          <input
            type="checkbox"
            checked={settings.enableModelLocking}
            onChange={(e) => setSettings({ enableModelLocking: e.target.checked })}
            className="lg-checkbox"
          />
        </div>

        {/* Require Lock Reason */}
        <div className="flex items-center justify-between">
          <label className="text-sm text-white/80">Require Lock Reason</label>
          <input
            type="checkbox"
            checked={settings.requireLockReason}
            onChange={(e) => setSettings({ requireLockReason: e.target.checked })}
            className="lg-checkbox"
          />
        </div>

        {/* Enable Model Unlocking */}
        <div className="flex items-center justify-between">
          <label className="text-sm text-white/80">Enable Model Unlocking</label>
          <input
            type="checkbox"
            checked={settings.enableModelUnlocking}
            onChange={(e) => setSettings({ enableModelUnlocking: e.target.checked })}
            className="lg-checkbox"
          />
        </div>

        {/* Require Unlock Approval */}
        <div className="flex items-center justify-between">
          <label className="text-sm text-white/80">Require Unlock Approval</label>
          <input
            type="checkbox"
            checked={settings.requireUnlockApproval}
            onChange={(e) => setSettings({ requireUnlockApproval: e.target.checked })}
            className="lg-checkbox"
          />
        </div>

        {/* Enable Model Deletion */}
        <div className="flex items-center justify-between">
          <label className="text-sm text-white/80">Enable Model Deletion</label>
          <input
            type="checkbox"
            checked={settings.enableModelDeletion}
            onChange={(e) => setSettings({ enableModelDeletion: e.target.checked })}
            className="lg-checkbox"
          />
        </div>

        {/* Require Deletion Approval */}
        <div className="flex items-center justify-between">
          <label className="text-sm text-white/80">Require Deletion Approval</label>
          <input
            type="checkbox"
            checked={settings.requireDeletionApproval}
            onChange={(e) => setSettings({ requireDeletionApproval: e.target.checked })}
            className="lg-checkbox"
          />
        </div>

        {/* Require Deletion Reason */}
        <div className="flex items-center justify-between">
          <label className="text-sm text-white/80">Require Deletion Reason</label>
          <input
            type="checkbox"
            checked={settings.requireDeletionReason}
            onChange={(e) => setSettings({ requireDeletionReason: e.target.checked })}
            className="lg-checkbox"
          />
        </div>

        {/* Enable Model Restoration */}
        <div className="flex items-center justify-between">
          <label className="text-sm text-white/80">Enable Model Restoration</label>
          <input
            type="checkbox"
            checked={settings.enableModelRestoration}
            onChange={(e) => setSettings({ enableModelRestoration: e.target.checked })}
            className="lg-checkbox"
          />
        </div>

        {/* Require Restoration Approval */}
        <div className="flex items-center justify-between">
          <label className="text-sm text-white/80">Require Restoration Approval</label>
          <input
            type="checkbox"
            checked={settings.requireRestorationApproval}
            onChange={(e) => setSettings({ requireRestorationApproval: e.target.checked })}
            className="lg-checkbox"
          />
        </div>

      </div>
    </div>
  );
};

export default ModerationSettings;
