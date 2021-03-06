#!/bin/bash
GREEN='\033[0;32m'
YELLOW='\033[0;33m'
BLUE='\033[0;34m'
NC='\033[0m' # No Color

# Variables
os="linux"

if [[ "$OSTYPE" == "msys" ]]
then
	os="win"
elif [[ "$OSTYPE" == "darwin"* ]]
then
	os="osx"
fi

connectionString=""
version="latest"

# Get parameters
for i in "$@"
do
case $i in
    -c=*|--connection-string=*)
    connectionString="${i#*=}"
    ;;
    -v=*|--version=*)
    version="${i#*=}"
    ;;
    *)
    # unknown option
    ;;
esac
done

# Add "v" prefix to version
if [[ ! $version == v* ]] && [ "$version" != "latest" ] ; then
    version="v$version"
fi

fileMigrator=${PRIMEAPPS_FILE_MIGRATOR:-"http://file.primeapps.io/pre/migrator-$os-x64.zip"}

if [ "$version" != "latest" ] ; then
    fileMigrator=${PRIMEAPPS_FILE_MIGRATOR:-"http://file.primeapps.io/pre/$version/migrator-$os-x64.zip"}
fi

# Remove migrator folder and migrator.zip 
rm -rf migrator
rm migrator.zip

# Download Migrator
echo -e "${GREEN}Downloading Migrator...${NC}"
curl $fileMigrator -L --output migrator.zip

# Extract Migrator
echo -e "${GREEN}Extracting Migrator...${NC}"
unzip -q migrator.zip -d migrator
rm migrator.zip

# Run PRE migrate command
cd migrator
echo -e "${GREEN}Running migrate command...${NC}"
./migrator update-pre "$connectionString"

echo -e "${BLUE}Completed${NC}"