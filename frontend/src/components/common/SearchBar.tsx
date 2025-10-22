import React, { useState, useRef, useEffect } from 'react';

export type SearchType = 'models' | 'collections' | 'users' | 'all';

interface SearchBarProps {
  onSearch?: (query: string, searchTypes?: SearchType[]) => void;
  onClearSearch?: () => void;
  searchPlaceholder?: string;
  searchQuery?: string;
  searchTags?: string[];
  onSearchTagRemove?: (tag: string) => void;
  onSearchTagAdd?: (tag: string) => void;
  isSearching?: boolean;
  disabled?: boolean;
  className?: string;
  selectedSearchTypes?: SearchType[];
  onSearchTypeChange?: (searchTypes: SearchType[]) => void;
}

const SearchBar: React.FC<SearchBarProps> = ({
  onSearch,
  onClearSearch,
  searchPlaceholder = "Search for models...",
  searchQuery = "",
  searchTags = [],
  onSearchTagRemove,
  onSearchTagAdd,
  isSearching = false,
  disabled = false,
  className = "",
  selectedSearchTypes = ['models'],
  onSearchTypeChange
}) => {
  // Search input state
  const [inputValue, setInputValue] = useState('');
  const [showTagInput, setShowTagInput] = useState(false);
  const textareaRef = useRef<HTMLTextAreaElement>(null);
  const tagInputRef = useRef<HTMLInputElement>(null);

  // Update input value when searchQuery changes
  useEffect(() => {
    if (searchQuery && !searchTags.includes(searchQuery)) {
      setInputValue(searchQuery);
    }
  }, [searchQuery, searchTags]);

  // Generate dynamic placeholder text based on selected search types
  const getDynamicPlaceholder = () => {
    if (selectedSearchTypes.length === 0 || selectedSearchTypes.includes('all')) {
      return "Search everything...";
    }
    
    if (selectedSearchTypes.length === 1) {
      const type = selectedSearchTypes[0];
      return `Search for ${type}...`;
    }
    
    if (selectedSearchTypes.length === 2) {
      return `Search for ${selectedSearchTypes.join(' and ')}...`;
    }
    
    if (selectedSearchTypes.length === 3) {
      return "Search for models, collections and users...";
    }
    
    return "Search for models...";
  };

  const handleSearch = (e: React.KeyboardEvent<HTMLTextAreaElement>) => {
    if (e.key === 'Enter' && !e.shiftKey && onSearch) {
      e.preventDefault();
      const value = (e.target as HTMLTextAreaElement).value.trim();
      if (value) {
        onSearch(value, selectedSearchTypes);
        setInputValue('');
      }
    }
  };

  const handleClearSearch = () => {
    setInputValue('');
    if (onClearSearch) {
      onClearSearch();
    }
  };

  const handleTagRemove = (tag: string) => {
    if (onSearchTagRemove) {
      onSearchTagRemove(tag);
    }
  };

  const handleTagAdd = (e: React.KeyboardEvent<HTMLInputElement>) => {
    if (e.key === 'Enter' && onSearchTagAdd) {
      const value = (e.target as HTMLInputElement).value.trim();
      if (value && !searchTags.includes(value)) {
        onSearchTagAdd(value);
        (e.target as HTMLInputElement).value = '';
        setShowTagInput(false);
      }
    }
  };

  const handleInputFocus = () => {
    if (searchTags.length > 0) {
      setShowTagInput(true);
    }
  };

  const handleInputBlur = () => {
    // Delay hiding tag input to allow for clicks on tags
    setTimeout(() => {
      setShowTagInput(false);
    }, 200);
  };

  const handleSearchTypeClick = (searchType: SearchType) => {
    if (onSearchTypeChange) {
      let newSearchTypes: SearchType[];
      
      if (selectedSearchTypes.includes(searchType)) {
        // Remove the type if it's already selected
        newSearchTypes = selectedSearchTypes.filter(type => type !== searchType);
        // If no types are selected, default to models
        if (newSearchTypes.length === 0) {
          newSearchTypes = ['models'];
        }
      } else {
        // Add the type if it's not selected
        newSearchTypes = [...selectedSearchTypes, searchType];
      }
      
      onSearchTypeChange(newSearchTypes);
    }
  };

  const getSearchTypeIcon = (searchType: SearchType) => {
    switch (searchType) {
      case 'models':
        return (
          <svg
            className="text-sky-500"
            strokeLinejoin="round"
            strokeLinecap="round"
            strokeWidth="2"
            stroke="currentColor"
            fill="none"
            viewBox="0 0 24 24"
            height="16"
            width="16"
            xmlns="http://www.w3.org/2000/svg"
          >
            <path d="M12 2l3.09 6.26L22 9.27l-5 4.87 1.18 6.88L12 17.77l-6.18 3.25L7 14.14 2 9.27l6.91-1.01L12 2z" />
          </svg>
        );
      case 'collections':
        return (
          <svg
            className="text-gray-500"
            strokeLinejoin="round"
            strokeLinecap="round"
            strokeWidth="2"
            stroke="currentColor"
            fill="none"
            viewBox="0 0 24 24"
            height="16"
            width="16"
            xmlns="http://www.w3.org/2000/svg"
          >
            <path d="M3 7v10a2 2 0 002 2h14a2 2 0 002-2V9a2 2 0 00-2-2H5a2 2 0 00-2-2z" />
            <path d="M8 21v-4a2 2 0 012-2h4a2 2 0 012 2v4" />
          </svg>
        );
      case 'users':
        return (
          <svg
            className="text-gray-500"
            strokeLinejoin="round"
            strokeLinecap="round"
            strokeWidth="2"
            stroke="currentColor"
            fill="none"
            viewBox="0 0 24 24"
            height="16"
            width="16"
            xmlns="http://www.w3.org/2000/svg"
          >
            <path d="M20 21v-2a4 4 0 00-4-4H8a4 4 0 00-4 4v2" />
            <circle cx="12" cy="7" r="4" />
          </svg>
        );
      default:
        return (
          <svg
            className="text-sky-500"
            strokeLinejoin="round"
            strokeLinecap="round"
            strokeWidth="2"
            stroke="currentColor"
            fill="none"
            viewBox="0 0 24 24"
            height="16"
            width="16"
            xmlns="http://www.w3.org/2000/svg"
          >
            <circle r="10" cy="12" cx="12"></circle>
            <path d="M2 12h20"></path>
            <path d="M12 2a15.3 15.3 0 0 1 4 10 15.3 15.3 0 0 1-4 10 15.3 15.3 0 0 1-4-10 15.3 15.3 0 0 1 4-10z"></path>
          </svg>
        );
    }
  };

  return (
    <div className={`w-full py-4 ${className}`}>
      <div className="relative max-w-xl w-full mx-auto">
        <div className="relative flex flex-col bg-black/5">
          {/* Search Textarea */}
          <textarea
            ref={textareaRef}
            className="w-full min-h-[52px] max-h-[200px] rounded-xl rounded-b-none px-4 py-3 bg-black text-white placeholder:text-white/70 border-0 outline-none resize-none focus:ring-0 focus:outline-none leading-[1.2]"
            placeholder={getDynamicPlaceholder()}
            onKeyDown={handleSearch}
            onFocus={handleInputFocus}
            onBlur={handleInputBlur}
            onChange={(e) => setInputValue(e.target.value)}
            value={inputValue}
            disabled={disabled || isSearching}
          />

          {/* Filter Buttons Section */}
          <div className="h-12 bg-black rounded-b-xl">
            <div className="absolute left-3 bottom-3 flex items-center gap-2">
              {/* Models Button */}
              <button
                className={`rounded-full flex items-center gap-2 px-1.5 py-1 border h-8 cursor-pointer transition-colors ${
                  selectedSearchTypes.includes('models')
                    ? 'bg-sky-500/20 border-sky-400 text-sky-500'
                    : 'bg-sky-500/15 hover:bg-sky-500/20 border-sky-400 text-sky-500'
                }`}
                type="button"
                onClick={() => handleSearchTypeClick('models')}
                disabled={disabled}
              >
                <div className="w-4 h-4 flex items-center justify-center shrink-0">
                  {getSearchTypeIcon('models')}
                </div>
                <span className="text-sm text-sky-500">Models</span>
              </button>

              {/* Collections Button */}
              <button
                className={`rounded-full flex items-center gap-2 px-1.5 py-1 border h-8 cursor-pointer transition-colors ${
                  selectedSearchTypes.includes('collections')
                    ? 'bg-sky-500/20 border-sky-400 text-sky-500'
                    : 'bg-sky-500/15 hover:bg-sky-500/20 border-sky-400 text-sky-500'
                }`}
                type="button"
                onClick={() => handleSearchTypeClick('collections')}
                disabled={disabled}
              >
                <div className="w-4 h-4 flex items-center justify-center shrink-0">
                  {getSearchTypeIcon('collections')}
                </div>
                <span className="text-sm text-sky-500">Collections</span>
              </button>

              {/* Users Button */}
              <button
                className={`rounded-full flex items-center gap-2 px-1.5 py-1 border h-8 cursor-pointer transition-colors ${
                  selectedSearchTypes.includes('users')
                    ? 'bg-sky-500/20 border-sky-400 text-sky-500'
                    : 'bg-sky-500/15 hover:bg-sky-500/20 border-sky-400 text-sky-500'
                }`}
                type="button"
                onClick={() => handleSearchTypeClick('users')}
                disabled={disabled}
              >
                <div className="w-4 h-4 flex items-center justify-center shrink-0">
                  {getSearchTypeIcon('users')}
                </div>
                <span className="text-sm text-sky-500">Users</span>
              </button>
            </div>

            {/* Clear button */}
            {(inputValue || searchTags.length > 0) && (
              <button
                onClick={handleClearSearch}
                className="absolute right-3 bottom-3 text-gray-400 hover:text-white transition-colors"
                disabled={disabled}
              >
                <svg className="w-4 h-4" fill="none" stroke="currentColor" viewBox="0 0 24 24">
                  <path strokeLinecap="round" strokeLinejoin="round" strokeWidth={2} d="M6 18L18 6M6 6l12 12" />
                </svg>
              </button>
            )}

            {/* Loading spinner */}
            {isSearching && (
              <div className="absolute right-3 bottom-3">
                <div className="lg-spinner h-4 w-4"></div>
              </div>
            )}
          </div>
        </div>
        
        {/* Tag Input (appears when focused and has existing tags) */}
        {showTagInput && onSearchTagAdd && (
          <div className="absolute top-full left-0 right-0 mt-1 bg-white rounded-lg shadow-lg border border-gray-200 z-10">
            <input
              ref={tagInputRef}
              type="text"
              placeholder="Add search tag..."
              className="w-full px-3 py-2 text-sm border-0 rounded-lg focus:ring-0 focus:outline-none"
              onKeyPress={handleTagAdd}
              autoFocus
            />
          </div>
        )}
      </div>
    </div>
  );
};

export default SearchBar;
