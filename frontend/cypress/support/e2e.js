// ***********************************************************
// This example support/e2e.js is processed and
// loaded automatically before your test files.
//
// This is a great place to put global configuration and
// behavior that modifies Cypress.
//
// You can change the location of this file or turn off
// automatically serving support files with the
// 'supportFile' configuration option.
//
// You can read more here:
// https://on.cypress.io/configuration
// ***********************************************************

// Import commands.js using ES2015 syntax:
import './commands'

// Alternatively you can use CommonJS syntax:
// require('./commands')

// Ignore React errors during testing
Cypress.on('uncaught:exception', (err, runnable) => {
  // Returning false here prevents Cypress from failing the test
  return false;
});

// Add custom commands here
Cypress.Commands.add('login', (email, password) => {
  cy.request({
    method: 'POST',
    url: 'http://localhost:11666/api/auth/login',
    body: {
      emailOrUsername: email,
      password: password
    }
  }).then((response) => {
    if (response.body.token) {
      localStorage.setItem('user_token', response.body.token);
    }
  });
});

Cypress.Commands.add('clearDatabase', () => {
  // This would typically call an API endpoint to clear the database
  // For now, we'll just clear localStorage
  cy.clearLocalStorage();
}); 