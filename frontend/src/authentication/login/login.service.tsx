/* eslint-disable @typescript-eslint/no-explicit-any */
import axios from "axios";
import api from '../../services/api';
import { AppDispatch } from '../../store/store';
import { setCredentials } from '../../store/slices/auth-slice';
import { fetchUserDetails } from '../../store/slices/user-slice';

interface CreateUserLoginRequest {
    email: string;
    password: string;
}

interface LoginResponse {
    token: string;
}

// Login API call
export const loginService = async (credentials: CreateUserLoginRequest): Promise<LoginResponse> => {
    try {
        const response = await api.post<LoginResponse>(
            '/Authentication/login',
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

// Redux-integrated login handler
export const handleLogin = async (
    credentials: CreateUserLoginRequest,
    dispatch: AppDispatch
): Promise<void> => {
    const response = await loginService(credentials);

    // Update Redux store with credentials
    dispatch(setCredentials({
        token: response.token,
    }));
    
    // Fetch user details after successful login
    await dispatch(fetchUserDetails()).unwrap();
};