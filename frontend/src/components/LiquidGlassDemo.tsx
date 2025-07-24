import React from 'react';
import { useTheme } from '../context/ThemeContext';

const LiquidGlassDemo: React.FC = () => {
  const { colors } = useTheme();

  return (
    <div className="lg-container p-8">
      <div className="max-w-6xl mx-auto space-y-8">
        {/* Header */}
        <div className="text-center mb-12">
          <h1 className="text-4xl font-bold text-white mb-4">Liquid Glass Design System</h1>
          <p className="text-xl text-gray-300">
            A modern, customizable design system with glassmorphism effects
          </p>
        </div>

        {/* Cards Section */}
        <div className="grid grid-cols-1 md:grid-cols-2 lg:grid-cols-3 gap-6">
          <div className="lg-card p-6">
            <h3 className="text-xl font-semibold text-white mb-3">Glass Card</h3>
            <p className="text-gray-300 mb-4">
              This card demonstrates the liquid glass effect with backdrop blur and transparency.
            </p>
            <button className="lg-button lg-button-primary">Primary Action</button>
          </div>

          <div className="lg-card p-6">
            <h3 className="text-xl font-semibold text-white mb-3">Interactive Elements</h3>
            <p className="text-gray-300 mb-4">
              Hover over this card to see the subtle animation and border effects.
            </p>
            <button className="lg-button lg-button-secondary">Secondary Action</button>
          </div>

          <div className="lg-card p-6">
            <h3 className="text-xl font-semibold text-white mb-3">Custom Colors</h3>
            <p className="text-gray-300 mb-4">
              All colors are customizable through the admin panel theme settings.
            </p>
            <button className="lg-button lg-button-accent">Accent Action</button>
          </div>
        </div>

        {/* Form Elements */}
        <div className="lg-card p-8">
          <h2 className="text-2xl font-semibold text-white mb-6">Form Elements</h2>
          <div className="grid grid-cols-1 md:grid-cols-2 gap-6">
            <div>
              <label className="block text-sm font-medium text-white mb-2">Email Address</label>
              <input
                type="email"
                placeholder="Enter your email"
                className="lg-input"
              />
            </div>
            <div>
              <label className="block text-sm font-medium text-white mb-2">Password</label>
              <input
                type="password"
                placeholder="Enter your password"
                className="lg-input"
              />
            </div>
          </div>
          <div className="mt-6">
            <button className="lg-button lg-button-primary mr-3">Submit</button>
            <button className="lg-button">Cancel</button>
          </div>
        </div>

        {/* Navigation Demo */}
        <div className="lg-card p-6">
          <h2 className="text-2xl font-semibold text-white mb-6">Navigation</h2>
          <nav className="lg-nav">
            <div className="flex space-x-4">
              <a href="#" className="lg-nav-item active">Home</a>
              <a href="#" className="lg-nav-item">Dashboard</a>
              <a href="#" className="lg-nav-item">Models</a>
              <a href="#" className="lg-nav-item">Collections</a>
              <a href="#" className="lg-nav-item">Settings</a>
            </div>
          </nav>
        </div>

        {/* Badges and Status */}
        <div className="lg-card p-6">
          <h2 className="text-2xl font-semibold text-white mb-6">Status Indicators</h2>
          <div className="flex flex-wrap gap-4">
            <span className="lg-badge lg-badge-success">Success</span>
            <span className="lg-badge lg-badge-warning">Warning</span>
            <span className="lg-badge lg-badge-error">Error</span>
            <span className="lg-badge lg-badge-info">Info</span>
          </div>
        </div>

        {/* Table Demo */}
        <div className="lg-card p-6">
          <h2 className="text-2xl font-semibold text-white mb-6">Data Table</h2>
          <table className="lg-table">
            <thead>
              <tr>
                <th>Name</th>
                <th>Status</th>
                <th>Role</th>
                <th>Actions</th>
              </tr>
            </thead>
            <tbody>
              <tr>
                <td>John Doe</td>
                <td><span className="lg-badge lg-badge-success">Active</span></td>
                <td>Admin</td>
                <td>
                  <button className="lg-button text-sm">Edit</button>
                </td>
              </tr>
              <tr>
                <td>Jane Smith</td>
                <td><span className="lg-badge lg-badge-warning">Pending</span></td>
                <td>User</td>
                <td>
                  <button className="lg-button text-sm">Edit</button>
                </td>
              </tr>
              <tr>
                <td>Bob Johnson</td>
                <td><span className="lg-badge lg-badge-error">Inactive</span></td>
                <td>Moderator</td>
                <td>
                  <button className="lg-button text-sm">Edit</button>
                </td>
              </tr>
            </tbody>
          </table>
        </div>

        {/* Color Palette Display */}
        <div className="lg-card p-6">
          <h2 className="text-2xl font-semibold text-white mb-6">Current Color Palette</h2>
          <div className="grid grid-cols-2 md:grid-cols-4 gap-4">
            <div className="text-center">
              <div 
                className="w-16 h-16 rounded-lg mx-auto mb-2 border border-gray-600"
                style={{ backgroundColor: colors.primary }}
              />
              <p className="text-sm text-white">Primary</p>
              <p className="text-xs text-gray-400">{colors.primary}</p>
            </div>
            <div className="text-center">
              <div 
                className="w-16 h-16 rounded-lg mx-auto mb-2 border border-gray-600"
                style={{ backgroundColor: colors.secondary }}
              />
              <p className="text-sm text-white">Secondary</p>
              <p className="text-xs text-gray-400">{colors.secondary}</p>
            </div>
            <div className="text-center">
              <div 
                className="w-16 h-16 rounded-lg mx-auto mb-2 border border-gray-600"
                style={{ backgroundColor: colors.accent }}
              />
              <p className="text-sm text-white">Accent</p>
              <p className="text-xs text-gray-400">{colors.accent}</p>
            </div>
            <div className="text-center">
              <div 
                className="w-16 h-16 rounded-lg mx-auto mb-2 border border-gray-600"
                style={{ backgroundColor: colors.bgPrimary }}
              />
              <p className="text-sm text-white">Background</p>
              <p className="text-xs text-gray-400">{colors.bgPrimary}</p>
            </div>
          </div>
        </div>

        {/* Loading States */}
        <div className="lg-card p-6">
          <h2 className="text-2xl font-semibold text-white mb-6">Loading States</h2>
          <div className="flex items-center space-x-4">
            <div className="lg-spinner"></div>
            <span className="text-white">Loading content...</span>
          </div>
        </div>

        {/* Footer */}
        <div className="text-center text-gray-400 text-sm">
          <p>Liquid Glass Design System - PolyBucket</p>
          <p className="mt-2">
            Customize colors in the admin panel under Settings → Theme
          </p>
        </div>
      </div>
    </div>
  );
};

export default LiquidGlassDemo; 