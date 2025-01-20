/* eslint-disable @typescript-eslint/no-explicit-any */
import axios from "axios";

const API_URL = "https://localhost:44378"; // Replace with your backend's URL and port

// Login API call
export const loginService = async (credentials: any) => {
  try {
    const response = await axios.post(`${API_URL}/login`, credentials, {
      headers: { "Content-Type": "application/json" },
    });
    return response.data;
  } catch (error) {
    console.error("Error during login:", error);
    if (axios.isAxiosError(error)) {
      throw error.response?.data || error.message;
    } else {
      throw error;
    }
  }
};