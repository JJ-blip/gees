
## Lside Overview

* C# Windows application that captures and displays Landing data from the Microsoft Flight Simulator 2020
* The application automatically connects to the running instance of MSFS.
* Its main control window can be hidden or moved to another screen.
* On passing down through a descent altitude it automatically captures landing data and presents it via a sliding window after the 
plane transitions to taxing.
* Optionally, on passing down through a higher descent altitude it automatically captures slip data into a side file.
* Slip data can be displayed tabularly of graphically.


## Significant updates (from original fork)

Taking the original scelts\gees (V1.1.0) as base:
* Operates using States (Flying, Landing, Taxing & Taking off). I use simple rules for identifying your state. Go below 100 ft then you are Landing. On the ground below 30 kts you are Taxiing. If Taxing & rise above the ground, you are Flying etc. 
* Captures:
  - Plane being used
  - Vertical speed at touchdown (feet per min)
  - Impact (G)
  - Stopping distance (ft) 
   * The distance from first touching the ground to when your speed drops below 30 Kts (Max Taxi speed)
  - Number of bounces
  - If landing on an ATC runway
    * Touchdown distance from Runways aim point (ft)
    * Distance from runway center line (ft)
    * Airport
  - Indicated Airspeed (kts)
  - Ground speed (Kts)
  - Headwind (Kts)
    * The average over the landing, +ve is a headwind, -ve is a tailwind
  - Crosswind (kts)
    * The average over the landing, + is blowing from the left to right
  - Slip angle (deg)
    * The angle between relative wind speed in planes longitudinal and lateral  axis. A slip to right is +ve
  - Bank angle (deg)
    * The angle between planes lateral  axis and the horizon 
  - Drift angle (deg)
    * The angle between the planes heading and track
  - Relative Wind Z (kts)
    * The relative windspeed along the planes longitudinal (Z) axis
  - Relative Wind X (kts)
    * The relative windspeed along the planes lateral (X) axis
* Main Window is draggable (to another screen)
* Main window min / max is-as any std window.
* Build testing revealed I could only reliably build with option x64, others generated main Window exceptions.
* Creates to directory C:\Users\\{user}\Documents\MSFS2020Landings-Lside
* Creates landing log file, currently Landings.v5.cvs
* Creates slip log file SlipLog-{plane}-{airport}-{datetime}.csv
* Landing buttons
  - 'My landing', details all landing captured to date
  - 'Show Last' button, reveals an auto closing sliding window displaying details of the most recent landing
* Slip buttons
  - 'Show Last' button, details the captured data from the most recent landing
  - 'Browse All' button, details list of captured data files from which one can be opened
  - 'On' button, commences slip capture. Capture automatically starts when you descend below 1000 ft
  - 'Off' button, ends the current slip capture and writes the data to a side file
* Tabular slip data can be shown graphically, its sort of self evident how it works. The 'All Off' will be a key you should levitate too as the data is very busy. Its questionable how much use it is, but it interesting to see.

## Configuration

* The file Lside.exe.config contains configuration properties for the above. If you edit it, you need to follow the files obvious pattern.
 - AutoCloseLanding
   * True or False, whether the sliding window should auto close
 - CloseAfterLanding
   * number, how long e.g. 10 (seconds) the sliding window should remain open
 - MaxTaxiSpeedKts
   * number e.g. 30, the speed that transitions between landing and taxing
 - LandingThresholdFt
   * number e.g. 100, the height in feet at which Landing data commences to be captured
 - LandingDirectory
   * the path to the directory where files are saved
 - LandingFile
   * the file name where Landing data is saved
 - SlipLoggingThresholdFt
   * number e.g. 1000, the height in feet at which Slip data commences to be captured 
 - SlipLoggingIsEnabled
   * True or False, whether the Slip data is to be captured.

## To Install (Tested on 64 bit windows only)

Link to executable:  https://github.com/JJ-blip/lside/releases/download/v3.1.0/Lside.V310.zip

* unzip the application zip (e.g. Lside.V310.zip)
* execute Lside.exe

Lside.exe.config contains user changeable properties. 

## Known issues
* unfortunately, the sign of the headwind is in error, when written to the side files. It will be 1st priority to get fixed.

## Fork Status
- This is a fork of the great work done within scelts/gees (https://github.com/scelts/gees).
- This was done as an excercise in programming. 
- The look and feel of the application is as was, but I've played around with the implementation.

## License
Distributed under the GNU General Public License v3.0 License. See `LICENSE` for more information. (Whatever you do with this, keep it open source)
