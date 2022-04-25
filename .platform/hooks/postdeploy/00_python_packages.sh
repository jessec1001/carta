#!/bin/bash

# Echo on
set -x 

# For logging purposes, make sure Python3 and Pip3 are installed.
pwd
python3 --version
pip3 --version

# Install pipenv.
echo "Installing pipenv..."
sudo -H -u webapp pip3 install --user pipenv
echo "pipenv install completed"

# Change to the execution directory.
cd /var/app/current

# Remove any previously installed environments
# (Workaround for https://github.com/pypa/pipenv/issues/5052 which was run into)
sudo -H -u webapp /home/webapp/.local/bin/pipenv --rm

# Install Python packages.
# After this point, you can run Python scripts with "pipenv run python3 script.py".
sudo -H -u webapp /home/webapp/.local/bin/pipenv install numpy
