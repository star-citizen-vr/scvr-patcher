# merge the keys of two json files recursively but only keep one of the values

import json
import os


def get_files():
    files = []
    # return all json files in hmdq directory
    for f in os.listdir("hmdq"):
        if f.endswith('.json'):
            if "copy" in f:
                print(f"skipping file {f}")
                continue
            print(f"added file {f}")
            files.append("hmdq/" + f)
    return files

def merge_files():
    # merge all json files in hmdq directory
    dicts = []
    for f in get_files():
        with open(f, 'r') as f:
            dicts.append(json.load(f))
            print(f"read file {f}")
    merged = {}
    for i, d in enumerate(dicts):
        merged.update(d)
        print(f"merged dict {i}")
    output_file = "hmdq/merged.json"
    with open(output_file, 'w') as f:
        json.dump(merged, f, indent=4)
        print(f"dumped merged dict to {output_file}")

merge_files()