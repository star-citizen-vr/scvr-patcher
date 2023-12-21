#Terms and Definitions# 2D (Mono) Terms:
"""
    HRES            - Horizontal Resolution
    VRES            - Vertical Resolution
    HAR/H-FOV       - Horizontal Angular Range / Horizontal Field of View (Magenta Triangle)
    VAR/V-FOV       - Vertical Angular Range / Vertical Field of View (Yellow Triangle)
    DAR/D-FOV       - Diagonal Angular Range / Diagonal Field of View (Cyan Triangle)
    NCP             - Near Clipping Plane
    FCP             - Far Clipping Plane
"""

#3D (Stereo) Terms:
"""
    PE-HRES         - Per Eye Horizontal Resolution
    PE-VRES         - Per Eye Vertical Resolution
    RT-HRES         - Render Target Horizontal Resolution
    RT-VRES         - Render Target Vertical Resolution
    NAR             - Native Aspect Ratio (of HMD)
    RRAR            - Render Resolution's Aspect Ratio (of HMD)
    OAR             - Overlap Angular Range (of HMD)

    ST-HAR          - Total Stereo Horizontal FOV (Left Eye's (Left of Center) View + Right Eye's (Right of Center) View) | This is only necessary for Stereo HAR calculations. (Purple Triangle)
        ST-HAR = LE(LoC)+RE(RoC)
    ST-OAR/SO-FOV   - Stereo Total Overlap Angular Ranage / Overlap Field of View (Left Eye's (Right of Center) + (Right Eye's (Left of Center) | This is only necessary for Stereo OAR (Red Triangle)
        ST-OAR = LE(RoC)+RE(LoC)
    ST-DAR          - Total Stereo Diagonal FOV (Left Eye's (Top-Left_Diagonal of Center) + Right Eye's Bottom-Right_Diagonal of Center) | This is only necessary for Stereo DAR (Teal Triangle)
        ST-DAR = LE(TLDoC)+RE(BRDoC)
    ST-VAR          - Total Stereo Vertical FOV (Unchanged from 2D VAR) | Defining anyways, but Stereo vision doesn't change vertical's FOV unless Lens is uncentered from display. (This could be an edge case that comes up, so we will define it anyways) (Dark-Yellow Triangle)
        ST-VAR = VAR
        Potential Future Equation:  ST-VAR = LE(ToC)+RE(BoC)

    HAM             - Hidden Area Mask (Obscurities / Unused Rectangular display vs Circular FOV of physical lenses)
    HST-HAR         - Total Stereo Horizontal FOV minus HAM's percentage of horizontal cover up. (If HAM = 0, use ST-HAR)
        If HAM = 0, HST-HAR = ST-HAR; Else If HAM != 0, HST-HAR = ST-HAR-(ST-HAR*HAM/100)
    HST-OAR         - Total Overlap Angular Range minus HAM's percentage of overlap cover up. (If HAM = 0, use ST-OAR)
        If HAM = 0, HST-OAR = ST-OAR; Else If HAM != 0, HST-OAR = ST-OAR-(ST-OAR*HAM/100)
    HST-DAR         - Total Diagonal Angular Range minus HAM's percentage of overlap cover up. (If HAM = 0, use ST-DAR)
        If HAM = 0, HST-DAR = ST-DAR; Else If HAM != 0, HST-DAR = ST-DAR-(ST-DAR*HAM/100)
    HST-VAR         - Total Vertical Angular Rnage minus HAM's percentage of overlap cover up. (If HAM = 0, use ST-VAR)
        If HAM = 0, HST-VAR = ST-VAR; Else If HAM != 0, HST-VAR = ST-VAR-(ST-VAR*HAM/100)
    
    PP              - Parallel Projection
    VG              - View Geometry
    OPL             - Overscan Percentage of Loss (Parts of the 4:3 (or other) aspect ratio of Star Citizen that would be lost to the viewer's perspective should vertical be 1:1 PE-VRES:VRES) note: a small percentage would be good, especially for lower frame rates as the user looks left to right.

"""
# Formulas:
"""
    ST-VAR = VAR
    VAR = 
"""



# Sample Data:
"""
            SAMPLE: --------------------------------------------------------|
            |  Headset: Pimax Crystal                                       |
            |    Left eye FOV:                    Right eye FOV:            |
            |    left:       -54.41 deg           left:       -51.29 deg    |
            |    right:       51.35 deg           right:       54.36 deg    |
            |    bottom:     -55.91 deg           bottom:     -55.87 deg    |
            |    top:         55.67 deg           top:         55.61 deg    |
            |    horiz.:     105.76 deg           horiz.:     105.65 deg    |
            |    vert.:      111.58 deg           vert.:      111.48 deg    |
            |                                                               |
            |               Total FOV:                                      |
            |               horizontal: 108.77 deg                          |
            |               vertical:   111.48 deg                          |
            |               diagonal:   127.48 deg                          |
            |               overlap:    102.64 deg                          |
            |                                                               |
            |               View geometry:                                  |
            |               left panel rotation:     0.0 deg                |
            |               right panel rotation:    0.0 deg                |
            |---------------------------------------------------------------|
"""

# Sample 2 Data:
"""
Recommended render target size: [2468, 2740]                         
                                                                     
Left eye HAM mesh:                                   Right eye HAM mesh:
     original vertices: 99, triangles: 33                 original vertices: 99, triangles: 33
    optimized vertices: 38, n-gons: 3                    optimized vertices: 38, n-gons: 3
             mesh area: 24.88 %                                   mesh area: 24.88 %
                                                     
Left eye to head transformation matrix:              Right eye to head transformation matrix:
    [[ 0.996195,  0.      ,  0.087156, -0.029   ],       [[ 0.996195,  0.      , -0.087156,  0.029   ],
     [ 0.      ,  1.      ,  0.      ,  0.      ],        [ 0.      ,  1.      ,  0.      ,  0.      ],
     [-0.087156,  0.      ,  0.996195,  0.      ]]        [ 0.087156,  0.      ,  0.996195,  0.      ]]
                                                     
Left eye raw LRBT values:                            Right eye raw LRBT values:
    left:        -1.279120                               left:        -1.257541
    right:        1.258676                               right:        1.281748
    bottom:      -1.408557                               bottom:      -1.407222
    top:          1.413973                               top:          1.415720
                                                     
Left eye raw FOV:                                    Right eye raw FOV:
    left:       -49.00 deg                               left:       -47.95 deg
    right:       47.98 deg                               right:       49.06 deg
    bottom:     -54.63 deg                               bottom:     -54.60 deg
    top:         54.73 deg                               top:         54.76 deg
    horiz.:      96.98 deg                               horiz.:      97.01 deg
    vert.:      109.36 deg                               vert.:      109.37 deg
                                                     
Left eye head FOV:                                   Right eye head FOV:
    left:       -54.00 deg                               left:       -42.95 deg
    right:       42.98 deg                               right:       54.06 deg
    bottom:     -54.52 deg                               bottom:     -54.50 deg
    top:         54.63 deg                               top:         54.66 deg
    horiz.:      96.98 deg                               horiz.:      97.01 deg
    vert.:      109.15 deg                               vert.:      109.16 deg
                            
                            Total FOV:
                                horizontal: 108.06 deg
                                vertical:   109.16 deg
                                diagonal:   114.43 deg
                                overlap:     85.93 deg
                            
                            View geometry:
                                left view rotation:    -5.0 deg
                                right view rotation:    5.0 deg
                                reported IPD:          58.0 mm

"""


# Steps that are needed:
"""
1) Check that Parallel Projection is disabled on Pimax and Index headsets

2) Find Native Aspect Ratio (NAR) of HMD
    a. This can be found from pulling HMD informaiton, See: https://github.com/risa2000/hmdq

3) Compare Native Aspect Ratio (NAR) to the Render Resolution's Aspect Ratio (RRAR)





X) Calculate Star Citizen DAR based on 4:3 aspect ratio lock
    Find HAR and VAR from DAR
    
    Match VAR to ST-VAR, Else Match VAR to HST-VAR

    If VAR = ST-VAR/HST-VAR, FOV should be equal
    Zoom VorpX to match V-RES


    

?   Target Render Res = Left Eye resolution - Over lap degrees + right eye resolution       ?

    Check to see if headset target render res is wider than 4:3 aspect ratio, If so, look at 16:10 or 16:9 aspect ratios.


    Cal all data to find render res + aspection ratio
    change aspect to 4:3 and use fix vertical to find the new horizontal & diagonal
    headset vertical = screen vertical
    screen vertical lock to 4:3 and calculate new horizontal and old vertical to find new diagonal.

"""


import math
"""
# Temporarily define the headset geometry
headset_geometry = {
  "left": {"left": -51.65, "right": 41.65, "bottom": -51.88, "top": 51.88, "horiz": 93.31, "vert": 103.75},
  "right": {"left": -41.65, "right": 51.65, "bottom": -51.88, "top": 51.88, "horiz": 93.31, "vert": 103.75}
}

# Temporarily define the render resolution
render_resolution = [3232, 3824]

# Temporarily define the HAM
ham = {"left": None, "right": None}

# Temporarily define the view geometry (Is Parallel Projection on?)
view_geometry = {"left": 0.0, "right": 0.0, "IPD": 62.0}


# Calculate the HMD total FOV
#def calculate_fov_hmd(headset_geometry):
    # Check to see if Parallel Projection is on
    if 

"""