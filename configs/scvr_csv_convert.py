import csv
import json
from re import compile

resolution_regex = compile(r"(\d+)\s[Xx]\s(\d+)\s\((.+?)\)\s*(.+)?")
attributes_static_regex = compile(r"(\w+):\s*([^,]+)")
attributes_nameonly_regex = compile(r"\b(\w+)\b")
# attributes_optional_regex = re.compile(r'(\w+): (\d+) \| \(([\w\s-]+)\)') # Ran into a few issues with this one. Just separated them manually below
attributes_other_regex = compile(r'(?:(?P<key>[^:\n]+):\s*(?P<value>[^,\n]+),?\s*)+') # Having issues using static_regex... built this to specifically handle the 'other values'

# Main
"""
= row["Concatenated Naming"]
= row["Headset Name"]
= row["Headset Brand"]
= row["Headset Model"]
= row["Lens Configuration"]
= row["Physical Per-Eye Resolution Width"]
= row["Physical Per-Eye Resolution Height"]
= row["Render target size (native) Width"]
= row["Render target size (native) Height"]
= row["Refresh Rate Max(Hz)"]
= row["SC Attributes FOV"]
= row["VorpX Config Pixel 1:1 Zoom (Calculated)"]
= row["All Possible Lens Configurations"]
"""
# Resolutions
"""
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

# Attributes
"""
= row["Attributes - HDR Check if Enabled Historically"]
= row["Attributes - FPS Color Correction Profile"]
= row["Attributes - Flight Color Correction Profile"]
= row["Attributes - Scatter Distance Options"]
= row["Attributes - Tessellation Distance Options"]
= row["Attributes - Volumetric Clouds On/Off Options"]
= row["Attributes - Look Ahead"]
= row["Attributes - Head Tracking"]
= row["Attributes - Auto Zoom"]
= row["Attributes - G Force"]
= row["Attributes - Remove Lines"]
= row["Attributes - Static Values"]
= row["Attributes - Other Values"]
"""

def convert_if_number(value: str):
    if value.isdigit(): return int(value)
    return value

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

def static_attributes(attributes: str):
    try:
        matches = attributes_static_regex.findall(attributes)
        if not matches:
            return None

        # Extract values using match.group() as needed
        attribute_dict = {key.strip(): convert_if_number(value.strip()) for key, value in matches}

        # Insert debug print
        #print(f"Debug - static_attributes: {attributes}")
        #print(f"Debug - matches: {matches}")

        return attribute_dict  # Make sure to return the result

    except Exception as e:
        print(f"Error in static_attributes: {e}")
        print(f"Attributes causing the error: {attributes}")
        return None


def optional_attributes(attributes):
    options = [option.strip() for option in attributes.split(',')]
    return options if options else None
"""
def optional_attributes(attributes):
    matches = attributes_optional_regex.findall(attributes)
    if not matches:
        return None

    # Extract values from matches
    attribute_dict = {}
    for match in matches:
        key, value, description = match
        if key not in attribute_dict:
            attribute_dict[key] = []
        attribute_dict[key].append({
            "value": int(value),
            "description": description.strip()
        })

    return attribute_dict if attribute_dict else None
"""
# Try and categorize static values
"""def categorize_attributes(key, value, data):
    categories = {
        
    }
    
    for category, attributes in categories.items():
        if key in attributes:
            if category not in data:
                data[category] = {}
            data[category][key] = value
            return True

    return False
"""
def csv_to_json(csvFilePath, jsonFilePath):
    # Create a dictionary to store the data
    data = {}
    data["common"] = {
        "Attributes": {  # Added "Attributes" section
            "Static Values": {
                "Forced Attributes": {},    # All Important "Static" Attributes, these should be regardless of the user's wants/needs
                "Look Ahead Features": {},  # LookAhead Attributes
                "Head Tracking Features": {},   # HeadTracking (Headtracking) Attributes + Tobii
                "Auto Zoom Features": {},   # AutoZoom Attributes
                "G Force Features": {}   #Gforce Attributes
            }, # All static attributes we should set, should the user decide they want to over ride.
            "Check Lines": [], # Lines that need to be checked from the attributes.xml
            "Remove Lines": [], # Lines that need to be removed from the attributes.xml
            "User Options": {}, # Options listed to the user to choose from.
            "Other": {} # Not sure if we want to do anything with these yet, but here they are.
        },
        "Resolutions": {}
    }
    data["brands"] = {}
    brands = data["brands"]


    # Open the CSV file and read it using csv.DictReader
    with open(csvFilePath, encoding='utf-8') as csvf:
        csvReader = csv.DictReader(csvf)

        # Iterate over each row in the CSV file
        for row in csvReader:
            #print(row)
            # Create a dictionary for each row
            row_dict = {}

            if row['Headset Brand'] not in brands:
                brands[row['Headset Brand']] = {}

            if row['Headset Model'] not in brands[row['Headset Brand']]:
                brands[row['Headset Brand']][row['Headset Model']] = {}

            if row['Lens Configuration'] not in brands[row['Headset Brand']][row['Headset Model']]:
                brands[row['Headset Brand']][row['Headset Model']][row['Lens Configuration']] = {}

            if 'Render target size' not in row_dict:
                row_dict['Render target size'] = {}

            if 'Physical Per-Eye Resolution' not in row_dict:
                row_dict['Physical Per-Eye Resolution'] = {}

            for key, value in row.items():
                # Skip the keys that we don't want to include in the JSON file
                # if key not in ['Headset Name', 'Headset Brand', 'Headset Model', 'Lens Configuration', 'Concatenated Naming', 'All Possible Lens Configurations', 'Unique Database Identifier', 'FOV hor. (degrees)', 'FOV ver. (degrees)', 'FOV diag. (degrees)', 'Overlap (degrees)', 'HAM (hidden area mask - percentage)', 'Rot LE (view geometry - degrees)', 'Rot RE (view geometry - degrees)', 'FOV H-value (variable)', 'FOV V-value (variable)', 'FOV D-Value (variable)', 'FOV Overlap (variable)', 'Render target size (native) Width', 'Render target size (native) Height', 'Refresh Rate Max(Hz)', 'Note', 'Monitor (Ignore me)', 'SC Attributes FOV', 'Error Report (SC FOV Cap 120)', 'Raw Calculated Pixel Zoom Minimum', 'Raw Calculated Pixel Zoom Maximum', 'VorpX Pixel 1:1 Variable-Min', 'VorpX Pixel 1:1 Variable-Max', 'VorpX Config Pixel 1:1 Zoom (Calculated)', 'Error Report (Too Much Zoom For VorpX)', 'VorpX User Max (Not Complete)', 'Concatenated Notes+Errors', 'Native Horizontal', 'Native Vertical', 'Native Aspect Ratio', '4:3 Translation (H-locked) Width', '4:3 Translation (H-locked) Height', 'H-Locked Aspect Check', '4:3 Translation (V-limited) Width', '4:3 Translation (V-limited) Height', 'H-Limited Aspect Check', 'Custom Resolutions V-Translated', 'Custom Resolutions H-Translated (Preferred)', 'Combined Custom Resolutions', 'Every 6th up to 5440 x 4080', 'Every 8th up to 5440 x 4080', 'Every 10th up to 5440 x 4080', 'Every 6th+8th up to 5440 x 4080', 'Every 6th+8th+10th up to 5440 x 4080', 'All Integer Resolutions up to 5440 x 4080', 'Every 6th up to 19840 x 14880', 'Every 8th up to 19840 x 14880', 'Every 10th up to 19840 x 14880', 'Every 6th+8th up to 19840 x 14880', 'Every 6th+8th+10th up to 19840 x 14880', 'All Integer Resolutions up to 19840 x 14880']:
                #print(f"Processing key: {key}, value: {value}")
                # Try to categorize the attribute based on its functionality
                # Add the key-value pair to the row dictionary
                if not value or value.strip() == '': continue
                if key in ["Give me all the resolutions", "Alternative Integer Resolutions (big list)", "Alternative Integer Resolutions (small list)"]:
                    data['common']['Resolutions'][key] = [split_resolution(v) for v in value.split(', ') if v]
                    continue
                if key in [
                    "Monitor (Ignore me)",
                    "Concatenated Naming",
                    "Headset Name",
                    "Headset Brand",
                    "Headset Model",
                    "Lens Configuration",
                ]: continue
                # Grab the static attributes
                if key in ["Attributes - Static Values", "Attributes - HeadTracking"]:
                    static_values = data['common']['Attributes']['Static Values']
                    attributes_result = static_attributes(value)
                    if attributes_result is not None:
                        if isinstance(attributes_result, dict):
                            # Move all attributes to Forced Attributes under Static Values
                            static_values['Forced Attributes'].update(attributes_result)
                        else:
                            print(f"Invalid static attributes format for key {key}: {attributes_result}")
                    continue

                # Grab the Look Ahead attributes
                elif key == "Attributes - Look Ahead":
                    static_values = data['common']['Attributes']['Static Values']
                    attributes_result = static_attributes(value)
                    if attributes_result is not None:
                        if isinstance(attributes_result, dict):
                            # Move all attributes to Look Ahead Features under Static Values
                            static_values['Look Ahead Features'].update(attributes_result)
                        else:
                            print(f"Invalid Look Ahead attributes format: {attributes_result}")
                    continue

                # Grab the Head Tracking attributes
                elif key == "Attributes - Head Tracking":
                    static_values = data['common']['Attributes']['Static Values']
                    attributes_result = static_attributes(value)
                    if attributes_result is not None:
                        if isinstance(attributes_result, dict):
                            # Move all attributes to Head Tracking Features under Static Values
                            static_values['Head Tracking Features'].update(attributes_result)
                        else:
                            print(f"Invalid Head Tracking attributes format: {attributes_result}")
                    continue

                # Grab the Auto Zoom attributes
                elif key == "Attributes - Auto Zoom":
                    static_values = data['common']['Attributes']['Static Values']
                    attributes_result = static_attributes(value)
                    if attributes_result is not None:
                        if isinstance(attributes_result, dict):
                            # Move all attributes to Auto Zoom Features under Static Values
                            static_values['Auto Zoom Features'].update(attributes_result)
                        else:
                            print(f"Invalid Auto Zoom attributes format: {attributes_result}")
                    continue

                # Grab the G Force attributes
                elif key == "Attributes - G Force":
                        try:
                            static_values = data['common']['Attributes']['Static Values']
                            attributes_result = static_attributes(value)
                            if attributes_result is not None:
                                if isinstance(attributes_result, dict):
                                    # Move all attributes to G Force Features under Static Values
                                    static_values['G Force Features'].update(attributes_result)
                                else:
                                    print(f"Invalid G Force attributes format: {attributes_result}")
                        except KeyError as e:
                            print(f"KeyError: {e}")
                            print(f"Row data: {row}")
                        continue                

                # Grab the Remove Attribute Lines
                elif key == "Attributes - Remove Lines":
                    data['common']['Attributes']['Remove Lines'] = value.split(', ')
                    continue
                # Grab the HDR Attributes Lines
                elif key == "Attributes - HDR Check if Enabled Historically":
                    data['common']['Attributes']['Check Lines'] = value.split(', ')
                    continue
                # Other Values - attributes_other_regex
                elif key == "Attributes - Other Values":
                    #print("Before regex match:", value)
                    other_attributes_matches = attributes_nameonly_regex.finditer(value)
                    if other_attributes_matches:
                        other_attributes = [match.group(1) for match in other_attributes_matches]
                        #print("After regex match:", other_attributes)
                        #print("Before update:", data['common']['Attributes']['Other'])
                        data['common']['Attributes']['Other'] = other_attributes
                        #print("After update:", data['common']['Attributes']['Other'])
                    else:
                        print(f"Invalid Other attributes format: {value}")
                    continue
                # Grab the user optional attributes together
                optional_values = data.get('common', {}).get('Attributes', {}).get('User Options', {}) # Temp
                if key in ["Attributes - Scatter Distance Options", "Attributes - Tessellation Distance Options", "Attributes - Volumetric Clouds On/Off Options"]:
                    if key not in optional_values:
                        optional_values[key] = []
                    options = value.split('; ')
                    try:
                        if key == "Attributes - Scatter Distance Options":
                            attr_list = [{"ScatterDist": int(option.split(':')[-1].strip().split()[0]), "Description": option.split(' | ')[1]} for option in options]
                        elif key == "Attributes - Tessellation Distance Options":
                            attr_list = [{"TerrainTessDistance": int(option.split(':')[-1].strip().split()[0]), "Description": option.split(' | ')[1]} for option in options]
                        elif key == "Attributes - Volumetric Clouds On/Off Options":
                            attr_list = [] # [{"SysSpecPlanetVolumetricClouds": int(option.split(':')[-1].strip().split()[0]), "Description": option.split(' | ')[1]} for option in options]
                            for option in options:
                                option_split = option.split(' | ')
                                attrib_value = option_split[0].split(': ')
                                description = option_split[1]
                                attr_list.append({"SysSpecPlanetVolumetricClouds": int(attrib_value[1]), "Description": description})
                        optional_values[key] = attr_list
                    except ValueError as e:
                        print(f"Error processing {key}: {e}") # Debug
                        # print(f"Row data: {row}") # Hope you don't have to use this to debug :P
                    continue
                if key == "Concatenated Notes+Errors":
                    key = "Notes"
                    if value.replace(' ','').replace('|','') == '' : value = list()
                    else: value = value.split(' | ')
                elif key in ["Custom Resolution List","Custom Resolutions V-Translated","Custom Resolutions H-Translated (Preferred)","Combined Custom Resolutions","Every 6th up to 5440 x 4080","Every 8th up to 5440 x 4080","Every 10th up to 5440 x 4080","Every 6th+8th up to 5440 x 4080","Every 6th+8th+10th up to 5440 x 4080","All Integer Resolutions up to 5440 x 4080","Every 6th up to 19840 x 14880","Every 8th up to 19840 x 14880","Every 10th up to 19840 x 14880","Every 6th+8th up to 19840 x 14880","Every 6th+8th+10th up to 19840 x 14880","All Integer Resolutions up to 19840 x 14880"]:
                    value = [split_resolution(v) for v in value.split(', ') if v]
                elif key in ["All Possible Lens Configurations"]:
                    value = value.split(', ')
                elif key == "Physical Per-Eye Resolution Width":
                    row_dict["Physical Per-Eye Resolution"]["w"] = value
                    continue
                elif key == "Physical Per-Eye Resolution Height":
                    row_dict["Physical Per-Eye Resolution"]["h"] = value
                    continue
                elif key == "Render target size (native) Width":
                    row_dict["Render target size"]["w"] = value
                    continue
                elif key == "Render target size (native) Height":
                    row_dict["Render target size"]["h"] = value
                    continue
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

"""
newlist = []
newdict = {}

newlist.append("trgdefgbfg")

newdict["eqweq"] = "fdsmflsdkf"
"""