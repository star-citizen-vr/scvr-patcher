from json import load, dump

new_data = {}

with open('configs.json', 'r') as f:
    data = load(f)

for key, value in data.items():
    print("Processing",key)
    new_value = value
    if isinstance(value["All Possible Lens Configurations"], str):
        value["All Possible Lens Configurations"] = [value["All Possible Lens Configurations"]]
    new_data[key] = new_value


with open('configs-converted.json', 'w') as f:
    dump(new_data, f, indent=4)