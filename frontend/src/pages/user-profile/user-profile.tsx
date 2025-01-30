import { useState } from 'react';
import { ModelCard } from '../../components/model-card/model-card';
import { User, Model, Collection } from '../../types/models';
import { useParams } from 'react-router-dom';
import { CollectionCard } from '../../components/collection-card/collection-card';

export const UserProfile = () => {
  const { username } = useParams<{ username: string }>();
  const [activeTab, setActiveTab] = useState<'models' | 'collections'>('models');
  const [user] = useState<User>({
    id: 1,
    username: "John Doe",
    avatarUrl: "/sample/avatar.jpg",
    bio: "3D printing enthusiast and designer. Creating models for everyone to enjoy!",
    joinDate: "2023-01-15",
    totalLikes: 1500,
    totalDownloads: 10000,
  });

  // This would normally come from an API
  const [userModels] = useState<Model[]>([
    {
      id: 1,
      title: "Cool 3D Model",
      description: "An awesome 3D printable model",
      thumbnailUrl: "/sample/thumbnail1.jpg",
      modelUrl: "src/sample/model1.stl",
      likes: 156,
      downloads: 1234,
      creator: "John Doe"
    },
    // Add more models as needed
  ]);

  const [collections] = useState<Collection[]>([
    {
      id: 1,
      title: "Favorite Prints",
      description: "My go-to 3D printing models",
      visibility: "public",
      createdAt: "2024-03-20",
      updatedAt: "2024-03-20",
      userId: 1,
      models: [] // This would be populated from the API
    }
  ]);

  return (
    <div className="container px-4 py-8 mx-auto">
      <div className="bg-white rounded-lg shadow-md p-6 mb-8">
        <div className="flex items-center gap-6">
          <img
            src={user.avatarUrl}
            alt={user.username}
            className="w-24 h-24 rounded-full object-cover"
          />
          <div>
            <h1 className="text-3xl font-bold mb-2">{user.username}</h1>
            <p className="text-gray-600 mb-2">{user.bio}</p>
            <div className="flex gap-4 text-sm text-gray-500">
              <span>Joined {new Date(user.joinDate).toLocaleDateString()}</span>
              <span>•</span>
              <span>{user.totalLikes.toLocaleString()} Total Likes</span>
              <span>•</span>
              <span>{user.totalDownloads.toLocaleString()} Total Downloads</span>
            </div>
          </div>
        </div>
      </div>

      <div className="mb-8">
        <div className="flex gap-4 border-b border-gray-200">
          <button
            className={`px-4 py-2 ${activeTab === 'models' ? 'border-b-2 border-green-500 text-green-500' : 'text-gray-500'}`}
            onClick={() => setActiveTab('models')}
          >
            Models
          </button>
          <button
            className={`px-4 py-2 ${activeTab === 'collections' ? 'border-b-2 border-green-500 text-green-500' : 'text-gray-500'}`}
            onClick={() => setActiveTab('collections')}
          >
            Collections
          </button>
        </div>
      </div>

      {activeTab === 'models' ? (
        <div className="grid grid-cols-1 gap-6 md:grid-cols-2 lg:grid-cols-3">
          {userModels.map((model) => (
            <ModelCard key={model.id} model={model} />
          ))}
        </div>
      ) : (
        <div className="grid grid-cols-1 gap-6 md:grid-cols-2 lg:grid-cols-3">
          {collections.map((collection) => (
            <CollectionCard key={collection.id} collection={collection} />
          ))}
        </div>
      )}
    </div>
  );
}; 