// ***********************************************
// This example commands.js shows you how to
// create various custom commands and overwrite
// existing commands.
//
// For more comprehensive examples of custom
// commands please read more here:
// https://on.cypress.io/custom-commands
// ***********************************************

// Custom command to wait for API to be ready
Cypress.Commands.add('waitForApi', () => {
  cy.request({
    method: 'GET',
    url: 'http://localhost:11666/api/system-settings/setup-status',
    failOnStatusCode: false
  });
});

// Custom command to check if admin setup is complete
Cypress.Commands.add('checkAdminSetupStatus', () => {
  return cy.request({
    method: 'GET',
    url: 'http://localhost:11666/api/system-settings/setup-status'
  });
}); 