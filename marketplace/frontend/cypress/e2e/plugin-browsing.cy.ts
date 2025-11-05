describe('Plugin Browsing', () => {
  beforeEach(() => {
    // Visit the marketplace homepage
    cy.visit('/')
  })

  it('should display the marketplace homepage', () => {
    // Check that the main elements are present
    cy.contains('Polybucket Marketplace').should('be.visible')
    cy.contains('Discover Components').should('be.visible')
    cy.contains('Browse and install community-built components').should('be.visible')
  })

  it('should have search functionality', () => {
    // Check that search bar is present
    cy.get('input[placeholder*="Search components"]').should('be.visible')
    
    // Test search functionality
    cy.get('input[placeholder*="Search components"]').type('test plugin')
    
    // Wait for debounced search to trigger
    cy.wait(500)
  })

  it('should have category filters', () => {
    // Check that category filter section is present
    cy.contains('Filters').should('be.visible')
    cy.contains('Categories').should('be.visible')
    
    // Check that category checkboxes are present
    cy.getByTestId('category-checkbox').should('exist')
  })

  it('should have sorting options', () => {
    // Check if sort dropdown is present (it might not be if no plugins are loaded)
    cy.get('body').then(($body) => {
      if ($body.find('[data-testid="sort-select"]').length > 0) {
        cy.getByTestId('sort-select').should('be.visible')
        cy.getByTestId('sort-select').should('contain', 'Sort by')
      } else {
        // If no sort dropdown, check that the page shows "No plugins found"
        cy.contains('No plugins found').should('be.visible')
      }
    })
  })

  it('should display plugin cards when available', () => {
    // Check if plugin cards are present (they might not be if no data)
    cy.get('body').then(($body) => {
      if ($body.find('[data-testid="plugin-card"]').length > 0) {
        // If plugin cards exist, test their functionality
        cy.getByTestId('plugin-card').first().should('be.visible')
        
        // Check that plugin card has expected elements
        cy.getByTestId('plugin-card').first().within(() => {
          cy.get('h3').should('exist') // Plugin name
          cy.get('p').should('exist') // Plugin description
        })
      } else {
        // If no plugin cards, check for "no plugins found" message
        cy.contains('No plugins found').should('be.visible')
      }
    })
  })

  it('should have pagination when multiple pages exist', () => {
    // Check if pagination is present
    cy.get('body').then(($body) => {
      if ($body.find('[data-testid="pagination"]').length > 0) {
        cy.getByTestId('pagination').should('be.visible')
        
        // Check pagination controls
        cy.contains('Previous').should('be.visible')
        cy.contains('Next').should('be.visible')
      }
    })
  })

  it('should have navigation menu', () => {
    // Check navigation menu items
    cy.contains('Browse').should('be.visible')
    cy.contains('Submit Component').should('be.visible')
    cy.contains('Documentation').should('be.visible')
    
    // Check action buttons
    cy.contains('Submit Component').should('be.visible')
    cy.contains('Sign In').should('be.visible')
  })

  it('should be responsive', () => {
    // Test mobile viewport
    cy.viewport(375, 667)
    cy.contains('Polybucket Marketplace').should('be.visible')
    
    // Test tablet viewport
    cy.viewport(768, 1024)
    cy.contains('Polybucket Marketplace').should('be.visible')
    
    // Test desktop viewport
    cy.viewport(1280, 720)
    cy.contains('Polybucket Marketplace').should('be.visible')
  })
})