import React from 'react';

const AuthTab: React.FC = () => (
  <div className="space-y-6">
    <h2 className="text-2xl font-bold text-white">Authentication Settings</h2>
    
    <div className="lg-card p-6">
      <h3 className="text-lg font-medium text-white mb-4">OAuth Providers</h3>
      
      <div className="space-y-4">
        <div className="flex items-center justify-between p-4 bg-white/5 rounded-lg">
          <div className="flex items-center space-x-3">
            <div className="w-8 h-8 bg-blue-500 rounded flex items-center justify-center">
              <span className="text-white font-bold">G</span>
            </div>
            <div>
              <div className="text-white font-medium">Google OAuth</div>
              <div className="text-sm text-white/60">Sign in with Google</div>
            </div>
          </div>
          <input
            type="checkbox"
            className="h-4 w-4 text-indigo-600 focus:ring-indigo-500 border-white/20 rounded bg-transparent"
          />
        </div>
        
        <div className="flex items-center justify-between p-4 bg-white/5 rounded-lg">
          <div className="flex items-center space-x-3">
            <div className="w-8 h-8 bg-black rounded flex items-center justify-center">
              <span className="text-white font-bold">G</span>
            </div>
            <div>
              <div className="text-white font-medium">GitHub OAuth</div>
              <div className="text-sm text-white/60">Sign in with GitHub</div>
            </div>
          </div>
          <input
            type="checkbox"
            className="h-4 w-4 text-indigo-600 focus:ring-indigo-500 border-white/20 rounded bg-transparent"
          />
        </div>
      </div>
    </div>
    
    <div className="lg-card p-6">
      <h3 className="text-lg font-medium text-white mb-4">Password Policy</h3>
      
      <div className="space-y-4">
        <div>
          <label className="block text-sm font-medium text-white/80 mb-2">Minimum Password Length</label>
          <input
            type="number"
            className="lg-input w-32"
            defaultValue={8}
          />
        </div>
        
        <div className="flex items-center">
          <input
            type="checkbox"
            id="requireUppercase"
            className="h-4 w-4 text-indigo-600 focus:ring-indigo-500 border-white/20 rounded bg-transparent"
          />
          <label htmlFor="requireUppercase" className="ml-2 text-sm text-white/80">
            Require uppercase letters
          </label>
        </div>
        
        <div className="flex items-center">
          <input
            type="checkbox"
            id="requireNumbers"
            className="h-4 w-4 text-indigo-600 focus:ring-indigo-500 border-white/20 rounded bg-transparent"
          />
          <label htmlFor="requireNumbers" className="ml-2 text-sm text-white/80">
            Require numbers
          </label>
        </div>
        
        <div className="flex items-center">
          <input
            type="checkbox"
            id="requireSpecialChars"
            className="h-4 w-4 text-indigo-600 focus:ring-indigo-500 border-white/20 rounded bg-transparent"
          />
          <label htmlFor="requireSpecialChars" className="ml-2 text-sm text-white/80">
            Require special characters
          </label>
        </div>
      </div>
    </div>
  </div>
);

export default AuthTab;
