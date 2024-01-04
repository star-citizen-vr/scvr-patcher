import numpy as np
from sklearn.linear_model import LinearRegression
from sklearn.preprocessing import PolynomialFeatures
import matplotlib.pyplot as plt

# Data for 4:3 aspect ratio
poly_attributes_4_3 = [
    54.5387, 55.4129, 56.2909, 57.1725, 58.058, 58.9473, 59.8404, 60.7376, 61.6387,
    62.5438, 63.4531, 64.3664, 65.284, 66.2059, 67.132, 68.0626, 68.9975, 69.9368,
    70.8807, 71.8291, 72.7821, 73.7398, 74.7021, 75.6693, 76.6411, 77.6178, 78.5994,
    79.5859, 80.5773, 81.5737, 82.5751, 83.5817
]

fov_4_3 = [
    69, 70, 71, 72, 73, 74, 75, 76, 77, 78, 79, 80, 81, 82, 83, 84, 85, 86, 87, 88, 89,
    90, 91, 92, 93, 94, 95, 96, 97, 98, 99, 100
]

# Your fixed FOV value
target_fov = 123.1382167

# Outlier attributes.xml values
outlier_attributes = np.array([15, 120])  # Extremes that attributes.xml can read (fov unknown as they are locked to the known minimum/maximum)

# Convert to NumPy array
attributes_4_3 = np.array(poly_attributes_4_3).reshape(-1, 1)

# Create polynomial features
poly_degree = 2  # You can try different degrees

poly_features_4_3 = PolynomialFeatures(degree=poly_degree)
poly_attributes_4_3 = poly_features_4_3.fit_transform(attributes_4_3)

# Fit the model
model_4_3 = LinearRegression().fit(poly_attributes_4_3, fov_4_3)

# Predict using the model
predicted_fov_4_3 = model_4_3.predict(poly_attributes_4_3)

# Predict FOV for outlier attributes.xml values for 4:3 aspect ratio
outlier_attributes_4_3 = outlier_attributes.reshape(-1, 1)
poly_outlier_attributes_4_3 = poly_features_4_3.transform(outlier_attributes_4_3)
predicted_outlier_fov_4_3 = model_4_3.predict(poly_outlier_attributes_4_3)

### PREDICTION GENERATION ###

# Generate predictions for the normal range for 4:3 aspect ratio
extension_range_normal = np.linspace(min(attributes_4_3), max(attributes_4_3), num=100).reshape(-1, 1)
poly_extension_range_normal_4_3 = poly_features_4_3.transform(extension_range_normal)
predicted_extension_fov_normal_4_3 = model_4_3.predict(poly_extension_range_normal_4_3)

# Generate predictions for the outlier range for 4:3 aspect ratio
extension_range_outlier = np.linspace(min(outlier_attributes), max(outlier_attributes), num=100).reshape(-1, 1)
poly_extension_range_outlier_4_3 = poly_features_4_3.transform(extension_range_outlier)
predicted_extension_fov_outlier_4_3 = model_4_3.predict(poly_extension_range_outlier_4_3)

### CHECK IF TARGET FOV CROSSES NORMAL RANGE ###

# Find the indices where predicted FOV crosses the target FOV in the normal range, if it doesn't cross the normal range, move to the outlier range.
crossed_indices_normal = np.where(predicted_fov_4_3 >= target_fov)[0]
if len(crossed_indices_normal) > 0:
    print("The predicted FOV crosses the target FOV in the normal range.")
    first_crossed_index_normal = crossed_indices_normal[0]
    crossed_attributes_value_normal = attributes_4_3[first_crossed_index_normal][0]
    print(f"Attributes.xml value: {crossed_attributes_value_normal}")

    ### PLOT RESULTS AND DRAW EXAMPLE HEADSET H-FOV ###

    # Plot the results for 4:3 aspect ratio
    plt.scatter(attributes_4_3, fov_4_3, label='4:3 Integer FOV degrees', color='red')
    plt.plot(attributes_4_3, predicted_fov_4_3, color='red', linestyle='--', label='4:3 Predicted FOV')

    # Draw the horizontal line for the fixed FOV
    plt.axhline(y=target_fov, color='grey', linestyle='--', label=f'Target H-FOV ({target_fov:.2f})')

    # Plot the extension range for 4:3 aspect ratio
    plt.plot(extension_range_normal, predicted_extension_fov_normal_4_3, color='red', linestyle='--')

else:
    # Target FOV does not cross the normal range
    print("The predicted FOV does not cross the target FOV in the normal range.")

    ### CHECK IF TARGET FOV CROSSES OUTLIER RANGE ###

    # Find the indices where predicted FOV crosses the target FOV in the outlier range
    crossed_indices_outlier = np.where(predicted_outlier_fov_4_3 >= target_fov)[0]

    if len(crossed_indices_outlier) > 0:
        # Target FOV crosses the outlier range
        first_crossed_index_outlier = crossed_indices_outlier[0]
        crossed_attributes_value_outlier = outlier_attributes[first_crossed_index_outlier]
        print(f"The predicted FOV crosses the target FOV in the outlier range at attributes.xml value: {crossed_attributes_value_outlier}")

        ### PLOT OUTLIER POINTS AND EXTENSION RANGE ###

        # Plot the outlier points for 4:3 aspect ratio
        plt.scatter(outlier_attributes, predicted_outlier_fov_4_3, label='4:3 Outlier FOV degrees', color='red', marker='x')

        # Plot the extension range for 4:3 aspect ratio
        plt.plot(extension_range_outlier, predicted_extension_fov_outlier_4_3, color='red', linestyle='--')

    else:
        # Target FOV does not cross the outlier range
        print("The predicted FOV does not cross the target FOV in the outlier range. Please try a wider aspect ratio.")

### PLOT RESULTS AND DRAW EXAMPLE HEADSET H-FOV ###

# Plot the results for 4:3 aspect ratio
plt.scatter(attributes_4_3, fov_4_3, label='4:3 Integer FOV degrees', color='red', marker='o')  # <-- Add marker='o' to plot points
plt.plot(attributes_4_3, predicted_fov_4_3, color='red', linestyle='--', label='4:3 Predicted FOV')

# Draw the horizontal line for the fixed FOV
plt.axhline(y=target_fov, color='grey', linestyle='--', label=f'Target H-FOV ({target_fov:.2f})')

# Plot the extension range for 4:3 aspect ratio
plt.plot(extension_range_normal, predicted_extension_fov_normal_4_3, color='red', linestyle='--')

# Plot the extension range for 4:3 aspect ratio in the outlier case
plt.plot(extension_range_outlier, predicted_extension_fov_outlier_4_3, color='red', linestyle='--')

# Plot the outlier points for 4:3 aspect ratio
plt.scatter(outlier_attributes, predicted_outlier_fov_4_3, label='4:3 Outlier FOV degrees', color='red', marker='x')

# Finalize the plot
plt.xlabel('attributes.xml FOV value')
plt.ylabel('Actual Game Horizontal Degrees (Points on Whole Number)')
plt.legend()
plt.show()