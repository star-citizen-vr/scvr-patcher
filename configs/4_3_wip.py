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

# Target FOV to check against
target_fov = 123.1382167

# Define the outlier ranges for attributes.xml values
outlier_attributes = np.array([15, 120])

# Convert to NumPy arrays
attributes_4_3 = np.array(poly_attributes_4_3).reshape((-1, 1))

# Create polynomial features
poly_degree = 2

poly_features_4_3 = PolynomialFeatures(degree=poly_degree)
poly_attributes_4_3 = poly_features_4_3.fit_transform(attributes_4_3)

# Fit the model
model_4_3 = LinearRegression().fit(poly_attributes_4_3, fov_4_3)

# Predict using the model
predicted_fov_4_3 = model_4_3.predict(poly_attributes_4_3)

### PREDICTION GENERATION ###

# Generate predictions for the normal range for 4:3 aspect ratio
extension_range_normal = np.linspace(min(attributes_4_3), max(attributes_4_3), num=100).reshape(-1, 1)
poly_extension_range_normal_4_3 = poly_features_4_3.transform(extension_range_normal)
predicted_extension_fov_normal_4_3 = model_4_3.predict(poly_extension_range_normal_4_3)

# Generate predictions for the outlier range for 4:3 aspect ratio
extension_range_outlier = np.linspace(min(outlier_attributes), max(outlier_attributes), num=100).reshape(-1, 1)
poly_extension_range_outlier_4_3 = poly_features_4_3.transform(extension_range_outlier)
predicted_extension_fov_outlier_4_3 = model_4_3.predict(poly_extension_range_outlier_4_3)

# Predict outlier attributes.xml values
outlier_attributes_4_3 = outlier_attributes.reshape(-1, 1)
poly_outlier_attributes_4_3 = poly_features_4_3.transform(outlier_attributes_4_3)
predicted_outlier_fov_4_3 = model_4_3.predict(poly_outlier_attributes_4_3)

# Plot the results for 4:3 aspect ratio
plt.scatter(attributes_4_3, fov_4_3, label='4:3 Integer FOV degrees', color='red', marker='o')  # <-- Add marker='o' to plot points
plt.plot(attributes_4_3, predicted_fov_4_3, color='red', linestyle='--', label='4:3 Predicted FOV')

# Check if the target FOV crosses the normal range of data.
    # If it does, mark the intersection with an 'x' and print what the attributes.xml would be for the Target FOV intersection.
    # If it does not cross the normal range of data
        #Then generate the outlier range and check again to see if it crosses, mark with an 'x' and notify the user of the attributes.xml value
        #if it does not, then tell the user to try a different aspect ratio.
if predicted_fov_4_3[0] > target_fov:
    print(f'The target FOV ({target_fov}) is below the minimum FOV ({predicted_fov_4_3[0]:.2f}) for the 4:3 aspect ratio.')
elif predicted_fov_4_3[-1] < target_fov:
    print(f'The target FOV ({target_fov}) is above the maximum FOV ({predicted_fov_4_3[-1]:.2f}) for the 4:3 aspect ratio.')
crossed_indices_normal = np.where(predicted_fov_4_3 >= target_fov)[0]
if len(crossed_indices_normal) > 0:
    print("The predicted FOV crosses the target FOV in the normal range.")
    first_crossed_index_normal = crossed_indices_normal[0]
    crossed_attributes_value_normal = attributes_4_3[first_crossed_index_normal][0]
    print(f"Attributes.xml value: {crossed_attributes_value_normal}")
    # Draw the horizontal line for the fixed FOV
    plt.axhline(y=target_fov, color='grey', linestyle='--', label=f'Target H-FOV ({target_fov:.2f})')
    # Draw the dashed red lines for the normal range
    plt.plot(extension_range_normal, predicted_extension_fov_normal_4_3, color='red', linestyle='--')
    # Put a dot where Target FOV crosses the outlier range prediction line
    plt.scatter(crossed_attributes_value_normal, target_fov, color='black', marker='o', s=100)
    # Finalize the plot
    plt.xlabel('attributes.xml FOV value')
    plt.ylabel('Actual Game Horizontal Degrees (Points on Whole Number)')
    plt.legend()
    plt.show()
else:
    print("The predicted FOV does not cross the target FOV in the normal range.")
    crossed_indices_outlier = np.where(predicted_extension_fov_outlier_4_3 >= target_fov)[0]
    if len(crossed_indices_outlier) > 0:
        print("The predicted FOV crosses the target FOV in the outlier range.")
        first_crossed_index_outlier = crossed_indices_outlier[0]
        crossed_attributes_value_outlier = extension_range_outlier[first_crossed_index_outlier][0]
        print(f"Attributes.xml value: {crossed_attributes_value_outlier}")
        # Plot the outlier range for 4:3 aspect ratio in red dashed line and put an 'x' on minimum and maximum attributes.xml values
        plt.scatter(outlier_attributes, predicted_outlier_fov_4_3, label='Outlier Integer FOV degrees', color='red', marker='x', s=100)
        # Draw the horizontal line for the fixed FOV
        plt.axhline(y=target_fov, color='grey', linestyle='--', label=f'Target H-FOV ({target_fov:.2f})')
        # Draw the dashed red lines for the outlier range
        plt.plot(extension_range_outlier, predicted_extension_fov_outlier_4_3, color='red', linestyle='--')
        # Put a dot where Target FOV crosses the outlier range prediction line
        plt.scatter(crossed_attributes_value_outlier, target_fov, color='black', marker='o', s=100)
        # Finalize the plot
        plt.xlabel('attributes.xml FOV value')
        plt.ylabel('Actual Game Horizontal Degrees (Points on Whole Number)')
        plt.legend()
        plt.show()
    else:
        print("The predicted FOV does not cross the target FOV in the outlier range.")
        print("Try a different aspect ratio.")