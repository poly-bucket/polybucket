import api from './api';

export interface UserSettings {
    id: string;
    userId: string;
    language: string;
    theme: 'dark' | 'light';
    emailNotifications: boolean;
    defaultPrinterId?: string;
    measurementSystem: 'metric' | 'imperial';
    timeZone: string;
    customSettings: Record<string, string>;
}

export interface GetUserSettingsResponse {
    settings: UserSettings;
}

export interface UpdateUserSettingsRequest {
    language?: string;
    theme?: 'dark' | 'light';
    emailNotifications?: boolean;
    defaultPrinterId?: string;
    measurementSystem?: 'metric' | 'imperial';
    timeZone?: string;
    customSettings?: Record<string, string>;
}

const BASE_URL = 'api/users/settings';

export const settingsService = {
    async getUserSettings(): Promise<UserSettings> {
        const response = await api.get<GetUserSettingsResponse>(BASE_URL);
        return response.data.settings;
    },

    async updateUserSettings(settings: UpdateUserSettingsRequest): Promise<void> {
        await api.put(BASE_URL, settings);
    }
}; 