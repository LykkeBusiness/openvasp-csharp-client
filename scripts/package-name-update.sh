#!/bin/bash
ReplaceWhat='<PackageId>.*</PackageId>'
BuildVersion="<PackageId>$1<\/PackageId>"
ReplacementString="s@${ReplaceWhat}@${BuildVersion}@g"
FILES=$(find -type f -name '*.csproj')
echo $ReplaceWhat
echo $BuildVersion
for file in $FILES; do
    foundPackageId=$(grep $ReplaceWhat $file)
    if [ "foundPackageId" ]; then
        sed -i $ReplacementString $file
    echo $file
    else
        echo "Not found PackageId property in ${file}"
        exit 1
    fi
done