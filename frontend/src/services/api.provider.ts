import {
    LoginClient,
    // ... import other clients as needed
} from './api.client';

const baseUrl = import.meta.env.VITE_API_URL || "http://localhost:11666";

export const loginClient = new LoginClient(baseUrl);
// ... export other clients as needed 