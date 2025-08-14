import React from 'react';
import { render, screen, fireEvent, waitFor } from '@testing-library/react';
import '@testing-library/jest-dom';
import IconSelector from '../IconSelector';

// Mock FontAwesome
jest.mock('@fortawesome/react-fontawesome', () => ({
  FontAwesomeIcon: ({ icon, className, ...props }: any) => (
    <span data-testid="fontawesome-icon" className={className} {...props}>
      {icon ? 'icon' : 'no-icon'}
    </span>
  ),
}));

// Mock minidenticons
jest.mock('minidenticons', () => ({
  minidenticon: jest.fn(() => '<svg>test</svg>'),
}));

describe('IconSelector', () => {
  const mockOnIconSelect = jest.fn();

  beforeEach(() => {
    jest.clearAllMocks();
  });

  it('renders without crashing', () => {
    render(
      <IconSelector
        selectedIcon=""
        onIconSelect={mockOnIconSelect}
        collectionName="Test Collection"
        collectionId="test-id"
      />
    );

    expect(screen.getByText('Collection Icon')).toBeInTheDocument();
    expect(screen.getByText('Choose an icon for your collection')).toBeInTheDocument();
  });

  it('shows icon picker when button is clicked', async () => {
    render(
      <IconSelector
        selectedIcon=""
        onIconSelect={mockOnIconSelect}
        collectionName="Test Collection"
        collectionId="test-id"
      />
    );

    const button = screen.getByText('Choose an icon for your collection');
    fireEvent.click(button);

    await waitFor(() => {
      expect(screen.getByText('Choose Collection Icon')).toBeInTheDocument();
    });
  });

  it('displays selected icon when one is provided', () => {
    render(
      <IconSelector
        selectedIcon="solid:star"
        onIconSelect={mockOnIconSelect}
        collectionName="Test Collection"
        collectionId="test-id"
      />
    );

    expect(screen.getByText('Change Icon')).toBeInTheDocument();
  });

  it('calls onIconSelect when icon is selected', async () => {
    render(
      <IconSelector
        selectedIcon=""
        onIconSelect={mockOnIconSelect}
        collectionName="Test Collection"
        collectionId="test-id"
      />
    );

    const button = screen.getByText('Choose an icon for your collection');
    fireEvent.click(button);

    await waitFor(() => {
      expect(screen.getByText('Choose Collection Icon')).toBeInTheDocument();
    });

    // Test generated avatar tab
    const generatedTab = screen.getByText('Generate Avatar');
    fireEvent.click(generatedTab);

    const useButton = screen.getByText('Use This Avatar');
    fireEvent.click(useButton);

    expect(mockOnIconSelect).toHaveBeenCalledWith(expect.any(String), 'generated');
  });

  it('allows clearing selected icon', () => {
    render(
      <IconSelector
        selectedIcon="solid:star"
        onIconSelect={mockOnIconSelect}
        collectionName="Test Collection"
        collectionId="test-id"
      />
    );

    const clearButton = screen.getByRole('button', { name: /clear/i });
    fireEvent.click(clearButton);

    expect(mockOnIconSelect).toHaveBeenCalledWith('', 'fontawesome');
  });
});
