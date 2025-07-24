import React from 'react';
import UserAvatar from './UserAvatar';

const AvatarDemo: React.FC = () => {
  const testUsers = [
    { username: 'john_doe', email: 'john@example.com' },
    { username: 'sarah_maker', email: 'sarah@example.com' },
    { username: 'alex_3d', email: 'alex@example.com' },
    { username: 'maker_bot', email: 'bot@example.com' },
    { username: 'print_master', email: 'master@example.com' },
    { username: 'designer_pro', email: 'pro@example.com' },
    { username: 'model_guru', email: 'guru@example.com' },
    { username: 'creative_mind', email: 'creative@example.com' },
    { username: 'tech_wizard', email: 'wizard@example.com' },
    { username: 'art_creator', email: 'art@example.com' },
  ];

  const sizes: Array<'xs' | 'sm' | 'md' | 'lg' | 'xl' | '2xl'> = ['xs', 'sm', 'md', 'lg', 'xl', '2xl'];

  return (
    <div className="max-w-6xl mx-auto p-8">
      <h1 className="text-3xl font-bold text-gray-900 mb-8">Avatar Demo</h1>
      
      {/* Size Demonstration */}
      <div className="bg-white rounded-lg shadow p-6 mb-8">
        <h2 className="text-xl font-semibold text-gray-900 mb-4">Avatar Sizes</h2>
        <div className="flex items-end space-x-4">
          {sizes.map(size => (
            <div key={size} className="text-center">
              <UserAvatar userId="demo-user-id" username="demo_user" size={size} className="mb-2" />
              <p className="text-sm text-gray-600">{size}</p>
            </div>
          ))}
        </div>
      </div>

      {/* Different Users */}
      <div className="bg-white rounded-lg shadow p-6 mb-8">
        <h2 className="text-xl font-semibold text-gray-900 mb-4">Different Users</h2>
        <div className="grid grid-cols-2 md:grid-cols-3 lg:grid-cols-5 gap-6">
          {testUsers.map(user => (
            <div key={user.username} className="text-center">
              <UserAvatar userId={`user-${user.username}`} username={user.username} size="lg" className="mx-auto mb-2" />
              <p className="text-sm font-medium text-gray-900">{user.username}</p>
              <p className="text-xs text-gray-500">{user.email}</p>
            </div>
          ))}
        </div>
      </div>

      {/* Same User with Profile Picture Override */}
      <div className="bg-white rounded-lg shadow p-6">
        <h2 className="text-xl font-semibold text-gray-900 mb-4">Profile Picture Override</h2>
        <div className="flex items-center space-x-8">
          <div className="text-center">
            <UserAvatar userId="profile-user-id" username="profile_user" size="lg" className="mx-auto mb-2" />
            <p className="text-sm font-medium text-gray-900">Generated Avatar</p>
            <p className="text-xs text-gray-500">No profile picture</p>
          </div>
          <div className="text-center">
            <UserAvatar 
              userId="profile-user-id"
              username="profile_user" 
              profilePictureUrl="https://images.unsplash.com/photo-1472099645785-5658abf4ff4e?w=80&h=80&fit=crop&crop=face"
              size="lg" 
              className="mx-auto mb-2" 
            />
            <p className="text-sm font-medium text-gray-900">Custom Picture</p>
            <p className="text-xs text-gray-500">With profile picture</p>
          </div>
        </div>
      </div>

      {/* Integration Examples */}
      <div className="bg-white rounded-lg shadow p-6 mt-8">
        <h2 className="text-xl font-semibold text-gray-900 mb-4">Usage Examples</h2>
        
        {/* Dashboard Style */}
        <div className="mb-6">
          <h3 className="text-lg font-medium text-gray-900 mb-2">Dashboard Navigation</h3>
          <div className="bg-gray-50 p-4 rounded border">
            <div className="flex items-center">
              <UserAvatar userId="john-doe-id" username="john_doe" size="sm" className="mr-3" />
              <span className="text-sm font-medium text-gray-700">john_doe</span>
            </div>
          </div>
        </div>

        {/* Model Author Style */}
        <div className="mb-6">
          <h3 className="text-lg font-medium text-gray-900 mb-2">Model Author</h3>
          <div className="bg-gray-50 p-4 rounded border">
            <div className="flex items-center">
              <UserAvatar userId="sarah-maker-id" username="sarah_maker" size="md" />
              <div className="ml-3">
                <p className="font-medium text-gray-900">sarah_maker</p>
                <p className="text-sm text-gray-600">Creator</p>
              </div>
            </div>
          </div>
        </div>

        {/* Profile Section Style */}
        <div>
          <h3 className="text-lg font-medium text-gray-900 mb-2">Profile Section</h3>
          <div className="bg-gray-50 p-4 rounded border">
            <div className="flex items-center">
              <UserAvatar userId="alex-3d-id" username="alex_3d" size="xl" className="mr-6" />
              <div>
                <h3 className="text-lg font-medium text-gray-900">alex_3d</h3>
                <p className="text-sm text-gray-600">alex@example.com</p>
                <p className="text-xs text-gray-500 mt-1">
                  Your avatar is automatically generated from your username
                </p>
              </div>
            </div>
          </div>
        </div>
      </div>
    </div>
  );
};

export default AvatarDemo; 