/* eslint-disable @typescript-eslint/no-explicit-any */
import axios from "axios";
import api from '../../services/api';

const API_URL = "https://localhost:44378"; // Note: using HTTPS

interface CreateUserLoginRequest {
    email: string;
    password: string;
}

interface LoginResponse {
    token: string;
    user: {
        id: string;
        username: string;
        email: string;
        firstName?: string;
        lastName?: string;
    };
}

// Login API call
export const loginService = async (credentials: CreateUserLoginRequest): Promise<LoginResponse> => {
    try {
        const response = await api.post<LoginResponse>(
            '/Auth/login',
            {
                email: credentials.email,
                password: credentials.password,
                userAgent: navigator.userAgent,
            }
        );
        return response.data;
    } catch (error) {
        if (axios.isAxiosError(error)) {
            console.error("Login error:", error.response);
            if (error.response?.status === 401) {
                throw new Error("Invalid email or password");
            }
            throw new Error(error.response?.data?.message || "An error occurred during login");
        }
        throw new Error("An unexpected error occurred");
    }
};