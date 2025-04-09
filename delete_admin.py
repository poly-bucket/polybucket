#!/usr/bin/env python3

import psycopg2
import sys
import os

# Database connection parameters - using the same as found in the application
DB_HOST = 'localhost'  # In Docker this would be 'polybucket-db' or 'db'
DB_PORT = 5432
DB_NAME = 'polybucket'
DB_USER = 'polybucket'
DB_PASSWORD = 'polybucket'

def delete_admin_users():
    """Connect to PostgreSQL and delete all admin users"""
    try:
        # Connect to the database
        print(f"Connecting to PostgreSQL at {DB_HOST}:{DB_PORT}...")
        conn = psycopg2.connect(
            host=DB_HOST,
            port=DB_PORT,
            dbname=DB_NAME,
            user=DB_USER,
            password=DB_PASSWORD
        )
        conn.autocommit = True
        cursor = conn.cursor()
        
        # First, let's check for admin users
        cursor.execute('SELECT id, username, email FROM "Users" WHERE "IsAdmin" = true')
        admin_users = cursor.fetchall()
        
        if not admin_users:
            print("No admin users found in the database.")
            return
        
        print(f"Found {len(admin_users)} admin user(s):")
        for user_id, username, email in admin_users:
            print(f"  - {username} ({email}) [ID: {user_id}]")
        
        # Now, delete each admin user
        for user_id, username, email in admin_users:
            # First delete related records in RefreshTokens table
            cursor.execute('DELETE FROM "RefreshTokens" WHERE "UserId" = %s', (user_id,))
            refresh_tokens_deleted = cursor.rowcount
            
            # Now delete the user
            cursor.execute('DELETE FROM "Users" WHERE id = %s', (user_id,))
            if cursor.rowcount > 0:
                print(f"Successfully deleted admin user: {username} ({email}) and {refresh_tokens_deleted} refresh tokens")
            else:
                print(f"Failed to delete admin user: {username} ({email})")
        
        print("\nAdmin user(s) successfully deleted. You can now restart the application and access the first-time setup wizard.")
    
    except Exception as e:
        print(f"Error: {e}")
        sys.exit(1)
    finally:
        if 'conn' in locals():
            conn.close()

if __name__ == "__main__":
    delete_admin_users() 