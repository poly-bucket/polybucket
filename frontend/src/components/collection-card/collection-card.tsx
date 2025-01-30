import { Link } from 'react-router-dom';
import { Collection } from '../../types/models';

interface CollectionCardProps {
  collection: Collection;
}

export const CollectionCard = ({ collection }: CollectionCardProps) => {
  return (
    <Link to={`/collections/${collection.id}`} className="block">
      <div className="bg-gray-900 rounded-lg border border-green-500/30 overflow-hidden transition-transform duration-200 hover:scale-[1.02]">
        <div className="p-4">
          <div className="flex items-center justify-between mb-2">
            <h3 className="text-lg font-semibold text-green-400">{collection.title}</h3>
            <span className={`text-xs px-2 py-1 rounded-full ${
              collection.visibility === 'public' ? 'bg-green-500/20 text-green-400' :
              collection.visibility === 'private' ? 'bg-red-500/20 text-red-400' :
              'bg-gray-500/20 text-gray-400'
            }`}>
              {collection.visibility}
            </span>
          </div>
          <p className="text-sm text-green-300/80 line-clamp-2 mb-2">{collection.description}</p>
          <p className="text-sm text-gray-500">
            {collection.models.length} models
          </p>
        </div>
      </div>
    </Link>
  );
}; 