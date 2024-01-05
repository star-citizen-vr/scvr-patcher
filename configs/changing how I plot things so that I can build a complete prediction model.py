import numpy as np
from sklearn.linear_model import LinearRegression
from sklearn.preprocessing import PolynomialFeatures
import matplotlib.pyplot as plt

def process_aspect_ratio(poly_attributes, fov_values):
    attributes = np.array(poly_attributes).reshape(-1, 1)
    poly_degree = 2
    poly_features = PolynomialFeatures(degree=poly_degree)
    poly_attributes = poly_features.fit_transform(attributes)
    model = LinearRegression().fit(poly_attributes, fov_values)
    predicted_fov = model.predict(poly_attributes)
    return attributes, fov_values, poly_attributes, model, predicted_fov, poly_features

def predict_outliers(outlier_attributes, poly_features, model):
    outlier_attributes = outlier_attributes.reshape(-1, 1)
    poly_outlier_attributes = poly_features.transform(outlier_attributes)
    predicted_outlier_fov = model.predict(poly_outlier_attributes)
    return predicted_outlier_fov

def plot_results(attributes, fov_values, predicted_fov, label, color, marker=None):
    plt.scatter(attributes, fov_values, label=f'{label} Integer FOV degrees', color=color)
    plt.plot(attributes, predicted_fov, color=color, linestyle='--', label=f'{label} Predicted FOV', marker=marker)

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

# Data for 16:9 aspect ratio
poly_attributes_16_9 = [
    54.5364, 55.3576, 56.1859, 57.0216, 57.8648, 58.7155, 59.574, 60.4404, 61.3148,
    62.1974, 63.0883, 63.9877, 64.8956, 65.8124, 66.738, 67.6728, 68.6167, 69.57,
    70.5328, 71.5053, 72.4876, 73.4799, 74.4824, 75.4951, 76.5183, 77.5521, 78.5967,
    79.6522, 80.7188, 81.7965, 82.8857, 83.9863
]

fov_16_9 = [
    85, 86, 87, 88, 89, 90, 91, 92, 93, 94, 95, 96, 97, 98, 99, 100, 101, 102, 103,
    104, 105, 106, 107, 108, 109, 110, 111, 112, 113, 114, 115, 116
]

# Your fixed FOV value
fixed_fov = 170

# Outlier attributes.xml values
outlier_attributes = np.array([15, 120])

# Data for different aspect ratios
aspect_ratios = {
    "4:3": {"poly_attributes": poly_attributes_4_3, "fov_values": fov_4_3, "color": "red"},
    "16:9": {"poly_attributes": poly_attributes_16_9, "fov_values": fov_16_9, "color": "teal"},
    # Add more aspect ratios as needed
}

# Process and plot results for each aspect ratio
for ratio, data in aspect_ratios.items():
    attributes, fov_values, poly_attributes, model, predicted_fov, poly_features = process_aspect_ratio(data["poly_attributes"], data["fov_values"])
    data["attributes"] = attributes
    data["fov_values"] = fov_values
    data["predicted_fov"] = predicted_fov
    data["model"] = model
    data["poly_features"] = poly_features  # Store poly_features in data dictionary

# Check if fixed FOV is within the range of plotted points for each aspect ratio
should_plot_extension = all(
    fixed_fov < min(data["fov_values"]) or fixed_fov > max(data["fov_values"])
    for data in aspect_ratios.values()
)

# Draw the horizontal line for the fixed FOV
plt.axhline(y=fixed_fov, color='grey', linestyle='-', label=f'Quest 3 Example H-FOV ({fixed_fov:.2f})')

# Plot extension range only if necessary
if should_plot_extension:
    # Plot extension range for each aspect ratio
    for ratio, data in aspect_ratios.items():
        poly_features = data["poly_features"]  # Retrieve poly_features here
        color = data["color"]

        # Plot extension range
        extension_range = np.linspace(15, 120, num=100).reshape(-1, 1)
        poly_extension_range = poly_features.transform(extension_range)
        predicted_extension_fov = data["model"].predict(poly_extension_range)
        plt.plot(extension_range, predicted_extension_fov, color=color, linestyle='--')

        # Mark the ends (15, 120) with 'x' in the same color as the aspect ratio
        plt.scatter([15, 120], data["model"].predict(poly_features.transform([[15], [120]])), color=color, marker='x')

# Plot the results for each aspect ratio outside the loop
for ratio, data in aspect_ratios.items():
    plot_results(data["attributes"], data["fov_values"], data["predicted_fov"], ratio, color=data["color"])

plt.xlabel('attributes.xml FOV value')
plt.ylabel('Actual Game Horizontal Degrees (Points on Whole Number)')
plt.legend()
plt.show()