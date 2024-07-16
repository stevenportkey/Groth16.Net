#!/bin/bash

# Get the directory of the script
script_dir=$(dirname "$0")

# Define the path to the configuration file
config_file="$script_dir/config.sh"

# Check if the configuration file exists
if [ ! -f "$config_file" ]; then
    echo "Error: Configuration file not found"
    exit 1
fi

# Source the configuration file
source $config_file

# Print the version number
echo "Version: $LIB_VERSION"

# Define the array of platforms and their respective destination subfolders
platforms=("linux-amd64" "linux-arm64" "win-amd64" "osx-amd64" "osx-arm64") # remote
subfolders=("linux-x64" "linux-arm64" "win-x64" "osx-x64" "osx-arm64") # local

# Loop over the platforms
for index in "${!platforms[@]}"
do
    echo "Downloading: ${platforms[index]}"
    # Define the URL
    url="https://github.com/stevenportkey/libgroth16/releases/download/${LIB_VERSION}/libgroth16-${platforms[index]}-${LIB_VERSION}.zip"

    # Define the target directory relative to the script directory
    dir="$script_dir/../Groth16.Net/native/${subfolders[index]}"

    # Create the target directory if it doesn't exist
    mkdir -p $dir

    # Download the zip file to the target directory
    curl -L $url --silent -o $dir/bin.zip

    # Unzip the downloaded file
    unzip -o $dir/bin.zip -d $dir

    # Remove the downloaded zip file
    rm $dir/bin.zip
done