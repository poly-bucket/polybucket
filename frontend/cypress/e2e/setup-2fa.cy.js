/// <reference types="cypress" />

describe('Setup Process - Two-Factor Authentication', () => {
  beforeEach(() => {
    // Clear localStorage to simulate fresh state
    cy.clearLocalStorage();
    cy.clearCookies();
    
    // Visit the application
    cy.visit('/');
  });

  it('should complete 2FA setup successfully', () => {
    // Navigate through setup to 2FA step
    cy.get('button').contains('Continue with Default Admin').click();
    cy.url().should('include', '/setup');
    
    // Skip password change to get to 2FA step
    cy.get('button').contains('Skip This Step').click();
    
    // Should be on 2FA step
    cy.get('h3').should('contain', 'Two-Factor Authentication');
    cy.get('p').should('contain', 'Enhance your account security by enabling two-factor authentication');
    
    // Enable 2FA
    cy.get('input[type="checkbox"]').check();
    
    // Click continue to initialize 2FA
    cy.get('button').contains('Continue').click();
    
    // Should show QR code setup
    cy.get('h4').should('contain', 'Scan QR Code with Your Authenticator App');
    cy.get('svg').should('exist'); // QR code should be rendered
    
    // Should show manual entry option
    cy.get('p').should('contain', 'Manual Entry');
    
    // Enter verification code (mock a valid code)
    cy.get('input[placeholder*="6-digit code"]').type('123456');
    
    // Click verify
    cy.get('button').contains('Verify & Continue').click();
    
    // Should proceed to next step (site settings)
    cy.url().should('include', '/setup');
    cy.get('h3').should('contain', 'Site Configuration');
  });

  it('should skip 2FA setup successfully', () => {
    // Navigate through setup to 2FA step
    cy.get('button').contains('Continue with Default Admin').click();
    cy.url().should('include', '/setup');
    
    // Skip password change to get to 2FA step
    cy.get('button').contains('Skip This Step').click();
    
    // Should be on 2FA step
    cy.get('h3').should('contain', 'Two-Factor Authentication');
    
    // Leave checkbox unchecked and click continue
    cy.get('button').contains('Continue').click();
    
    // Should proceed to next step (site settings)
    cy.url().should('include', '/setup');
    cy.get('h3').should('contain', 'Site Configuration');
  });

  it('should skip 2FA step using skip button', () => {
    // Navigate through setup to 2FA step
    cy.get('button').contains('Continue with Default Admin').click();
    cy.url().should('include', '/setup');
    
    // Skip password change to get to 2FA step
    cy.get('button').contains('Skip This Step').click();
    
    // Should be on 2FA step
    cy.get('h3').should('contain', 'Two-Factor Authentication');
    
    // Click skip this step button
    cy.get('button').contains('Skip This Step').click();
    
    // Should proceed to next step (site settings)
    cy.url().should('include', '/setup');
    cy.get('h3').should('contain', 'Site Configuration');
  });

  it('should handle 2FA initialization errors', () => {
    // Mock 2FA initialization error
    cy.intercept('POST', '**/api/auth/2fa/initialize', {
      statusCode: 400,
      body: { message: '2FA is already initialized for this user' }
    }).as('twoFactorInitError');
    
    // Navigate through setup to 2FA step
    cy.get('button').contains('Continue with Default Admin').click();
    cy.url().should('include', '/setup');
    
    // Skip password change to get to 2FA step
    cy.get('button').contains('Skip This Step').click();
    
    // Enable 2FA and try to continue
    cy.get('input[type="checkbox"]').check();
    cy.get('button').contains('Continue').click();
    
    // Should show error message
    cy.get('.lg-badge-error').should('contain', '2FA is already initialized for this user');
  });

  it('should handle 2FA verification errors', () => {
    // Mock 2FA enable error
    cy.intercept('POST', '**/api/auth/2fa/enable', {
      statusCode: 400,
      body: { message: 'Invalid token provided' }
    }).as('twoFactorEnableError');
    
    // Navigate through setup to 2FA step
    cy.get('button').contains('Continue with Default Admin').click();
    cy.url().should('include', '/setup');
    
    // Skip password change to get to 2FA step
    cy.get('button').contains('Skip This Step').click();
    
    // Enable 2FA and continue to QR setup
    cy.get('input[type="checkbox"]').check();
    cy.get('button').contains('Continue').click();
    
    // Enter invalid verification code
    cy.get('input[placeholder*="6-digit code"]').type('000000');
    
    // Click verify
    cy.get('button').contains('Verify & Continue').click();
    
    // Should show error message
    cy.get('p.text-red-400').should('contain', 'Invalid token provided');
  });

  it('should validate verification code format', () => {
    // Navigate through setup to 2FA step
    cy.get('button').contains('Continue with Default Admin').click();
    cy.url().should('include', '/setup');
    
    // Skip password change to get to 2FA step
    cy.get('button').contains('Skip This Step').click();
    
    // Enable 2FA and continue to QR setup
    cy.get('input[type="checkbox"]').check();
    cy.get('button').contains('Continue').click();
    
    // Try to enter non-numeric code
    cy.get('input[placeholder*="6-digit code"]').type('abcdef');
    
    // Input should only accept numbers (max 6 digits)
    cy.get('input[placeholder*="6-digit code"]').should('have.value', '');
  });

  it('should show QR code and manual entry details', () => {
    // Navigate through setup to 2FA step
    cy.get('button').contains('Continue with Default Admin').click();
    cy.url().should('include', '/setup');
    
    // Skip password change to get to 2FA step
    cy.get('button').contains('Skip This Step').click();
    
    // Enable 2FA and continue to QR setup
    cy.get('input[type="checkbox"]').check();
    cy.get('button').contains('Continue').click();
    
    // Should show QR code
    cy.get('svg').should('exist');
    
    // Should show manual entry instructions
    cy.get('p').should('contain', 'Manual Entry');
    cy.get('p').should('contain', 'If scanning doesn\'t work, you can manually enter the setup key');
    
    // Should show the setup key (otpauth URL)
    cy.get('p.text-gray-400').should('contain', 'otpauth://');
  });

  it('should handle back navigation from QR setup', () => {
    // Navigate through setup to 2FA step
    cy.get('button').contains('Continue with Default Admin').click();
    cy.url().should('include', '/setup');
    
    // Skip password change to get to 2FA step
    cy.get('button').contains('Skip This Step').click();
    
    // Enable 2FA and continue to QR setup
    cy.get('input[type="checkbox"]').check();
    cy.get('button').contains('Continue').click();
    
    // Click back button
    cy.get('button').contains('Back').click();
    
    // Should return to 2FA options screen
    cy.get('h3').should('contain', 'Two-Factor Authentication');
    cy.get('input[type="checkbox"]').should('be.checked');
  });

  it('should disable continue button when 2FA is not enabled', () => {
    // Navigate through setup to 2FA step
    cy.get('button').contains('Continue with Default Admin').click();
    cy.url().should('include', '/setup');
    
    // Skip password change to get to 2FA step
    cy.get('button').contains('Skip This Step').click();
    
    // Should be on 2FA step
    cy.get('h3').should('contain', 'Two-Factor Authentication');
    
    // Continue button should be disabled when checkbox is unchecked
    cy.get('button').contains('Continue').should('be.disabled');
    
    // Check the checkbox
    cy.get('input[type="checkbox"]').check();
    
    // Continue button should be enabled
    cy.get('button').contains('Continue').should('not.be.disabled');
  });

  it('should disable verify button when no verification code is entered', () => {
    // Navigate through setup to 2FA step
    cy.get('button').contains('Continue with Default Admin').click();
    cy.url().should('include', '/setup');
    
    // Skip password change to get to 2FA step
    cy.get('button').contains('Skip This Step').click();
    
    // Enable 2FA and continue to QR setup
    cy.get('input[type="checkbox"]').check();
    cy.get('button').contains('Continue').click();
    
    // Verify button should be disabled when no code is entered
    cy.get('button').contains('Verify & Continue').should('be.disabled');
    
    // Enter a code
    cy.get('input[placeholder*="6-digit code"]').type('123456');
    
    // Verify button should be enabled
    cy.get('button').contains('Verify & Continue').should('not.be.disabled');
  });

  it('should show loading states during 2FA operations', () => {
    // Mock slow 2FA initialization
    cy.intercept('POST', '**/api/auth/2fa/initialize', (req) => {
      req.reply({
        delay: 2000,
        statusCode: 200,
        body: {
          qrCodeUrl: 'otpauth://totp/PolyBucket:test@example.com?secret=JBSWY3DPEHPK3PXP&issuer=PolyBucket',
          secretKey: 'JBSWY3DPEHPK3PXP'
        }
      });
    }).as('slowTwoFactorInit');
    
    // Navigate through setup to 2FA step
    cy.get('button').contains('Continue with Default Admin').click();
    cy.url().should('include', '/setup');
    
    // Skip password change to get to 2FA step
    cy.get('button').contains('Skip This Step').click();
    
    // Enable 2FA and continue to QR setup
    cy.get('input[type="checkbox"]').check();
    cy.get('button').contains('Continue').click();
    
    // Should show loading state
    cy.get('button').contains('Setting up 2FA...').should('exist');
    cy.get('.lg-spinner').should('exist');
    
    // Wait for response
    cy.wait('@slowTwoFactorInit');
    
    // Should show QR code after loading
    cy.get('svg').should('exist');
  });
}); 