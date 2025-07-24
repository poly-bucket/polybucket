describe('Thumbnail Debug Test', () => {
  it('should test models API directly via test page', () => {
    // Visit the test models page
    cy.visit('http://localhost:32768/test-models');
    
    // Wait for the page to load
    cy.wait(5000);
    
    // Check if the page loaded
    cy.get('h1').should('contain', 'Test Models API');
    
    // Check if there are any models or error messages
    cy.get('body').then(($body) => {
      cy.log('Page content:');
      cy.log($body.text());
    });
    
    // Check if there's an error message
    cy.get('body').then(($body) => {
      if ($body.text().includes('Error:')) {
        cy.log('Found error message on page');
      } else if ($body.text().includes('Found 0 models')) {
        cy.log('No models found');
      } else if ($body.text().includes('Found')) {
        cy.log('Models found');
      }
    });
  });

  it('should test thumbnail loading on test page', () => {
    // Visit the test models page
    cy.visit('http://localhost:32768/test-models');
    
    // Wait for the page to load
    cy.wait(5000);
    
    // Check what content is displayed
    cy.get('body').then(($body) => {
      cy.log('Full page content:');
      cy.log($body.text());
    });
    
    // Check if there are any div elements with model data
    cy.get('div').then(($divs) => {
      cy.log(`Found ${$divs.length} div elements`);
      
      // Look for divs that might contain model data
      $divs.each((index, div) => {
        const text = div.textContent;
        if (text && (text.includes('Thumbnail URL') || text.includes('Found'))) {
          cy.log(`Div ${index} contains relevant data: ${text}`);
        }
      });
    });
    
    // Check if there are any error messages
    cy.get('body').then(($body) => {
      if ($body.text().includes('Error:')) {
        cy.log('Found error message on page');
      }
    });
    
    // Check if models are being displayed
    cy.get('body').then(($body) => {
      if ($body.text().includes('Found 0 models')) {
        cy.log('No models found');
      } else if ($body.text().includes('Found') && $body.text().includes('models')) {
        cy.log('Models are being displayed');
      }
    });

    // Now check for images specifically - don't fail if none found
    cy.get('body').then(($body) => {
      const $imgs = $body.find('img');
      cy.log(`Found ${$imgs.length} img elements`);
      
      if ($imgs.length > 0) {
        $imgs.each((index, img) => {
          cy.log(`Image ${index}:`);
          cy.log(`  src: ${img.src}`);
          cy.log(`  style.display: ${img.style.display}`);
          cy.log(`  style.visibility: ${img.style.visibility}`);
          cy.log(`  naturalWidth: ${img.naturalWidth}`);
          cy.log(`  naturalHeight: ${img.naturalHeight}`);
          cy.log(`  complete: ${img.complete}`);
          cy.log(`  onerror: ${img.onerror ? 'has handler' : 'no handler'}`);
        });
      } else {
        cy.log('No img elements found on the page');
      }
    });

    // Check for model divs with borders (our test models styling) - don't fail if none found
    cy.get('body').then(($body) => {
      const $modelDivs = $body.find('div[style*="border: 1px solid #ccc"]');
      cy.log(`Found ${$modelDivs.length} model divs with border styling`);
      
      if ($modelDivs.length > 0) {
        $modelDivs.each((index, div) => {
          cy.log(`Model div ${index} content: ${div.textContent}`);
          
          // Check if this div contains an image
          const $img = $(div).find('img');
          if ($img.length > 0) {
            cy.log(`  Contains image with src: ${$img.attr('src')}`);
          } else {
            cy.log(`  No image found in this model div`);
          }
        });
      }
    });
  });

  it('should test API calls with network monitoring', () => {
    // Intercept API requests specifically
    cy.intercept('GET', '**/api/models**').as('getModels');
    
    // Visit the test page
    cy.visit('http://localhost:32768/test-models');
    
    // Wait for the API call to be made
    cy.wait('@getModels', { timeout: 10000 }).then((interception) => {
      cy.log('Models API call intercepted:');
      cy.log(`URL: ${interception.request.url}`);
      cy.log(`Method: ${interception.request.method}`);
      cy.log(`Status: ${interception.response?.statusCode}`);
      cy.log(`Has Authorization header: ${!!interception.request.headers.authorization}`);
      
      if (interception.response?.body) {
        cy.log('Response body:');
        cy.log(JSON.stringify(interception.response.body, null, 2));
      }
    });
  });

  it('should test image loading directly', () => {
    // Visit the test models page
    cy.visit('http://localhost:32768/test-models');
    cy.wait(5000);

    // Get the first model div and check its image - don't fail if none found
    cy.get('body').then(($body) => {
      const $modelDiv = $body.find('div[style*="border: 1px solid #ccc"]').first();
      
      if ($modelDiv.length > 0) {
        const $img = $modelDiv.find('img');
        
        if ($img.length > 0) {
          const imgSrc = $img.attr('src');
          cy.log(`Found image with src: ${imgSrc}`);
          
          if (imgSrc) {
            // Test if the image URL is accessible
            cy.request({
              method: 'GET',
              url: imgSrc,
              failOnStatusCode: false
            }).then((response) => {
              cy.log(`Image URL status: ${response.status}`);
              if (response.status === 200) {
                cy.log('Image URL is accessible');
              } else {
                cy.log(`Image URL failed with status: ${response.status}`);
              }
            });
          }
        } else {
          cy.log('No image found in the first model div');
        }
      } else {
        cy.log('No model divs found');
      }
    });
  });
}); 