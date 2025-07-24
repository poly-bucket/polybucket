/// <reference types="cypress" />

describe('Model Management E2E', () => {
  const testUser = {
    email: 'test@example.com',
    password: 'TestPassword123!',
    username: 'testuser'
  };

  const testModel = {
    name: 'Test 3D Model',
    description: 'This is a test 3D model for E2E testing',
    privacy: 'public',
    license: 'MIT',
    categories: ['Art', 'Technology'],
    aiGenerated: false,
    workInProgress: false,
    nsfw: false,
    remix: false
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

  describe('Model Upload Flow', () => {
    it('should successfully upload a model with valid data', () => {
      // Login first
      cy.visit('/login');
      cy.get('input[name="email"]').type(testUser.email);
      cy.get('input[name="password"]').type(testUser.password);
      cy.get('button[type="submit"]').click();
      
      // Wait for login to complete
      cy.url().should('include', '/dashboard', { timeout: 10000 });
      
      // Navigate to upload page
      cy.visit('/upload');
      
      // Fill in model information
      cy.get('input[placeholder*="title" i], input[name="title"]').type(testModel.name);
      cy.get('textarea[placeholder*="description" i], textarea[name="description"]').type(testModel.description);
      
      // Set privacy to public
      cy.get('select[name="privacy"], select[value="public"]').select('public');
      
      // Set license
      cy.get('select[name="license"], select[value="MIT"]').select('MIT');
      
      // Upload a test file
      cy.fixture('test-model.stl').then((fileContent) => {
        cy.get('input[type="file"]').attachFile({
          fileContent: fileContent,
          fileName: 'test-model.stl',
          mimeType: 'application/octet-stream'
        });
      });
      
      // Submit the form
      cy.get('button').contains('Upload').click();
      
      // Verify success message or redirect
      cy.url().should('include', '/dashboard', { timeout: 15000 });
      cy.contains('Upload successful').should('be.visible');
    });

    it('should show validation errors for required fields', () => {
      // Login first
      cy.visit('/login');
      cy.get('input[name="email"]').type(testUser.email);
      cy.get('input[name="password"]').type(testUser.password);
      cy.get('button[type="submit"]').click();
      
      // Navigate to upload page
      cy.visit('/upload');
      
      // Try to submit without required fields
      cy.get('button').contains('Upload').click();
      
      // Verify validation errors
      cy.contains('Model name is required').should('be.visible');
      cy.contains('At least one file is required').should('be.visible');
    });

    it('should show error for unsupported file types', () => {
      // Login first
      cy.visit('/login');
      cy.get('input[name="email"]').type(testUser.email);
      cy.get('input[name="password"]').type(testUser.password);
      cy.get('button[type="submit"]').click();
      
      // Navigate to upload page
      cy.visit('/upload');
      
      // Try to upload unsupported file
      cy.fixture('test.txt').then((fileContent) => {
        cy.get('input[type="file"]').attachFile({
          fileContent: fileContent,
          fileName: 'test.txt',
          mimeType: 'text/plain'
        });
      });
      
      // Verify error message
      cy.contains('Unsupported file type').should('be.visible');
    });

    it('should support drag and drop file upload', () => {
      // Login first
      cy.visit('/login');
      cy.get('input[name="email"]').type(testUser.email);
      cy.get('input[name="password"]').type(testUser.password);
      cy.get('button[type="submit"]').click();
      
      // Navigate to upload page
      cy.visit('/upload');
      
      // Fill in basic info
      cy.get('input[placeholder*="title" i], input[name="title"]').type(testModel.name);
      cy.get('textarea[placeholder*="description" i], textarea[name="description"]').type(testModel.description);
      
      // Drag and drop a file
      cy.fixture('test-model.stl').then((fileContent) => {
        cy.get('[data-testid="drop-zone"], .drop-zone, [class*="drop"]').first().attachFile({
          fileContent: fileContent,
          fileName: 'test-model.stl',
          mimeType: 'application/octet-stream'
        }, { subjectType: 'drag-n-drop' });
      });
      
      // Verify file was uploaded
      cy.contains('test-model.stl').should('be.visible');
    });
  });

  describe('Model Edit Flow', () => {
    it('should successfully edit a model', () => {
      // Login first
      cy.visit('/login');
      cy.get('input[name="email"]').type(testUser.email);
      cy.get('input[name="password"]').type(testUser.password);
      cy.get('button[type="submit"]').click();
      
      // Navigate to a model detail page (assuming we have one)
      cy.visit('/models/some-model-id');
      
      // Click edit button
      cy.get('button').contains('Edit').click();
      
      // Update model information
      const updatedName = 'Updated Model Name';
      cy.get('input[name="name"], input[placeholder*="name" i]').clear().type(updatedName);
      
      const updatedDescription = 'Updated description for the model';
      cy.get('textarea[name="description"], textarea[placeholder*="description" i]').clear().type(updatedDescription);
      
      // Change privacy setting
      cy.get('select[name="privacy"]').select('private');
      
      // Save changes
      cy.get('button').contains('Save').click();
      
      // Verify changes were saved
      cy.contains(updatedName).should('be.visible');
      cy.contains(updatedDescription).should('be.visible');
      cy.contains('Private').should('be.visible');
    });

    it('should show validation errors when editing with invalid data', () => {
      // Login first
      cy.visit('/login');
      cy.get('input[name="email"]').type(testUser.email);
      cy.get('input[name="password"]').type(testUser.password);
      cy.get('button[type="submit"]').click();
      
      // Navigate to model edit page
      cy.visit('/models/some-model-id/edit');
      
      // Clear required field
      cy.get('input[name="name"]').clear();
      
      // Try to save
      cy.get('button').contains('Save').click();
      
      // Verify validation error
      cy.contains('Model name is required').should('be.visible');
    });

    it('should allow uploading new version files', () => {
      // Login first
      cy.visit('/login');
      cy.get('input[name="email"]').type(testUser.email);
      cy.get('input[name="password"]').type(testUser.password);
      cy.get('button[type="submit"]').click();
      
      // Navigate to model detail page
      cy.visit('/models/some-model-id');
      
      // Click on version management tab or button
      cy.get('button').contains('Versions').click();
      
      // Click add new version
      cy.get('button').contains('Add Version').click();
      
      // Fill version information
      cy.get('input[name="versionName"]').type('Version 2.0');
      cy.get('textarea[name="versionNotes"]').type('Improved version with better details');
      
      // Upload new files
      cy.fixture('test-model-v2.stl').then((fileContent) => {
        cy.get('input[type="file"]').attachFile({
          fileContent: fileContent,
          fileName: 'test-model-v2.stl',
          mimeType: 'application/octet-stream'
        });
      });
      
      // Save version
      cy.get('button').contains('Create Version').click();
      
      // Verify version was created
      cy.contains('Version 2.0').should('be.visible');
      cy.contains('Improved version with better details').should('be.visible');
    });
  });

  describe('Model Delete Flow', () => {
    it('should show confirmation dialog when deleting a model', () => {
      // Login first
      cy.visit('/login');
      cy.get('input[name="email"]').type(testUser.email);
      cy.get('input[name="password"]').type(testUser.password);
      cy.get('button[type="submit"]').click();
      
      // Navigate to model detail page
      cy.visit('/models/some-model-id');
      
      // Click delete button
      cy.get('button').contains('Delete').click();
      
      // Verify confirmation dialog appears
      cy.get('[role="dialog"], .modal, [class*="modal"]').should('be.visible');
      cy.contains('Are you sure').should('be.visible');
      cy.contains('This action cannot be undone').should('be.visible');
    });

    it('should successfully delete a model after confirmation', () => {
      // Login first
      cy.visit('/login');
      cy.get('input[name="email"]').type(testUser.email);
      cy.get('input[name="password"]').type(testUser.password);
      cy.get('button[type="submit"]').click();
      
      // Navigate to model detail page
      cy.visit('/models/some-model-id');
      
      // Click delete button
      cy.get('button').contains('Delete').click();
      
      // Confirm deletion
      cy.get('button').contains('Delete').click();
      
      // Verify redirect to dashboard or models list
      cy.url().should('include', '/dashboard');
      cy.contains('Model deleted successfully').should('be.visible');
    });

    it('should cancel deletion when user clicks cancel', () => {
      // Login first
      cy.visit('/login');
      cy.get('input[name="email"]').type(testUser.email);
      cy.get('input[name="password"]').type(testUser.password);
      cy.get('button[type="submit"]').click();
      
      // Navigate to model detail page
      cy.visit('/models/some-model-id');
      
      // Click delete button
      cy.get('button').contains('Delete').click();
      
      // Click cancel
      cy.get('button').contains('Cancel').click();
      
      // Verify dialog is closed and we're still on the same page
      cy.get('[role="dialog"], .modal, [class*="modal"]').should('not.exist');
      cy.url().should('include', '/models/some-model-id');
    });

    it('should prevent non-owners from deleting models', () => {
      // Login as a different user
      cy.visit('/login');
      cy.get('input[name="email"]').type('other@example.com');
      cy.get('input[name="password"]').type('OtherPassword123!');
      cy.get('button[type="submit"]').click();
      
      // Navigate to a model owned by another user
      cy.visit('/models/some-model-id');
      
      // Verify delete button is not visible or disabled
      cy.get('button').contains('Delete').should('not.exist');
      // OR if button exists but is disabled
      // cy.get('button').contains('Delete').should('be.disabled');
    });
  });

  describe('Error Handling', () => {
    it('should show error message when upload fails', () => {
      // Login first
      cy.visit('/login');
      cy.get('input[name="email"]').type(testUser.email);
      cy.get('input[name="password"]').type(testUser.password);
      cy.get('button[type="submit"]').click();
      
      // Navigate to upload page
      cy.visit('/upload');
      
      // Intercept the upload request and make it fail
      cy.intercept('POST', '/api/models', {
        statusCode: 500,
        body: { message: 'Upload failed due to server error' }
      }).as('uploadRequest');
      
      // Fill form and submit
      cy.get('input[placeholder*="title" i], input[name="title"]').type(testModel.name);
      cy.fixture('test-model.stl').then((fileContent) => {
        cy.get('input[type="file"]').attachFile({
          fileContent: fileContent,
          fileName: 'test-model.stl',
          mimeType: 'application/octet-stream'
        });
      });
      cy.get('button').contains('Upload').click();
      
      // Verify error message is shown
      cy.contains('Upload failed').should('be.visible');
      cy.contains('Please try again').should('be.visible');
    });

    it('should show error message when edit fails', () => {
      // Login first
      cy.visit('/login');
      cy.get('input[name="email"]').type(testUser.email);
      cy.get('input[name="password"]').type(testUser.password);
      cy.get('button[type="submit"]').click();
      
      // Navigate to model edit page
      cy.visit('/models/some-model-id/edit');
      
      // Intercept the update request and make it fail
      cy.intercept('PUT', '/api/models/*', {
        statusCode: 500,
        body: { message: 'Update failed due to server error' }
      }).as('updateRequest');
      
      // Make changes and save
      cy.get('input[name="name"]').clear().type('Updated Name');
      cy.get('button').contains('Save').click();
      
      // Verify error message is shown
      cy.contains('Update failed').should('be.visible');
      cy.contains('Please try again').should('be.visible');
    });
  });

  describe('Progress Indicators', () => {
    it('should show upload progress during file upload', () => {
      // Login first
      cy.visit('/login');
      cy.get('input[name="email"]').type(testUser.email);
      cy.get('input[name="password"]').type(testUser.password);
      cy.get('button[type="submit"]').click();
      
      // Navigate to upload page
      cy.visit('/upload');
      
      // Fill form
      cy.get('input[placeholder*="title" i], input[name="title"]').type(testModel.name);
      cy.fixture('test-model.stl').then((fileContent) => {
        cy.get('input[type="file"]').attachFile({
          fileContent: fileContent,
          fileName: 'test-model.stl',
          mimeType: 'application/octet-stream'
        });
      });
      
      // Submit form
      cy.get('button').contains('Upload').click();
      
      // Verify progress indicator is shown
      cy.get('[class*="progress"], [class*="spinner"], [class*="loading"]').should('be.visible');
      cy.contains('Uploading').should('be.visible');
    });
  });
}); 