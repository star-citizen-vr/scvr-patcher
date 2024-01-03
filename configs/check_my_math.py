import math

horizontal_fov = 90  # replace with the actual horizontal field of view
vertical_fov = 60    # replace with the actual vertical field of view

diagonal_fov = math.degrees(math.sqrt(horizontal_fov**2 + vertical_fov**2))
print(diagonal_fov)
