#!/bin/bash

# For logging purposes, make sure Python3 and Pip3 are installed.
pwd
python3 --version
pip3 --version

# Install pipenv.
pip3 install --user pipenv

# Change to the execution directory.
cd /var/app/current

# Install Python packages.
# After this point, you can run Python scripts with "pipenv run python3 script.py".
pipenv install numpy