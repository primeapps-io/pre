#!/bin/bash
GREEN='\033[0;32m'
YELLOW='\033[0;33m'
BLUE='\033[0;34m'
NC='\033[0m' # No Color

cd ..

# Variables
basePath=$(pwd -W)
filePostgres=${PRIMEAPPS_FILE_POSTGRES:-"http://file.primeapps.io/binaries/win/postgresql-12.1-1-windows-x64-binaries.zip"}
fileMinio=${PRIMEAPPS_FILE_MINIO:-"http://file.primeapps.io/binaries/win/minio.exe"}
fileRedis=${PRIMEAPPS_FILE_REDIS:-"http://file.primeapps.io/binaries/win/Redis-x64-3.0.504.zip"}
fileWinSW=${PRIMEAPPS_FILE_WINSW:-"http://file.primeapps.io/binaries/win/WinSW.NET4.exe"}
postgresLocale="en-US"
postgresPath="$basePath/programs/pgsql/bin"
hostname=$(hostname)

# Create programs directory
mkdir programs
cd programs

# Install PostgreSQL
echo -e "${GREEN}Downloading PostgreSQL...${NC}"
curl $filePostgres -L --output postgres.zip
unzip postgres.zip
rm postgres.zip

# Install Minio
cd "$basePath/programs"
mkdir minio
cd minio
echo -e "${GREEN}Downloading Minio...${NC}"
curl $fileMinio -L --output minio.exe

# Install Redis
cd "$basePath/programs"
mkdir redis
cd redis
echo -e "${GREEN}Downloading Redis...${NC}"
curl $fileRedis -L --output Redis-x64-3.0.504.zip
unzip Redis-x64-3.0.504.zip
rm Redis-x64-3.0.504.zip

# Download WinSW
cd "$basePath/programs"
mkdir winsw
cd winsw
echo -e "${GREEN}Downloading WinSW...${NC}"
curl $fileWinSW -L --output winsw.exe

# Init database instances
cd $postgresPath
echo -e "${GREEN}Initializing database instances...${NC}"
./initdb -D "$basePath/data/pgsql_pre" --no-locale --encoding=UTF8

# Register database instances
echo -e "${GREEN}Registering database instances...${NC}"
./pg_ctl register -D "$basePath/data/pgsql_pre" -o "-F -p 5436" -N "Postgres-PrimeApps"

# Start database instances
echo -e "${GREEN}Starting database instances...${NC}"
net start "Postgres-PrimeApps"

# Wait Postgres wakeup
timeout 15 bash -c 'until echo > /dev/tcp/localhost/5436; do sleep 1; done'

# Create postgres role
echo -e "${GREEN}Creating postgres role for database instances...${NC}"
./psql -d postgres -p 5436 -c "CREATE ROLE postgres SUPERUSER CREATEDB CREATEROLE LOGIN REPLICATION BYPASSRLS;"

# Create databases
echo -e "${GREEN}Creating databases...${NC}"
./createdb -h localhost -U postgres -p 5436 --template=template0 --encoding=UTF8 --lc-ctype=$postgresLocale --lc-collate=$postgresLocale auth
./createdb -h localhost -U postgres -p 5436 --template=template0 --encoding=UTF8 --lc-ctype=$postgresLocale --lc-collate=$postgresLocale platform

# Restore databases
echo -e "${GREEN}Restoring databases...${NC}"
./pg_restore -h localhost -U postgres -p 5436 --no-owner --role=postgres -Fc -d auth "$basePath/database/auth.bak"
./pg_restore -h localhost -U postgres -p 5436 --no-owner --role=postgres -Fc -d platform "$basePath/database/platform.bak"

# Init storage instances
echo -e "${GREEN}Initializing storage instances...${NC}"
cd "$basePath/programs/minio"
cp "$basePath/programs/winsw/winsw.exe" minio-pre.exe
cp "$basePath/setup/xml/minio-pre.xml" minio-pre.xml

./minio-pre.exe install

echo -e "${GREEN}Starting storage instances...${NC}"
net start "MinIO-PrimeApps"

# Init cache instance
echo -e "${GREEN}Initializing cache instances...${NC}"
cd "$basePath/programs/redis"
cp "$basePath/programs/winsw/winsw.exe" redis-pre.exe
cp "$basePath/setup/xml/redis-pre.xml" redis-pre.xml

mkdir "$basePath/data/redis_pre"
cp redis.windows.conf "$basePath/data/redis_pre/redis.windows.conf"

./redis-pre.exe install

echo -e "${GREEN}Starting cache instances...${NC}"
net start "Redis-PrimeApps"

# Create directory for dump, package, etc.
mkdir "$basePath/data/primeapps"

sleep 3 # Sleep 3 seconds for write database before backup

# Backup
echo -e "${GREEN}Compressing data folders...${NC}"
cd "$basePath/data"
tar -czf pgsql_pre.tar.gz pgsql_pre
tar -czf redis_pre.tar.gz redis_pre
tar -czf minio_pre1.tar.gz minio_pre1
tar -czf minio_pre2.tar.gz minio_pre2
tar -czf minio_pre3.tar.gz minio_pre3
tar -czf minio_pre4.tar.gz minio_pre4

echo -e "${BLUE}Completed${NC}"