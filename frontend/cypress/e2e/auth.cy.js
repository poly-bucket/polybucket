/// <reference types="cypress" />

/* global cy */
// Prevents ESLint errors about undefined 'cy'

describe('Authentication Flow', () => {
  beforeEach(() => {
    // Mock the API responses
    cy.intercept('GET', 'http://localhost:5166/api/auth/check-first-run', {
      statusCode: 200,
      body: { isFirstRun: true }
    }).as('checkFirstRun');

    cy.intercept('POST', 'http://localhost:5166/api/auth/register', {
      statusCode: 200,
      body: {
        id: '1',
        username: 'admin',
        email: 'admin@example.com',
        token: 'fake-jwt-token',
        refreshToken: 'fake-refresh-token',
        roles: ['Admin']
      }
    }).as('registerAdmin');

    cy.intercept('POST', 'http://localhost:5166/api/auth/login', {
      statusCode: 200,
      body: {
        id: '1',
        username: 'admin',
        email: 'admin@example.com',
        token: 'fake-jwt-token',
        refreshToken: 'fake-refresh-token',
        roles: ['Admin']
      }
    }).as('login');
  });

  it('should show admin registration on first run', () => {
    cy.visit('/');
    cy.wait('@checkFirstRun');

    cy.contains('Create Admin Account').should('be.visible');
    cy.contains('This is the first time setup').should('be.visible');
  });

  it('should allow admin registration', () => {
    cy.visit('/');
    cy.wait('@checkFirstRun');

    cy.get('input[name="username"]').type('admin');
    cy.get('input[name="email"]').type('admin@example.com');
    cy.get('input[name="password"]').type('Password123!');
    cy.get('input[name="confirmPassword"]').type('Password123!');
    cy.contains('button', 'Create Admin Account').click();

    cy.wait('@registerAdmin');
    cy.url().should('include', '/dashboard');
    cy.contains('Welcome to your dashboard, admin!').should('be.visible');
  });

  it('should show validation errors for password mismatch', () => {
    cy.visit('/');
    cy.wait('@checkFirstRun');

    cy.get('input[name="username"]').type('admin');
    cy.get('input[name="email"]').type('admin@example.com');
    cy.get('input[name="password"]').type('Password123!');
    cy.get('input[name="confirmPassword"]').type('DifferentPassword!');
    cy.contains('button', 'Create Admin Account').click();

    cy.contains('Passwords do not match').should('be.visible');
    cy.url().should('not.include', '/dashboard');
  });

  it('should allow login with correct credentials', () => {
    // Mock first run check to return false (admin exists)
    cy.intercept('GET', 'http://localhost:5166/api/auth/check-first-run', {
      statusCode: 200,
      body: { isFirstRun: false }
    }).as('checkFirstRunFalse');

    cy.visit('/');
    cy.wait('@checkFirstRunFalse');
    cy.url().should('include', '/login');

    cy.get('input[name="email"]').type('admin@example.com');
    cy.get('input[name="password"]').type('Password123!');
    cy.contains('button', 'Sign in').click();

    cy.wait('@login');
    cy.url().should('include', '/dashboard');
    cy.contains('Welcome to your dashboard, admin!').should('be.visible');
  });

  it('should allow logout', () => {
    // Mock first run check to return false (admin exists)
    cy.intercept('GET', 'http://localhost:5166/api/auth/check-first-run', {
      statusCode: 200,
      body: { isFirstRun: false }
    }).as('checkFirstRunFalse');

    // Log in first
    cy.visit('/login');
    cy.get('input[name="email"]').type('admin@example.com');
    cy.get('input[name="password"]').type('Password123!');
    cy.contains('button', 'Sign in').click();
    cy.wait('@login');

    // Now test logout
    cy.contains('button', 'Logout').click();
    cy.url().should('include', '/login');
  });
}); 