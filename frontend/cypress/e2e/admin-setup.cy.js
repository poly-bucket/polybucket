describe('Admin Setup Flow', () => {
  beforeEach(() => {
    // Clear database and restart containers before each test
    // This ensures we start with a fresh state
    cy.exec('docker-compose down -v && docker-compose up -d', { timeout: 60000 });
    
    // Wait for containers to be ready
    cy.wait(10000);
    
    // Visit the application
    cy.visit('/');
  });

  it('should show the choice screen on first visit', () => {
    // Should show the welcome screen with two options
    cy.get('h2').should('contain', 'Welcome to PolyBucket');
    cy.get('p').should('contain', 'Choose how you\'d like to get started');
    
    // Should have two main options
    cy.get('button').should('contain', 'Continue with Default Admin');
    cy.get('button').should('contain', 'Create Custom Admin Account');
    
    // Should show default admin credentials
    cy.get('p').should('contain', 'Default admin credentials: admin@polybucket.com / admin');
  });

  it('should successfully use default admin account', () => {
    // Click on "Continue with Default Admin"
    cy.get('button').contains('Continue with Default Admin').click();
    
    // Should redirect to dashboard after successful login
    cy.url().should('include', '/dashboard');
    
    // Should be logged in as admin
    cy.get('[data-testid="user-menu"]').should('exist');
    // or check for admin-specific elements
    cy.get('body').should('contain', 'Dashboard');
  });

  it('should allow creating custom admin account', () => {
    // Click on "Create Custom Admin Account"
    cy.get('button').contains('Create Custom Admin Account').click();
    
    // Should show the admin setup form
    cy.get('h1').should('contain', 'Welcome to PolyBucket');
    cy.get('p').should('contain', 'Let\'s set up your admin account');
    
    // Fill out the form
    cy.get('input[name="username"]').type('customadmin');
    cy.get('input[name="email"]').type('custom@example.com');
    cy.get('input[name="firstName"]').type('Custom');
    cy.get('input[name="lastName"]').type('Admin');
    cy.get('input[name="password"]').type('SecurePassword123!');
    cy.get('input[name="confirmPassword"]').type('SecurePassword123!');
    
    // Submit the form
    cy.get('button[type="submit"]').click();
    
    // Should redirect to role setup after successful creation
    cy.url().should('include', '/custom-role-setup');
  });

  it('should handle validation errors in custom admin form', () => {
    // Click on "Create Custom Admin Account"
    cy.get('button').contains('Create Custom Admin Account').click();
    
    // Try to submit without filling required fields
    cy.get('button[type="submit"]').click();
    
    // Should show validation errors
    cy.get('p.text-red-600').should('exist');
  });

  it('should handle password mismatch in custom admin form', () => {
    // Click on "Create Custom Admin Account"
    cy.get('button').contains('Create Custom Admin Account').click();
    
    // Fill out the form with mismatched passwords
    cy.get('input[name="username"]').type('testadmin');
    cy.get('input[name="email"]').type('test@example.com');
    cy.get('input[name="password"]').type('Password123!');
    cy.get('input[name="confirmPassword"]').type('DifferentPassword123!');
    
    // Submit the form
    cy.get('button[type="submit"]').click();
    
    // Should show password mismatch error
    cy.get('.bg-red-100').should('contain', 'Passwords do not match');
  });

  it('should handle network errors gracefully', () => {
    // Mock a network error
    cy.intercept('POST', '**/api/system-settings/admin-setup', {
      statusCode: 500,
      body: { message: 'Internal server error' }
    }).as('adminSetupError');
    
    // Click on "Create Custom Admin Account"
    cy.get('button').contains('Create Custom Admin Account').click();
    
    // Fill out the form
    cy.get('input[name="username"]').type('erroradmin');
    cy.get('input[name="email"]').type('error@example.com');
    cy.get('input[name="password"]').type('Password123!');
    cy.get('input[name="confirmPassword"]').type('Password123!');
    
    // Submit the form
    cy.get('button[type="submit"]').click();
    
    // Should show error message
    cy.get('.bg-red-100').should('contain', 'Failed to create admin account');
  });

  it('should not show choice screen after admin is configured', () => {
    // First, set up admin using default account
    cy.get('button').contains('Continue with Default Admin').click();
    cy.url().should('include', '/dashboard');
    
    // Clear localStorage and visit again
    cy.clearLocalStorage();
    cy.visit('/');
    
    // Should not show choice screen, should redirect to login or dashboard
    cy.get('h2').should('not.contain', 'Welcome to PolyBucket');
    cy.get('button').should('not.contain', 'Continue with Default Admin');
  });
}); 