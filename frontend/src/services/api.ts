import axios from 'axios';

const api = axios.create({
  baseURL: 'https://localhost:44378',
  withCredentials: true,
});

// Add a request interceptor
api.interceptors.request.use((config) => {
  // Get token from localStorage
  const auth = localStorage.getItem('auth');
  if (auth) {
    const { token } = JSON.parse(auth);
    if (token) {
      config.headers.Authorization = `Bearer ${token}`;
    }
  }
  return config;
});

export default api; 