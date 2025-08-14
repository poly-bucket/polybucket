import React from 'react';
import { render, fireEvent, screen, waitFor } from '@testing-library/react';
import '@testing-library/jest-dom';
import { BrowserRouter } from 'react-router-dom';
import CollectionsBar from '../CollectionsBar';
import * as collectionsService from '../../../services/collectionsService';

// Mock the collections service
jest.mock('../../../services/collectionsService');
const mockedCollectionsService = collectionsService as jest.Mocked<typeof collectionsService>;

// Mock the collections data
const mockCollections = [
  {
    id: '1',
    name: 'Test Collection 1',
    visibility: 'Public' as const,
    ownerId: '1',
    collectionModels: []
  },
  {
    id: '2',
    name: 'Test Collection 2',
    visibility: 'Private' as const,
    ownerId: '1',
    collectionModels: [{ id: '1', modelId: '1', addedAt: '2024-01-01' }]
  }
];

const renderWithRouter = (component: React.ReactElement) => {
  return render(
    <BrowserRouter>
      {component}
    </BrowserRouter>
  );
};

describe('CollectionsBar', () => {
  beforeEach(() => {
    jest.clearAllMocks();
    mockedCollectionsService.getUserCollections.mockResolvedValue(mockCollections);
  });

  it('renders collections correctly', async () => {
    renderWithRouter(<CollectionsBar isCollapsed={false} onToggle={jest.fn()} />);
    
    await waitFor(() => {
      expect(screen.getByText('Test Collection 1')).toBeInTheDocument();
      expect(screen.getByText('Test Collection 2')).toBeInTheDocument();
    });
  });

  it('shows drag and drop instruction', async () => {
    renderWithRouter(<CollectionsBar isCollapsed={false} onToggle={false} />);
    
    await waitFor(() => {
      expect(screen.getByText('Drag models here to add them to collections')).toBeInTheDocument();
    });
  });

  it('handles drag over correctly', async () => {
    renderWithRouter(<CollectionsBar isCollapsed={false} onToggle={jest.fn()} />);
    
    await waitFor(() => {
      const collectionElement = screen.getByText('Test Collection 1').closest('div');
      expect(collectionElement).toBeInTheDocument();
      
      if (collectionElement) {
        const dragOverEvent = new DragEvent('dragover');
        Object.defineProperty(dragOverEvent, 'dataTransfer', {
          value: {
            getData: () => JSON.stringify({ type: 'model', modelId: '1', modelName: 'Test Model' })
          }
        });
        
        fireEvent.dragOver(collectionElement, dragOverEvent);
        
        // The collection should show drag over state
        expect(collectionElement).toHaveClass('drag-over');
      }
    });
  });

  it('handles drop correctly', async () => {
    mockedCollectionsService.addModelToCollection.mockResolvedValue();
    
    renderWithRouter(<CollectionsBar isCollapsed={false} onToggle={jest.fn()} />);
    
    await waitFor(() => {
      const collectionElement = screen.getByText('Test Collection 1').closest('div');
      expect(collectionElement).toBeInTheDocument();
      
      if (collectionElement) {
        const dropEvent = new DragEvent('drop');
        Object.defineProperty(dropEvent, 'dataTransfer', {
          value: {
            getData: () => JSON.stringify({ type: 'model', modelId: '1', modelName: 'Test Model' })
          }
        });
        
        fireEvent.drop(collectionElement, dropEvent);
        
        // Should call the service to add model to collection
        expect(mockedCollectionsService.addModelToCollection).toHaveBeenCalledWith('1', '1');
      }
    });
  });

  it('shows success feedback after successful drop', async () => {
    mockedCollectionsService.addModelToCollection.mockResolvedValue();
    
    renderWithRouter(<CollectionsBar isCollapsed={false} onToggle={jest.fn()} />);
    
    await waitFor(() => {
      const collectionElement = screen.getByText('Test Collection 1').closest('div');
      expect(collectionElement).toBeInTheDocument();
      
      if (collectionElement) {
        const dropEvent = new DragEvent('drop');
        Object.defineProperty(dropEvent, 'dataTransfer', {
          value: {
            getData: () => JSON.stringify({ type: 'model', modelId: '1', modelName: 'Test Model' })
          }
        });
        
        fireEvent.drop(collectionElement, dropEvent);
        
        // Should show success feedback
        expect(screen.getByText('Added!')).toBeInTheDocument();
      }
    });
  });

  it('shows error feedback after failed drop', async () => {
    mockedCollectionsService.addModelToCollection.mockRejectedValue(new Error('Failed to add model'));
    
    renderWithRouter(<CollectionsBar isCollapsed={false} onToggle={jest.fn()} />);
    
    await waitFor(() => {
      const collectionElement = screen.getByText('Test Collection 1').closest('div');
      expect(collectionElement).toBeInTheDocument();
      
      if (collectionElement) {
        const dropEvent = new DragEvent('drop');
        Object.defineProperty(dropEvent, 'dataTransfer', {
          value: {
            getData: () => JSON.stringify({ type: 'model', modelId: '1', modelName: 'Test Model' })
          }
        });
        
        fireEvent.drop(collectionElement, dropEvent);
        
        // Should show error feedback
        expect(screen.getByText('Failed')).toBeInTheDocument();
      }
    });
  });

  it('toggles between collapsed and expanded states', async () => {
    const mockOnToggle = jest.fn();
    renderWithRouter(<CollectionsBar isCollapsed={false} onToggle={mockOnToggle} />);
    
    await waitFor(() => {
      const toggleButton = screen.getByTitle('Collapse Collections');
      fireEvent.click(toggleButton);
      expect(mockOnToggle).toHaveBeenCalled();
    });
  });

  it('shows collapsed view correctly', async () => {
    renderWithRouter(<CollectionsBar isCollapsed={true} onToggle={jest.fn()} />);
    
    await waitFor(() => {
      // Should show expand button
      expect(screen.getByTitle('Expand Collections')).toBeInTheDocument();
      
      // Should not show collection names in collapsed view
      expect(screen.queryByText('Test Collection 1')).not.toBeInTheDocument();
    });
  });

  it('navigates to create collection page', async () => {
    renderWithRouter(<CollectionsBar isCollapsed={false} onToggle={jest.fn()} />);
    
    await waitFor(() => {
      const createButton = screen.getByText('New Collection');
      fireEvent.click(createButton);
      
      // Should navigate to create collection page
      expect(window.location.pathname).toBe('/my-collections/create');
    });
  });
});
