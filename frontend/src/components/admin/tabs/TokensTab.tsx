import React from 'react';
import TokenSettings from '../TokenSettings';

const TokensTab: React.FC = () => {
  return (
    <div className="space-y-6">
      <h2 className="text-2xl font-bold text-white">Token Settings</h2>
      <TokenSettings />
    </div>
  );
};

export default TokensTab;
