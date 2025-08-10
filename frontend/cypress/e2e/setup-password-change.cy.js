/// <reference types="cypress" />

describe('Setup Process - Password Change Step', () => {
  beforeEach(() => {
    // Clear localStorage to simulate fresh state
    cy.clearLocalStorage();
    cy.clearCookies();
    
    // Visit the application
    cy.visit('/');
  });

  it('should complete password change step successfully', () => {
    // Wait for redirects to complete
    cy.wait(3000);
    
    // Should be redirected to login page
    cy.url().should('include', '/login');
    
    // Login as admin user
    cy.get('input[name="email"]').type('admin@polybucket.com');
    cy.get('input[name="password"]').type('admin123');
    cy.get('button').contains('Sign in').click();
    
    // Wait for login to complete and redirect to setup
    cy.wait(3000);
    cy.url().should('include', '/setup');
    
    // Should be on step 1 (password change)
    cy.get('h3').should('contain', 'Change Admin Password');
    
    // Fill in current password
    cy.get('input[name="currentPassword"]').type('admin123');
    
    // Fill in new password
    cy.get('input[name="newPassword"]').type('NewSecurePassword123!');
    
    // Fill in password confirmation
    cy.get('input[name="confirmPassword"]').type('NewSecurePassword123!');
    
    // Click continue
    cy.get('button').contains('Continue').click();
    
    // Should move to next step (2FA)
    cy.get('h3').should('contain', 'Two-Factor Authentication');
  });

  it('should skip password change step successfully', () => {
    // Wait for redirects to complete
    cy.wait(3000);
    
    // Should be redirected to login page
    cy.url().should('include', '/login');
    
    // Login as admin user
    cy.get('input[name="email"]').type('admin@polybucket.com');
    cy.get('input[name="password"]').type('admin123');
    cy.get('button').contains('Sign in').click();
    
    // Wait for login to complete and redirect to setup
    cy.wait(3000);
    cy.url().should('include', '/setup');
    
    // Should be on step 1 (password change)
    cy.get('h3').should('contain', 'Change Admin Password');
    
    // Click skip button
    cy.get('button').contains('Skip This Step').click();
    
    // Should move to next step (2FA)
    cy.get('h3').should('contain', 'Two-Factor Authentication');
  });

  it('should validate password requirements', () => {
    // Wait for redirects to complete
    cy.wait(3000);
    
    // Should be redirected to login page
    cy.url().should('include', '/login');
    
    // Login as admin user
    cy.get('input[name="email"]').type('admin@polybucket.com');
    cy.get('input[name="password"]').type('admin123');
    cy.get('button').contains('Sign in').click();
    
    // Wait for login to complete and redirect to setup
    cy.wait(3000);
    cy.url().should('include', '/setup');
    
    // Should be on step 1 (password change)
    cy.get('h3').should('contain', 'Change Admin Password');
    
    // Fill in current password
    cy.get('input[name="currentPassword"]').type('admin123');
    
    // Try to submit with weak password
    cy.get('input[name="newPassword"]').type('weak');
    cy.get('input[name="confirmPassword"]').type('weak');
    
    // Should show validation error
    cy.get('button').contains('Continue').click();
    cy.get('.error-message').should('contain', 'Password must be at least 8 characters');
  });

  it('should validate password confirmation match', () => {
    // Wait for redirects to complete
    cy.wait(3000);
    
    // Should be redirected to login page
    cy.url().should('include', '/login');
    
    // Login as admin user
    cy.get('input[name="email"]').type('admin@polybucket.com');
    cy.get('input[name="password"]').type('admin123');
    cy.get('button').contains('Sign in').click();
    
    // Wait for login to complete and redirect to setup
    cy.wait(3000);
    cy.url().should('include', '/setup');
    
    // Should be on step 1 (password change)
    cy.get('h3').should('contain', 'Change Admin Password');
    
    // Fill in current password
    cy.get('input[name="currentPassword"]').type('admin123');
    
    // Fill in mismatched passwords
    cy.get('input[name="newPassword"]').type('NewSecurePassword123!');
    cy.get('input[name="confirmPassword"]').type('DifferentPassword123!');
    
    // Should show validation error
    cy.get('button').contains('Continue').click();
    cy.get('.error-message').should('contain', 'Passwords do not match');
  });

  it('should validate current password is required', () => {
    // Wait for redirects to complete
    cy.wait(3000);
    
    // Should be redirected to login page
    cy.url().should('include', '/login');
    
    // Login as admin user
    cy.get('input[name="email"]').type('admin@polybucket.com');
    cy.get('input[name="password"]').type('admin123');
    cy.get('button').contains('Sign in').click();
    
    // Wait for login to complete and redirect to setup
    cy.wait(3000);
    cy.url().should('include', '/setup');
    
    // Should be on step 1 (password change)
    cy.get('h3').should('contain', 'Change Admin Password');
    
    // Try to submit without current password
    cy.get('input[name="newPassword"]').type('NewSecurePassword123!');
    cy.get('input[name="confirmPassword"]').type('NewSecurePassword123!');
    
    // Should show validation error
    cy.get('button').contains('Continue').click();
    cy.get('.error-message').should('contain', 'Current password is required');
  });

  it('should show password strength indicator', () => {
    // Wait for redirects to complete
    cy.wait(3000);
    
    // Should be redirected to login page
    cy.url().should('include', '/login');
    
    // Login as admin user
    cy.get('input[name="email"]').type('admin@polybucket.com');
    cy.get('input[name="password"]').type('admin123');
    cy.get('button').contains('Sign in').click();
    
    // Wait for login to complete and redirect to setup
    cy.wait(3000);
    cy.url().should('include', '/setup');
    
    // Should be on step 1 (password change)
    cy.get('h3').should('contain', 'Change Admin Password');
    
    // Type a strong password
    cy.get('input[name="newPassword"]').type('VeryStrongPassword123!@#');
    
    // Should show strong password indicator
    cy.get('.password-strength').should('contain', 'Strong');
  });

  it('should handle password visibility toggle', () => {
    // Wait for redirects to complete
    cy.wait(3000);
    
    // Should be redirected to login page
    cy.url().should('include', '/login');
    
    // Login as admin user
    cy.get('input[name="email"]').type('admin@polybucket.com');
    cy.get('input[name="password"]').type('admin123');
    cy.get('button').contains('Sign in').click();
    
    // Wait for login to complete and redirect to setup
    cy.wait(3000);
    cy.url().should('include', '/setup');
    
    // Should be on step 1 (password change)
    cy.get('h3').should('contain', 'Change Admin Password');
    
    // Check that password is hidden by default
    cy.get('input[name="newPassword"]').should('have.attr', 'type', 'password');
    
    // Click visibility toggle
    cy.get('button[aria-label="Toggle password visibility"]').click();
    
    // Password should now be visible
    cy.get('input[name="newPassword"]').should('have.attr', 'type', 'text');
  });

  it('should handle network errors gracefully', () => {
    // Wait for redirects to complete
    cy.wait(3000);
    
    // Should be redirected to login page
    cy.url().should('include', '/login');
    
    // Login as admin user
    cy.get('input[name="email"]').type('admin@polybucket.com');
    cy.get('input[name="password"]').type('admin123');
    cy.get('button').contains('Sign in').click();
    
    // Wait for login to complete and redirect to setup
    cy.wait(3000);
    cy.url().should('include', '/setup');
    
    // Should be on step 1 (password change)
    cy.get('h3').should('contain', 'Change Admin Password');
    
    // Intercept the API call and return an error
    cy.intercept('POST', '/api/auth/change-password', {
      statusCode: 500,
      body: { message: 'Internal server error' }
    }).as('changePasswordError');
    
    // Fill in form and submit
    cy.get('input[name="currentPassword"]').type('admin123');
    cy.get('input[name="newPassword"]').type('NewSecurePassword123!');
    cy.get('input[name="confirmPassword"]').type('NewSecurePassword123!');
    cy.get('button').contains('Continue').click();
    
    // Should show error message
    cy.get('.error-message').should('contain', 'An error occurred');
  });

  it('should handle skip network errors gracefully', () => {
    // Wait for redirects to complete
    cy.wait(3000);
    
    // Should be redirected to login page
    cy.url().should('include', '/login');
    
    // Login as admin user
    cy.get('input[name="email"]').type('admin@polybucket.com');
    cy.get('input[name="password"]').type('admin123');
    cy.get('button').contains('Sign in').click();
    
    // Wait for login to complete and redirect to setup
    cy.wait(3000);
    cy.url().should('include', '/setup');
    
    // Should be on step 1 (password change)
    cy.get('h3').should('contain', 'Change Admin Password');
    
    // Intercept the API call and return an error
    cy.intercept('POST', '/api/setup/skip-password-change', {
      statusCode: 500,
      body: { message: 'Internal server error' }
    }).as('skipPasswordError');
    
    // Click skip button
    cy.get('button').contains('Skip This Step').click();
    
    // Should show error message
    cy.get('.error-message').should('contain', 'An error occurred');
  });
}); 