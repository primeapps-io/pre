#!/bin/bash
GREEN='\033[0;32m'
NC='\033[0m' # No Color

PORTAUTH=5000

for i in "$@"
do
case $i in
    -pa=*|--port-auth=*)
    PORTAUTH="${i#*=}"
    ;;
    *)
    # unknown option
    ;;
esac
done

echo -e "${GREEN}auth${NC}"

cd ..
cd PrimeApps.Auth

export ASPNETCORE_ENVIRONMENT=Production
export AppSettings__Authority="http://localhost:$PORTAUTH"

dotnet PrimeApps.Auth.dll --urls="http://localhost:$PORTAUTH"