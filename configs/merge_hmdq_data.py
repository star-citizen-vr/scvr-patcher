# merge the keys of two json files recursively but only keep one of the values

import json

def merge_hmdq_data():
    with open('configs/left.json', 'r') as f:
        hmdq_data = json.load(f)
    with open('configs/right.json', 'r') as f:
        hmdq_data_2 = json.load(f)
    hmdq_data.update(hmdq_data_2)
    with open('configs/merged.json', 'w') as f:
        json.dump(hmdq_data, f, indent=4)
    print('merged.json created')

merge_hmdq_data()