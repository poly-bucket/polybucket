/// <reference types="cypress" />

describe('Setup Process - Basic Test', () => {
  beforeEach(() => {
    // Clear localStorage to simulate fresh state
    cy.clearLocalStorage();
    cy.clearCookies();
    
    // Visit the application
    cy.visit('/');
  });

  it('should load the page and show basic structure', () => {
    // Should show the page title
    cy.get('title').should('contain', 'polybucket');
    
    // Should have a root div
    cy.get('#root').should('exist');
    
    // Log the page content to understand what's actually there
    cy.get('body').then(($body) => {
      cy.log('Page content:', $body.text());
    });
    
    // Take a screenshot to see what the page looks like
    cy.screenshot('setup-basic-page');
  });

  it('should check current URL and redirects', () => {
    // Wait for any redirects to complete
    cy.wait(3000);
    
    // Log the current URL
    cy.url().then((url) => {
      cy.log('Current URL:', url);
      
      // Check if we're on the setup page
      if (url.includes('/setup')) {
        cy.log('Successfully redirected to setup page');
      } else if (url.includes('/login')) {
        cy.log('Redirected to login page');
      } else if (url.includes('/dashboard')) {
        cy.log('Redirected to dashboard');
      } else {
        cy.log('On unknown page:', url);
      }
    });
  });

  it('should check if React app is loading and log all content', () => {
    // Wait for React to load
    cy.wait(3000);
    
    // Log all text content on the page
    cy.get('body').then(($body) => {
      const allText = $body.text();
      cy.log('All text content on page:', allText);
      
      // Check for specific keywords
      const keywords = ['Welcome', 'PolyBucket', 'admin', 'login', 'setup', 'password', 'continue'];
      keywords.forEach(keyword => {
        if (allText.toLowerCase().includes(keyword.toLowerCase())) {
          cy.log(`Found keyword: ${keyword}`);
        }
      });
    });
    
    // Check if there are any React elements (don't fail if not found)
    cy.get('body').then(($body) => {
      const testElements = $body.find('[data-testid]');
      cy.log(`Found ${testElements.length} elements with data-testid`);
    });
    
    // Check for any buttons or interactive elements
    cy.get('body').then(($body) => {
      const buttons = $body.find('button');
      cy.log(`Found ${buttons.length} buttons`);
      if (buttons.length > 0) {
        const buttonTexts = buttons.map((i, el) => Cypress.$(el).text()).get();
        cy.log('Button texts:', buttonTexts);
      }
    });
    
    // Check for any headings
    cy.get('body').then(($body) => {
      const headings = $body.find('h1, h2, h3, h4, h5, h6');
      cy.log(`Found ${headings.length} headings`);
      if (headings.length > 0) {
        const headingTexts = headings.map((i, el) => Cypress.$(el).text()).get();
        cy.log('Heading texts:', headingTexts);
      }
    });
    
    // Check for any divs with text content
    cy.get('body').then(($body) => {
      const divs = $body.find('div');
      const divsWithText = divs.filter((i, el) => Cypress.$(el).text().trim().length > 0);
      cy.log(`Found ${divsWithText.length} divs with text content`);
      if (divsWithText.length > 0) {
        const divTexts = divsWithText.slice(0, 10).map((i, el) => Cypress.$(el).text().trim()).get();
        cy.log('First 10 div texts:', divTexts);
      }
    });
    
    // Check for any forms or inputs
    cy.get('body').then(($body) => {
      const forms = $body.find('form');
      const inputs = $body.find('input');
      cy.log(`Found ${forms.length} forms and ${inputs.length} inputs`);
    });
  });

  it('should check for any error messages or loading states', () => {
    // Wait for potential loading
    cy.wait(5000);
    
    // Check for error messages
    cy.get('body').then(($body) => {
      const text = $body.text();
      if (text.includes('error') || text.includes('Error')) {
        cy.log('Found error text:', text);
      }
    });
    
    // Check for loading indicators (don't fail if not found)
    cy.get('body').then(($body) => {
      const loadingElements = $body.find('.loading, .spinner, [class*="loading"], [class*="spinner"]');
      if (loadingElements.length > 0) {
        cy.log(`Found ${loadingElements.length} loading indicators`);
      } else {
        cy.log('No loading indicators found');
      }
    });
    
    // Check for any console errors
    cy.window().then((win) => {
      cy.log('Console errors:', win.console.error);
    });
  });
}); 