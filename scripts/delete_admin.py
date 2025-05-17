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

def reset_system_settings():
    """Connect to PostgreSQL and reset system settings"""
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

        # Update SystemSetups table
        cursor.execute('UPDATE "SystemSetups" SET "IsAdminConfigured" = false WHERE "IsAdminConfigured" = true')
        cursor.execute('UPDATE "SystemSetups" SET "IsRoleConfigured" = false WHERE "IsRoleConfigured" = true')
        cursor.execute('UPDATE "SystemSetups" SET "IsModerationConfigured" = false WHERE "IsModerationConfigured" = true')
        print("Updated SystemSetups configurations to false where necessary.")

    except Exception as e:
        print(f"Error: {e}")
        sys.exit(1)
    finally:
        if 'conn' in locals():
            conn.close()

if __name__ == "__main__":
    reset_system_settings() 