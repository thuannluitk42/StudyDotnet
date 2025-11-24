#!/bin/bash

echo "======================================"
echo "  DATABASE CHECK - BookAPI"
echo "======================================"

# Check if database file exists
echo -e "\n📁 Database Files:"
docker exec bookapi ls -lh /data/ 2>/dev/null || echo "❌ Cannot access /data folder"

# Check tables
echo -e "\n📊 Database Tables:"
docker exec bookapi sqlite3 /data/bookapi.db ".tables" 2>/dev/null || echo "❌ Database not found"

# Count records
echo -e "\n👥 Users Count:"
docker exec bookapi sqlite3 /data/bookapi.db "SELECT COUNT(*) as Total FROM AspNetUsers;" 2>/dev/null

echo -e "\n🎭 Roles Count:"
docker exec bookapi sqlite3 /data/bookapi.db "SELECT COUNT(*) as Total FROM AspNetRoles;" 2>/dev/null

# List users
echo -e "\n📋 Users List:"
docker exec bookapi sqlite3 /data/bookapi.db \
  "SELECT Email, UserName, EmailConfirmed FROM AspNetUsers;" 2>/dev/null

# List roles
echo -e "\n🔐 Roles List:"
docker exec bookapi sqlite3 /data/bookapi.db \
  "SELECT Id, Name FROM AspNetRoles;" 2>/dev/null

# User-Role mapping
echo -e "\n👤 User-Role Mapping:"
docker exec bookapi sqlite3 /data/bookapi.db \
  "SELECT u.Email, r.Name as Role 
   FROM AspNetUsers u 
   JOIN AspNetUserRoles ur ON u.Id = ur.UserId 
   JOIN AspNetRoles r ON ur.RoleId = r.Id;" 2>/dev/null

# Check migrations
echo -e "\n🔄 Applied Migrations:"
docker exec bookapi sqlite3 /data/bookapi.db \
  "SELECT MigrationId FROM __EFMigrationsHistory ORDER BY MigrationId;" 2>/dev/null

echo -e "\n======================================"