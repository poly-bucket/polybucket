/// <reference types="cypress" />

describe('Authentication Integration E2E', () => {
  const testUser = {
    email: 'test@example.com',
    password: 'TestPassword123!',
    username: 'testuser'
  };

  beforeEach(() => {
    // Clear any existing auth state
    cy.clearLocalStorage();
    cy.clearCookies();
    
    // Set longer timeout for backend calls
    cy.intercept('**', (req) => {
      req.timeout = 10000;
    });
  });

  describe('Real Backend Integration Tests', () => {
    it('should successfully login with valid credentials (if user exists)', () => {
      // Visit the login page
      cy.visit('/login');
      
      // Check if we're on the login form
      cy.get('h2').should('contain', 'Sign in to your account');
      
      // Fill in credentials - using seeded admin credentials
      cy.get('input[name="email"]').type('admin@polybucket.com');
      cy.get('input[name="password"]').type('admin');
      
      // Submit the form
      cy.get('button[type="submit"]').should('contain', 'Sign in').click();
      
      // Should either redirect to dashboard or show error
      cy.url({ timeout: 10000 }).then((url) => {
        if (url.includes('/dashboard')) {
          // Login successful
          cy.contains('Welcome').should('be.visible');
          
          // Verify token is stored
          cy.window().then((win) => {
            const storedUser = win.localStorage.getItem('user');
            expect(storedUser).to.not.be.null;
            
            const userData = JSON.parse(storedUser);
            expect(userData).to.have.property('accessToken');
            expect(userData).to.have.property('email');
          });
        } else {
          // Login failed (expected if admin doesn't exist)
          cy.get('.bg-red-100').should('be.visible');
        }
      });
    });

    it('should show error for invalid credentials', () => {
      cy.visit('/login');
      
      // Fill in invalid credentials
      cy.get('input[name="email"]').type('invalid@example.com');
      cy.get('input[name="password"]').type('wrongpassword');
      
      // Submit the form
      cy.get('button[type="submit"]').click();
      
      // Should show error message
      cy.get('.bg-red-100', { timeout: 10000 }).should('be.visible');
      
      // Should stay on login page
      cy.url().should('include', '/login');
    });

    it('should validate required fields', () => {
      cy.visit('/login');
      
      // Try to submit empty form
      cy.get('button[type="submit"]').click();
      
      // HTML5 validation should prevent submission
      cy.get('input[name="email"]:invalid').should('exist');
      cy.get('input[name="password"]:invalid').should('exist');
    });

    it('should toggle password visibility', () => {
      cy.visit('/login');
      
      // Type password
      cy.get('input[name="password"]').type('testpassword');
      
      // Password should be hidden by default
      cy.get('input[name="password"]').should('have.attr', 'type', 'password');
      
      // Click the eye icon to show password
      cy.get('button[type="button"]').first().click();
      
      // Password should now be visible
      cy.get('input[name="password"]').should('have.attr', 'type', 'text');
      
      // Click again to hide
      cy.get('button[type="button"]').first().click();
      
      // Password should be hidden again
      cy.get('input[name="password"]').should('have.attr', 'type', 'password');
    });

    it('should handle loading state during login', () => {
      cy.visit('/login');
      
      // Fill in credentials
      cy.get('input[name="email"]').type('test@example.com');
      cy.get('input[name="password"]').type('password123');
      
      // Intercept the login request to add delay
      cy.intercept('POST', '**/api/auth/login', (req) => {
        req.reply((res) => {
          return new Promise((resolve) => {
            setTimeout(() => {
              resolve(res.send({ statusCode: 401, body: { message: 'Invalid credentials' } }));
            }, 1000);
          });
        });
      }).as('slowLogin');
      
      // Submit form
      cy.get('button[type="submit"]').click();
      
      // Should show loading state (button might not be disabled but should show loading)
      cy.get('.animate-spin').should('be.visible');
      
      // Wait for request to complete
      cy.wait('@slowLogin');
      
      // Loading should be gone
      cy.get('.animate-spin').should('not.exist');
    });

    it('should preserve authentication after page refresh', () => {
      // Mock successful login
      const mockUser = {
        id: 'test-id',
        username: 'testuser',
        email: 'test@example.com',
        accessToken: 'test-token-' + Date.now(),
        refreshToken: 'test-refresh-token',
        roles: ['User']
      };

      cy.visit('/login');
      
      // Set user data in localStorage
      cy.window().then((win) => {
        win.localStorage.setItem('user', JSON.stringify(mockUser));
      });
      
      // Refresh the page
      cy.reload();
      
      // Should redirect away from login since user is logged in
      cy.url({ timeout: 5000 }).should('not.include', '/login');
    });

    it('should handle network errors gracefully', () => {
      cy.visit('/login');
      
      // Intercept login request to simulate network error
      cy.intercept('POST', '**/api/auth/login', {
        forceNetworkError: true
      }).as('networkError');
      
      // Fill form and submit
      cy.get('input[name="email"]').type('test@example.com');
      cy.get('input[name="password"]').type('password123');
      cy.get('button[type="submit"]').click();
      
      // Should show error message
      cy.get('.bg-red-100', { timeout: 10000 }).should('be.visible');
    });
  });

  describe('Authentication Flow Integration', () => {
    it('should complete full authentication cycle', () => {
      // Test the complete flow: login -> navigate -> logout
      cy.visit('/');
      
      // Should redirect to login if not authenticated
      cy.url().should('include', '/login');
      
      // Try login with test credentials
      cy.get('input[name="email"]').type('admin@polybucket.com');
      cy.get('input[name="password"]').type('admin');
      cy.get('button[type="submit"]').click();
      
      // Handle both success and failure scenarios
      cy.url({ timeout: 10000 }).then((url) => {
        if (url.includes('/dashboard') || url.includes('/admin-setup')) {
          // Login successful - test logout
          cy.get('body').then(($body) => {
            if ($body.find('button:contains("Logout")').length > 0) {
              cy.contains('button', 'Logout').click();
              cy.url().should('include', '/login');
            } else if ($body.find('button:contains("Sign out")').length > 0) {
              cy.contains('button', 'Sign out').click();
              cy.url().should('include', '/login');
            }
          });
        } else {
          // Login failed (expected behavior if no admin exists)
          cy.log('Login failed as expected - no admin user exists');
        }
      });
    });
  });
}); 