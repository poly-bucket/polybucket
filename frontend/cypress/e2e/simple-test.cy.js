describe('Simple Test', () => {
  it('should load test-models page', () => {
    cy.visit('http://localhost:32768/test-models');
    cy.wait(5000);
    
    // Check if the page loads
    cy.get('body').then(($body) => {
      cy.log('Page content:');
      cy.log($body.text());
    });
    
    // Check for basic elements
    cy.get('h1').should('contain', 'Test Models API');
    
    // Check for models count
    cy.get('body').then(($body) => {
      if ($body.text().includes('Found')) {
        cy.log('Found models count text');
      } else {
        cy.log('No models count text found');
      }
    });
    
    // Check for model divs - don't fail if none found
    cy.get('body').then(($body) => {
      const $modelDivs = $body.find('div[style*="border: 1px solid #ccc"]');
      cy.log(`Found ${$modelDivs.length} model divs`);
      
      if ($modelDivs.length > 0) {
        cy.log('Model divs found');
      } else {
        cy.log('No model divs found');
      }
    });
    
    // Check for images - don't fail if none found
    cy.get('body').then(($body) => {
      const $imgs = $body.find('img');
      cy.log(`Found ${$imgs.length} images`);
      
      if ($imgs.length > 0) {
        $imgs.each((index, img) => {
          cy.log(`Image ${index}: src="${img.src}"`);
        });
      } else {
        cy.log('No images found');
      }
    });
    
    // Check for any error messages
    cy.get('body').then(($body) => {
      if ($body.text().includes('Error:')) {
        cy.log('Found error message on page');
      } else {
        cy.log('No error messages found');
      }
    });
  });
}); 