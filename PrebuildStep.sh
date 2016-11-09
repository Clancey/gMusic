#!/bin/bash
file=buildServerSetup.sh
if [ -e "$file" ]; then
	sh $file
else 
    echo "File does not exist"
fi 