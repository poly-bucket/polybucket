describe('API Test', () => {
  it('should intercept API calls and check response', () => {
    // Intercept the models API call
    cy.intercept('GET', '**/api/models**').as('getModels');
    
    // Visit the test page
    cy.visit('http://localhost:32768/test-models');
    cy.wait(5000);
    
    // Wait for the API call to be made
    cy.wait('@getModels', { timeout: 10000 }).then((interception) => {
      cy.log('API call intercepted:');
      cy.log(`URL: ${interception.request.url}`);
      cy.log(`Method: ${interception.request.method}`);
      cy.log(`Status: ${interception.response?.statusCode}`);
      
      if (interception.response?.body) {
        cy.log('Response body:');
        cy.log(JSON.stringify(interception.response.body, null, 2));
      }
    });
    
    // Check what's on the page
    cy.get('body').then(($body) => {
      cy.log('Page content:');
      cy.log($body.text());
    });
  });
}); 