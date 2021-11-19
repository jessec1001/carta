import sys
import json


'''
Simple example of a python parser that processes standard input line by line, 
assuming 2 comma-delimitted entries on each line (key,value),
and outputs a JSON object with key=value items to standard output. 
Importantly, processing input from standard input and outputting to standard output
allows this parser to be used in the PythonParserOperation.
'''

def main():
    # Set input and output files to standard in and out
    infile = sys.stdin
    outfile = sys.stdout

    # Initialize the output data structure
    data = {}

    # Process the file
    for line in infile:
        values = line.strip().split(",")
        data[values[0]] = values[1]
    
    # Output in JSON format
    json.dump(data, outfile)

if __name__ == '__main__':
    main()

