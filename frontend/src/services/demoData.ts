import { ExtendedModel } from './modelsService';

// Demo users - simplified for demo purposes
const demoUsers = [
  { id: '1', username: 'ModelMaster3D', email: 'master@example.com' } as any,
  { id: '2', username: 'CreativeDesigner', email: 'creative@example.com' } as any,
  { id: '3', username: 'TechBuilder', email: 'tech@example.com' } as any,
  { id: '4', username: 'ArtisticPrinter', email: 'artist@example.com' } as any,
  { id: '5', username: 'FunctionalMaker', email: 'functional@example.com' } as any,
  { id: '6', username: 'GameAssetPro', email: 'games@example.com' } as any,
  { id: '7', username: 'ArchViz3D', email: 'arch@example.com' } as any,
  { id: '8', username: 'MiniatureCrafter', email: 'mini@example.com' } as any,
];

// Demo model data
export const demoModels = [
  {
    id: '1',
    name: 'Articulated Dragon',
    description: 'A beautifully detailed articulated dragon that prints without supports. Perfect for display or as a fidget toy.',
    author: demoUsers[0],
    authorId: '1',
    thumbnailUrl: 'https://images.unsplash.com/photo-1578662996442-48f60103fc96?w=400&h=225&fit=crop',
    downloads: 15420,
    likes: [{ id: '1' }, { id: '2' }, { id: '3' }],
    comments: [{ id: '1' }, { id: '2' }],
    wip: false,
    nsfw: false,
    aiGenerated: false,
    isPublic: true,
    isFeatured: true,
    isRemix: false,
    privacy: 1, // Public
    license: 1, // MIT
    createdAt: '2024-01-15',
    categories: [0], // Art
    tags: [{ name: 'dragon' }, { name: 'articulated' }, { name: 'fidget' }],
    versions: [{ 
      id: '1', 
      name: 'Articulated Dragon v1.0', 
      version: 1,
      billOfMaterials: [
        { id: 'bom1', name: 'PLA Filament (1kg)', quantity: 1, price: 25.99, url: 'https://example.com/pla', required: true, notes: 'Any color' },
        { id: 'bom2', name: 'Support Material (Optional)', quantity: 1, price: 15.99, url: '#', required: false, notes: 'Only if printer requires supports' }
      ],
      printSettings: {
        id: '1',
        layerHeight: '0.2mm',
        infill: '15%',
        supports: false,
        printSpeed: '50mm/s',
        nozzleTemp: '210°C',
        bedTemp: '60°C',
        material: 'PLA',
        nozzleSize: '0.4mm',
        retraction: '6mm',
        buildPlateAdhesion: 'skirt',
        notes: 'Print-in-place design, no assembly required. Articulated joints may need light sanding if too tight.'
      }
    }],
  },
  {
    id: '2',
    name: 'Modular Desk Organizer',
    description: 'Customizable desk organizer with interlocking modules. Print as many pieces as you need!',
    author: demoUsers[1],
    authorId: '2',
    thumbnailUrl: 'https://images.unsplash.com/photo-1586023492125-27b2c045efd7?w=400&h=225&fit=crop',
    downloads: 8930,
    likes: [{ id: '1' }, { id: '2' }, { id: '3' }, { id: '4' }, { id: '5' }],
    comments: [{ id: '1' }, { id: '2' }, { id: '3' }],
    wip: false,
    nsfw: false,
    aiGenerated: false,
    isPublic: true,
    isFeatured: false,
    isRemix: false,
    privacy: 1, // Public
    license: 1, // MIT
    createdAt: new Date('2024-01-20').toISOString(),
    categories: [11], // Organization
    tags: [{ name: 'organizer' }, { name: 'modular' }, { name: 'desk' }, { name: 'office' }],
    versions: [{ 
      id: '2', 
      name: 'Modular Desk Organizer v1.0', 
      version: 1,
      billOfMaterials: [
        { id: 'bom3', name: 'PETG Filament (1kg)', quantity: 1, price: 29.99, url: 'https://example.com/petg', required: true, notes: 'Durable material recommended' },
        { id: 'bom4', name:'Velcro Strips (Optional)', quantity: 1, price: 8.99, url: '#', required: false, notes: 'For securing to desk surface' }
      ],
      printSettings: {
        id: '2',
        layerHeight: '0.3mm',
        infill: '20%',
        supports: false,
        printSpeed: '45mm/s',
        nozzleTemp: '230°C',
        bedTemp: '80°C',
        material: 'PETG',
        nozzleSize: '0.4mm',
        retraction: '6.5mm',
        buildPlateAdhesion: 'brim',
        notes: 'Print multiple units as needed. Each module is stackable and interlocks with others.'
      }
    }],
  },
  {
    id: '3',
    name: 'Cyberpunk Helmet - WIP',
    description: 'Futuristic helmet design inspired by cyberpunk aesthetics. Still working on the visor attachment mechanism.',
    author: demoUsers[2],
    authorId: '3',
    thumbnailUrl: 'https://images.unsplash.com/photo-1592478411213-6153e4ebc696?w=400&h=225&fit=crop',
    downloads: 2150,
    likes: [{ id: '1' }, { id: '2' }],
    comments: [{ id: '1' }],
    wip: true,
    nsfw: false,
    aiGenerated: false,
    isPublic: true,
    isFeatured: false,
    isRemix: false,
    privacy: 1, // Public
    license: 4, // CC BY 4.0
    createdAt: new Date('2024-01-25').toISOString(),
    categories: [33], // Clothing
    tags: [{ name: 'cyberpunk' }, { name: 'helmet' }, { name: 'futuristic' }, { name: 'wip' }],
    versions: [{ 
      id: '3', 
      name: 'Cyberpunk Helmet v0.8', 
      version: 1,
      billOfMaterials: [
        { id: 'bom9', name: 'PETG Filament (1kg)', quantity: 2, price: 29.99, url: '#', required: true, notes: 'Strong material for wearable item' }
      ],
      printSettings: {
        id: '3',
        layerHeight: '0.2mm',
        infill: '25%',
        supports: true,
        printSpeed: '40mm/s',
        nozzleTemp: '230°C',
        bedTemp: '80°C',
        material: 'PETG',
        nozzleSize: '0.4mm',
        retraction: '6.5mm',
        buildPlateAdhesion: 'brim',
        notes: 'Work in progress - visor attachment mechanism still being refined.'
      }
    }],
  },
  {
    id: '4',
    name: 'AI Generated Vase Collection',
    description: 'A set of 12 unique vases created using AI generation and refined for 3D printing. Modern geometric designs.',
    author: demoUsers[3],
    authorId: '4',
    thumbnailUrl: 'https://images.unsplash.com/photo-1578749556568-bc2c40e68b61?w=400&h=225&fit=crop',
    downloads: 5670,
    likes: [{ id: '1' }, { id: '2' }, { id: '3' }, { id: '4' }],
    comments: [{ id: '1' }, { id: '2' }, { id: '3' }, { id: '4' }],
    wip: false,
    nsfw: false,
    aiGenerated: true,
    isPublic: true,
    isFeatured: true,
    isRemix: false,
    privacy: 1, // Public
    license: 5, // CC BY-SA 4.0
    createdAt: new Date('2024-01-18').toISOString(),
    categories: [0], // Art
    tags: [{ name: 'vase' }, { name: 'ai-generated' }, { name: 'geometric' }, { name: 'modern' }],
    versions: [{ 
      id: '4', 
      name: 'AI Vase Collection v1.0', 
      version: 1,
      billOfMaterials: [
        { id: 'bom10', name: 'PLA Filament (1kg)', quantity: 1, price: 25.99, url: '#', required: true, notes: 'Multiple colors available' }
      ],
      printSettings: {
        id: '4',
        layerHeight: '0.2mm',
        infill: '10%',
        supports: false,
        printSpeed: '60mm/s',
        nozzleTemp: '210°C',
        bedTemp: '60°C',
        material: 'PLA',
        nozzleSize: '0.4mm',
        retraction: '6mm',
        buildPlateAdhesion: 'skirt',
        notes: 'Decorative vases - not watertight. 12 different designs included in collection.'
      }
    }],
  },
  {
    id: '5',
    name: 'Functional Tool Holder',
    description: 'Wall-mounted tool holder for screwdrivers, pliers, and other workshop tools. Includes mounting hardware.',
    author: demoUsers[4],
    authorId: '5',
    thumbnailUrl: 'https://images.unsplash.com/photo-1567016432779-094069958ea5?w=400&h=225&fit=crop',
    downloads: 12340,
    likes: [{ id: '1' }, { id: '2' }, { id: '3' }, { id: '4' }, { id: '5' }, { id: '6' }],
    comments: [{ id: '1' }, { id: '2' }],
    wip: false,
    nsfw: false,
    aiGenerated: false,
    isPublic: true,
    isFeatured: false,
    isRemix: false,
    privacy: 1, // Public
    license: 1, // MIT
    createdAt: new Date('2024-01-10').toISOString(),
    categories: [5], // Tools
    tags: [{ name: 'tools' }, { name: 'organizer' }, { name: 'workshop' }, { name: 'functional' }],
    versions: [{ 
      id: '5', 
      name: 'Tool Holder v1.0', 
      version: 1,
      billOfMaterials: [
        { id: 'bom11', name: 'ABS Filament (1kg)', quantity: 1, price: 27.99, url: '#', required: true, notes: 'Durable material for tools' },
        { id: 'bom12', name: 'Wall Anchors', quantity: 4, price: 3.99, url: '#', required: true, notes: 'For mounting to wall' }
      ],
      printSettings: {
        id: '5',
        layerHeight: '0.3mm',
        infill: '30%',
        supports: false,
        printSpeed: '45mm/s',
        nozzleTemp: '250°C',
        bedTemp: '100°C',
        material: 'ABS',
        nozzleSize: '0.4mm',
        retraction: '6mm',
        buildPlateAdhesion: 'brim',
        notes: 'Functional tool holder designed for workshop use. ABS provides durability.'
      }
    }],
  },
  {
    id: '6',
    name: 'Fantasy Miniature - Orc Warrior',
    description: 'Detailed miniature figure for tabletop gaming. 28mm scale with removable weapons and base.',
    author: demoUsers[7],
    authorId: '8',
    thumbnailUrl: 'https://images.unsplash.com/photo-1612198188060-c7c2a3b66eae?w=400&h=225&fit=crop',
    downloads: 7820,
    likes: [{ id: '1' }, { id: '2' }, { id: '3' }],
    comments: [{ id: '1' }, { id: '2' }, { id: '3' }, { id: '4' }, { id: '5' }],
    wip: false,
    nsfw: false,
    aiGenerated: false,
    isPublic: true,
    isFeatured: true,
    isRemix: false,
    privacy: 1, // Public
    license: 4, // CC BY 4.0
    createdAt: new Date('2024-01-22').toISOString(),
    categories: [3], // Games
    tags: [{ name: 'miniature' }, { name: 'fantasy' }, { name: 'orc' }, { name: 'gaming' }, { name: '28mm' }],
    versions: [{ 
      id: '6', 
      name: 'Orc Warrior v1.0', 
      version: 1,
      billOfMaterials: [
        { id: 'bom5', name: 'Resin (500ml)', quantity: 1, price: 45.99, url: 'https://example.com/resin', required: true, notes: '8K resin recommended for detail' },
        { id: 'bom6', name: 'Acrylic Paints Set', quantity: 1, price: 24.99, url: '#', required: false, notes: 'For painting the finished miniature' },
        { id: 'bom7', name: 'Primer Spray', quantity: 1, price: 12.99, url: '#', required: false, notes: 'Black or white primer' },
        { id: 'bom8', name: '28mm Base', quantity: 1, price: 1.99, url: '#', required: true, notes: 'Standard gaming base' }
      ],
      printSettings: {
        id: '6',
        layerHeight: '0.05mm',
        infill: '100%',
        supports: true,
        printSpeed: '25mm/s',
        nozzleTemp: 'N/A',
        bedTemp: 'N/A',
        material: '8K Resin',
        nozzleSize: 'N/A',
        retraction: 'N/A',
        buildPlateAdhesion: 'raft',
        notes: 'Resin printer required. Use heavy supports for overhangs. Post-processing: wash in IPA, cure under UV light. Sand support contact points carefully.'
      }
    }],
  },
  {
    id: '7',
    name: 'Phone Stand with Cable Management',
    description: 'Adjustable phone stand with built-in cable routing. Compatible with most smartphones and cases.',
    author: demoUsers[1],
    authorId: '2',
    thumbnailUrl: 'https://images.unsplash.com/photo-1556656793-08538906a9f8?w=400&h=225&fit=crop',
    downloads: 18950,
    likes: [{ id: '1' }, { id: '2' }, { id: '3' }, { id: '4' }, { id: '5' }, { id: '6' }, { id: '7' }, { id: '8' }],
    comments: [{ id: '1' }, { id: '2' }, { id: '3' }],
    wip: false,
    nsfw: false,
    aiGenerated: false,
    isPublic: true,
    isFeatured: false,
    isRemix: false,
    privacy: 1, // Public
    license: 1, // MIT
    createdAt: new Date('2024-01-05').toISOString(),
    categories: [6], // Gadget
    tags: [{ name: 'phone' }, { name: 'stand' }, { name: 'cable' }, { name: 'management' }, { name: 'gadget' }],
    versions: [{ 
      id: '7', 
      name: 'Phone Stand v1.0', 
      version: 1,
      billOfMaterials: [{ id: 'bom13', name: 'PLA Filament (1kg)', quantity: 1, price: 25.99, url: '#', required: true }],
      printSettings: { id: '7', layerHeight: '0.2mm', infill: '20%', supports: false, printSpeed: '50mm/s', nozzleTemp: '210°C', bedTemp: '60°C', material: 'PLA', nozzleSize: '0.4mm', retraction: '6mm', buildPlateAdhesion: 'skirt' }
    }],
  },
  {
    id: '8',
    name: 'Architectural Model - Modern House',
    description: 'Scale model of a contemporary house design. Great for architecture students and presentation purposes.',
    author: demoUsers[6],
    authorId: '7',
    thumbnailUrl: 'https://images.unsplash.com/photo-1600585154340-be6161a56a0c?w=400&h=225&fit=crop',
    downloads: 3420,
    likes: [{ id: '1' }, { id: '2' }],
    comments: [{ id: '1' }],
    wip: false,
    nsfw: false,
    aiGenerated: false,
    isPublic: true,
    isFeatured: false,
    isRemix: false,
    privacy: 1, // Public
    license: 4, // CC BY 4.0
    createdAt: new Date('2024-01-28').toISOString(),
    categories: [25], // Architecture
    tags: [{ name: 'architecture' }, { name: 'house' }, { name: 'modern' }, { name: 'scale-model' }],
    versions: [{ 
      id: '8', 
      name: 'Modern House v1.0', 
      version: 1,
      billOfMaterials: [{ id: 'bom14', name: 'PLA Filament (1kg)', quantity: 1, price: 25.99, url: '#', required: true }],
      printSettings: { id: '8', layerHeight: '0.1mm', infill: '15%', supports: true, printSpeed: '30mm/s', nozzleTemp: '210°C', bedTemp: '60°C', material: 'PLA', nozzleSize: '0.4mm', retraction: '6mm', buildPlateAdhesion: 'brim' }
    }],
  },
  {
    id: '9',
    name: 'Custom Keycaps Set',
    description: 'Cherry MX compatible keycaps with unique designs. Includes 104 keys for full-size keyboards.',
    author: demoUsers[2],
    authorId: '3',
    thumbnailUrl: 'https://images.unsplash.com/photo-1541140134513-85a161dc4a00?w=400&h=225&fit=crop',
    downloads: 6780,
    likes: [{ id: '1' }, { id: '2' }, { id: '3' }, { id: '4' }, { id: '5' }],
    comments: [{ id: '1' }, { id: '2' }, { id: '3' }, { id: '4' }],
    wip: false,
    nsfw: false,
    aiGenerated: false,
    isPublic: true,
    isFeatured: false,
    isRemix: false,
    privacy: 1, // Public
    license: 3, // Apache 2.0
    createdAt: new Date('2024-01-12').toISOString(),
    categories: [16], // Electronics
    tags: [{ name: 'keycaps' }, { name: 'keyboard' }, { name: 'cherry-mx' }, { name: 'custom' }],
    versions: [{ 
      id: '9', 
      name: 'Keycaps Set v1.0', 
      version: 1,
      billOfMaterials: [{ id: 'bom15', name: 'ABS Filament (1kg)', quantity: 2, price: 27.99, url: '#', required: true }],
      printSettings: { id: '9', layerHeight: '0.2mm', infill: '100%', supports: false, printSpeed: '40mm/s', nozzleTemp: '250°C', bedTemp: '100°C', material: 'ABS', nozzleSize: '0.4mm', retraction: '6mm', buildPlateAdhesion: 'brim' }
    }],
  },
  {
    id: '10',
    name: 'Fidget Infinity Cube',
    description: 'Satisfying fidget toy that prints in place. No assembly required, just print and use!',
    author: demoUsers[0],
    authorId: '1',
    thumbnailUrl: 'https://images.unsplash.com/photo-1558618047-3c8c76ca7d13?w=400&h=225&fit=crop',
    downloads: 24680,
    likes: [{ id: '1' }, { id: '2' }, { id: '3' }, { id: '4' }, { id: '5' }, { id: '6' }, { id: '7' }, { id: '8' }, { id: '9' }, { id: '10' }],
    comments: [{ id: '1' }, { id: '2' }, { id: '3' }, { id: '4' }, { id: '5' }, { id: '6' }],
    wip: false,
    nsfw: false,
    aiGenerated: false,
    isPublic: true,
    isFeatured: true,
    isRemix: false,
    privacy: 1, // Public
    license: 1, // MIT
    createdAt: new Date('2024-01-08').toISOString(),
    categories: [4], // Toys
    tags: [{ name: 'fidget' }, { name: 'toy' }, { name: 'cube' }, { name: 'print-in-place' }],
    versions: [{ 
      id: '10', 
      name: 'Infinity Cube v1.0', 
      version: 1,
      billOfMaterials: [{ id: 'bom16', name: 'PLA Filament (1kg)', quantity: 1, price: 25.99, url: '#', required: true }],
      printSettings: { id: '10', layerHeight: '0.2mm', infill: '15%', supports: false, printSpeed: '50mm/s', nozzleTemp: '210°C', bedTemp: '60°C', material: 'PLA', nozzleSize: '0.4mm', retraction: '6mm', buildPlateAdhesion: 'skirt' }
    }],
  },
  {
    id: '11',
    name: 'Succulent Planter Collection',
    description: 'Set of 6 geometric planters perfect for small succulents. Includes drainage holes and saucers.',
    author: demoUsers[3],
    authorId: '4',
    thumbnailUrl: 'https://images.unsplash.com/photo-1416879595882-3373a0480b5b?w=400&h=225&fit=crop',
    downloads: 9150,
    likes: [{ id: '1' }, { id: '2' }, { id: '3' }, { id: '4' }],
    comments: [{ id: '1' }, { id: '2' }],
    wip: false,
    nsfw: false,
    aiGenerated: false,
    isPublic: true,
    isFeatured: false,
    isRemix: false,
    privacy: 1, // Public
    license: 5, // CC BY-SA 4.0
    createdAt: new Date('2024-01-30').toISOString(),
    categories: [13], // Garden
    tags: [{ name: 'planter' }, { name: 'succulent' }, { name: 'geometric' }, { name: 'garden' }],
    versions: [{ 
      id: '11', 
      name: 'Planter Collection v1.0', 
      version: 1,
      billOfMaterials: [{ id: 'bom17', name: 'PETG Filament (1kg)', quantity: 1, price: 29.99, url: '#', required: true }],
      printSettings: { id: '11', layerHeight: '0.3mm', infill: '20%', supports: false, printSpeed: '45mm/s', nozzleTemp: '230°C', bedTemp: '80°C', material: 'PETG', nozzleSize: '0.4mm', retraction: '6.5mm', buildPlateAdhesion: 'brim' }
    }],
  },
  {
    id: '12',
    name: 'Gaming Dice Set - D&D',
    description: 'Complete 7-piece polyhedral dice set for tabletop RPGs. Balanced and tested for fair rolls.',
    author: demoUsers[5],
    authorId: '6',
    thumbnailUrl: 'https://images.unsplash.com/photo-1570303345338-e1f0eddf4946?w=400&h=225&fit=crop',
    downloads: 11230,
    likes: [{ id: '1' }, { id: '2' }, { id: '3' }, { id: '4' }, { id: '5' }, { id: '6' }, { id: '7' }],
    comments: [{ id: '1' }, { id: '2' }, { id: '3' }],
    wip: false,
    nsfw: false,
    aiGenerated: false,
    isPublic: true,
    isFeatured: false,
    isRemix: false,
    privacy: 1, // Public
    license: 4, // CC BY 4.0
    createdAt: new Date('2024-01-14').toISOString(),
    categories: [3], // Games
    tags: [{ name: 'dice' }, { name: 'gaming' }, { name: 'dnd' }, { name: 'tabletop' }, { name: 'rpg' }],
    versions: [{ 
      id: '12', 
      name: 'Dice Set v1.0', 
      version: 1,
      billOfMaterials: [{ id: 'bom18', name: 'Resin (500ml)', quantity: 1, price: 45.99, url: '#', required: true }],
      printSettings: { id: '12', layerHeight: '0.05mm', infill: '100%', supports: true, printSpeed: '25mm/s', nozzleTemp: 'N/A', bedTemp: 'N/A', material: '8K Resin', nozzleSize: 'N/A', retraction: 'N/A', buildPlateAdhesion: 'raft' }
    }],
  },
] as unknown as ExtendedModel[];

// Helper functions to filter demo data
export const getFeaturedDemoModels = (): ExtendedModel[] => {
  return (demoModels as ExtendedModel[]).filter(model => model.isFeatured);
};

export const getPopularDemoModels = (): ExtendedModel[] => {
  return [...(demoModels as ExtendedModel[])].sort((a, b) => (b.likes?.length || 0) - (a.likes?.length || 0));
};

export const getRecentDemoModels = (): ExtendedModel[] => {
  return [...(demoModels as ExtendedModel[])].sort((a, b) => 
    new Date(b.createdAt || 0).getTime() - new Date(a.createdAt || 0).getTime()
  );
};

export const searchDemoModels = (query: string): ExtendedModel[] => {
  if (!query.trim()) return demoModels as ExtendedModel[];
  
  const searchTerm = query.toLowerCase();
  return (demoModels as ExtendedModel[]).filter(model => 
    model.name?.toLowerCase().includes(searchTerm) ||
    model.description?.toLowerCase().includes(searchTerm) ||
    model.author?.username?.toLowerCase().includes(searchTerm)
  );
}; 