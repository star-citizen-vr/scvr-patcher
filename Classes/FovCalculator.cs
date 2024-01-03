using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

//Terms and Definitions
// 2D (Mono) Terms:
/* AS = Aspect Value of HRES/VRES
* AR = Aspect Ratio formatted as HRES:VRES
* HRES = Horizontal Resolution
* VRES = Vertical Resolution
* FOV = Field of View (generic term)
* H-FOV = Horizontal FOV
* V-FOV = Vertical FOV
* D-FOV = Diagonal FOV
* HAR = Horizontal Arc Radiant Value
* VAR = Vertical Arc Radiant Value
* DAR = Diagonal Arc Radiant Value
* NCP = Near Clipping Plane
* FCP = Far Clipping Plane
*/
// 3D (Stereo) Terms:
/* PE-AS = Per-Eye Aspect Value of HRES/VRES
 * PE-AR = Per-Eye Aspect Ratio formatted as HRES:VRES
 * PE-HRES = Per-Eye Horizontal Resolution
 * PE-VRES = Per-Eye Vertical Resolution
 * RT-AS = Render Target Aspect Value of HRES/VRES
 * RT-AR = Render Target Aspect Ratio formatted as HRES:VRES
 * RT-HRES = Render Target Horizontal Resolution
 * RT-VRES = Render Target Vertical Resolution
 * NAS = Native Aspect Value of HRES/VRES (of HMD)
 * NAR = Native Aspect Ratio formatted as HRES:VRES (of HMD)
 * RRAR = Render Resolution Aspect Ratio formatted as HRES:VRES
 * OAR = Overlap Angular Range (of HMD)
 */
/* ST-HAR = Total Stereo Horizontal FOV (Left Eye's (Left of Center) View + Right Eye's (Right of Center) View)
 *      This is only necessary for Stereo HAR calculations. (Purple Triangle)
 *      ST-HAR = LE(LoC) + RE(RoC)
 * ST-VAR = Total Stereo Vertical FOV (Unchanged from 2D VAR)
 *      Defining this anyways, normally Stereo vision doesn't change vertcal's FOV unless lenses are uncentered from the display.
 *      This could be an edge case that comes up in the fugure, so I'm defining it now. (Dark Yellow Triangle)
 *      ST-VAR = VAR
 *      Potential Future Equation: ST-VAR = AVERAGE(LE(ToC+BoC), RE(ToC+BoC))
 * ST-DAR = Total Stereo Diagonal FOV (Left Eye's (Top-Left_Diagonal of Center) + Right Eye's (Bottom-Right_Diagonal of Center)) (Teal Triangle)
 *      ST-DAR = AVERAGE(LE(TLDoC)+RE(BRDoC), LE(BLDoC)+RE(TRDoC))
 * ST-OAR = Stereo Total Overlap Angular Range (Left Eye's (Right of Center) + Right Eye's (Left of Center))
 *      This is only necessary for Stereo OAR (Red Triangle)
 */
/* HAM = Hidden Area Mask (Obscurities / Unused Rectangular display vs Circular Lenses)
 * HST-HAR = Total Stereo Horizontal FOV (Left Eye's (Left of Center) View + Right Eye's (Right of Center) View) with HAM applied
 * HST-VAR = Total Stereo Vertical FOV (Unchanged from 2D VAR) with HAM applied
 * HST-DAR = Total Stereo Diagonal FOV (Left Eye's (Top-Left_Diagonal of Center) + Right Eye's (Bottom-Right_Diagonal of Center)) with HAM applied
 * HST-OAR = Stereo Total Overlap Angular Range (Left Eye's (Right of Center) + Right Eye's (Left of Center)) with HAM applied
 */
/* PP = Parallel Projection
 * VG = View Geometry
 * OPoL = Overscan Percentage of Loss (Parts of the 4:3 (or other) aspect ratio of Star Citizen that would be lost to the viewer's perspective if the vertical is matched 1:1 PE-VRES=VRES)
 * Note: A small percentage would be good to save on performance.
 *       A larger percentage would be good for low framerates when a user looks left or right at speed. Geometry is already there for the user to look at
 */
/* HMD = Head Mounted Display
 * LE = Left Eye
 * RE = Right Eye
 * LoC = Left of Center
 * RoC = Right of Center
 * ToC = Top of Center
 * BoC = Bottom of Center
 * TLDoC = Top-Left Diagonal of Center
 * TRDoC = Top-Right Diagonal of Center
 * BLDoC = Bottom-Left Diagonal of Center
 * BRDoC = Bottom-Right Diagonal of Center
 */
// 2D (Mono) Equations:
// 3D (Stereo) Equations:
//Use Headset's ST-VAR and AR to transpose it's ST-DAR to SC's DAR by using SC's AR (VAR should still equal ST-VAR)

namespace SCVRPatcher.Classes
{
    internal class FovCalculator
    {
        private static readonly NLog.Logger Logger = NLog.LogManager.GetCurrentClassLogger();
        internal void Initialize()
        {
            Logger.Info($"{nameof(FovCalculator)}");
            
        }
    }
}
