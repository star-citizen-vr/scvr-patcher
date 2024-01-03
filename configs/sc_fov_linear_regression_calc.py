import numpy as np
from sklearn.linear_model import LinearRegression
from sklearn.preprocessing import PolynomialFeatures
import matplotlib.pyplot as plt

# Data for 32:9 aspect ratio
attributes_32_9 = np.array([54.7682, 55.7536, 56.7626, 57.796, 58.8547, 59.9396, 61.0517, 62.1919, 63.3614, 64.5611,
                           65.7921, 67.0556, 68.3528, 69.685, 71.0533, 72.4591, 73.9037, 75.3885, 76.915, 78.4846,
                           80.0987, 81.7589, 83.4667])
fov_32_9 = np.array([123, 124, 125, 126, 127, 128, 129, 130, 131, 132, 133, 134, 135, 136, 137, 138, 139, 140, 141,
                     142, 143, 144, 145])

# Data for 16:9 aspect ratio
attributes_16_9 = np.array([54.5364, 55.3576, 56.1859, 57.0216, 57.8648, 58.7155, 59.574, 60.4404, 61.3148, 62.1974,
                           63.0883, 63.9877, 64.8956, 65.8124, 66.738, 67.6728, 68.6167, 69.57, 70.5328, 71.5053, 72.4876,
                           73.4799, 74.4824, 75.4951, 76.5183, 77.5521, 78.5967, 79.6522, 80.7188, 81.7965, 82.8857, 83.9863])
fov_16_9 = np.array([85, 86, 87, 88, 89, 90, 91, 92, 93, 94, 95, 96, 97, 98, 99, 100, 101, 102, 103, 104, 105, 106,
                     107, 108, 109, 110, 111, 112, 113, 114, 115, 116])

# Reshape the data to meet scikit-learn requirements
attributes_32_9 = attributes_32_9.reshape(-1, 1)
attributes_16_9 = attributes_16_9.reshape(-1, 1)

# Create polynomial features
poly_degree = 2  # You can try different degrees
poly_features_32_9 = PolynomialFeatures(degree=poly_degree)
poly_attributes_32_9 = poly_features_32_9.fit_transform(attributes_32_9)

poly_features_16_9 = PolynomialFeatures(degree=poly_degree)
poly_attributes_16_9 = poly_features_16_9.fit_transform(attributes_16_9)

# Fit the model
model_32_9 = LinearRegression().fit(poly_attributes_32_9, fov_32_9)
model_16_9 = LinearRegression().fit(poly_attributes_16_9, fov_16_9)

# Predict using the model
predicted_fov_32_9 = model_32_9.predict(poly_attributes_32_9)
predicted_fov_16_9 = model_16_9.predict(poly_attributes_16_9)

# Plot the results
plt.scatter(attributes_32_9, fov_32_9, label='32:9 Actual FOV', color='blue')
plt.plot(attributes_32_9, predicted_fov_32_9, color='blue', linestyle='--', label='32:9 Predicted FOV')

plt.scatter(attributes_16_9, fov_16_9, label='16:9 Actual FOV', color='red')
plt.plot(attributes_16_9, predicted_fov_16_9, color='red', linestyle='--', label='16:9 Predicted FOV')

plt.xlabel('attributes.xml')
plt.ylabel('Game Actual FOV')
plt.legend()
plt.show()
