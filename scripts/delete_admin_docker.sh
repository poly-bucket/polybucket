#!/bin/bash

# Database connection parameters from docker-compose.yml
CONTAINER_NAME="polybucket-db"
DB_NAME="polybucket"
DB_USER="polybucket"  # from docker-compose.yml POSTGRES_USER
DB_PASSWORD="polybucket"  # from docker-compose.yml POSTGRES_PASSWORD

echo "Checking if container is running..."
if ! docker ps | grep -q $CONTAINER_NAME; then
  echo "Container '$CONTAINER_NAME' is not running. Make sure your containers are up."
  exit 1
fi

echo "Connecting to PostgreSQL container..."

# Get list of admin users
echo "Looking for admin users..."
ADMIN_USERS=$(docker exec $CONTAINER_NAME psql -U $DB_USER -d $DB_NAME -t -c 'SELECT "Id", "Username", "Email" FROM "Users" WHERE "IsAdmin" = true;')

if [ -z "$ADMIN_USERS" ]; then
  echo "No admin users found in the database."
else
  # Display found admin users
  echo "Found admin user(s):"
  echo "$ADMIN_USERS" | while read -r user_id username email; do
    if [ -z "$user_id" ]; then
      continue
    fi
    
    echo "  - $username ($email) [ID: $user_id]"
    
    # Delete refresh tokens
    docker exec $CONTAINER_NAME psql -U $DB_USER -d $DB_NAME -c "DELETE FROM \"RefreshTokens\" WHERE \"UserId\" = '$user_id';"
    
    # Delete user
    docker exec $CONTAINER_NAME psql -U $DB_USER -d $DB_NAME -c "DELETE FROM \"Users\" WHERE \"Id\" = '$user_id';"
    
    echo "Successfully deleted admin user: $username ($email)"
  done
fi

# Reset the system settings table
echo "Resetting system settings table..."
docker exec $CONTAINER_NAME psql -U $DB_USER -d $DB_NAME -c 'DELETE FROM "SystemSetups";'
echo "System settings reset successfully."

echo -e "\nSystem reset complete. You can now restart the application and access the first-time setup wizard." 