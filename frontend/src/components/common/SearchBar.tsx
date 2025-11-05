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
  const [inputValue, setInputValue] = useState('');
  const [showDropdown, setShowDropdown] = useState(false);
  const [isFocused, setIsFocused] = useState(false);
  const inputRef = useRef<HTMLInputElement>(null);
  const dropdownRef = useRef<HTMLDivElement>(null);

  useEffect(() => {
    if (searchQuery && !searchTags.includes(searchQuery)) {
      setInputValue(searchQuery);
    }
  }, [searchQuery, searchTags]);

  useEffect(() => {
    const handleClickOutside = (event: MouseEvent) => {
      if (
        dropdownRef.current &&
        !dropdownRef.current.contains(event.target as Node) &&
        inputRef.current &&
        !inputRef.current.contains(event.target as Node)
      ) {
        setShowDropdown(false);
      }
    };

    document.addEventListener('mousedown', handleClickOutside);
    return () => document.removeEventListener('mousedown', handleClickOutside);
  }, []);

  useEffect(() => {
    if (isFocused) {
      setShowDropdown(true);
    } else {
      setShowDropdown(false);
    }
  }, [isFocused]);

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

  const handleSearch = (e: React.KeyboardEvent<HTMLInputElement>) => {
    if (e.key === 'Enter' && onSearch) {
      e.preventDefault();
      const value = inputValue.trim();
      if (value) {
        onSearch(value, selectedSearchTypes);
        setInputValue('');
        setShowDropdown(false);
      }
    }
  };

  const handleClearSearch = () => {
    setInputValue('');
    setShowDropdown(false);
    if (onClearSearch) {
      onClearSearch();
    }
  };

  const handleInputFocus = () => {
    setIsFocused(true);
  };

  const handleInputBlur = () => {
    setTimeout(() => {
      setIsFocused(false);
    }, 200);
  };

  const handleSearchTypeClick = (searchType: SearchType, e?: React.MouseEvent) => {
    if (e) {
      e.stopPropagation();
      e.preventDefault();
    }
    if (onSearchTypeChange) {
      let newSearchTypes: SearchType[];
      
      if (selectedSearchTypes.includes(searchType)) {
        newSearchTypes = selectedSearchTypes.filter(type => type !== searchType);
        if (newSearchTypes.length === 0) {
          newSearchTypes = ['models'];
        }
      } else {
        newSearchTypes = [...selectedSearchTypes, searchType];
      }
      
      onSearchTypeChange(newSearchTypes);
    }
  };

  const handleQuickFilter = (searchType: SearchType) => {
    handleSearchTypeClick(searchType);
    if (inputValue.trim() && onSearch) {
      const newTypes = selectedSearchTypes.includes(searchType)
        ? selectedSearchTypes.filter(type => type !== searchType)
        : [...selectedSearchTypes, searchType];
      if (newTypes.length === 0) {
        onSearch(inputValue.trim(), ['models']);
      } else {
        onSearch(inputValue.trim(), newTypes);
      }
    }
  };

  const getSearchTypeIcon = (searchType: SearchType) => {
    switch (searchType) {
      case 'models':
        return (
          <svg
            className="w-4 h-4"
            strokeLinejoin="round"
            strokeLinecap="round"
            strokeWidth="2"
            stroke="currentColor"
            fill="none"
            viewBox="0 0 24 24"
            xmlns="http://www.w3.org/2000/svg"
          >
            <path d="M12 2l3.09 6.26L22 9.27l-5 4.87 1.18 6.88L12 17.77l-6.18 3.25L7 14.14 2 9.27l6.91-1.01L12 2z" />
          </svg>
        );
      case 'collections':
        return (
          <svg
            className="w-4 h-4"
            strokeLinejoin="round"
            strokeLinecap="round"
            strokeWidth="2"
            stroke="currentColor"
            fill="none"
            viewBox="0 0 24 24"
            xmlns="http://www.w3.org/2000/svg"
          >
            <path d="M3 7v10a2 2 0 002 2h14a2 2 0 002-2V9a2 2 0 00-2-2H5a2 2 0 00-2-2z" />
            <path d="M8 21v-4a2 2 0 012-2h4a2 2 0 012 2v4" />
          </svg>
        );
      case 'users':
        return (
          <svg
            className="w-4 h-4"
            strokeLinejoin="round"
            strokeLinecap="round"
            strokeWidth="2"
            stroke="currentColor"
            fill="none"
            viewBox="0 0 24 24"
            xmlns="http://www.w3.org/2000/svg"
          >
            <path d="M20 21v-2a4 4 0 00-4-4H8a4 4 0 00-4 4v2" />
            <circle cx="12" cy="7" r="4" />
          </svg>
        );
      default:
        return null;
    }
  };

  const getSearchTypeLabel = (searchType: SearchType) => {
    switch (searchType) {
      case 'models':
        return 'Models';
      case 'collections':
        return 'Collections';
      case 'users':
        return 'Users';
      default:
        return 'All';
    }
  };

  const activeFilters = selectedSearchTypes.filter(type => type !== 'all');
  const isDefaultState = activeFilters.length === 1 && activeFilters[0] === 'models';
  const hasActiveFilters = activeFilters.length > 0 && !isDefaultState;

  const calculateInputPadding = () => {
    let rightPadding = 12;
    if (hasActiveFilters) {
      rightPadding += activeFilters.length * 70 + 8;
    }
    if (inputValue || isSearching) {
      rightPadding += 24;
    }
    return rightPadding;
  };

  return (
    <div className={`relative ${className}`}>
      <div className="relative max-w-2xl w-full">
        <div className="relative">
          <div className="absolute inset-y-0 left-0 flex items-center pl-3 pointer-events-none z-10">
            <svg
              className="w-4 h-4 text-white/40"
              fill="none"
              stroke="currentColor"
              viewBox="0 0 24 24"
            >
              <path strokeLinecap="round" strokeLinejoin="round" strokeWidth={2} d="M21 21l-6-6m2-5a7 7 0 11-14 0 7 7 0 0114 0z" />
            </svg>
          </div>
          
          <input
            ref={inputRef}
            type="text"
            className="w-full h-9 pl-9 bg-white/5 border border-white/10 rounded-lg text-white placeholder:text-white/40 text-sm focus:outline-none focus:ring-2 focus:ring-sky-500/50 focus:border-sky-500/50 transition-all"
            style={{ paddingRight: `${calculateInputPadding()}px` }}
            placeholder={hasActiveFilters ? "Search..." : getDynamicPlaceholder()}
            value={inputValue}
            onChange={(e) => setInputValue(e.target.value)}
            onKeyDown={handleSearch}
            onFocus={handleInputFocus}
            onBlur={handleInputBlur}
            disabled={disabled || isSearching}
          />

          <div className="absolute inset-y-0 right-0 flex items-center pr-2 gap-2">
            {hasActiveFilters && (
              <div className="flex items-center gap-1.5">
                {activeFilters.map((type) => (
                  <button
                    key={type}
                    onClick={(e) => handleSearchTypeClick(type, e)}
                    className="inline-flex items-center gap-1 px-2 py-0.5 rounded-full text-xs font-medium transition-all bg-sky-500/30 border border-sky-400/50 text-white hover:bg-sky-500/40 shrink-0"
                    disabled={disabled}
                    type="button"
                  >
                    <div className="text-white">
                      {getSearchTypeIcon(type)}
                    </div>
                    <span>{getSearchTypeLabel(type)}</span>
                  </button>
                ))}
              </div>
            )}

            {isSearching && (
              <div className="animate-spin rounded-full h-3.5 w-3.5 border-2 border-white/20 border-t-sky-500 shrink-0"></div>
            )}
            {inputValue && !isSearching && (
              <button
                onClick={handleClearSearch}
                className="text-white/40 hover:text-white transition-colors p-0.5 shrink-0"
                disabled={disabled}
                type="button"
              >
                <svg className="w-3.5 h-3.5" fill="none" stroke="currentColor" viewBox="0 0 24 24">
                  <path strokeLinecap="round" strokeLinejoin="round" strokeWidth={2} d="M6 18L18 6M6 6l12 12" />
                </svg>
              </button>
            )}
          </div>
        </div>

        {showDropdown && (
          <div
            ref={dropdownRef}
            className="absolute top-full left-0 right-0 mt-2 bg-black/95 backdrop-blur-sm border border-white/10 rounded-lg shadow-xl z-50 overflow-hidden"
          >
            <div className="p-2">
              <div className="text-xs font-medium text-white/60 px-2 py-1 mb-1">Filter by type</div>
              <div className="space-y-1">
                {(['models', 'collections', 'users'] as SearchType[]).map((type) => (
                  <button
                    key={type}
                    onClick={() => handleQuickFilter(type)}
                    className={`w-full flex items-center gap-2 px-3 py-2 rounded-md text-sm transition-colors ${
                      selectedSearchTypes.includes(type)
                        ? 'bg-sky-500/20 text-sky-400'
                        : 'text-white/70 hover:bg-white/5 hover:text-white'
                    }`}
                    disabled={disabled}
                    type="button"
                  >
                    <div className={`${selectedSearchTypes.includes(type) ? 'text-sky-400' : 'text-white/50'}`}>
                      {getSearchTypeIcon(type)}
                    </div>
                    <span>{getSearchTypeLabel(type)}</span>
                    {selectedSearchTypes.includes(type) && (
                      <svg className="w-4 h-4 ml-auto text-sky-400" fill="none" stroke="currentColor" viewBox="0 0 24 24">
                        <path strokeLinecap="round" strokeLinejoin="round" strokeWidth={2} d="M5 13l4 4L19 7" />
                      </svg>
                    )}
                  </button>
                ))}
              </div>
            </div>
          </div>
        )}
      </div>
    </div>
  );
};

export default SearchBar;
