import React, { useEffect, useState } from 'react';
import { settingsService, UserSettings, UpdateUserSettingsRequest } from '../services/settings.service';
import { useNavigate } from 'react-router-dom';
import { routes } from '../navigation/routes';
import { AxiosError } from 'axios';

const LANGUAGE_OPTIONS = [
    { value: 'en', label: 'English' },
    { value: 'es', label: 'Español' },
    { value: 'fr', label: 'Français' },
    { value: 'de', label: 'Deutsch' }
];

const MEASUREMENT_SYSTEMS = [
    { value: 'metric', label: 'Metric' },
    { value: 'imperial', label: 'Imperial' }
];

interface ApiErrorResponse {
    message: string;
}

export const Settings: React.FC = () => {
    const [settings, setSettings] = useState<UserSettings | null>(null);
    const [isLoading, setIsLoading] = useState(true);
    const [isSaving, setIsSaving] = useState(false);
    const [error, setError] = useState<string | null>(null);
    const [successMessage, setSuccessMessage] = useState<string | null>(null);
    const navigate = useNavigate();

    useEffect(() => {
        loadSettings();
    }, []);

    const loadSettings = async () => {
        try {
            setIsLoading(true);
            setError(null);
            const userSettings = await settingsService.getUserSettings();
            setSettings(userSettings);
        } catch (err) {
            const error = err as AxiosError<ApiErrorResponse>;
            const errorMessage = error.response?.data?.message || 'Failed to load settings';
            setError(errorMessage);
            console.error('Error loading settings:', err);
        } finally {
            setIsLoading(false);
        }
    };

    const handleSubmit = async (e: React.FormEvent) => {
        e.preventDefault();
        if (!settings) return;

        try {
            setIsSaving(true);
            setError(null);
            setSuccessMessage(null);
            
            const updateRequest: UpdateUserSettingsRequest = {
                language: settings.language,
                theme: settings.theme,
                emailNotifications: settings.emailNotifications,
                defaultPrinterId: settings.defaultPrinterId,
                measurementSystem: settings.measurementSystem,
                timeZone: settings.timeZone,
                customSettings: settings.customSettings
            };
            await settingsService.updateUserSettings(updateRequest);
            setSuccessMessage('Settings saved successfully');
            
            // Clear success message after 3 seconds
            setTimeout(() => {
                setSuccessMessage(null);
            }, 3000);
        } catch (err) {
            const error = err as AxiosError<ApiErrorResponse>;
            const errorMessage = error.response?.data?.message || 'Failed to save settings';
            setError(errorMessage);
            console.error('Error saving settings:', err);
        } finally {
            setIsSaving(false);
        }
    };

    const handleChange = (field: keyof UserSettings, value: string | boolean) => {
        if (settings) {
            setSettings({ ...settings, [field]: value });
        }
    };

    if (isLoading) {
        return (
            <div className="flex items-center justify-center min-h-screen">
                <div className="text-green-400">Loading settings...</div>
            </div>
        );
    }

    if (!settings) {
        return (
            <div className="flex items-center justify-center min-h-screen">
                <div className="text-red-500">Failed to load settings</div>
            </div>
        );
    }

    return (
        <div className="container mx-auto px-4 py-8">
            <h1 className="text-3xl font-bold text-green-400 mb-8">User Settings</h1>
            
            {error && (
                <div className="bg-red-100 border border-red-400 text-red-700 px-4 py-3 rounded mb-4">
                    {error}
                </div>
            )}

            {successMessage && (
                <div className="bg-green-100 border border-green-400 text-green-700 px-4 py-3 rounded mb-4">
                    {successMessage}
                </div>
            )}

            <form onSubmit={handleSubmit} className="space-y-6 max-w-2xl">
                {/* Language */}
                <div>
                    <label htmlFor="language" className="block text-green-400 mb-2">Language</label>
                    <select
                        id="language"
                        value={settings.language}
                        onChange={(e) => handleChange('language', e.target.value)}
                        className="w-full p-2 bg-gray-800 border border-green-500 rounded text-green-400"
                        aria-label="Select language"
                    >
                        {LANGUAGE_OPTIONS.map(option => (
                            <option key={option.value} value={option.value}>
                                {option.label}
                            </option>
                        ))}
                    </select>
                </div>

                {/* Theme */}
                <div>
                    <label className="block text-green-400 mb-2">Theme</label>
                    <div className="flex space-x-4">
                        <label className="inline-flex items-center">
                            <input
                                type="radio"
                                value="light"
                                checked={settings.theme === 'light'}
                                onChange={(e) => handleChange('theme', e.target.value)}
                                className="form-radio text-green-500"
                            />
                            <span className="ml-2 text-green-400">Light</span>
                        </label>
                        <label className="inline-flex items-center">
                            <input
                                type="radio"
                                value="dark"
                                checked={settings.theme === 'dark'}
                                onChange={(e) => handleChange('theme', e.target.value)}
                                className="form-radio text-green-500"
                            />
                            <span className="ml-2 text-green-400">Dark</span>
                        </label>
                    </div>
                </div>

                {/* Email Notifications */}
                <div>
                    <label className="inline-flex items-center">
                        <input
                            type="checkbox"
                            checked={settings.emailNotifications}
                            onChange={(e) => handleChange('emailNotifications', e.target.checked)}
                            className="form-checkbox text-green-500"
                        />
                        <span className="ml-2 text-green-400">Enable Email Notifications</span>
                    </label>
                </div>

                {/* Measurement System */}
                <div>
                    <label htmlFor="measurementSystem" className="block text-green-400 mb-2">Measurement System</label>
                    <select
                        id="measurementSystem"
                        value={settings.measurementSystem}
                        onChange={(e) => handleChange('measurementSystem', e.target.value)}
                        className="w-full p-2 bg-gray-800 border border-green-500 rounded text-green-400"
                        aria-label="Select measurement system"
                    >
                        {MEASUREMENT_SYSTEMS.map(option => (
                            <option key={option.value} value={option.value}>
                                {option.label}
                            </option>
                        ))}
                    </select>
                </div>

                {/* Time Zone */}
                <div>
                    <label htmlFor="timeZone" className="block text-green-400 mb-2">Time Zone</label>
                    <input
                        id="timeZone"
                        type="text"
                        value={settings.timeZone}
                        onChange={(e) => handleChange('timeZone', e.target.value)}
                        className="w-full p-2 bg-gray-800 border border-green-500 rounded text-green-400"
                        aria-label="Enter time zone"
                        placeholder="e.g., UTC, UTC+1, America/New_York"
                    />
                </div>

                <div className="flex justify-end space-x-4 pt-4">
                    <button
                        type="button"
                        onClick={() => navigate(routes.profile)}
                        className="px-4 py-2 text-green-400 border border-green-400 rounded hover:bg-green-400 hover:text-black transition-colors"
                    >
                        Cancel
                    </button>
                    <button
                        type="submit"
                        disabled={isSaving}
                        className="px-4 py-2 bg-green-400 text-black rounded hover:bg-green-500 transition-colors disabled:opacity-50"
                    >
                        {isSaving ? 'Saving...' : 'Save Settings'}
                    </button>
                </div>
            </form>
        </div>
    );
}; 