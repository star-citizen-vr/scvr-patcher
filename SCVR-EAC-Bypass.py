import os
import json
import tkinter as tk
from tkinter import *
from tkinter import messagebox
from PIL import Image, ImageTk
import ctypes  # Import ctypes to check and request admin privileges

def is_admin():
    try:
        return ctypes.windll.shell32.IsUserAnAdmin()
    except:
        return False

def is_hosts_updated():
    try:
        hosts_path = r"C:\Windows\System32\drivers\etc\hosts"
        hosts_content = "\n#Star Citizen EAC workaround\n127.0.0.1        modules-cdn.eac-prod.on.epicgames.com"

        with open(hosts_path, 'r') as hosts_file:
            existing_content = hosts_file.read()

        # Check if the lines are already present
        return hosts_content not in existing_content

    except FileNotFoundError:
        return False  # Indicate failure: File not found

    except Exception as e:
        return False  # Indicate failure: An error occurred

def update_settings(directory, new_values):
    try:
        file_path = os.path.join(directory, 'settings.json')
        # Load the JSON file
        with open(file_path, 'r') as file:
            settings = json.load(file)

        # Update the specific lines
        settings["productid"] = new_values["productid"]
        settings["sandboxid"] = new_values["sandboxid"]
        settings["clientid"] = new_values["clientid"]
        settings["deploymentid"] = new_values["deploymentid"]

        # Save the updated JSON back to the file
        with open(file_path, 'w') as file:
            json.dump(settings, file, indent=4)

        return True  # Indicate success

    except FileNotFoundError:
        return False  # Indicate failure: File not found

    except Exception as e:
        return False  # Indicate failure: An error occurred

def update_settings_gui(drive_var):
    drive_letter = drive_var.get()

    # Define the new values
    new_values = {
        "productid": "vorpx-eac-workaround",
        "sandboxid": "vorpx-eac-workaround",
        "clientid": "vorpx-eac-workaround",
        "deploymentid": "vorpx-eac-workaround",
    }

    # Construct the directory paths
    ptu_directory = os.path.join(drive_letter, 'Program Files', 'Roberts Space Industries', 'StarCitizen', 'PTU', 'EasyAntiCheat')
    live_directory = os.path.join(drive_letter, 'Program Files', 'Roberts Space Industries', 'StarCitizen', 'LIVE', 'EasyAntiCheat')
    eptu_directory = os.path.join(drive_letter, 'Program Files', 'Roberts Space Industries', 'StarCitizen', 'EPTU', 'EasyAntiCheat')
    tech_preview = os.path.join(drive_letter, 'Program Files', 'Roberts Space Industries', 'StarCitizen', 'TECH-PREVIEW', 'EasyAntiCheat')

    # Update settings
    results = [
        update_settings(ptu_directory, new_values),
        update_settings(live_directory, new_values),
        update_settings(eptu_directory, new_values),
        update_settings(tech_preview, new_values),
    ]

    # Check and update hosts file
    hosts_updated = is_hosts_updated()

    result_message = "Settings updated successfully in the following directories:\n\n"
    updated = False

    for directory, result in zip([ptu_directory, live_directory, eptu_directory, tech_preview], results):
        if result:
            result_message += f"{directory}\n"
            updated = True

    if hosts_updated:
        result_message += "\nHosts file updated successfully."
        updated = True
    elif hosts_updated is False:
        result_message += "\nHosts file is already up to date."

    if updated:
        result_message += "\nClose the program now."
    else:
        result_message = "Failed to update settings. Check the paths and try again."

    messagebox.showinfo("Update Result", result_message)

    if updated:
        # Create a Close Program button
        close_button = tk.Button(root, text="Close Program", command=root.destroy, font=("Arial", 12))
        close_button.pack(pady=10)

# GUI setup
root = tk.Tk()
root.title("Easy Anti-Cheat Settings Updater")
root.configure(bg="green")

# Set the initial size of the window
root.geometry("600x600")

# GUI Image
imgVR = Image.open("VRCitizen.png").resize((450, 225))
imgVR = ImageTk.PhotoImage(imgVR)
label_image = Label(root, image=imgVR, justify="center", anchor="n")
label_image.pack(padx=10, pady=10, anchor="n")

# Additional text label
additional_text = "Special thanks to SilvanVR at CIG and Chachi Sanchez for getting VRCitizen going. Find them both on YouTube and Twitch. See you in the 'VRse  o7 "
label_text = Label(root, text=additional_text, font=("Arial", 12), wraplength=400, justify="center", anchor="n")
label_text.pack(padx=10, pady=10, anchor="n")

# Get the list of available drives
import string
drives = [f"{letter}:" for letter in string.ascii_uppercase if os.path.exists(f"{letter}:\\")]

# Label for drive selection
drive_label = tk.Label(root, text="Select the drive in dropdown below, where Star Citizen is installed.\nThen click 'UPDATE SETTINGS' button below", anchor="c", font=("Arial", 12))
drive_label.pack(padx=10, pady=10, anchor="c")

# Dropdown for drive selection
drive_var = tk.StringVar()
drive_dropdown = tk.OptionMenu(root, drive_var, *drives)
drive_dropdown.pack(padx=10, pady=10, anchor="c")

# Update button
update_button = tk.Button(root, text="Update Settings", command=lambda: update_settings_gui(drive_var), font=("Arial", 12))
update_button.pack(pady=10)

# Make the window non-resizable
root.resizable(False, False)

root.mainloop()
