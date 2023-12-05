import json
import xml.etree.ElementTree as ET
import re

def load_configurations(filename):
    with open(filename, 'r') as file:
        configurations = json.load(file)
    return configurations

def update_xml_config(xml_filename, fov, height, width, vsync_value, window_mode_value):
    tree = ET.parse(xml_filename)
    root = tree.getroot()

    for attr in root.findall(".//Attr"):
        if attr.attrib["name"] == "FOV":
            attr.set("value", str(fov))
        elif attr.attrib["name"] == "Height":
            attr.set("value", str(height))
        elif attr.attrib["name"] == "Width":
            attr.set("value", str(width))
        elif attr.attrib["name"] == "VSync":
            # Update VSync value
            attr.set("value", str(vsync_value))
        elif attr.attrib["name"] == "WindowMode":
            # Update WindowMode value
            attr.set("value", str(window_mode_value))

    tree.write(xml_filename)

def print_ordered_options(options, additional_option=None):
    for index, option in enumerate(options, 1):
        print(f"{index}) {option}")

    if additional_option:
        print(f"{len(options) + 1}) {additional_option}")

def print_resolutions(resolutions, header="Available resolutions:"):
    print(header)
    for index, resolution in enumerate(resolutions, 1):
        print(f"{index}) {resolution}")

def main():
    json_filename = 'SCVR.json'
    xml_filename = 'attributes.xml'

    configurations = load_configurations(json_filename)

    # Prompt user for brand selection
    available_brands = sorted(set(configurations[key]["Headset Brand"] for key in configurations))
    print("Available headset brands:")
    for index, brand in enumerate(available_brands, 1):
        print(f"{index}. {brand}")
    
    brand_index = int(input("Enter the index of the desired headset brand: ")) - 1
    selected_brand = available_brands[brand_index]

    # Prompt user for headset model selection
    available_models = sorted(set(f"{configurations[key]['Headset Brand']} {configurations[key]['Headset Model']}" for key in configurations if configurations[key]["Headset Brand"] == selected_brand))
    print(f"Available headset models for {selected_brand}:")
    for index, model in enumerate(available_models, 1):
        print(f"{index}. {model}")
    
    model_index = int(input("Enter the index of the desired headset model: ")) - 1
    selected_model_full = available_models[model_index]
    
    # Extracting only the model name from the full string
    selected_model = selected_model_full.split(maxsplit=1)[-1]

    # Find the correct key for the selected brand and model
    for key in configurations:
        if configurations[key]["Headset Brand"] == selected_brand and configurations[key]["Headset Model"] == selected_model:
            break
    else:
        print("Selected brand and model not found in the configurations.")
        return

    # Check if there's only one lens configuration available
    available_lens_configs = configurations[key]["All Possible Lens Configurations"]
    available_lens_configs = [config for config in available_lens_configs if config != "Choose One"]

    if len(available_lens_configs) == 1:
        selected_lens_config = available_lens_configs[0]
    else:
        # Prompt user for lens configuration selection
        print(f"Available lens configurations for {selected_brand} {selected_model}:")
        for index, lens_config in enumerate(available_lens_configs, 1):
            print(f"{index}. {lens_config}")
        lens_index = int(input("Enter the index of the desired lens configuration: ")) - 1
        selected_lens_config = available_lens_configs[lens_index]

    # Display resolutions based on user's choice
    additional_options = [
        "Alternative Integer Resolutions (small list)",
        "Alternative Integer Resolutions (big list)",
        "Give me all the resolutions"
    ]
    print_ordered_options(additional_options)

    user_choice = int(input("Enter the index of your choice: ")) - 1

    additional_option = additional_options[user_choice]

    # Display resolutions based on user's choice
    if additional_option.startswith("Alternative Integer Resolutions"):
        # Extract the resolution key from the additional_option
        resolution_key = additional_option.split()[-3].lower()
        resolutions = configurations[key][resolution_key]
        print_resolutions(resolutions)
    elif additional_option == "Give me all the resolutions":
        # Display all available resolutions
        resolutions = configurations[key]["resolutions"]
        print_resolutions(resolutions)
    else:
        print("Invalid choice. Exiting...")
        return

    # Prompt user for resolution selection
    while True:
        try:
            resolution_index = int(input("Enter the resolution index: ")) - 1

            if 0 <= resolution_index < len(configurations[key]["resolutions"]):
                break  # Valid index, exit the loop
            else:
                print("Invalid resolution index. Please enter a valid index.")
        except ValueError:
            print("Invalid input. Please enter a valid number.")

    selected_resolution = configurations[key]["resolutions"][resolution_index]
    print(f"Selected resolution: {selected_resolution}")

    # Use regular expression to extract numeric values from the resolution string
    match = re.search(r'(\d+)\s*X\s*(\d+)', selected_resolution)
    if match:
        width, height = map(int, match.groups())
    else:
        print("Invalid resolution format. Please enter resolutions in the format 'width X height'.")
        return

    # Update XML file with selected values and disable VSync
    update_xml_config(xml_filename, configurations[key]["SC Attributes FOV"], height, width)

    # Get the recommended VorpX Config Pixel 1:1 Zoom value
    vorpx_config_zoom = configurations[key]["VorpX Config Pixel 1:1 Zoom"]

    # Print all selections made by the user
    print("\nSelected Configuration:")
    print(f"Headset: {selected_brand} {selected_model}")
    print(f"Lens Configuration: {selected_lens_config}")
    print(f"Resolution: {selected_resolution}")
    print(f"\033[1mRecommended setting your VorpX Pixel 1:1 Zoom value to at least:\033[0m {vorpx_config_zoom}")
    print("VSync has been disabled (recommended)")

    # Print the monitor mode setting message
    print("SC window mode set to Borderless")

    print("\nConfiguration updated successfully!")

if __name__ == "__main__":
    main()

# Pause to allow the user to see and take note of the vorpx zoom value
input("Press Enter to close... Else, you can close me now.")