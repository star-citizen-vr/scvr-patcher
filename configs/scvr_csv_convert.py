import csv
import json
from re import compile

resolution_regex = compile(r"(\d+)\s[Xx]\s(\d+)\s\((.+?)\)\s*(.+)?")

"""
= row["Headset Name"]
= row["Headset Brand"]
= row["Headset Model"]
= row["Lens Configuration"]
= row["Concatenated Naming"]
= row["All Possible Lens Configurations"]
= row["Unique Database Identifier"]
= row["FOV hor. (degrees)"]
= row["FOV ver. (degrees)"]
= row["FOV diag. (degrees)"]
= row["Overlap (degrees)"]
= row["HAM (hidden area mask - percentage)"]
= row["Rot LE (view geometry - degrees)"]
= row["Rot RE (view geometry - degrees)"]
= row["FOV H-value (variable)"]
= row["FOV V-value (variable)"]
= row["FOV D-Value (variable)"]
= row["FOV Overlap (variable)"]
= row["Render target size (native) Width"]
= row["Render target size (native) Height"]
= row["Refresh Rate Max(Hz)"]
= row["Note"]
= row["Monitor (Ignore me)"]
= row["SC Attributes FOV"]
= row["Error Report (SC FOV Cap 120)"]
= row["Raw Calculated Pixel Zoom Minimum"]
= row["Raw Calculated Pixel Zoom Maximum"]
= row["VorpX Pixel 1:1 Variable-Min"]
= row["VorpX Pixel 1:1 Variable-Max"]
= row["VorpX Config Pixel 1:1 Zoom (Calculated)"]
= row["Error Report (Too Much Zoom For VorpX)"]
= row["VorpX User Max (Not Complete)"]
= row["Concatenated Notes+Errors"]
= row["Native Horizontal"]
= row["Native Vertical"]
= row["Native Aspect Ratio"]
= row["4:3 Translation (H-locked) Width"]
= row["4:3 Translation (H-locked) Height"]
= row["H-Locked Aspect Check"]
= row["4:3 Translation (V-limited) Width"]
= row["4:3 Translation (V-limited) Height"]
= row["H-Limited Aspect Check"]
= row["Custom Resolutions V-Translated"]
= row["Custom Resolutions H-Translated (Preferred)"]
= row["Combined Custom Resolutions"]
= row["Every 6th up to 5440 x 4080"]
= row["Every 8th up to 5440 x 4080"]
= row["Every 10th up to 5440 x 4080"]
= row["Every 6th+8th up to 5440 x 4080"]
= row["Every 6th+8th+10th up to 5440 x 4080"]
= row["All Integer Resolutions up to 5440 x 4080"]
= row["Every 6th up to 19840 x 14880"]
= row["Every 8th up to 19840 x 14880"]
= row["Every 10th up to 19840 x 14880"]
= row["Every 6th+8th up to 19840 x 14880"]
= row["Every 6th+8th+10th up to 19840 x 14880"]
= row["All Integer Resolutions up to 19840 x 14880"]
"""

def split_resolution(resolution: str):
    match = resolution_regex.match(resolution)
    if not match: return None
    grps = len(match.groups())
    if grps < 1: return None
    dic = {}
    if match.group(1): dic["w"] = int(match.group(1))
    if match.group(2): dic["h"] = int(match.group(2))
    if match.group(3): dic["d"] = match.group(3).strip()
    if match.group(4): dic["p"] = match.group(4).replace('%','').strip()
    return dic

def csv_to_json(csvFilePath, jsonFilePath):
   # Create a dictionary to store the data
   data = {}
   data["common"] = {}
   data["brands"] = {}
   brands = data["brands"]


   # Open the CSV file and read it using csv.DictReader
   with open(csvFilePath, encoding='utf-8') as csvf:
       csvReader = csv.DictReader(csvf)

       # Iterate over each row in the CSV file
       for row in csvReader:
            #    print(row)
            # Create a dictionary for each row
            row_dict = {}

            if row['Headset Brand'] not in brands:
                brands[row['Headset Brand']] = { }

            if row['Headset Model'] not in brands[row['Headset Brand']]:
                brands[row['Headset Brand']][row['Headset Model']] = { }

            if row['Lens Configuration'] not in brands[row['Headset Brand']][row['Headset Model']]:
                brands[row['Headset Brand']][row['Headset Model']][row['Lens Configuration']] = {}

            for key, value in row.items():
                # Skip the keys that we don't want to include in the JSON file
                # if key not in ['Headset Name', 'Headset Brand', 'Headset Model', 'Lens Configuration', 'Concatenated Naming', 'All Possible Lens Configurations', 'Unique Database Identifier', 'FOV hor. (degrees)', 'FOV ver. (degrees)', 'FOV diag. (degrees)', 'Overlap (degrees)', 'HAM (hidden area mask - percentage)', 'Rot LE (view geometry - degrees)', 'Rot RE (view geometry - degrees)', 'FOV H-value (variable)', 'FOV V-value (variable)', 'FOV D-Value (variable)', 'FOV Overlap (variable)', 'Render target size (native) Width', 'Render target size (native) Height', 'Refresh Rate Max(Hz)', 'Note', 'Monitor (Ignore me)', 'SC Attributes FOV', 'Error Report (SC FOV Cap 120)', 'Raw Calculated Pixel Zoom Minimum', 'Raw Calculated Pixel Zoom Maximum', 'VorpX Pixel 1:1 Variable-Min', 'VorpX Pixel 1:1 Variable-Max', 'VorpX Config Pixel 1:1 Zoom (Calculated)', 'Error Report (Too Much Zoom For VorpX)', 'VorpX User Max (Not Complete)', 'Concatenated Notes+Errors', 'Native Horizontal', 'Native Vertical', 'Native Aspect Ratio', '4:3 Translation (H-locked) Width', '4:3 Translation (H-locked) Height', 'H-Locked Aspect Check', '4:3 Translation (V-limited) Width', '4:3 Translation (V-limited) Height', 'H-Limited Aspect Check', 'Custom Resolutions V-Translated', 'Custom Resolutions H-Translated (Preferred)', 'Combined Custom Resolutions', 'Every 6th up to 5440 x 4080', 'Every 8th up to 5440 x 4080', 'Every 10th up to 5440 x 4080', 'Every 6th+8th up to 5440 x 4080', 'Every 6th+8th+10th up to 5440 x 4080', 'All Integer Resolutions up to 5440 x 4080', 'Every 6th up to 19840 x 14880', 'Every 8th up to 19840 x 14880', 'Every 10th up to 19840 x 14880', 'Every 6th+8th up to 19840 x 14880', 'Every 6th+8th+10th up to 19840 x 14880', 'All Integer Resolutions up to 19840 x 14880']:
                # Add the key-value pair to the row dictionary
                if not value or value.strip() == '': continue
                if key in ["Give me all the resolutions","Alternative Interger Resolutions (big list)","Alternative Interger Resolutions (small list)"]:
                    data['common'][key] = [split_resolution(v) for v in value.split(', ') if v]
                    continue
                if key in [
                    "Monitor (Ignore me)",
                    "Concatenated Naming",
                    "Headset Name",
                    "Headset Brand",
                    "Headset Model",
                    "Lens Configuration",
                ]: continue
                if key == "Concatenated Notes+Errors":
                    key = "Notes"
                    if value.replace(' ','').replace('|','') == '' : value = list()
                    else: value = value.split(' | ')
                elif key in ["Custom Resolution List","Custom Resolutions V-Translated","Custom Resolutions H-Translated (Preferred)","Combined Custom Resolutions","Every 6th up to 5440 x 4080","Every 8th up to 5440 x 4080","Every 10th up to 5440 x 4080","Every 6th+8th up to 5440 x 4080","Every 6th+8th+10th up to 5440 x 4080","All Integer Resolutions up to 5440 x 4080","Every 6th up to 19840 x 14880","Every 8th up to 19840 x 14880","Every 10th up to 19840 x 14880","Every 6th+8th up to 19840 x 14880","Every 6th+8th+10th up to 19840 x 14880","All Integer Resolutions up to 19840 x 14880"]:
                    value = [split_resolution(v) for v in value.split(', ') if v]
                elif key == "All Possible Lens Configurations":
                    value = value.split(', ')
                else:
                    if value.startswith('W '): value = value[2:]
                    elif value.startswith('H '): value = value[2:]
                    if value.endswith('Hz'): value = value[:-2]
                    if value.endswith('px'): value = value[:-2]
                    if 'aw ' in value: exit(1) # pls fix in csv
                    value = value.replace('\u00b0','')
                if not isinstance(value, list):
                    if value.isdigit(): value = int(value)
                    try:
                        newv = float(value)
                        value = newv
                    except: pass
                    # if isinstance(value, float): value = float(value)
                row_dict[key] = value

            # Add the row dictionary to the data dictionary
            brands[row['Headset Brand']][row['Headset Model']][row['Lens Configuration']] = row_dict

   data["brands"] = brands

   # Open the output JSON file and write the data to it
   with open(jsonFilePath, 'w', encoding='utf-8') as jsonf:
       jsonf.write(json.dumps(data, indent=4))

csvFilePath = 'Simplified_Configurations.csv'
jsonFilePath = 'configs-converted.json'
csv_to_json(csvFilePath, jsonFilePath)
