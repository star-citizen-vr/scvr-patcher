# merge the keys of two json files recursively but only keep one of the values

import json
import os


def get_files():
    files = []
    # return all json files in hmdq directory
    for f in os.listdir("hmdq"):
        if f.endswith('.json'):
            if "copy" in f or f == "merged.json":
                print(f"skipping file {f}")
                continue
            print(f"added file {f}")
            files.append("hmdq/" + f)
    return files

def merge_dicts(d1, d2):
    # merge two dicts recursively but only keep one of the values
    for k in d2:
        if k in d1:
            if isinstance(d1[k], dict) and isinstance(d2[k], dict):
                merge_dicts(d1[k], d2[k])
            else:
                print(f"skipping key {k}")
        else:
            d1[k] = d2[k]

def merge_files():
    # merge all json files in hmdq directory
    dicts = []
    for f in get_files():
        with open(f, 'r') as f:
            dicts.append(json.load(f))
            print(f"read file {f}")
    merged = {}
    for i, d in enumerate(dicts):
        print(f"dict {i} has {len(d)} keys")
        print(f"dict {i} has {len(d['oculus'])} oculus keys")
        print(f"dict {i} has {len(d['openvr'])} openvr keys")
        merge_dicts(merged, d) # merged.update(d)
        print(f"merged dict {i}")
    output_file = "hmdq/merged.json"
    with open(output_file, 'w') as f:
        json.dump(merged, f, indent=4)
        print(f"dumped merged dict to {output_file}")

merge_files()