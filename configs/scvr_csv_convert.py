import csv
import json
import re
from re import compile

resolution_regex = compile(r"(\d+)\s[Xx]\s(\d+)\s\((.+?)\)\s*(.+)?")
attributes_fixed_regex = compile(r"(\w+):\s*([^,]+)")
attributes_nameonly_regex = compile(r"\b(\w+)\b")
attributes_optional_regex = compile(r'(\w+): (\d+) \| \(([\w\s-]+)\)')

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
= row["FOV H-check"]
= row["H-FOV is same?"]
= row["FOV V-check"]
= row["V-FOV is same?"]
= row["FOV hor. Deviation"]
= row["FOV hor. Scaled Value"]
= row["FOV hor. Coefficient"]
= row["FOV ver. Deviation"]
= row["FOV ver. Scaled Value"]
= row["FOV ver. Coefficient"]
= row["FOV diag. Deviation"]
= row["FOV diag. Scaled Value"]
= row["FOV diag. Coefficient"]
= row["Overlap Deviation"]
= row["Overlap Scaled Value"]
= row["Overlap Coefficient"]
= row["HAM Deviation"]
= row["HAM Scaled Value"]
= row["HAM Coefficient"]
= row["Bias"]
= row["SC Attributes FOV"]
= row["Physical Per-Eye Resolution Width"]
= row["Physical Per-Eye Resolution Height"]
= row["Render target size (native) Width"]
= row["Render target size (native) Height"]
= row["Refresh Rate Max(Hz)"]
= row["Refresh Rate Max(Hz) - VRCompare"]
= row["Refresh is same?"]
= row["Note"]
= row["Error Report (SC FOV Cap 120)"]
= row["Raw Calculated Pixel Zoom Minimum"]
= row["Raw Calculated Pixel Zoom Maximum"]
= row["VorpX Pixel 1:1 Variable-Min"]
= row["VorpX Pixel 1:1 Variable-Max"]
= row["VorpX Config Pixel 1:1 Zoom (Calculated)"]
= row["V6 Calculator (WIP)"]
= row["Desired Output"]
= row["Error Report (Too Much Zoom For VorpX)"]
= row["Brightness Max (%)"]
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
= row["Attributes - FPS Color Correction Profile"]
= row["Attributes - Flight Color Correction Profile"]
= row["Attributes - Scatter Distance Options"]
= row["Attributes - Tesselation Distance Options"]
= row["Attributes - Volumetric Clouds On/Off Options"]
= row["Attributes - HeadTracking"]
= row["Attributes - Remove These Lines from Attributes.xml"]
= row["Attributes - Fixed Values"]
= row["Attributes - Other"]
= row["Color Correction - FPS"]
= row["Color Correction - Flight"]
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

def fixed_attributes(attributes: str):
    matches = attributes_fixed_regex.findall(attributes)
    if not matches:
        return None

    # Extract values using match.group() as needed
    attribute_dict = {key.strip(): value.strip() for key, value in matches}
    return attribute_dict

def remove_attributes(attributes):
    matches = attributes_nameonly_regex.findall(attributes)
    if not matches:
        return None
    return {attribute: None for attribute in matches}

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
# Try and categorize fixed values
"""
def categorize_attributes(key, value, data):
    categories = {
        "Zoom Features": ["AutoZoomOnSelectedTarget", "AutoZoomOnSelectedTargetStrength", "ZoomSensitivityMultiplierToggle"],
        "GForce Features": ["GForceHeadBobScale", "GForceZoomScale"],
        "HeadTracking Features": ["HeadTrackingFaceWareDeadzoneRotationPitch", "HeadTrackingFaceWareDeadzoneRotationRoll", "HeadTrackingFaceWareDeadzoneRotationYaw", "HeadTrackingFaceWareSmoothingThreshold", "HeadTrackingFacewarePitchMultiplier", "HeadTrackingFacewareYawMultiplier", "HeadtrackingDisableDuringADS", "HeadtrackingDisableDuringWalking", "HeadtrackingEnableRollFPS", "HeadtrackingGlobalSmoothingPosition", "HeadtrackingGlobalSmoothingRotation", "HeadtrackingInactivityTime", "HeadtrackingSource", "HeadtrackingThirdPersonCameraToggle", "HeadtrackingToggle", "HeadtrackingToggleAutoCalibrate"],
        "LookAhead Features": ["LookAheadStrengthForward", "LookAheadStrengthHorizonAlignment", "LookAheadStrengthHorizonLookAt", "LookAheadStrengthJumpPointSpline", "LookAheadStrengthMgvForward", "LookAheadStrengthMgvHorizonAlignment", "LookAheadStrengthMgvPitchYaw", "LookAheadStrengthMgvVJoy", "LookAheadStrengthQuantumBoostTarget", "LookAheadStrengthRoll", "LookAheadStrengthTargetSoft", "LookAheadStrengthTurretForward", "LookAheadStrengthTurretPitchYaw", "LookAheadStrengthTurretVJoy", "LookAheadStrengthVJoy", "LookAheadStrengthVelocityVector", "LookAheadStrengthYawPitch"],
        "Tobii Features": ["TobiiHeadPositionScale", "TobiiHeadSensitivityRoll_Profile0", "TobiiHeadSensitivityRoll_Profile1"],
        "Fixed Features": ["ChromaticAberration", "FilmGrain", "MotionBlur", "ShakeScale", "Sharpening", "VSync", "WindowMode"]
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
            "Fixed Values": {}, # All fixed values (including headtracking)
            "Remove me from attributes.xml if exists": {}, # Lines that need to be removed from the attributes.xml
            "User Options": {}, # Options listed to the user to choose from.
            "Other": {} # Not sure if we want to do anything with these yet, but here they are.
        },
        "Resolutions": {
            "Alternative Integer Resolutions (big list)": [],
            "Give me all the resolutions": [],
            "All Possible Lens Configurations": []
        }
    }
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
                
                # Try to categorize the attribute based on its functionality
                """
                if key in ["Attributes - Fixed Values", "Attributes - HeadTracking"]:
                    fixed_values = data['common']['Attributes']['Fixed Values']
                    if not categorize_attributes(key, value, fixed_values):
                        # If not categorized, add it directly to "Fixed Features"
                        fixed_values[key] = value
                    continue
                """
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
                # Grab the fixed attrbiutes
                if key in ["Attributes - Fixed Values", "Attributes - HeadTracking"]:
                    fixed_values = data['common']['Attributes']['Fixed Values']
                    fixed_values.update(fixed_attributes(value))
                    continue
                # Grab the remove me attributes
                if key in ["Attributes - Remove These Lines from Attributes.xml"]:
                    remove_attributes_dict = data['common']['Attributes']['Remove me from attributes.xml if exists']
                    remove_attributes_dict.update(remove_attributes(value))
                    continue

                # Grab the user optional attributes together
                optional_values = data.get('common', {}).get('Attributes', {}).get('User Options', {})
                if key in ["Attributes - Scatter Distance Options", "Attributes - Tesselation Distance Options", "Attributes - Volumetric Clouds On/Off Options"]:
                    if key not in optional_values:
                        optional_values[key] = []
                    options = value.split(', ')
                    try:
                        if key == "Attributes - Scatter Distance Options":
                            attr_list = [{"ScatterDist": int(option.split(':')[-1].strip().split()[0]), "Description": option.split(' | ')[1]} for option in options]
                        elif key == "Attributes - Tesselation Distance Options":
                            attr_list = [{"TerrainTessDistance": int(option.split(':')[-1].strip().split()[0]), "Description": option.split(' | ')[1]} for option in options]
                        elif key == "Attributes - Volumetric Clouds On/Off Options":
                            attr_list = [{"SysSpecPlanetVolumetricClouds": int(option.split(':')[-1].strip().split()[0]), "Description": option.split(' | ')[1]} for option in options]
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

"""
newlist = []
newdict = {}

newlist.append("trgdefgbfg")

newdict["eqweq"] = "fdsmflsdkf"


"common": {
	"Attributes": {
		"Fixed Values":{
			"ChromaticAberration": 0,
			"FilmGrain": 0,
			"MotionBlur": 0,
			"ShakeScale": 1,
			"Sharpening": 1,
			"VSync": 0,
			"WindowMode": 2,
        }
	},
	"Resolutions: {
		"Alternate Ineger Resolutions (small list)":,
		"Alternative Integer Resolutions (big list)",
		"Give me all the resolutions",
		}
	},
"brands": {
"""