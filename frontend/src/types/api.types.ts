// Custom type definitions for missing types
export interface FileParameter {
  name: string;
  size: number;
  type: string;
  lastModified: number;
  slice: (start?: number, end?: number) => Blob;
  stream: () => ReadableStream;
  text: () => Promise<string>;
  arrayBuffer: () => Promise<ArrayBuffer>;
}

// Model categories enum
export enum ModelCategories {
  Art = 'Art',
  Fashion = 'Fashion',
  Technology = 'Technology',
  Games = 'Games',
  Toys = 'Toys',
  Tools = 'Tools',
  Gadget = 'Gadget',
  Survival = 'Survival',
  Learning = 'Learning',
  Office = 'Office',
  Ornament = 'Ornament',
  Organization = 'Organization',
  Outdoors = 'Outdoors',
  Garden = 'Garden',
  Home = 'Home',
  Kitchen = 'Kitchen',
  Electronics = 'Electronics',
  Automotive = 'Automotive',
  Sports = 'Sports',
  Music = 'Music',
  Other = 'Other',
  Firearms = 'Firearms',
  Drones = 'Drones',
  Jewelry = 'Jewelry',
  Furniture = 'Furniture',
  Architecture = 'Architecture',
  Medical = 'Medical',
  Science = 'Science',
  Space = 'Space',
  Animals = 'Animals',
  People = 'People',
  Nature = 'Nature',
  Transportation = 'Transportation',
  Clothing = 'Clothing',
  Accessories = 'Accessories',
  Footwear = 'Footwear',
  Bags = 'Bags',
  Headwear = 'Headwear',
  Eyewear = 'Eyewear',
  Hair = 'Hair',
  Makeup = 'Makeup',
  Skincare = 'Skincare',
  Fragrance = 'Fragrance',
  Health = 'Health',
  Fitness = 'Fitness',
  Nutrition = 'Nutrition',
  Wellness = 'Wellness',
  Travel = 'Travel',
  Adventure = 'Adventure',
  Culture = 'Culture',
  History = 'History',
  Religion = 'Religion',
  Philosophy = 'Philosophy',
  Psychology = 'Psychology',
  Sociology = 'Sociology',
  Anthropology = 'Anthropology',
  Economics = 'Economics',
  Politics = 'Politics',
  Law = 'Law',
  Education = 'Education',
  Literature = 'Literature'
}

// Re-export types from the generated API client
export type { LoginCommand, LoginCommandResponse } from '../services/api.client';
