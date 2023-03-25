

## Fork Status
- This is a fork of the great work done within scelts/gees (https://github.com/scelts/gees).
- This was done as an excercise in programming. 
- The look and feel of the application is as was, but I've played around with the implementation.

## significant updates

Taking the original scelts\gees (V1.1.0) as base:
* Operates using states (Flying, Landing, Taxiing & Taking off). I use simple rules for identifying your state. Go below 100 ft then you are Landing. On the ground below 30 kts you are Taxiing. If Taxing & rise above the ground, you are Flying etc. 
* Captures 'Landing distance' being the distance from first touching the ground to when your speed drops below 30 Kts (Max Taxi speed)
* The original test button, now is 'Show last' and retrieves the last landing details
* Main Window is dragable (to another screen)
* Main window min / max is as any std window, I no longer using the tray.
* Build testing revealed I could only reliably build with option x64, others generated main Window exceptions.
* Creates to directory C:\Users\\{user}\Documents\MSFS2020Landings-Lside
* Creates landing log file Landings.v2.cvs

## To Install (Tested on 64 bit windows only)

Link to executable:  https://github.com/JJ-blip/lside/releases/download/v2.0.0/Lside.V2.zip

* unzip Lside.V2.zip
* execute Lside.exe

Lside.exe.config contains user changeable properties. 

## License
Distributed under the GNU General Public License v3.0 License. See `LICENSE` for more information. (Whatever you do with this, keep it open source)
