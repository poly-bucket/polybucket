import React from 'react';
import { render, fireEvent, screen } from '@testing-library/react';
import '@testing-library/jest-dom';
import ModelCard from '../ModelCard';
import { AuthProvider } from '../../context/AuthContext';
import { UserSettingsProvider } from '../../context/UserSettingsContext';

// Mock the ExtendedModel type
const mockModel = {
  id: '1',
  name: 'Test Model',
  description: 'A test model for testing',
  thumbnailUrl: 'https://example.com/thumbnail.jpg',
  author: {
    id: '1',
    username: 'testuser',
    profilePictureUrl: 'https://example.com/avatar.jpg'
  },
  likes: [],
  comments: [],
  wip: false,
  aiGenerated: false,
  nsfw: false
};

const renderWithProviders = (component: React.ReactElement) => {
  return render(
    <AuthProvider>
      <UserSettingsProvider>
        {component}
      </UserSettingsProvider>
    </AuthProvider>
  );
};

describe('ModelCard', () => {
  it('renders model information correctly', () => {
    renderWithProviders(<ModelCard model={mockModel} />);
    
    expect(screen.getByText('Test Model')).toBeInTheDocument();
    expect(screen.getByText('by testuser')).toBeInTheDocument();
    expect(screen.getByTestId('model-card')).toBeInTheDocument();
  });

  it('is draggable', () => {
    renderWithProviders(<ModelCard model={mockModel} />);
    
    const modelCard = screen.getByTestId('model-card');
    expect(modelCard).toHaveAttribute('draggable', 'true');
  });

  it('handles drag start correctly', () => {
    renderWithProviders(<ModelCard model={mockModel} />);
    
    const modelCard = screen.getByTestId('model-card');
    const dragStartEvent = new DragEvent('dragstart');
    
    fireEvent.dragStart(modelCard);
    
    // The element should have the drag-start class
    expect(modelCard).toHaveClass('drag-start');
  });

  it('handles drag end correctly', () => {
    renderWithProviders(<ModelCard model={mockModel} />);
    
    const modelCard = screen.getByTestId('model-card');
    
    // Start drag
    fireEvent.dragStart(modelCard);
    expect(modelCard).toHaveClass('drag-start');
    
    // End drag
    fireEvent.dragEnd(modelCard);
    expect(modelCard).toHaveClass('drag-end');
  });

  it('calls onClick when clicked', () => {
    const mockOnClick = jest.fn();
    renderWithProviders(<ModelCard model={mockModel} onClick={mockOnClick} />);
    
    const modelCard = screen.getByTestId('model-card');
    fireEvent.click(modelCard);
    
    expect(mockOnClick).toHaveBeenCalledWith(mockModel);
  });

  it('shows drag handle on hover', () => {
    renderWithProviders(<ModelCard model={mockModel} />);
    
    const modelCard = screen.getByTestId('model-card');
    fireEvent.mouseEnter(modelCard);
    
    // The drag handle should be visible (opacity-100)
    const dragHandle = modelCard.querySelector('[class*="opacity-100"]');
    expect(dragHandle).toBeInTheDocument();
  });

  it('displays correct model stats', () => {
    const modelWithStats = {
      ...mockModel,
      likes: [{ id: '1' }, { id: '2' }],
      comments: [{ id: '1' }]
    };
    
    renderWithProviders(<ModelCard model={modelWithStats} />);
    
    expect(screen.getByText('2')).toBeInTheDocument(); // Likes count
    expect(screen.getByText('1')).toBeInTheDocument(); // Comments count
  });

  it('shows status badges when applicable', () => {
    const modelWithStatus = {
      ...mockModel,
      wip: true,
      aiGenerated: true,
      nsfw: true
    };
    
    renderWithProviders(<ModelCard model={modelWithStatus} />);
    
    expect(screen.getByText('WIP')).toBeInTheDocument();
    expect(screen.getByText('AI')).toBeInTheDocument();
    expect(screen.getByText('NSFW')).toBeInTheDocument();
  });
});
