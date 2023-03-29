

## Fork Status
- This is a fork of the great work done within scelts/gees (https://github.com/scelts/gees).
- This was done as an excercise in programming. 
- The look and feel of the application is as was, but I've played around with the implementation.

## significant updates

Taking the original scelts\gees (V1.1.0) as base:
* Operates using states (Flying, Landing, Taxiing & Taking off). I use simple rules for identifying your state. Go below 100 ft then you are Landing. On the ground below 30 kts you are Taxiing. If Taxing & rise above the ground, you are Flying etc. 
* Captures:
  - Plane being used
  - descent rate (feet per min)
  - impact (G)
  - 'Stopping distance' being the distance from first touching the ground to when your speed drops below 30 Kts (Max Taxi speed)
  - Number of bounces
  - If landing on an ATC managed runway
    * Touchdown distance from Runways aim point (m)
    * distance from runway center line (m)
    * Airport
  - Indicated Airspeed (kts)
  - Ground speed (Kts)
  - Headwind (Kts)
  - Crosswind (kts)
  - Sideslip (deg)
  - Bank angle (deg)
* The original test button, now is 'Show last' and retrieves the last landing details captured
* Main Window is draggable (to another screen)
* Main window min / max is-as any std window, It no longer using the tray.
* Build testing revealed I could only reliably build with option x64, others generated main Window exceptions.
* Creates to directory C:\Users\\{user}\Documents\MSFS2020Landings-Lside
* Creates landing log file Landings.v3.cvs

## To Install (Tested on 64 bit windows only)

Link to executable:  https://github.com/JJ-blip/lside/releases/download/v2.1.0/Lside.V210.zip

* unzip the application zip (e.g. Lside.V210.zip)
* execute Lside.exe

Lside.exe.config contains user changeable properties. 

## Known issues
* none aware of

## License
Distributed under the GNU General Public License v3.0 License. See `LICENSE` for more information. (Whatever you do with this, keep it open source)
