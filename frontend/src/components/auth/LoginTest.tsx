import React, { useState } from 'react';
import { useAppDispatch, useAppSelector } from '../../store';
import { loginUser, logoutUser } from '../../store/thunks/authThunks';
import { clearUser } from '../../store/slices/authSlice';
import { LoginRequest } from '../../store/thunks/authThunks';

const LoginTest: React.FC = () => {
  const dispatch = useAppDispatch();
  const { user, isLoading, isError, errorMessage } = useAppSelector(state => state.auth);
  
  const [email, setEmail] = useState('');
  const [password, setPassword] = useState('');
  const [success, setSuccess] = useState('');

  const handleLogin = async (e: React.FormEvent) => {
    e.preventDefault();
    setSuccess('');

    try {
      const credentials: LoginRequest = { email, password };
      const result = await dispatch(loginUser(credentials));
      
      if (loginUser.fulfilled.match(result)) {
        const userData = result.payload;
        setSuccess(`Login successful! User: ${userData.username}, Roles: ${userData.roles.join(', ')}`);
        console.log('Login result:', userData);
      } else {
        setSuccess('Login failed: ' + (result.payload as string));
      }
    } catch (err: any) {
      setSuccess('Login error: ' + (err.message || 'Login failed'));
      console.error('Login error:', err);
    }
  };

  const handleLogout = async () => {
    try {
      await dispatch(logoutUser());
      dispatch(clearUser());
      setSuccess('Logged out successfully');
    } catch (error) {
      console.error('Logout error:', error);
      setSuccess('Logout failed');
    }
  };

  const checkAuth = () => {
    const isAuthenticated = !!user && !!user.accessToken;
    setSuccess(`Auth check: User exists: ${!!user}, Is authenticated: ${isAuthenticated}`);
  };

  return (
    <div style={{ padding: '20px', maxWidth: '400px', margin: '0 auto' }}>
      <h2>Login Test (Redux Store)</h2>
      
      <form onSubmit={handleLogin}>
        <div style={{ marginBottom: '15px' }}>
          <label>
            Email:
            <input
              type="email"
              value={email}
              onChange={(e) => setEmail(e.target.value)}
              required
              style={{ width: '100%', padding: '8px', marginTop: '5px' }}
            />
          </label>
        </div>
        
        <div style={{ marginBottom: '15px' }}>
          <label>
            Password:
            <input
              type="password"
              value={password}
              onChange={(e) => setPassword(e.target.value)}
              required
              style={{ width: '100%', padding: '8px', marginTop: '5px' }}
            />
          </label>
        </div>
        
        <button 
          type="submit" 
          disabled={isLoading}
          style={{ 
            width: '100%', 
            padding: '10px', 
            backgroundColor: '#007bff', 
            color: 'white', 
            border: 'none', 
            borderRadius: '4px',
            cursor: isLoading ? 'not-allowed' : 'pointer'
          }}
        >
          {isLoading ? 'Logging in...' : 'Login'}
        </button>
      </form>

      <div style={{ marginTop: '20px' }}>
        <button 
          onClick={handleLogout}
          style={{ 
            padding: '8px 16px', 
            backgroundColor: '#dc3545', 
            color: 'white', 
            border: 'none', 
            borderRadius: '4px',
            marginRight: '10px',
            cursor: 'pointer'
          }}
        >
          Logout
        </button>
        
        <button 
          onClick={checkAuth}
          style={{ 
            padding: '8px 16px', 
            backgroundColor: '#28a745', 
            color: 'white', 
            border: 'none', 
            borderRadius: '4px',
            cursor: 'pointer'
          }}
        >
          Check Auth
        </button>
      </div>

      {isError && (
        <div style={{ 
          marginTop: '15px', 
          padding: '10px', 
          backgroundColor: '#f8d7da', 
          color: '#721c24', 
          border: '1px solid #f5c6cb', 
          borderRadius: '4px' 
        }}>
          {errorMessage}
        </div>
      )}

      {success && (
        <div style={{ 
          marginTop: '15px', 
          padding: '10px', 
          backgroundColor: '#d4edda', 
          color: '#155724', 
          border: '1px solid #c3e6cb', 
          borderRadius: '4px' 
        }}>
          {success}
        </div>
      )}

      {user && (
        <div style={{ 
          marginTop: '15px', 
          padding: '10px', 
          backgroundColor: '#e2e3e5', 
          color: '#383d41', 
          border: '1px solid #d6d8db', 
          borderRadius: '4px' 
        }}>
          <strong>Current User:</strong><br />
          ID: {user.id}<br />
          Username: {user.username}<br />
          Email: {user.email}<br />
          Roles: {user.roles.join(', ')}<br />
          Token: {user.accessToken ? `${user.accessToken.substring(0, 20)}...` : 'None'}
        </div>
      )}
    </div>
  );
};

export default LoginTest; 