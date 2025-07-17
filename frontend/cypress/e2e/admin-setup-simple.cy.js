describe('Admin Setup Flow - Simple Tests', () => {
  beforeEach(() => {
    // Clear localStorage to simulate fresh state
    cy.clearLocalStorage();
    
    // Visit the application
    cy.visit('/');
  });

  it('should show choice screen and handle default admin selection', () => {
    // Should show the welcome screen
    cy.get('h2').should('contain', 'Welcome to PolyBucket');
    
    // Should have the choice options
    cy.get('button').should('contain', 'Continue with Default Admin');
    cy.get('button').should('contain', 'Create Custom Admin Account');
    
    // Click default admin option
    cy.get('button').contains('Continue with Default Admin').click();
    
    // Should redirect to dashboard
    cy.url().should('include', '/dashboard');
  });

  it('should show custom admin form when selected', () => {
    // Click custom admin option
    cy.get('button').contains('Create Custom Admin Account').click();
    
    // Should show the admin setup form
    cy.get('h1').should('contain', 'Welcome to PolyBucket');
    cy.get('p').should('contain', 'Let\'s set up your admin account');
    
    // Should have form fields
    cy.get('input[name="username"]').should('exist');
    cy.get('input[name="email"]').should('exist');
    cy.get('input[name="password"]').should('exist');
    cy.get('input[name="confirmPassword"]').should('exist');
  });

  it('should validate password confirmation', () => {
    // Go to custom admin form
    cy.get('button').contains('Create Custom Admin Account').click();
    
    // Fill form with mismatched passwords
    cy.get('input[name="username"]').type('testuser');
    cy.get('input[name="email"]').type('test@example.com');
    cy.get('input[name="password"]').type('password123');
    cy.get('input[name="confirmPassword"]').type('differentpassword');
    
    // Submit form
    cy.get('button[type="submit"]').click();
    
    // Should show password mismatch error
    cy.get('.bg-red-100').should('contain', 'Passwords do not match');
  });

  it('should show loading states', () => {
    // Should show loading spinner initially
    cy.get('.animate-spin').should('exist');
    
    // Should eventually show the choice screen
    cy.get('h2').should('contain', 'Welcome to PolyBucket');
  });
}); 