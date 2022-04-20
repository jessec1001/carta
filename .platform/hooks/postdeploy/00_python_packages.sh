#!/bin/bash

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
echo "Content of /var/app/current:"
ls -alh 

# Install Python packages.
# After this point, you can run Python scripts with "pipenv run python3 script.py".
echo "Check pipfiles pre install:"
ls -alh Pip*
echo "Content of Pipfile:"
cat Pipfile
echo "Installing numpy..."
sudo -H -u webapp /home/webapp/.local/bin/pipenv install numpy
echo "numpy install completed"
echo "Check pipfiles post install:"
ls -alh Pip*
echo "Content of Pipfile:"
cat Pipfile
