import numpy as np
from sklearn.linear_model import LinearRegression
from sklearn.preprocessing import PolynomialFeatures
import matplotlib.pyplot as plt

'''
1:1
5:4
4:3
3:2
16:10
27:16
16:9
256:135
64:27
43:18
12:5
32:9
4:1
16:3
'''
# Data for 1:1 aspect ratio


# Data for 5:4 aspect ratio
poly_attributes_5_4 = [
    54.0119, 54.9061, 55.8032, 56.7032, 57.6062, 58.5121, 59.4211, 60.3332, 61.2484,
    62.1668, 63.0883, 64.0131, 64.9412, 65.8725, 66.8072, 67.7453, 68.6868, 69.6318,
    70.5802, 71.5322, 72.4877, 73.4468, 74.4095, 75.3758, 76.3459, 77.3196, 78.2971,
    79.2784, 80.2634, 81.2523, 82.245, 83.2415
]

fov_5_4 = [
    65, 66, 67, 68, 69, 70, 71, 72, 73, 74, 75, 76, 77, 78, 79, 80, 81, 82, 83, 84, 85,
    86, 87, 88, 89, 90, 91, 92, 93, 94, 95, 96
]

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

# Data for 3:2 aspect ratio
poly_attributes_3_2 = [
    54.1842, 55.0261, 55.8732, 56.7253, 57.5827, 58.4453, 59.3133, 60.1867, 61.0656,
    61.9502, 62.8405, 63.7365, 64.6384, 65.5463, 66.4602, 67.3801, 68.3063, 69.2388,
    70.1776, 71.1229, 72.0747, 73.0331, 73.9982, 74.9701, 75.9488, 76.9345, 77.9271,
    78.9269, 79.9338, 80.9479, 81.9693, 82.9982
]

fov_3_2 = [
    75, 76, 77, 78, 79, 80, 81, 82, 83, 84, 85, 86, 87, 88, 89, 90, 91, 92, 93, 94, 95,
    96, 97, 98, 99, 100, 101, 102, 103, 104, 105, 106
]

# Data for 16:10 aspect ratio
poly_attributes_16_10 = [
    54.516, 55.3484, 56.1866, 57.0308, 57.8811, 58.7375, 59.6001, 60.4691, 61.3446,
    62.2266, 63.1153, 64.0108, 64.9131, 65.8224, 66.7388, 67.6624, 68.5933, 69.5316,
    70.4774, 71.4308, 72.3919, 73.3609, 74.3378, 75.3227, 76.3158, 77.3171, 78.3267,
    79.3448, 80.3715, 81.4068, 82.4508, 83.5037
]

fov_16_10 = [
    79, 80, 81, 82, 83, 84, 85, 86, 87, 88, 89, 90, 91, 92, 93, 94, 95, 96, 97, 98, 99,
    100, 101, 102, 103, 104, 105, 106, 107, 108, 109, 110
]


# Data for 27:16 aspect ratio
poly_attributes_27_16 = [
    54.509, 55.3344, 56.1663, 57.0049, 57.8502, 58.7024, 59.5616, 60.4278, 61.3013,
    62.1822, 63.0705, 63.9665, 64.8701, 65.7816, 66.7011, 67.6287, 68.5645, 69.5087,
    70.4613, 71.4226, 72.3927, 73.3716, 74.3595, 75.3565, 76.3628, 77.3785, 78.4037,
    79.4385, 80.4831, 81.5376, 82.6021, 83.6768
]

fov_27_16 = [
    82, 83, 84, 85, 86, 87, 88, 89, 90, 91, 92, 93, 94, 95, 96, 97, 98, 99, 100, 101,
    102, 103, 104, 105, 106, 107, 108, 109, 110, 111, 112, 113
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

# Data for 256:135 aspect ratio
poly_attributes_256_135 = [
    54.7881, 55.6093, 56.4386, 57.2762, 58.1222, 58.9769, 59.8404, 60.7129, 61.5944,
    62.4853, 63.3857, 64.2958, 65.2157, 66.1456, 67.0857, 68.0363, 68.9974, 69.9693,
    70.9522, 71.9463, 72.9517, 73.9686, 74.9973, 76.038, 77.0907, 78.1558, 79.2335,
    80.3238, 81.4271, 82.5434, 83.6731
]

fov_256_135 = [
    89, 90, 91, 92, 93, 94, 95, 96, 97, 98, 99, 100, 101, 102, 103, 104, 105, 106, 107,
    108, 109, 110, 111, 112, 113, 114, 115, 116, 117, 118, 119
]

# Data for 2:1 aspect ratio


# Data for 64:27 aspect ratio
poly_attributes_64_27 = [
    54.2045, 55.0365, 55.8802, 56.7359, 57.6038, 58.4843, 59.3776, 60.2841, 61.2041, 62.1379,
    63.0859, 64.0483, 65.0255, 66.018, 67.026, 68.0498, 69.09, 70.1468, 71.2206, 72.3119,
    73.421, 74.5483, 75.6942, 76.8592, 78.0436, 79.2479, 80.4725, 81.7178, 82.9842
]

fov_64_27 = [
    101, 102, 103, 104, 105, 106, 107, 108, 109, 110, 111, 112, 113, 114, 115, 116, 117, 118,
    119, 120, 121, 122, 123, 124, 125, 126, 127, 128, 129
]

# Data for 43:18 aspect ratio
poly_attributes_43_18 = [
    54.6719, 55.5119, 56.3639, 57.2281, 58.105, 58.9947, 59.8976, 60.8141, 61.7445, 62.689,
    63.6481, 64.622, 65.6112, 66.6161, 67.6369, 68.674, 69.728, 70.799, 71.8876, 72.9941,
    74.1189, 75.2626, 76.4254, 77.6078, 78.8102, 80.0331, 81.2768, 82.5419, 83.8287
]

fov_43_18 = [
    102, 103, 104, 105, 106, 107, 108, 109, 110, 111, 112, 113, 114, 115, 116, 117, 118, 119,
    120, 121, 122, 123, 124, 125, 126, 127, 128, 129, 130
]

# Data for 12:5 aspect ratio
poly_attributes_12_5 = [
    54.4553, 55.2931, 56.1428, 57.0049, 57.8795, 58.7671, 59.6679, 60.5823, 61.5105, 62.453,
    63.4101, 64.382, 65.3693, 66.3723, 67.3912, 68.4266, 69.4787, 70.5481, 71.6351, 72.74,
    73.8634, 75.0056, 76.1671, 77.3482, 78.5495, 79.7713, 81.0141, 82.2783, 83.5644
]

fov_12_5 = [
    102, 103, 104, 105, 106, 107, 108, 109, 110, 111, 112, 113, 114, 115, 116, 117, 118, 119,
    120, 121, 122, 123, 124, 125, 126, 127, 128, 129, 130
]

# Data for 32:9 aspect ratio
poly_attributes_32_9 = [
    54.7682, 55.7536, 56.7626, 57.796, 58.8547, 59.9396, 61.0517, 62.1919, 63.3614, 64.5611,
    65.7921, 67.0556, 68.3528, 69.685, 71.0533, 72.4591, 73.9037, 75.3885, 76.915, 78.4846,
    80.0987, 81.7589, 83.4667
]

fov_32_9 = [
    123, 124, 125, 126, 127, 128, 129, 130, 131, 132, 133, 134, 135, 136, 137, 138, 139, 140,
    141, 142, 143, 144, 145
]

# Data for 4:1 aspect ratio
poly_attributes_4_1 = [
    54.2772, 55.3213, 56.3939, 57.4961, 58.6292, 59.7944, 60.9931, 62.2265, 63.4961, 64.8035,
    66.1502, 67.5378, 68.968, 70.4425, 71.9633, 73.5321, 75.1509, 76.8217, 78.5467, 80.3278,
    82.1673
]

fov_4_1 = [
    128, 129, 130, 131, 132, 133, 134, 135, 136, 137, 138, 139, 140, 141, 142, 143, 144, 145,
    146, 147, 148
]

# Data for 16:3 aspect ratio
poly_attributes_16_3 = [
    54.5107, 55.801, 57.1401, 58.5307, 59.9756, 61.4777, 63.0403, 64.6668, 66.3606, 68.1255,
    69.9656, 71.8849, 73.8879, 75.9793, 78.1637, 80.4463, 82.8322
]

fov_16_3 = [
    140, 141, 142, 143, 144, 145, 146, 147, 148, 149, 150, 151, 152, 153, 154, 155, 156
]

# Your fixed FOV value
fixed_fov = 109.1593028

# Outlier attributes.xml values
outlier_attributes = np.array([15, 120]) # Extremes that attributes.xml can read (fov unknown as they are locked to the known minimum/maximum)

'''
1:1
5:4
4:3
3:2
16:10
27:16
16:9
256:135
2:1
64:27
43:18
12:5
32:9
4:1
16:3
'''

# Convert to NumPy array
#attributes_1_1 = np.array(poly_attributes_1_1).reshape(-1, 1)
attributes_5_4 = np.array(poly_attributes_5_4).reshape(-1, 1)
attributes_4_3 = np.array(poly_attributes_4_3).reshape(-1, 1)
attributes_3_2 = np.array(poly_attributes_3_2).reshape(-1, 1)
attributes_16_10 = np.array(poly_attributes_16_10).reshape(-1, 1)
attributes_27_16 = np.array(poly_attributes_27_16).reshape(-1, 1)
attributes_16_9 = np.array(poly_attributes_16_9).reshape(-1, 1)
attributes_256_135 = np.array(poly_attributes_256_135).reshape(-1, 1)
#attributes_2_1 = np.array(poly_attributes_2_1).reshape(-1, 1)
attributes_64_27 = np.array(poly_attributes_64_27).reshape(-1, 1)
attributes_43_18 = np.array(poly_attributes_43_18).reshape(-1, 1)
attributes_12_5 = np.array(poly_attributes_12_5).reshape(-1, 1)
attributes_32_9 = np.array(poly_attributes_32_9).reshape(-1, 1)
attributes_4_1 = np.array(poly_attributes_4_1).reshape(-1, 1)
attributes_16_3 = np.array(poly_attributes_16_3).reshape(-1, 1)

# Create polynomial features
poly_degree = 2  # You can try different degrees

#poly_features_1_1 = PolynomialFeatures(degree=poly_degree)
#poly_attributes_1_1 = poly_features_1_1.fit_transform(attributes_1_1)

poly_features_5_4 = PolynomialFeatures(degree=poly_degree)
poly_attributes_5_4 = poly_features_5_4.fit_transform(attributes_5_4)

poly_features_4_3 = PolynomialFeatures(degree=poly_degree)
poly_attributes_4_3 = poly_features_4_3.fit_transform(attributes_4_3)

poly_features_3_2 = PolynomialFeatures(degree=poly_degree)
poly_attributes_3_2 = poly_features_3_2.fit_transform(attributes_3_2)

poly_features_16_10 = PolynomialFeatures(degree=poly_degree)
poly_attributes_16_10 = poly_features_16_10.fit_transform(attributes_16_10)

poly_features_27_16 = PolynomialFeatures(degree=poly_degree)
poly_attributes_27_16 = poly_features_27_16.fit_transform(attributes_27_16)

poly_features_16_9 = PolynomialFeatures(degree=poly_degree)
poly_attributes_16_9 = poly_features_16_9.fit_transform(attributes_16_9)

poly_features_256_135 = PolynomialFeatures(degree=poly_degree)
poly_attributes_256_135 = poly_features_256_135.fit_transform(attributes_256_135)

#poly_features_2_1 = PolynomialFeatures(degree=poly_degree)
#poly_attributes_2_1 = poly_features_2_1.fit_transform(attributes_2_1)

poly_features_64_27 = PolynomialFeatures(degree=poly_degree)
poly_attributes_64_27 = poly_features_64_27.fit_transform(attributes_64_27)

poly_features_43_18 = PolynomialFeatures(degree=poly_degree)
poly_attributes_43_18 = poly_features_43_18.fit_transform(attributes_43_18)

poly_features_12_5 = PolynomialFeatures(degree=poly_degree)
poly_attributes_12_5 = poly_features_12_5.fit_transform(attributes_12_5)

poly_features_32_9 = PolynomialFeatures(degree=poly_degree)
poly_attributes_32_9 = poly_features_32_9.fit_transform(attributes_32_9)

poly_features_4_1 = PolynomialFeatures(degree=poly_degree)
poly_attributes_4_1 = poly_features_4_1.fit_transform(attributes_4_1)

poly_features_16_3 = PolynomialFeatures(degree=poly_degree)
poly_attributes_16_3 = poly_features_16_3.fit_transform(attributes_16_3)

# Fit the model
#model_1_1 = LinearRegression().fit(poly_attributes_1_1, fov_1_1)
model_5_4 = LinearRegression().fit(poly_attributes_5_4, fov_5_4)
model_4_3 = LinearRegression().fit(poly_attributes_4_3, fov_4_3)
model_3_2 = LinearRegression().fit(poly_attributes_3_2, fov_3_2)
model_16_10 = LinearRegression().fit(poly_attributes_16_10, fov_16_10)
model_27_16 = LinearRegression().fit(poly_attributes_27_16, fov_27_16)
model_16_9 = LinearRegression().fit(poly_attributes_16_9, fov_16_9)
model_256_135 = LinearRegression().fit(poly_attributes_256_135, fov_256_135)
#model_2_1 = LinearRegression().fit(poly_attributes_2_1, fov_2_1)
model_64_27 = LinearRegression().fit(poly_attributes_64_27, fov_64_27)
model_43_18 = LinearRegression().fit(poly_attributes_43_18, fov_43_18)
model_12_5 = LinearRegression().fit(poly_attributes_12_5, fov_12_5)
model_32_9 = LinearRegression().fit(poly_attributes_32_9, fov_32_9)
model_4_1 = LinearRegression().fit(poly_attributes_4_1, fov_4_1)
model_16_3 = LinearRegression().fit(poly_attributes_16_3, fov_16_3)

# Predict using the model
#predicted_fov_1_1 = model_1_1.predict(poly_attributes_1_1)
predicted_fov_5_4 = model_5_4.predict(poly_attributes_5_4)
predicted_fov_4_3 = model_4_3.predict(poly_attributes_4_3)
predicted_fov_3_2 = model_3_2.predict(poly_attributes_3_2)
predicted_fov_16_10 = model_16_10.predict(poly_attributes_16_10)
predicted_fov_27_16 = model_27_16.predict(poly_attributes_27_16)
predicted_fov_16_9 = model_16_9.predict(poly_attributes_16_9)
predicted_fov_256_135 = model_256_135.predict(poly_attributes_256_135)
#predicted_fov_2_1 = model_2_1.predict(poly_attributes_2_1)
predicted_fov_64_27 = model_64_27.predict(poly_attributes_64_27)
predicted_fov_43_18 = model_43_18.predict(poly_attributes_43_18)
predicted_fov_12_5 = model_12_5.predict(poly_attributes_12_5)
predicted_fov_32_9 = model_32_9.predict(poly_attributes_32_9)
predicted_fov_4_1 = model_4_1.predict(poly_attributes_4_1)
predicted_fov_16_3 = model_16_3.predict(poly_attributes_16_3)

# Predict FOV for outlier attributes.xml values for 1:1 aspect ratio
#outlier_attributes_1_1 = outlier_attributes.reshape(-1, 1)
#poly_outlier_attributes_1_1 = poly_features_1_1.transform(outlier_attributes_1_1)
#predicted_outlier_fov_1_1 = model_1_1.predict(poly_outlier_attributes_1_1)

# Predict FOV for outlier attributes.xml values for 5:4 aspect ratio
outlier_attributes_5_4 = outlier_attributes.reshape(-1, 1)
poly_outlier_attributes_5_4 = poly_features_5_4.transform(outlier_attributes_5_4)
predicted_outlier_fov_5_4 = model_5_4.predict(poly_outlier_attributes_5_4)

# Predict FOV for outlier attributes.xml values for 4:3 aspect ratio
outlier_attributes_4_3 = outlier_attributes.reshape(-1, 1)
poly_outlier_attributes_4_3 = poly_features_4_3.transform(outlier_attributes_4_3)
predicted_outlier_fov_4_3 = model_4_3.predict(poly_outlier_attributes_4_3)

# Predict FOV for outlier attributes.xml values for 3:2 aspect ratio
outlier_attributes_3_2 = outlier_attributes.reshape(-1, 1)
poly_outlier_attributes_3_2 = poly_features_3_2.transform(outlier_attributes_3_2)
predicted_outlier_fov_3_2 = model_3_2.predict(poly_outlier_attributes_3_2)

# Predict FOV for outlier attributes.xml values for 16:10 aspect ratio
outlier_attributes_16_10 = outlier_attributes.reshape(-1, 1)
poly_outlier_attributes_16_10 = poly_features_16_10.transform(outlier_attributes_16_10)
predicted_outlier_fov_16_10 = model_16_10.predict(poly_outlier_attributes_16_10)

# Predict FOV for outlier attributes.xml values for 27:16 aspect ratio
outlier_attributes_27_16 = outlier_attributes.reshape(-1, 1)
poly_outlier_attributes_27_16 = poly_features_27_16.transform(outlier_attributes_27_16)
predicted_outlier_fov_27_16 = model_27_16.predict(poly_outlier_attributes_27_16)

# Predict FOV for outlier attributes.xml values for 16:9 aspect ratio
outlier_attributes_16_9 = outlier_attributes.reshape(-1, 1)
poly_outlier_attributes_16_9 = poly_features_16_9.transform(outlier_attributes_16_9)
predicted_outlier_fov_16_9 = model_16_9.predict(poly_outlier_attributes_16_9)

# Predict FOV for outlier attributes.xml values for 256:135 aspect ratio
outlier_attributes_256_135 = outlier_attributes.reshape(-1, 1)
poly_outlier_attributes_256_135 = poly_features_256_135.transform(outlier_attributes_256_135)
predicted_outlier_fov_256_135 = model_256_135.predict(poly_outlier_attributes_256_135)

# Predict FOV for outlier attributes.xml values for 2:1 aspect ratio
#outlier_attributes_2_1 = outlier_attributes.reshape(-1, 1)
#poly_outlier_attributes_2_1 = poly_features_2_1.transform(outlier_attributes_2_1)
#predicted_outlier_fov_2_1 = model_2_1.predict(poly_outlier_attributes_2_1)

# Predict FOV for outlier attributes.xml values for 64:27 aspect ratio
outlier_attributes_64_27 = outlier_attributes.reshape(-1, 1)
poly_outlier_attributes_64_27 = poly_features_64_27.transform(outlier_attributes_64_27)
predicted_outlier_fov_64_27 = model_64_27.predict(poly_outlier_attributes_64_27)

# Predict FOV for outlier attributes.xml values for 43:18 aspect ratio
outlier_attributes_43_18 = outlier_attributes.reshape(-1, 1)
poly_outlier_attributes_43_18 = poly_features_43_18.transform(outlier_attributes_43_18)
predicted_outlier_fov_43_18 = model_43_18.predict(poly_outlier_attributes_43_18)

# Predict FOV for outlier attributes.xml values for 12:5 aspect ratio
outlier_attributes_12_5 = outlier_attributes.reshape(-1, 1)
poly_outlier_attributes_12_5 = poly_features_12_5.transform(outlier_attributes_12_5)
predicted_outlier_fov_12_5 = model_12_5.predict(poly_outlier_attributes_12_5)

# Predict FOV for outlier attributes.xml values for 32:9 aspect ratio
outlier_attributes_32_9 = outlier_attributes.reshape(-1, 1)
poly_outlier_attributes_32_9 = poly_features_32_9.transform(outlier_attributes_32_9)
predicted_outlier_fov_32_9 = model_32_9.predict(poly_outlier_attributes_32_9)

# Predict FOV for outlier attributes.xml values for 4:1 aspect ratio
outlier_attributes_4_1 = outlier_attributes.reshape(-1, 1)
poly_outlier_attributes_4_1 = poly_features_4_1.transform(outlier_attributes_4_1)
predicted_outlier_fov_4_1 = model_4_1.predict(poly_outlier_attributes_4_1)

# Predict FOV for outlier attributes.xml values for 16:3 aspect ratio
outlier_attributes_16_3 = outlier_attributes.reshape(-1, 1)
poly_outlier_attributes_16_3 = poly_features_16_3.transform(outlier_attributes_16_3)
predicted_outlier_fov_16_3 = model_16_3.predict(poly_outlier_attributes_16_3)

# Prompt user to show outlier data or not
user_input = input("Do you want to show outlier data? (yes/no): ").lower()

# Update show_outliers based on user input
if user_input == 'yes':
    show_outliers = 'yes'
    extension_range = np.linspace(min(outlier_attributes), max(outlier_attributes), num=100).reshape(-1, 1)


    ### PLOT OUTLIER DATA ###

    # Plot the outlier points for 1:1 aspect ratio
    #plt.scatter(outlier_attributes, predicted_outlier_fov_1_1, label='Outlier Predicted FOV (1:1)', color='dark pink', marker='x')

    # Plot the outlier points for 5:4 aspect ratio
    plt.scatter(outlier_attributes, predicted_outlier_fov_5_4, label='Outlier Predicted FOV (5:4)', color='orange', marker='x')

    # Plot the outlier points for 4:3 aspect ratio
    plt.scatter(outlier_attributes, predicted_outlier_fov_4_3, label='Outlier Predicted FOV (4:3)', color='red', marker='x')

    # Plot the outlier points for 3:2 aspect ratio
    plt.scatter(outlier_attributes, predicted_outlier_fov_3_2, label='Outlier Predicted FOV (3:2)', color='maroon', marker='x')

    # Plot the outlier points for 16:10 aspect ratio
    plt.scatter(outlier_attributes, predicted_outlier_fov_16_10, label='Outlier Predicted FOV (16:10)', color='purple', marker='x')

    # Plot the outlier points for 27:16 aspect ratio
    plt.scatter(outlier_attributes, predicted_outlier_fov_27_16, label='Outlier Predicted FOV (27:16)', color='pink', marker='x')

    # Plot the outlier points for 16:9 aspect ratio
    plt.scatter(outlier_attributes, predicted_outlier_fov_16_9, label='Outlier Predicted FOV (16:9)', color='teal', marker='x')

    # Plot the outlier points for 256:135 aspect ratio
    plt.scatter(outlier_attributes, predicted_outlier_fov_256_135, label='Outlier Predicted FOV (256:135)', color='cyan', marker='x')

    # Plot the outlier points for 2:1 aspect ratio
    #plt.scatter(outlier_attributes, predicted_outlier_fov_2_1, label='Outlier Predicted FOV (2:1)', color='dark blue', marker='x')

    # Plot the outlier points for 64:27 aspect ratio
    plt.scatter(outlier_attributes, predicted_outlier_fov_64_27, label='Outlier Predicted FOV (64:27)', color='lime', marker='x')

    # Plot the outlier points for 43:18 aspect ratio
    plt.scatter(outlier_attributes, predicted_outlier_fov_43_18, label='Outlier Predicted FOV (43:18)', color='#FFD700', marker='x')

    # Plot the outlier points for 12:5 aspect ratio
    plt.scatter(outlier_attributes, predicted_outlier_fov_12_5, label='Outlier Predicted FOV (12:5)', color='#006400', marker='x')

    # Plot the outlier points for 32:9 aspect ratio
    plt.scatter(outlier_attributes, predicted_outlier_fov_32_9, label='Outlier Predicted FOV (32:9)', color='blue', marker='x')

    # Plot the outlier points for 4:1 aspect ratio
    plt.scatter(outlier_attributes, predicted_outlier_fov_4_1, label='Outlier Predicted FOV (4:1)', color='#8B0000', marker='x')

    # Plot the outlier points for 16:3 aspect ratio
    plt.scatter(outlier_attributes, predicted_outlier_fov_4_1, label='Outlier Predicted FOV (4:1)', color='#8B0050', marker='x')

else:
    extension_range = np.linspace(min(attributes_32_9), max(attributes_32_9), num=100).reshape(-1, 1)


### PREDICTION GENERATION ###


# Generate predictions for the extension range for 1:1 aspect ratio
#poly_extension_range_1_1 = poly_features_1_1.transform(extension_range)
#predicted_extension_fov_1_1 = model_1_1.predict(poly_extension_range_1_1)
    
# Generate predictions for the extension range for 5:4 aspect ratio
poly_extension_range_5_4 = poly_features_5_4.transform(extension_range)
predicted_extension_fov_5_4 = model_5_4.predict(poly_extension_range_5_4)

# Generate predictions for the extension range for 4:3 aspect ratio
poly_extension_range_4_3 = poly_features_4_3.transform(extension_range)
predicted_extension_fov_4_3 = model_4_3.predict(poly_extension_range_4_3)

# Generate predictions for the extension range for 3:2 aspect ratio
poly_extension_range_3_2 = poly_features_3_2.transform(extension_range)
predicted_extension_fov_3_2 = model_3_2.predict(poly_extension_range_3_2)

# Generate predictions for the extension range for 16:10 aspect ratio
poly_extension_range_16_10 = poly_features_16_10.transform(extension_range)
predicted_extension_fov_16_10 = model_16_10.predict(poly_extension_range_16_10)

# Generate predictions for the extension range for 27:16 aspect ratio
poly_extension_range_27_16 = poly_features_27_16.transform(extension_range)
predicted_extension_fov_27_16 = model_27_16.predict(poly_extension_range_27_16)

# Generate predictions for the extension range for 16:9 aspect ratio
poly_extension_range_16_9 = poly_features_16_9.transform(extension_range)
predicted_extension_fov_16_9 = model_16_9.predict(poly_extension_range_16_9)

# Generate predictions for the extension range for 256:135 aspect ratio
poly_extension_range_256_135 = poly_features_256_135.transform(extension_range)
predicted_extension_fov_256_135 = model_256_135.predict(poly_extension_range_256_135)

# Generate predictions for the extension range for 2:1 aspect ratio
#poly_extension_range_2_1 = poly_features_2_1.transform(extension_range)
#predicted_extension_fov_2_1 = model_2_1.predict(poly_extension_range_2_1)

# Generate predictions for the extension range for 64:27 aspect ratio
poly_extension_range_64_27 = poly_features_64_27.transform(extension_range)
predicted_extension_fov_64_27 = model_64_27.predict(poly_extension_range_64_27)

# Generate predictions for the extension range for 43:18 aspect ratio
poly_extension_range_43_18 = poly_features_43_18.transform(extension_range)
predicted_extension_fov_43_18 = model_43_18.predict(poly_extension_range_43_18)

# Generate predictions for the extension range for 12:5 aspect ratio
poly_extension_range_12_5 = poly_features_12_5.transform(extension_range)
predicted_extension_fov_12_5 = model_12_5.predict(poly_extension_range_12_5)

# Generate predictions for the extension range for 32:9 aspect ratio
poly_extension_range_32_9 = poly_features_32_9.transform(extension_range)
predicted_extension_fov_32_9 = model_32_9.predict(poly_extension_range_32_9)

# Generate predictions for the extension range for 4:1 aspect ratio
poly_extension_range_4_1 = poly_features_4_1.transform(extension_range)
predicted_extension_fov_4_1 = model_4_1.predict(poly_extension_range_4_1)

# Generate predictions for the extension range for 16:3 aspect ratio
poly_extension_range_16_3 = poly_features_16_3.transform(extension_range)
predicted_extension_fov_16_3 = model_16_3.predict(poly_extension_range_16_3)


### PLOT RESULTS ###

# Plot the results for 1:1 aspect ratio
#plt.scatter(attributes_1_1, fov_1_1, label='1:1 Integer FOV degrees', color='dark pink')
#plt.plot(attributes_1_1, predicted_fov_1_1, color='dark pink', linestyle='--', label='1:1 Predicted FOV')

# Plot the results for 5:4 aspect ratio
plt.scatter(attributes_5_4, fov_5_4, label='5:4 Integer FOV degrees', color='orange')
plt.plot(attributes_5_4, predicted_fov_5_4, color='orange', linestyle='--', label='5:4 Predicted FOV')

# Plot the results for 4:3 aspect ratio
plt.scatter(attributes_4_3, fov_4_3, label='4:3 Integer FOV degrees', color='red')
plt.plot(attributes_4_3, predicted_fov_4_3, color='red', linestyle='--', label='4:3 Predicted FOV')

# Plot the results for 3:2 aspect ratio
plt.scatter(attributes_3_2, fov_3_2, label='3:2 Integer FOV degrees', color='maroon')
plt.plot(attributes_3_2, predicted_fov_3_2, color='maroon', linestyle='--', label='3:2 Predicted FOV')

# Plot the results for 16:10 aspect ratio
plt.scatter(attributes_16_10, fov_16_10, label='16:10 Integer FOV degrees', color='purple')
plt.plot(attributes_16_10, predicted_fov_16_10, color='purple', linestyle='--', label='16:10 Predicted FOV')

# Plot the results for 27:16 aspect ratio
plt.scatter(attributes_27_16, fov_27_16, label='27:16 Integer FOV degrees', color='pink')
plt.plot(attributes_27_16, predicted_fov_27_16, color='pink', linestyle='--', label='27:16 Predicted FOV')

# Plot the results for 16:9 aspect ratio
plt.scatter(attributes_16_9, fov_16_9, label='16:9 Integer FOV degrees', color='teal')
plt.plot(attributes_16_9, predicted_fov_16_9, color='teal', linestyle='--', label='16:9 Predicted FOV')

# Plot the results for 256:135 aspect ratio
plt.scatter(attributes_256_135, fov_256_135, label='256:135 Integer FOV degrees', color='cyan')
plt.plot(attributes_256_135, predicted_fov_256_135, color='cyan', linestyle='--', label='256:135 Predicted FOV')

# Plot the results for 2:1 aspect ratio
#plt.scatter(attributes_2_1, fov_2_1, label='2:1 Integer FOV degrees', color='dark blue')
#plt.plot(attributes_2_1, predicted_fov_2_1, color='dark blue', linestyle='--', label='2:1 Predicted FOV')

# Plot the results for 64:27 aspect ratio
plt.scatter(attributes_64_27, fov_64_27, label='64:27 Integer FOV degrees', color='lime')
plt.plot(attributes_64_27, predicted_fov_64_27, color='lime', linestyle='--', label='64:27 Predicted FOV')

# Plot the results for 43:18 aspect ratio
plt.scatter(attributes_43_18, fov_43_18, label='43:18 Integer FOV degrees', color='#FFD700')
plt.plot(attributes_43_18, predicted_fov_43_18, color='#FFD700', linestyle='--', label='43:18 Predicted FOV')

# Plot the results for 12:5 aspect ratio
plt.scatter(attributes_12_5, fov_12_5, label='12:5 Integer FOV degrees', color='#006400')
plt.plot(attributes_12_5, predicted_fov_12_5, color='#006400', linestyle='--', label='12:5 Predicted FOV')

# Plot the results for 32:9 aspect ratio
plt.scatter(attributes_32_9, fov_32_9, label='32:9 Integer FOV degrees', color='blue')
plt.plot(attributes_32_9, predicted_fov_32_9, color='blue', linestyle='--', label='32:9 Predicted FOV')

# Plot the results for 4:1 aspect ratio
plt.scatter(attributes_4_1, fov_4_1, label='4:1 Integer FOV degrees', color='#8B0000')
plt.plot(attributes_4_1, predicted_fov_4_1, color='#8B0000', linestyle='--', label='4:1 Predicted FOV')

# Plot the results for 16:3 aspect ratio
plt.scatter(attributes_16_3, fov_16_3, label='16:3 Integer FOV degrees', color='#8B0050')
plt.plot(attributes_16_3, predicted_fov_16_3, color='#8B0050', linestyle='--', label='16:3 Predicted FOV')


### DRAW EXAMPLE HEADSET H-FOV ###

# Draw the horizontal line for the fixed FOV
plt.axhline(y=fixed_fov, color='grey', linestyle='-', label=f'Valve Index Example H-FOV ({fixed_fov:.2f})')


### PLOT EXTENSION RANGE ###

# Plot the extension range for 1:1 aspect ratio
#plt.plot(extension_range, predicted_extension_fov_1_1, color='dark pink', linestyle='--')

# Plot the extension range for 5:4 aspect ratio
plt.plot(extension_range, predicted_extension_fov_5_4, color='orange', linestyle='--')

# Plot the extension range for 4:3 aspect ratio
plt.plot(extension_range, predicted_extension_fov_4_3, color='red', linestyle='--')

# Plot the extension range for 3:2 aspect ratio
plt.plot(extension_range, predicted_extension_fov_3_2, color='maroon', linestyle='--')

# Plot the extension range for 16:10 aspect ratio
plt.plot(extension_range, predicted_extension_fov_16_10, color='purple', linestyle='--')

# Plot the extension range for 27:16 aspect ratio
plt.plot(extension_range, predicted_extension_fov_27_16, color='pink', linestyle='--')

# Plot the extension range for 16:9 aspect ratio
plt.plot(extension_range, predicted_extension_fov_16_9, color='teal', linestyle='--')

# Plot the extension range for 256:135 aspect ratio
plt.plot(extension_range, predicted_extension_fov_256_135, color='cyan', linestyle='--')

# Plot the extension range for 2:1 aspect ratio
#plt.plot(extension_range, predicted_extension_fov_2_1, color='dark blue', linestyle='--')

# Plot the extension range for 64:27 aspect ratio
plt.plot(extension_range, predicted_extension_fov_64_27, color='lime', linestyle='--')

# Plot the extension range for 43:18 aspect ratio
plt.plot(extension_range, predicted_extension_fov_43_18, color='#FFD700', linestyle='--')

# Plot the extension range for 12:5 aspect ratio
plt.plot(extension_range, predicted_extension_fov_12_5, color='#006400', linestyle='--')

# Plot the extension range for 32:9 aspect ratio
plt.plot(extension_range, predicted_extension_fov_32_9, color='blue', linestyle='--')

# Plot the extension range for 4:1 aspect ratio
plt.plot(extension_range, predicted_extension_fov_4_1, color='#8B0000', linestyle='--')

# Plot the extension range for 16:3 aspect ratio
plt.plot(extension_range, predicted_extension_fov_16_3, color='#8B0050', linestyle='--')

plt.xlabel('attributes.xml FOV value')
plt.ylabel('Actual Game Horizontal Degrees (Points on Whole Number)')
plt.legend()
plt.show()