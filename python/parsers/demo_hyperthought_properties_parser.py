import sys
import json


'''
Simple example of a python parser that processes standard input line by line, 
assuming 4 comma-delimitted entries on each line: keyname,value,unit,annotation
The unit and annotation fields may be empty (blank strings).
The output is a JSON object that represent a list of HyperthoughtProperty
objects, which in turn can be fed into the HyperthoughtProcessUpdateOperation. 
Note that processing input from standard input and outputting to standard output
allows this parser to be used in the PythonParserOperation.
'''

def main():
    # Set input and output files to standard in and out
    infile = sys.stdin
    outfile = sys.stdout

    # Initialize the output data structure
    data = []

    # Process the file
    for line in infile:
        property = {}
        values = line.strip().split(",")
        property["key"] = values[0]
        property["value"] = values[1]
        if values[2] != "":
            property["unit"] = values[2]
        if values[3] != "":
            property["annotation"] = values[3]
        data.append(property)
    
    # Output in JSON format
    json.dump(data, outfile)

if __name__ == '__main__':
    main()

