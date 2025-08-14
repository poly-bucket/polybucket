import React, { useState, useEffect, useMemo } from 'react';
import { FontAwesomeIcon } from '@fortawesome/react-fontawesome';
import { 
  faSearch, 
  faPalette, 
  faImage, 
  faRandom,
  faTimes,
  faStar,
  faHeart,
  faHome,
  faUser,
  faCog,
  faBookmark,
  faFolder,
  faTag,
  faCalendar,
  faClock,
  faMap,
  faPhone,
  faEnvelope,
  faGlobe,
  faLink
} from '@fortawesome/free-solid-svg-icons';
import { 
  faStar as farStar,
  faHeart as farHeart,
  faUser as farUser,
  faBookmark as farBookmark,
  faFolder as farFolder,
  faCalendar as farCalendar,
  faClock as farClock,
  faMap as farMap,
  faEnvelope as farEnvelope
} from '@fortawesome/free-regular-svg-icons';
import { 
  faGithub,
  faTwitter,
  faFacebook,
  faInstagram,
  faLinkedin,
  faYoutube,
  faDiscord,
  faSteam,
  faReddit,
  faTwitch
} from '@fortawesome/free-brands-svg-icons';
import { minidenticon } from 'minidenticons';

interface IconSelectorProps {
  selectedIcon?: string;
  onIconSelect: (icon: string, type: 'fontawesome' | 'generated' | 'model-thumbnail') => void;
  collectionName?: string;
  collectionId?: string;
  className?: string;
}

interface IconOption {
  name: string;
  icon: any;
  type: 'solid' | 'regular' | 'brands';
  searchTerms: string[];
}

const IconSelector: React.FC<IconSelectorProps> = ({
  selectedIcon,
  onIconSelect,
  collectionName = '',
  collectionId = '',
  className = ''
}) => {
  const [searchQuery, setSearchQuery] = useState('');
  const [showIconPicker, setShowIconPicker] = useState(false);
  const [selectedType, setSelectedType] = useState<'fontawesome' | 'generated' | 'model-thumbnail'>('fontawesome');
  const [generatedSalt, setGeneratedSalt] = useState('');

  // Create a comprehensive list of icons with search terms
  const allIcons = useMemo(() => {
    const icons: IconOption[] = [];
    
    // Add solid icons
    const solidIcons = [
      { icon: faStar, name: 'Star' },
      { icon: faHeart, name: 'Heart' },
      { icon: faHome, name: 'Home' },
      { icon: faUser, name: 'User' },
      { icon: faCog, name: 'Cog' },
      { icon: faBookmark, name: 'Bookmark' },
      { icon: faFolder, name: 'Folder' },
      { icon: faTag, name: 'Tag' },
      { icon: faCalendar, name: 'Calendar' },
      { icon: faClock, name: 'Clock' },
      { icon: faMap, name: 'Map' },
      { icon: faPhone, name: 'Phone' },
      { icon: faEnvelope, name: 'Envelope' },
      { icon: faGlobe, name: 'Globe' },
      { icon: faLink, name: 'Link' }
    ];
    
    solidIcons.forEach(({ icon, name }) => {
      icons.push({
        name,
        icon,
        type: 'solid' as const,
        searchTerms: [name.toLowerCase(), ...name.split(/(?=[A-Z])/).map(word => word.toLowerCase())]
      });
    });
    
    // Add regular icons
    const regularIcons = [
      { icon: farStar, name: 'Star' },
      { icon: farHeart, name: 'Heart' },
      { icon: farUser, name: 'User' },
      { icon: farBookmark, name: 'Bookmark' },
      { icon: farFolder, name: 'Folder' },
      { icon: farCalendar, name: 'Calendar' },
      { icon: farClock, name: 'Clock' },
      { icon: farMap, name: 'Map' },
      { icon: farEnvelope, name: 'Envelope' }
    ];
    
    regularIcons.forEach(({ icon, name }) => {
      icons.push({
        name,
        icon,
        type: 'regular' as const,
        searchTerms: [name.toLowerCase(), ...name.split(/(?=[A-Z])/).map(word => word.toLowerCase())]
      });
    });
    
    // Add brand icons
    const brandIcons = [
      { icon: faGithub, name: 'Github' },
      { icon: faTwitter, name: 'Twitter' },
      { icon: faFacebook, name: 'Facebook' },
      { icon: faInstagram, name: 'Instagram' },
      { icon: faLinkedin, name: 'Linkedin' },
      { icon: faYoutube, name: 'Youtube' },
      { icon: faDiscord, name: 'Discord' },
      { icon: faSteam, name: 'Steam' },
      { icon: faReddit, name: 'Reddit' },
      { icon: faTwitch, name: 'Twitch' }
    ];
    
    brandIcons.forEach(({ icon, name }) => {
      icons.push({
        name,
        icon,
        type: 'brands' as const,
        searchTerms: [name.toLowerCase(), ...name.split(/(?=[A-Z])/).map(word => word.toLowerCase())]
      });
    });
    
    return icons;
  }, []);

  // Filter icons based on search query
  const filteredIcons = useMemo(() => {
    if (!searchQuery.trim()) {
      return allIcons.slice(0, 100); // Show first 100 icons by default
    }
    
    const query = searchQuery.toLowerCase();
    return allIcons
      .filter(icon => 
        icon.searchTerms.some(term => term.includes(query))
      )
      .slice(0, 200); // Limit results for performance
  }, [allIcons, searchQuery]);

  // Generate preview for generated avatar
  const generatedPreview = useMemo(() => {
    if (!collectionId) return '';
    const seed = generatedSalt ? `${collectionId}-${generatedSalt}` : collectionId;
    return minidenticon(seed, 50, 50);
  }, [collectionId, generatedSalt]);

  const handleIconSelect = (iconName: string, icon: any, type: 'solid' | 'regular' | 'brands') => {
    const iconString = `${type}:${iconName}`;
    onIconSelect(iconString, 'fontawesome');
    setShowIconPicker(false);
  };

  const handleGeneratedSelect = () => {
    const seed = generatedSalt ? `${collectionId}-${generatedSalt}` : collectionId;
    const avatar = minidenticon(seed, 50, 50);
    onIconSelect(avatar, 'generated');
    setShowIconPicker(false);
  };

  const handleModelThumbnailSelect = () => {
    onIconSelect('model-thumbnail', 'model-thumbnail');
    setShowIconPicker(false);
  };

  const getSelectedIconDisplay = () => {
    if (!selectedIcon) return null;
    
    if (selectedIcon === 'model-thumbnail') {
      return (
        <div className="flex items-center space-x-2">
          <div className="w-8 h-8 bg-blue-500 rounded flex items-center justify-center">
            <FontAwesomeIcon icon={faImage} className="text-white text-sm" />
          </div>
          <span className="text-sm text-gray-300">Model Thumbnail</span>
        </div>
      );
    }
    
    if (selectedIcon.startsWith('solid:') || selectedIcon.startsWith('regular:') || selectedIcon.startsWith('brands:')) {
      const [type, name] = selectedIcon.split(':');
      const icon = allIcons.find(i => i.type === type && i.name === name)?.icon;
      
      if (icon) {
        return (
          <div className="flex items-center space-x-2">
            <FontAwesomeIcon icon={icon} className="w-8 h-8 text-green-400" />
            <span className="text-sm text-gray-300">{name}</span>
          </div>
        );
      }
    }
    
    // Generated avatar
    return (
      <div className="flex items-center space-x-2">
        <div className="w-8 h-8 rounded overflow-hidden">
          <img src={`data:image/svg+xml,${encodeURIComponent(selectedIcon)}`} alt="Generated" />
        </div>
        <span className="text-sm text-gray-300">Generated Avatar</span>
      </div>
    );
  };

  return (
    <div className={className}>
      <label className="block text-sm font-medium text-white mb-2">
        Collection Icon
      </label>
      
      {/* Current Selection Display */}
      {selectedIcon && (
        <div className="mb-4 p-3 bg-gray-800 rounded-lg border border-gray-600">
          <div className="flex items-center justify-between">
            {getSelectedIconDisplay()}
            <button
              onClick={() => onIconSelect('', 'fontawesome')}
              className="text-gray-400 hover:text-gray-300"
              title="Clear icon"
            >
              <FontAwesomeIcon icon={faTimes} />
            </button>
          </div>
        </div>
      )}
      
      {/* Icon Selection Button */}
      <button
        type="button"
        onClick={() => setShowIconPicker(!showIconPicker)}
        className="w-full lg-input flex items-center justify-between text-left"
      >
        <span className="text-gray-300">
          {selectedIcon ? 'Change Icon' : 'Choose an icon for your collection'}
        </span>
        <FontAwesomeIcon icon={faPalette} className="text-gray-400" />
      </button>
      
      {/* Icon Picker Modal */}
      {showIconPicker && (
        <div className="fixed inset-0 bg-black bg-opacity-50 flex items-center justify-center z-50 p-4">
          <div className="bg-gray-900 rounded-lg max-w-4xl w-full max-h-[80vh] overflow-hidden">
            {/* Header */}
            <div className="p-4 border-b border-gray-700 flex items-center justify-between">
              <h3 className="text-lg font-medium text-white">Choose Collection Icon</h3>
              <button
                onClick={() => setShowIconPicker(false)}
                className="text-gray-400 hover:text-gray-300"
              >
                <FontAwesomeIcon icon={faTimes} />
              </button>
            </div>
            
            {/* Tab Navigation */}
            <div className="border-b border-gray-700">
              <nav className="flex space-x-8 px-4">
                <button
                  onClick={() => setSelectedType('fontawesome')}
                  className={`py-3 px-1 border-b-2 font-medium text-sm transition-colors duration-200 ${
                    selectedType === 'fontawesome'
                      ? 'border-green-500 text-green-400'
                      : 'border-transparent text-gray-400 hover:text-gray-300'
                  }`}
                >
                  FontAwesome Icons
                </button>
                <button
                  onClick={() => setSelectedType('generated')}
                  className={`py-3 px-1 border-b-2 font-medium text-sm transition-colors duration-200 ${
                    selectedType === 'generated'
                      ? 'border-green-500 text-green-400'
                      : 'border-transparent text-gray-400 hover:text-gray-300'
                  }`}
                >
                  Generate Avatar
                </button>
                <button
                  onClick={() => setSelectedType('model-thumbnail')}
                  className={`py-3 px-1 border-b-2 font-medium text-sm transition-colors duration-200 ${
                    selectedType === 'model-thumbnail'
                      ? 'border-green-500 text-green-400'
                      : 'border-transparent text-gray-400 hover:text-gray-300'
                  }`}
                >
                  Model Thumbnail
                </button>
              </nav>
            </div>
            
            {/* Content */}
            <div className="p-4 overflow-y-auto max-h-[60vh]">
              {selectedType === 'fontawesome' && (
                <div>
                  {/* Search */}
                  <div className="relative mb-4">
                    <FontAwesomeIcon 
                      icon={faSearch} 
                      className="absolute left-3 top-1/2 transform -translate-y-1/2 text-gray-400" 
                    />
                    <input
                      type="text"
                      placeholder="Search icons..."
                      value={searchQuery}
                      onChange={(e) => setSearchQuery(e.target.value)}
                      className="lg-input pl-10"
                    />
                  </div>
                  
                  {/* Icons Grid */}
                  <div className="grid grid-cols-6 sm:grid-cols-8 md:grid-cols-10 gap-2">
                    {filteredIcons.map((iconOption) => (
                      <button
                        key={`${iconOption.type}-${iconOption.name}`}
                        onClick={() => handleIconSelect(iconOption.name, iconOption.icon, iconOption.type)}
                        className="p-3 hover:bg-gray-800 rounded-lg transition-colors group"
                        title={iconOption.name}
                      >
                        <FontAwesomeIcon 
                          icon={iconOption.icon} 
                          className="w-6 h-6 text-gray-300 group-hover:text-green-400 transition-colors" 
                        />
                      </button>
                    ))}
                  </div>
                  
                  {filteredIcons.length === 0 && (
                    <div className="text-center py-8 text-gray-400">
                      No icons found matching "{searchQuery}"
                    </div>
                  )}
                </div>
              )}
              
              {selectedType === 'generated' && (
                <div className="space-y-4">
                  <div>
                    <label className="block text-sm font-medium text-white mb-2">
                      Custom Salt (Optional)
                    </label>
                    <input
                      type="text"
                      value={generatedSalt}
                      onChange={(e) => setGeneratedSalt(e.target.value)}
                      placeholder="Add randomness to your avatar"
                      className="lg-input"
                    />
                    <p className="mt-1 text-sm text-gray-400">
                      Leave empty to use collection ID, or add text for more variety
                    </p>
                  </div>
                  
                  <div className="flex items-center justify-center">
                    <div className="p-4 bg-gray-800 rounded-lg">
                      <div 
                        className="w-32 h-32 rounded-lg overflow-hidden"
                        dangerouslySetInnerHTML={{ __html: generatedPreview }}
                      />
                    </div>
                  </div>
                  
                  <div className="text-center">
                    <button
                      onClick={handleGeneratedSelect}
                      className="lg-button lg-button-primary"
                    >
                      Use This Avatar
                    </button>
                  </div>
                </div>
              )}
              
              {selectedType === 'model-thumbnail' && (
                <div className="text-center py-8">
                  <FontAwesomeIcon icon={faImage} className="w-16 h-16 text-blue-400 mb-4" />
                  <h4 className="text-lg font-medium text-white mb-2">Use Model Thumbnail</h4>
                  <p className="text-gray-400 mb-6">
                    Your collection will automatically use the thumbnail of the last added model as its icon.
                  </p>
                  <button
                    onClick={handleModelThumbnailSelect}
                    className="lg-button lg-button-primary"
                  >
                    Use Model Thumbnail
                  </button>
                </div>
              )}
            </div>
          </div>
        </div>
      )}
    </div>
  );
};

export default IconSelector;
